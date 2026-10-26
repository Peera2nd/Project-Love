using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ProjectLove.Core;
using ProjectLove.Data;
namespace ProjectLove.Simulation
{
    public sealed partial class ProductionSystem
    {
        public bool PlanningEnabled => Enabled && Config.planning != null;
        public int Cursor => session.State.production.resolvedPhases;
        public TaskPlanState PlanFor(string id) => session.State.production.planning?.tasks.FirstOrDefault(t => t.taskId == id);
        public float ExactEffort(string id) => Find(id).effort + (PlanFor(id)?.fraction ?? 0);
        private void InitializePlanning()
        {
            if (!PlanningEnabled) return;
            var p = session.State.production.planning = new PlanningState { seed = Guid.NewGuid().ToString("N") };
            foreach (var t in Config.tasks)
                p.tasks.Add(new TaskPlanState { taskId=t.id,
                    start=(Math.Max(t.releaseDay,t.sprint.startDay)-1)*4,
                    due=(t.sprint.endDay-1)*4+(int)DayPhase.Evening });
            SnapshotPlanningDay(session.State);
        }
        private void SnapshotPlanningDay(RuntimeGameState state)
        {
            state.production.planning.workers = Config.workers.Select(w => new WorkerDayState {
                characterId=w.character.id,day=state.day,stress=state.FindCharacter(w.character.id).stress }).ToList();
        }
        private TaskState PlannedActive(RuntimeGameState state,string owner) => state.production.tasks
            .Where(t=>t.ownerId==owner && t.acceptedDay==0 && t.effort<Definition(t).effort &&
                Released(state,Definition(t)) && !Blocked(state,t) &&
                state.production.planning.tasks.Single(p=>p.taskId==t.taskId).start<=state.production.resolvedPhases)
            .OrderBy(t=>t.priority).FirstOrDefault();
        private PlanningStressBand PlanningBand(int stress) => Config.planning.bands
            .Where(b=>b.minimum<=stress).OrderByDescending(b=>b.minimum).First();
        // Keyed draws have no mutable RNG cursor; previews never consume production randomness.
        private static double Draw(string seed,string key)
        {
            using(var h=SHA256.Create())
            {
                var bytes=h.ComputeHash(Encoding.UTF8.GetBytes("planning-v1|"+seed+"|"+key));
                ulong n=0;for(int i=0;i<8;i++) n=(n<<8)|bytes[i];
                return (n>>11)*(1.0/9007199254740992.0);
            }
        }
        private float PlanningCapacity(RuntimeGameState state,WorkerDefinition worker,TaskState task)
        {
            var c=Config.planning;float factor=1;
            if(c.randomness)
            {
                // Definition owner, not mutable assignment, anchors the task identity.
                factor=(float)(1+c.taskSpread*(2*Draw(state.production.planning.seed,"task|"+Definition(task).owner.id+"|"+task.taskId)-1));
                int dayStress=state.production.planning.workers.Single(w=>w.characterId==worker.character.id).stress;
                factor*=(float)(1+PlanningBand(dayStress).dailySpread*(2*Draw(state.production.planning.seed,"day|"+worker.character.id+"|"+state.day.ToString(CultureInfo.InvariantCulture))-1));
            }
            return worker.baseRate*PlanningBand(state.FindCharacter(worker.character.id).stress).mean*factor;
        }
        private void ResolvePlanningPhase(RuntimeGameState state,ProductionAction? action,string target,List<WorkerForecast> forecast)
        {
            var p=state.production.planning;int cursor=state.production.resolvedPhases;
            var estimate=EstimatePlanning(state,null,-1);
            bool productive=Config.productivePhases.Contains(state.phase);
            var targets=Config.workers.ToDictionary(w=>w.character.id,w=>PlannedActive(state,w.character.id));
            foreach(var worker in Config.workers)
            {
                string id=worker.character.id;var person=state.FindCharacter(id);var task=targets[id];
                var delta=new WorkerForecast {characterId=id,taskId=task?.taskId,before=task?.effort??0,stressBefore=person.stress};
                bool rest=Ordered(state,id,StaffOrderKind.Rest);
                bool overtime=state.phase==DayPhase.Night&&Ordered(state,id,StaffOrderKind.Overtime);
                bool eligible=(productive||overtime)&&!rest&&!Absent(state,id)&&task!=null;
                if(overtime) p.nights.Add(new NightWorkerState {characterId=id,day=state.day});
                int pressure=PlanningPressure(state,id,estimate);
                if(rest) person.stress=Math.Max(0,person.stress-Config.restStressRecovery);
                else if(eligible)
                {
                    var plan=p.tasks.Single(t=>t.taskId==task.taskId);
                    if(plan.started<0) plan.started=cursor;
                    float progress=Math.Min(Definition(task).effort,task.effort+plan.fraction+PlanningCapacity(state,worker,task));
                    if(Definition(task).effort-progress<.0001f) progress=Definition(task).effort;
                    int whole=(int)Math.Floor(progress);plan.fraction=progress-whole;
                    delta.team=whole-task.effort;task.effort=whole;
                    if(task.effort==Definition(task).effort) {plan.finished=cursor;plan.fraction=0;}
                    if(productive) person.stress=Math.Min(100,person.stress+Config.planning.normalStress+pressure*Config.planning.pressureStress);
                }
                else if(productive&&!Absent(state,id))
                {
                    bool waiting=state.production.tasks.Any(t=>t.ownerId==id&&t.acceptedDay==0&&Released(state,Definition(t)));
                    person.stress=waiting&&pressure>0?Math.Min(100,person.stress+Config.planning.waitingStress):Math.Max(0,person.stress-Config.planning.idleRecovery);
                }
                if(action==ProductionAction.Help&&target==id)
                {
                    bool first=!p.shared.Any(s=>s.characterId==id&&s.cursor/4==cursor/4);
                    int affection=first?Math.Min(100-person.affection,Config.planning.sharedAffection):0;
                    person.affection+=affection;
                    p.shared.Add(new SharedTimeState {characterId=id,taskId=task?.taskId??"",cursor=cursor,affection=affection,
                        nightWork=overtime&&eligible});
                }
                delta.after=task?.effort??0;delta.stressAfter=person.stress;forecast?.Add(delta);
            }
        }
        private int PlanningPressure(RuntimeGameState state,string owner,PlanningEstimate estimate)
        {
            int pressure=0;
            foreach(var task in state.production.tasks.Where(t=>t.ownerId==owner&&t.acceptedDay==0&&t.effort<Definition(t).effort&&Released(state,Definition(t))))
            {
                var d=estimate.tasks[task.taskId];var plan=state.production.planning.tasks.Single(t=>t.taskId==task.taskId);
                if(d.start<0) {if(plan.due<=state.production.resolvedPhases+4) pressure=Math.Max(pressure,1);continue;}
                int available=0;for(int k=d.start;k<=plan.due;k++) if(Config.productivePhases.Contains((DayPhase)(k%4))) available++;
                float ratio=d.slots/(float)Math.Max(1,available);
                pressure=Math.Max(pressure,available==0||ratio>1?2:ratio>Config.planning.comfortableLoad?1:0);
            }
            return pressure;
        }
        private void AppendPlanningFingerprint(Action<object[]> add)
        {
            if(!PlanningEnabled) return;
            var c=Config.planning;
            add(new object[]{"planning-v1",c.id,c.randomness,c.taskSpread,c.normalStress,c.pressureStress,c.waitingStress,c.idleRecovery,c.overnightRecovery,c.comfortableLoad,c.sharedAffection,c.forecastSamples});
            foreach(var band in c.bands) add(new object[]{band.minimum,band.mean,band.dailySpread});
            foreach(var task in Config.tasks) add(new object[]{task.id,task.requiresManualReview});
        }
        private static void ValidatePlanningDefinitions(GameConfigSO game)
        {
            var c=game.production.planning;if(c==null)return;
            bool valid=game.production.useProducerStages&&c.bands!=null&&c.bands.Length>0&&c.bands.All(b=>b!=null&&b.minimum>=0&&b.minimum<=100&&b.mean>0&&b.mean<=1&&b.dailySpread>=0&&b.dailySpread<=.5f)&&
                c.bands.Any(b=>b.minimum==0)&&c.bands.Select(b=>b.minimum).Distinct().Count()==c.bands.Length&&c.taskSpread>=0&&c.taskSpread<=.5f&&
                c.comfortableLoad>0&&c.comfortableLoad<=1&&c.forecastSamples>=4&&c.forecastSamples<=64&&c.dragThreshold>0&&c.autoScrollSpeed>0&&c.timelineCellWidth>=24&&c.timelineRowHeight>=60&&
                new[]{c.normalStress,c.pressureStress,c.waitingStress,c.idleRecovery,c.overnightRecovery,c.sharedAffection}.All(n=>n>=0);
            if(!valid) throw new ArgumentException("Invalid planning configuration.");
        }
        private bool ValidPlanning(RuntimeGameState state)
        {
            var p=state.production.planning;
            if(!PlanningEnabled) return p==null;
            if(p==null||!Guid.TryParseExact(p.seed,"N",out _)||p.revision<0||p.tasks==null||p.workers==null||p.shared==null||p.nights==null||
                p.tasks.Count!=Config.tasks.Length||p.tasks.Any(t=>t==null)||p.tasks.Select(t=>t.taskId).Distinct().Count()!=p.tasks.Count||
                p.workers.Count!=Config.workers.Length||p.workers.Any(w=>w==null||!Config.workers.Any(d=>d.character.id==w.characterId)||w.day!=state.day||w.stress<0||w.stress>100)||
                p.workers.Select(w=>w.characterId).Distinct().Count()!=p.workers.Count)return false;
            foreach(var plan in p.tasks)
            {
                var task=state.production.tasks.FirstOrDefault(t=>t.taskId==plan.taskId);if(task==null)return false;
                var def=Definition(task);
                if(plan.start<0||plan.due<plan.start||plan.due>=session.Config.totalDays*4||plan.start<(def.sprint.startDay-1)*4||
                    float.IsNaN(plan.fraction)||float.IsInfinity(plan.fraction)||plan.fraction<0||plan.fraction>=1||
                    plan.started < -1||plan.finished < -1||plan.started>=state.production.resolvedPhases||plan.finished>=state.production.resolvedPhases||
                    plan.finished>=0&&(plan.started<0||plan.finished<plan.started||task.effort!=def.effort)||
                    task.effort==def.effort&&(plan.fraction!=0||plan.finished<0)||
                    (task.effort>0||plan.fraction>0)&&plan.started<0||
                    task.acceptedDay>0&&task.acceptedDay<def.sprint.startDay)return false;
            }
            if(p.shared.Any(s=>s==null||!Config.workers.Any(w=>w.character.id==s.characterId)||s.cursor<0||s.cursor>=state.production.resolvedPhases||
                s.affection<0||s.affection>Config.planning.sharedAffection||(!string.IsNullOrEmpty(s.taskId)&&!Config.tasks.Any(t=>t.id==s.taskId))||
                s.nightWork&&(s.cursor%4!=3||!p.nights.Any(n=>n.characterId==s.characterId&&n.day==s.cursor/4+1)))||
                p.shared.GroupBy(s=>s.cursor).Any(g=>g.Count()>1)||p.shared.GroupBy(s=>s.characterId+"|"+s.cursor/4).Any(g=>g.Count(s=>s.affection>0)>1)) return false;
            return p.nights.All(n=>n!=null&&Config.workers.Any(w=>w.character.id==n.characterId)&&n.day>=1&&n.day<=state.day&&
                (n.day-1)*4+3<state.production.resolvedPhases)&&p.nights.GroupBy(n=>n.characterId+"|"+n.day).All(g=>g.Count()==1);
        }
    }
}
