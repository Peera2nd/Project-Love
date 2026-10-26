using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.EditorTools;
using ProjectLove.Simulation;
using UnityEditor;
using UnityEngine;
namespace ProjectLove.Tests
{
    public sealed class PlanningTests
    {
        private GameConfigSO config;private GameSession session;private readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();private string directory;
        private T Copy<T>(T source) where T:UnityEngine.Object {var c=UnityEngine.Object.Instantiate(source);owned.Add(c);return c;}
        private T New<T>() where T:ScriptableObject {var c=ScriptableObject.CreateInstance<T>();owned.Add(c);return c;}
        [SetUp] public void Setup()
        {
            config=Copy(AssetDatabase.LoadAssetAtPath<GameConfigSO>(PlanningPrototypeBuilder.ConfigPath));
            config.production=Copy(config.production);config.production.planning=Copy(config.production.planning);config.events=Array.Empty<EventDataSO>();
            session=new GameSession(config);directory=Path.GetFullPath(Path.Combine("Logs","PlanningSaves",Guid.NewGuid().ToString("N")));
        }
        [TearDown] public void Teardown(){foreach(var o in owned)UnityEngine.Object.DestroyImmediate(o);owned.Clear();if(Directory.Exists(directory))Directory.Delete(directory,true);}
        private CharacterDataSO Grace=>config.characters.Single(c=>c.id=="studio_grace");
        private void Tiny(bool manual=false)
        {
            var p=config.production;p.planning.randomness=false;
            var sprint=New<SprintDataSO>();sprint.id="paper_sprint";sprint.startDay=1;sprint.endDay=config.totalDays;sprint.producerGoal=40;
            sprint.quotas=p.workers.Select(w=>new StreamQuota{stream=w.stream,effort=4}).ToArray();p.sprints=new[]{sprint};p.events=Array.Empty<WorkloadEvent>();
            WorkTaskSO Task(string id,string person,params WorkTaskSO[] deps)
            {
                var t=New<WorkTaskSO>();t.id=id;t.displayName=id;t.owner=config.characters.Single(c=>c.id==person);t.stream=p.workers.Single(w=>w.character==t.owner).stream;
                t.effort=4;t.mainWeight=1;t.releaseDay=1;t.sprint=sprint;t.prerequisites=deps;return t;
            }
            var a=Task("brief","studio_ploy");var b=Task("art","studio_plaifah",a);b.requiresManualReview=manual;
            p.tasks=new[]{a,b,Task("integration","studio_grace",b),Task("setup","studio_grace")};session=new GameSession(config);
        }
        private void Night(){while(session.State.phase!=DayPhase.Night)Assert.That(session.TryAdvancePhase(),Is.True);}
        [Test] public void ReadyAlternativeAndBoundaryAcceptanceRemoveRoutineBoardVisitsWithoutSpill()
        {
            Tiny();var work=session.Production;Assert.That(work.Active(Grace).taskId,Is.EqualTo("setup"));
            session.TryAdvancePhase();Assert.That(work.Find("brief").acceptedDay,Is.EqualTo(1));Assert.That(work.Find("art").effort,Is.Zero);
            session.TryAdvancePhase();Assert.That(work.Find("art").acceptedDay,Is.EqualTo(1));Assert.That(work.Find("integration").effort,Is.Zero);
            session.TryAdvancePhase();Assert.That(session.State.mainProgress,Is.EqualTo(100));Assert.That(session.CanRestore(session.State),Is.True);
        }
        [Test] public void ExplicitReviewCheckpointStillBlocksSuccessorUntilReview()
        {
            Tiny(true);session.TryAdvancePhase();session.TryAdvancePhase();Assert.That(session.Production.Find("art").acceptedDay,Is.Zero);
            Assert.That(session.Production.Active(Grace),Is.Null);Assert.That(session.Production.TryReview(),Is.True);
            session.TryAdvancePhase();Assert.That(session.Production.Find("integration").effort,Is.EqualTo(4));Assert.That(session.CanRestore(session.State),Is.True);
        }
        [Test] public void DraftPreviewAndInvalidCrossWorkerDropArePureAndApplyIsAtomic()
        {
            var work=session.Production;var draft=work.CreateDraft();string before=JsonUtility.ToJson(session.State);
            Assert.That(work.MoveDraft(draft,"studio_w1_grace_2","studio_w1_grace_1"),Is.True);
            Assert.That(work.MoveDraft(draft,"studio_w1_grace_2","studio_w1_ploy_1"),Is.False);
            work.ProjectPlan(draft);work.ProjectPlan(draft);Assert.That(JsonUtility.ToJson(session.State),Is.EqualTo(before));
            Assert.That(work.TryApplyDraft(draft),Is.True);Assert.That(session.State.production.planning.revision,Is.EqualTo(1));
            Assert.That(session.State.production.resolvedPhases,Is.Zero);Assert.That(work.Active(Grace).taskId,Is.EqualTo("studio_w1_grace_1"));
            Assert.That(work.TryApplyDraft(draft),Is.False);Assert.That(session.CanRestore(session.State),Is.True);
        }
        [Test] public void InvalidAndStaleDraftCannotPartiallyChangeDatesOrPriorities()
        {
            var work=session.Production;var draft=work.CreateDraft();string before=JsonUtility.ToJson(session.State);
            Assert.That(work.SetDraftDates(draft,draft.tasks[0].taskId,0,config.totalDays*4),Is.False);
            draft.tasks[0].priority=draft.tasks[1].priority;Assert.That(work.TryApplyDraft(draft),Is.False);Assert.That(JsonUtility.ToJson(session.State),Is.EqualTo(before));
            draft=work.CreateDraft();session.TryAdvancePhase();before=JsonUtility.ToJson(session.State);Assert.That(work.TryApplyDraft(draft),Is.False);
            Assert.That(JsonUtility.ToJson(session.State),Is.EqualTo(before));
        }
        [Test] public void WorkTogetherHasNoProductionOrStressBuffAndOnlyFirstDailyAffection()
        {
            var baseline=new GameSession(config);Assert.That(baseline.TryRestore(session.State.Clone()),Is.True);
            for(int i=0;i<2;i++)
            {Assert.That(session.Production.TryAct(ProductionAction.Help,Grace.id),Is.True);Assert.That(baseline.TryAdvancePhase(),Is.True);}
            foreach(var t in session.State.production.tasks)
            {Assert.That(t.effort,Is.EqualTo(baseline.Production.Find(t.taskId).effort));Assert.That(session.Production.PlanFor(t.taskId).fraction,Is.EqualTo(baseline.Production.PlanFor(t.taskId).fraction));}
            Assert.That(session.State.characters.Select(c=>c.stress),Is.EqualTo(baseline.State.characters.Select(c=>c.stress)));
            Assert.That(session.State.FindCharacter(Grace.id).affection,Is.EqualTo(config.production.planning.sharedAffection));
            Assert.That(session.State.production.planning.shared.Count,Is.EqualTo(2));Assert.That(session.CanRestore(session.State),Is.True);
        }
        [Test] public void SharedTimeWorksWithoutReadyTaskAndNightOffDoesNotCreateWorkCallback()
        {
            Tiny();Night();Assert.That(session.Production.Active(Grace),Is.Null);
            Assert.That(session.Production.TryAct(ProductionAction.Help,Grace.id),Is.True);
            Assert.That(session.State.production.planning.shared.Single().nightWork,Is.False);
            Assert.That(session.Stats.Meets(new[]{new Condition{type=ConditionType.CharacterSharedNight,character=Grace}}),Is.False);
            Assert.That(session.CanRestore(session.State),Is.True);
        }
        [Test] public void ProducerRewardsOnlyOwnWorkAndMoneyAndDoesNotOpenFutureSprint()
        {
            var baseline=new GameSession(config);baseline.TryRestore(session.State.Clone());
            Assert.That(session.Production.TryAct(ProductionAction.Producer),Is.True);baseline.TryAdvancePhase();
            Assert.That(session.State.production.tasks.Select(t=>t.effort),Is.EqualTo(baseline.State.production.tasks.Select(t=>t.effort)));
            Assert.That(session.State.characters.Select(c=>c.stress),Is.EqualTo(baseline.State.characters.Select(c=>c.stress)));
            for(int i=1;i<4;i++){if(session.State.dayEnded)session.TryStartNextDay();Assert.That(session.Production.TryAct(ProductionAction.Producer),Is.True);}
            Assert.That(session.Production.ProducerStage,Is.Null);Assert.That(session.State.production.producerStages[0].completedDay,Is.GreaterThan(0));
            Assert.That(session.Production.StageUnlocked(config.production.sprints[1]),Is.False);
            while(session.State.day<8){if(session.State.dayEnded)session.TryStartNextDay();else session.TryAdvancePhase();}
            Assert.That(session.Production.StageUnlocked(config.production.sprints[1]),Is.True);Assert.That(session.CanRestore(session.State),Is.True);
        }
        [Test] public void UnfinishedProducerDoesNotFreezeCalendarReleaseAndCoordinationHasNoBuff()
        {
            Assert.That(session.Production.TryAct(ProductionAction.Coordinate),Is.False);
            while(session.State.day<8){if(session.State.dayEnded)session.TryStartNextDay();else session.TryAdvancePhase();}
            Assert.That(session.Production.StageUnlocked(config.production.sprints[1]),Is.True);
            Assert.That(session.State.production.producerStages[0].completedDay,Is.Zero);Assert.That(session.CanRestore(session.State),Is.True);
        }
        [Test] public void TighteningDueDoesNotReduceWorkOrCreateOutputAndIncreasesPressure()
        {
            Tiny();var baseline=new GameSession(config);baseline.TryRestore(session.State.Clone());var draft=session.Production.CreateDraft();
            Assert.That(session.Production.SetDraftDates(draft,"integration",0,0),Is.True);session.Production.TryApplyDraft(draft);
            session.TryAdvancePhase();baseline.TryAdvancePhase();
            Assert.That(session.State.production.tasks.Select(t=>t.effort),Is.EqualTo(baseline.State.production.tasks.Select(t=>t.effort)));
            Assert.That(session.State.FindCharacter(Grace.id).stress,Is.GreaterThan(baseline.State.FindCharacter(Grace.id).stress));
        }
        [Test] public void PlanningSaveRoundTripRetainsFractionSeedAndFutureOutcomeAndHasSeparateSlot()
        {
            session.TryAdvancePhase();var store=new SaveStore(directory);Assert.That(store.Save(session,SaveSlot.Manual),Is.EqualTo(SaveResult.Success));
            var fresh=new GameSession(config);Assert.That(store.Load(fresh,SaveSlot.Manual),Is.EqualTo(SaveResult.Success));
            Assert.That(JsonUtility.ToJson(fresh.State),Is.EqualTo(JsonUtility.ToJson(session.State)));
            session.TryAdvancePhase();fresh.TryAdvancePhase();Assert.That(JsonUtility.ToJson(fresh.State),Is.EqualTo(JsonUtility.ToJson(session.State)));
            var old=new GameSession(AssetDatabase.LoadAssetAtPath<GameConfigSO>(StudioPrototypeBuilder.ConfigPath));Assert.That(store.PathFor(old,SaveSlot.Manual),Is.Not.EqualTo(store.PathFor(session,SaveSlot.Manual)));
            var bad=session.State.Clone();bad.production.planning.tasks[0].fraction=float.NaN;Assert.That(session.CanRestore(bad),Is.False);
            bad=session.State.Clone();bad.production.planning.shared.Add(new SharedTimeState{characterId=Grace.id,cursor=session.Production.Cursor+1});Assert.That(session.CanRestore(bad),Is.False);
        }
        [Test] public void InsufficientEnergyIsAtomicAndDoesNotAdvanceRandomWork()
        {
            session.State.stamina=0;string before=JsonUtility.ToJson(session.State);
            Assert.That(session.Production.TryAct(ProductionAction.Help,Grace.id),Is.False);Assert.That(JsonUtility.ToJson(session.State),Is.EqualTo(before));
        }

        [Test] public void PlanningSharedOtPreviewAndCommitKeepActualParticipationWithoutHelperUnits()
        {
            Night();var work=session.Production;int money=session.State.money;
            Assert.That(work.TryOrder(Grace.id,StaffOrderKind.Overtime),Is.True);
            string before=JsonUtility.ToJson(session.State);var task=work.Active(Grace);float effort=work.ExactEffort(task.taskId);
            var choice=new DialogueChoice{choiceId="planning_join",workAction=DialogueWorkAction.WorkTogether,workCharacter=Grace};
            var preview=work.PreviewDialogueWork(choice);Assert.That(preview.allowed,Is.True);Assert.That(preview.help,Is.Zero);
            Assert.That(preview.moneyCost,Is.Zero);Assert.That(JsonUtility.ToJson(session.State),Is.EqualTo(before));
            Assert.That(work.TryAct(ProductionAction.Help,Grace.id),Is.True);Assert.That(session.State.dayEnded,Is.True);
            Assert.That(work.ExactEffort(task.taskId),Is.GreaterThan(effort));Assert.That(session.State.money,Is.EqualTo(money-config.production.overtimeMoneyCost));
            Assert.That(session.State.production.planning.nights.Count,Is.EqualTo(1));Assert.That(session.State.production.planning.shared.Single().nightWork,Is.True);
            Assert.That(session.State.production.sharedNights,Is.Empty);Assert.That(session.CanRestore(session.State),Is.True);
            int stress=session.State.FindCharacter(Grace.id).stress;Assert.That(session.TryStartNextDay(),Is.True);
            Assert.That(session.State.FindCharacter(Grace.id).stress,Is.EqualTo(stress));
            Assert.That(session.Stats.Meets(new[]{new Condition{type=ConditionType.CharacterSharedNight,character=Grace}}),Is.True);
            var store=new SaveStore(directory);Assert.That(store.Save(session,SaveSlot.Manual),Is.EqualTo(SaveResult.Success));
            Assert.That(store.Load(new GameSession(config),SaveSlot.Manual),Is.EqualTo(SaveResult.Success));
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void ActualCatalogThirtyDaysRetainsClockAndSafeStateWithSocialTimeAndRecovery(int policy)
        {
            config.events=AssetDatabase.LoadAssetAtPath<GameConfigSO>(PlanningPrototypeBuilder.ConfigPath).events;
            session=new GameSession(config);session.State.production.planning.seed="00000000000000000000000000000037";
            int guard=0,talked=0,planned=0;var rest=config.rooms.First().hotspots.First(h=>h.activity!=null&&h.activity.staminaRecovery>0).activity;
            void Finish()
            {
                while(session.Dialogue.IsActive)
                {
                    if(!session.Dialogue.ChoicesReady){Assert.That(session.Dialogue.TryAdvance(),Is.True);continue;}
                    var choices=session.Dialogue.Node.choices;
                    var choice=choices.FirstOrDefault(c=>c.workAction==DialogueWorkAction.None&&session.Dialogue.CanChoose(c))??choices.First(session.Dialogue.CanChoose);
                    Assert.That(session.Dialogue.TryChoose(choice),Is.True);
                }
            }
            while(!session.Days.CampaignEnded)
            {
                Assert.That(++guard,Is.LessThan(700));
                if(session.State.dayEnded){Assert.That(session.TryStartNextDay(),Is.True);continue;}
                bool forced=session.Events.TryBeginForced();var bubble=forced?null:session.Events.BubbleEvents().FirstOrDefault();
                if(forced||bubble!=null){if(!forced)Assert.That(session.Dialogue.TryBegin(bubble),Is.True);Finish();Assert.That(session.CanRestore(session.State),Is.True);continue;}
                if(policy>0&&talked!=session.State.day)
                {
                    foreach(var c in config.characters)
                    {if(!session.Interactions.TryOpen(session.CurrentRoom.hotspots.First(h=>h.character==c)))continue;if(session.Interactions.TryTalk())Finish();session.Interactions.TryClose();}
                    talked=session.State.day;
                }
                bool acted=false;var work=session.Production;
                if(policy==3&&planned!=session.State.day)
                {
                    var draft=work.CreateDraft();var forecasts=work.ProjectPlan(draft);
                    foreach(var f in forecasts.Where(f=>!f.unknown&&work.Find(f.taskId).acceptedDay==0&&work.IsReleased(work.Find(f.taskId))))
                    {var e=draft.tasks.Single(e=>e.taskId==f.taskId);work.SetDraftDates(draft,e.taskId,e.start,Math.Min(config.totalDays*4-2,Math.Max(e.due,f.latest+8)));}
                    Assert.That(work.TryApplyDraft(draft),Is.True);planned=session.State.day;
                }
                if(policy!=2||session.State.day>7)
                {
                    var decision=session.State.production.tasks.FirstOrDefault(t=>work.IsReleased(t)&&work.Definition(t).decisionBlocked&&!t.decisionCleared);
                    if(decision!=null)acted=work.TryAct(ProductionAction.Clarify,decision.taskId);
                    if(!acted&&work.ProducerStage!=null)acted=work.TryAct(ProductionAction.Producer);
                    if(!acted&&policy>0)acted=work.TryAct(ProductionAction.Help,config.characters[session.State.day%config.characters.Length].id);
                }
                if(!acted&&session.State.stamina<config.maxStamina)acted=session.TryCompleteActivity(rest,true);
                if(!acted)Assert.That(session.TryAdvancePhase(),Is.True);
                Assert.That(session.CanRestore(session.State),Is.True,"Unsafe state on day "+session.State.day);
            }
            Assert.That(session.State.production.resolvedPhases,Is.EqualTo(120));Assert.That(session.State.production.reviews.Count,Is.EqualTo(5));
            Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Session037_30Day_Policy"+policy+".json",JsonUtility.ToJson(session.State,true));
        }

        [Test] public void TimelineShiftIsPureAtomicAndCannotMoveReleaseOrStartedHistory()
        {
            var work=session.Production;var draft=work.CreateDraft();var future=config.production.tasks.First(t=>t.sprint==config.production.sprints[1]);
            var edit=draft.tasks.Single(e=>e.taskId==future.id);int start=edit.start,due=edit.due;string before=JsonUtility.ToJson(session.State);
            Assert.That(work.ShiftDraftDates(draft,future.id,1),Is.True);Assert.That(edit.start,Is.EqualTo(start+1));Assert.That(edit.due,Is.EqualTo(due+1));
            Assert.That(JsonUtility.ToJson(session.State),Is.EqualTo(before));Assert.That(work.SetDraftDates(draft,future.id,0,edit.due),Is.False);
            work.ProjectPlan(draft);Assert.That(JsonUtility.ToJson(session.State),Is.EqualTo(before));Assert.That(work.TryApplyDraft(draft),Is.True);
            Assert.That(session.CanRestore(session.State),Is.True);session.TryAdvancePhase();draft=work.CreateDraft();
            var started=session.State.production.tasks.First(t=>work.PlanFor(t.taskId).started>=0&&t.acceptedDay==0);
            Assert.That(work.ShiftDraftDates(draft,started.taskId,1),Is.False);
            edit=draft.tasks.Single(e=>e.taskId==started.taskId);Assert.That(work.SetDraftDates(draft,edit.taskId,edit.start,edit.due+1),Is.True);
            Assert.That(work.TryApplyDraft(draft),Is.True);Assert.That(session.CanRestore(session.State),Is.True);
        }
        [Test] public void TimelineForecastShowsNextPhaseDependencyStartAndProductiveBufferWithoutChangingState()
        {
            Tiny();var work=session.Production;var draft=work.CreateDraft();string before=JsonUtility.ToJson(session.State);
            Assert.That(work.SetDraftDates(draft,"integration",0,6),Is.True);var forecast=work.ProjectPlan(draft).Single(f=>f.taskId=="integration");
            Assert.That(forecast.projectedStart,Is.EqualTo(2));Assert.That(forecast.expected,Is.EqualTo(2));Assert.That(forecast.bufferSlots,Is.EqualTo(3));
            Assert.That(work.SetDraftDates(draft,"integration",0,1),Is.True);forecast=work.ProjectPlan(draft).Single(f=>f.taskId=="integration");
            Assert.That(forecast.atRisk,Is.True);Assert.That(forecast.bufferSlots,Is.EqualTo(-1));Assert.That(JsonUtility.ToJson(session.State),Is.EqualTo(before));
        }
        [Test] public void TimelineCannotMoveCompletedWorkOrStartBeforeItsAuthoredRelease()
        {
            Tiny();config.production.tasks.Single(t=>t.id=="integration").releaseDay=3;session=new GameSession(config);
            var work=session.Production;var draft=work.CreateDraft();Assert.That(work.SetDraftDates(draft,"integration",0,20),Is.False);
            session.TryAdvancePhase();draft=work.CreateDraft();var edit=draft.tasks.Single(e=>e.taskId=="brief");
            Assert.That(work.SetDraftDates(draft,"brief",edit.start,edit.due-1),Is.False);
            Assert.That(work.ShiftDraftDates(draft,"brief",1),Is.False);Assert.That(session.CanRestore(session.State),Is.True);
        }
    }
}
