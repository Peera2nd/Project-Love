using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
namespace ProjectLove.Simulation
{
    public sealed class PlanEdit { public string taskId; public int start,due,priority; }
    public sealed class PlanningDraft
    {
        public string catalog; public int cursor,revision;
        public List<PlanEdit> tasks=new List<PlanEdit>();
    }
    public sealed class EstimateTask { public int start=-1,finish=-1,slots; }
    public sealed class PlanningEstimate { public Dictionary<string,EstimateTask> tasks=new Dictionary<string,EstimateTask>(); }
    public sealed class TaskProjection
    { public string taskId; public int earliest,expected,latest,due; public bool unknown,atRisk; public int projectedStart,bufferSlots; }
    public sealed partial class ProductionSystem
    {
        public PlanningDraft CreateDraft() => !PlanningEnabled?null:new PlanningDraft {
            catalog=session.State.production.catalog,cursor=Cursor,revision=session.State.production.planning.revision,
            tasks=session.State.production.tasks.Select(t=>new PlanEdit {taskId=t.taskId,priority=t.priority,
                start=PlanFor(t.taskId).start,due=PlanFor(t.taskId).due}).ToList() };
        public bool MoveDraft(PlanningDraft draft,string sourceId,string beforeId)
        {
            if(!ValidDraft(draft)||sourceId==beforeId)return false;
            var source=Find(sourceId);var before=Find(beforeId);
            if(source==null||before==null||source.ownerId!=before.ownerId||source.acceptedDay>0||before.acceptedDay>0||
                source.effort==Definition(source).effort||before.effort==Definition(before).effort)return false;
            var order=draft.tasks.Where(t=>Find(t.taskId).ownerId==source.ownerId&&Find(t.taskId).acceptedDay==0).OrderBy(t=>t.priority).ToList();
            var priorities=order.Select(t=>t.priority).OrderBy(v=>v).ToArray();
            var moved=order.Single(t=>t.taskId==sourceId);order.Remove(moved);int index=order.FindIndex(t=>t.taskId==beforeId);order.Insert(index,moved);
            for(int i=0;i<order.Count;i++) order[i].priority=priorities[i];
            return true;
        }
        public bool MoveDraftToEnd(PlanningDraft draft,string sourceId)
        {
            if(!ValidDraft(draft))return false;
            var source=Find(sourceId);if(source==null||source.acceptedDay>0||source.effort==Definition(source).effort)return false;
            var order=draft.tasks.Where(t=>Find(t.taskId).ownerId==source.ownerId&&Find(t.taskId).acceptedDay==0).OrderBy(t=>t.priority).ToList();
            var priorities=order.Select(t=>t.priority).OrderBy(v=>v).ToArray();var moved=order.Single(t=>t.taskId==sourceId);
            order.Remove(moved);order.Add(moved);for(int i=0;i<order.Count;i++)order[i].priority=priorities[i];return true;
        }
        public bool SetDraftDates(PlanningDraft draft,string taskId,int start,int due)
        {
            if(!ValidDraft(draft))return false;var edit=draft.tasks.FirstOrDefault(t=>t.taskId==taskId);if(edit==null)return false;
            int oldStart=edit.start,oldDue=edit.due;edit.start=start;edit.due=due;
            if(ValidDraft(draft))return true;edit.start=oldStart;edit.due=oldDue;return false;
        }
        public bool DraftChanged(PlanningDraft draft) => ValidDraft(draft)&&draft.tasks.Any(e=>
            e.priority!=Find(e.taskId).priority||e.start!=PlanFor(e.taskId).start||e.due!=PlanFor(e.taskId).due);
        public bool TryApplyDraft(PlanningDraft draft)
        {
            if(!ValidDraft(draft)||session.State.dayEnded||session.Dialogue.IsActive||session.Interactions.IsOpen||
                !session.BoardOpen&&session.Events.HasForcedPending)return false;
            if(!DraftChanged(draft))return true;
            foreach(var e in draft.tasks)
            {var task=Find(e.taskId);var plan=PlanFor(e.taskId);task.priority=e.priority;plan.start=e.start;plan.due=e.due;}
            session.State.production.planning.revision++;session.NotifyChanged();return true;
        }
        private bool ValidDraft(PlanningDraft draft)
        {
            if(!PlanningEnabled||draft==null||draft.catalog!=session.State.production.catalog||draft.cursor!=Cursor||
                draft.revision!=session.State.production.planning.revision||draft.tasks==null||draft.tasks.Count!=Config.tasks.Length||
                draft.tasks.Any(t=>t==null)||draft.tasks.Select(t=>t.taskId).Distinct().Count()!=draft.tasks.Count||
                draft.tasks.Select(t=>t.priority).OrderBy(n=>n).Where((n,i)=>n!=i).Any())return false;
            foreach(var e in draft.tasks)
            {
                var task=Find(e.taskId);if(task==null)return false;var plan=PlanFor(e.taskId);
                if(e.start<(Math.Max(Definition(task).releaseDay,Definition(task).sprint.startDay)-1)*4||e.due<e.start||e.due>=session.Config.totalDays*4||
                    e.start!=plan.start&&(plan.started>=0||e.start<Cursor)||e.due!=plan.due&&e.due<Cursor||
                    (task.acceptedDay>0||task.effort==Definition(task).effort)&&(e.start!=plan.start||e.due!=plan.due||e.priority!=task.priority))return false;
            }
            return true;
        }
        public TaskProjection[] ProjectPlan(PlanningDraft draft=null)
        {
            if(!PlanningEnabled||draft!=null&&!ValidDraft(draft))return Array.Empty<TaskProjection>();
            var samples=Enumerable.Range(0,Config.planning.forecastSamples).Select(i=>EstimatePlanning(session.State,draft,i)).ToArray();
            var mean=EstimatePlanning(session.State,draft,-1);
            return Config.tasks.Select(t=>
            {
                var values=samples.Select(s=>s.tasks[t.id].finish).OrderBy(v=>v).ToArray();
                int due=draft?.tasks.Single(e=>e.taskId==t.id).due??PlanFor(t.id).due;
                bool unknown=values[0]<0;int Pick(float p)=>values[Math.Max(0,(int)Math.Ceiling(p*values.Length)-1)];
                return new TaskProjection {taskId=t.id,due=due,unknown=unknown,earliest=Pick(.1f),expected=Pick(.5f),latest=Pick(.9f),atRisk=unknown||Pick(.9f)>due,projectedStart=mean.tasks[t.id].start,bufferSlots=unknown?0:CountWorkSlots(Pick(.5f),due)};
            }).ToArray();
        }
        private int CountWorkSlots(int finish,int due)
        {
            int count=0;for(int k=Math.Min(finish,due)+1;k<=Math.Max(finish,due);k++)if(Config.productivePhases.Contains((DayPhase)(k%4)))count++;
            return due>=finish?count:-count;
        }
        public bool ShiftDraftDates(PlanningDraft draft,string id,int phases)
        {
            if(!ValidDraft(draft))return false;var e=draft.tasks.FirstOrDefault(t=>t.taskId==id);if(e==null)return false;
            long start=(long)e.start+phases,due=(long)e.due+phases;
            if(start<0||due>=session.Config.totalDays*4)return false;
            return SetDraftDates(draft,id,(int)start,(int)due);
        }
        private PlanningEstimate EstimatePlanning(RuntimeGameState state,PlanningDraft draft,int sample)
        {
            var result=new PlanningEstimate();var definitions=Config.tasks;int n=definitions.Length;
            var index=definitions.Select((t,i)=>new {t.id,i}).ToDictionary(t=>t.id,t=>t.i);
            var tasks=state.production.tasks.ToDictionary(t=>t.taskId);
            var plans=state.production.planning.tasks.ToDictionary(t=>t.taskId);
            var edits=draft?.tasks.ToDictionary(t=>t.taskId);
            var remaining=definitions.Select(t=>Math.Max(0,t.effort-tasks[t.id].effort-plans[t.id].fraction)).ToArray();
            var done=definitions.Select(t=>tasks[t.id].acceptedDay>0).ToArray();
            var known=definitions.Select(t=>!Config.events.Any(e=>e.tasks.Contains(t)&&!state.production.events.Any(r=>r.eventId==e.id))).ToArray();
            var deps=definitions.Select(t=>t.prerequisites.Select(p=>index[p.id]).ToArray()).ToArray();
            foreach(var d in definitions)result.tasks[d.id]=new EstimateTask {finish=plans[d.id].finished};
            var queues=Config.workers.ToDictionary(w=>w.character.id,w=>Enumerable.Range(0,n).Where(i=>tasks[definitions[i].id].ownerId==w.character.id)
                .OrderBy(i=>edits==null?tasks[definitions[i].id].priority:edits[definitions[i].id].priority).ToArray());
            string forecastSeed="forecast-v1|"+sample;
            int end=(session.Config.totalDays+14)*4;
            for(int cursor=state.production.resolvedPhases;cursor<end;cursor++)
            {
                bool productive=Config.productivePhases.Contains((DayPhase)(cursor%4));int day=cursor/4+1;
                var finishedThis=new List<int>();
                foreach(var worker in Config.workers)
                {
                    bool overtime=cursor==state.production.resolvedPhases&&state.phase==DayPhase.Night&&Ordered(state,worker.character.id,StaffOrderKind.Overtime);
                    if(!productive&&!overtime||state.production.absences.Any(a=>a.characterId==worker.character.id&&a.throughDay>=day)||
                        cursor==state.production.resolvedPhases&&Ordered(state,worker.character.id,StaffOrderKind.Rest))continue;
                    int? chosen=queues[worker.character.id].Where(i=>!done[i]&&remaining[i]>.0001f&&known[i]&&definitions[i].sprint.startDay<=day&&
                        (edits==null?plans[definitions[i].id].start:edits[definitions[i].id].start)<=cursor&&
                        (!definitions[i].decisionBlocked||tasks[definitions[i].id].decisionCleared)&&deps[i].All(d=>done[d])).Select(i=>(int?)i).FirstOrDefault();
                    if(!chosen.HasValue)continue;int target=chosen.Value;
                    var definition=definitions[target];var e=result.tasks[definition.id];if(e.start<0)e.start=cursor;e.slots++;
                    var band=PlanningBand(state.FindCharacter(worker.character.id).stress);float factor=1;
                    if(sample>=0&&Config.planning.randomness)
                    {
                        factor=(float)(1+Config.planning.taskSpread*(2*Draw(forecastSeed,"task|"+definition.owner.id+"|"+definition.id)-1));
                        factor*=(float)(1+band.dailySpread*(2*Draw(forecastSeed,"day|"+worker.character.id+"|"+day)-1));
                    }
                    remaining[target]=Math.Max(0,remaining[target]-worker.baseRate*band.mean*factor);
                    if(remaining[target]<.0001f){e.finish=cursor;if(!definition.requiresManualReview)finishedThis.Add(target);}
                }
                foreach(int i in finishedThis)done[i]=true;
            }
            return result;
        }
    }
}
