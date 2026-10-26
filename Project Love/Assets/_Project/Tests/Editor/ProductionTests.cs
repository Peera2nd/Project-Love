using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.Simulation;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.Tests
{
    public sealed class ProductionTests
    {
        private readonly List<ScriptableObject> objects = new List<ScriptableObject>();
        private GameConfigSO config;
        private ProductionConfigSO production;
        private GameSession session;
        private CharacterDataSO worker;
        private WorkTaskSO first, second, bug;
        private string directory;
        private T Make<T>() where T : IdentifiedDataSO
        { var o = ScriptableObject.CreateInstance<T>(); o.id = Guid.NewGuid().ToString("N"); objects.Add(o); return o; }
        [SetUp] public void Setup()
        {
            config = Make<GameConfigSO>(); config.totalDays = 14; config.maxActionPointsPerDay = 4; config.usesStamina = true;
            production = Make<ProductionConfigSO>(); production.presentation = Make<RoomPresentationSO>();
            worker = Make<CharacterDataSO>(); worker.startingStress = 30;
            var stream = Make<WorkStreamSO>(); config.workStreams = new[] { stream }; config.characters = new[] { worker };
            first = Make<WorkTaskSO>(); first.owner = worker; first.stream = stream; first.effort = 20; first.mainWeight = 20; first.releaseDay = 1;
            second = Make<WorkTaskSO>(); second.owner = worker; second.stream = stream; second.effort = 20; second.mainWeight = 20; second.releaseDay = 1;
            bug = Make<WorkTaskSO>(); bug.owner = worker; bug.stream = stream; bug.effort = 24; bug.releaseDay = 1;
            production.workers = new[] { new WorkerDefinition { character = worker, stream = stream, baseRate = 4 } };
            production.tasks = new[] { first, second, bug };
            production.productivePhases = new[] { DayPhase.Morning, DayPhase.Daytime, DayPhase.Evening };
            production.stressBands = new[] { new StressBand { minimum = 0, outputPercent = 100 }, new StressBand { minimum = 40, outputPercent = 75 },
                new StressBand { minimum = 70, outputPercent = 50 }, new StressBand { minimum = 90, outputPercent = 25 } };
            production.workStress = 2; production.overnightStressRecovery = 4; production.helpCost = 20; production.helpEffort = 6; production.helpStressRecovery = 3;
            production.coordinateCost = 15; production.coordinateTicks = 2; production.coordinateBonus = 1;
            production.clarifyCost = 15; production.careCost = 10; production.careStressRecovery = 15;
            production.restStressRecovery = 5; production.overtimeMoneyCost = 5; production.overtimeStress = 10;
            production.producerCost = 20; production.producerEffort = 10; production.producerMoney = 10; production.producerTeamBonus = 1;
            var a = Make<SprintDataSO>(); a.startDay = 1; a.endDay = 7; a.quotas = new[] { new StreamQuota { stream = stream, effort = 40 } };
            var b = Make<SprintDataSO>(); b.startDay = 8; b.endDay = 14; b.quotas = a.quotas;
            production.sprints = new[] { a, b };
            production.events = new[] { new WorkloadEvent { id = "bug", earliestDay = 1, latestDay = 7, tasks = new[] { bug },
                extraQuotas = new[] { new StreamQuota { stream = stream, effort = 24 } } } };
            config.production = production;
            session = new GameSession(config);
            directory = Path.GetFullPath(Path.Combine("Logs", "ProductionSaves", Guid.NewGuid().ToString("N")));
        }
        [TearDown] public void Teardown()
        { foreach (var o in objects) UnityEngine.Object.DestroyImmediate(o); objects.Clear(); if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        private TaskState State(WorkTaskSO task) => session.Production.Find(task.id);
        private void ThroughDay(int day)
        { while (session.State.day < day) { Assert.That(session.TryEndDay(), Is.True); Assert.That(session.TryStartNextDay(), Is.True); } }
        [TestCase(0, 4)] [TestCase(39, 4)] [TestCase(40, 3)] [TestCase(69, 3)] [TestCase(70, 2)] [TestCase(89, 2)] [TestCase(90, 1)] [TestCase(100, 1)]
        public void StressRateUsesPreProductionBand(int stress, int rate)
        { session.State.FindCharacter(worker.id).stress = stress; session.TryAdvancePhase(); Assert.That(State(first).effort, Is.EqualTo(rate)); Assert.That(session.State.FindCharacter(worker.id).stress, Is.EqualTo(Math.Min(100, stress + 2))); }
        [Test] public void HelpPreviewMatchesCommitAndDoesNotMutateSource()
        {
            State(first).effort = 8; session.State.FindCharacter(worker.id).stress = 50;
            string before = JsonUtility.ToJson(session.State), source = JsonUtility.ToJson(production);
            var preview = session.Production.Preview(ProductionAction.Help, worker.id);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before)); Assert.That(preview.allowed, Is.True);
            Assert.That(preview.workers[0].help, Is.EqualTo(6)); Assert.That(preview.workers[0].team, Is.EqualTo(3));
            Assert.That(preview.workers[0].after, Is.EqualTo(17)); Assert.That(preview.workers[0].stressAfter, Is.EqualTo(49));
            Assert.That(session.Production.TryAct(ProductionAction.Help, worker.id), Is.True);
            Assert.That(State(first).effort, Is.EqualTo(17)); Assert.That(session.State.stamina, Is.EqualTo(80));
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Daytime)); Assert.That(session.State.mainProgress, Is.Zero);
            Assert.That(session.State.FindCharacter(worker.id).affection, Is.Zero);
            Assert.That(JsonUtility.ToJson(production), Is.EqualTo(source)); Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void HelpCapsAtCurrentCardAndNeverSpills()
        {
            State(first).effort = 18;
            var preview = session.Production.Preview(ProductionAction.Help, worker.id);
            Assert.That(preview.workers[0].help, Is.EqualTo(2)); Assert.That(preview.workers[0].team, Is.Zero);
            session.Production.TryAct(ProductionAction.Help, worker.id);
            Assert.That(State(first).effort, Is.EqualTo(20)); Assert.That(State(second).effort, Is.Zero);
            Assert.That(session.State.FindCharacter(worker.id).stress, Is.EqualTo(27));
            session.TryAdvancePhase(); Assert.That(State(second).effort, Is.EqualTo(4));
        }
        [Test] public void NightHasNoWorkAndEarlyEndResolvesExactlyThreeProductiveTicks()
        {
            Assert.That(session.TryEndDay(), Is.True); Assert.That(State(first).effort, Is.EqualTo(12));
            Assert.That(session.State.production.resolvedPhases, Is.EqualTo(4)); Assert.That(session.TryEndDay(), Is.False);
            Assert.That(session.TryStartNextDay(), Is.True); Assert.That(session.State.FindCharacter(worker.id).stress, Is.EqualTo(32));
            Assert.That(session.State.stamina, Is.EqualTo(100)); Assert.That(session.State.production.resolvedPhases, Is.EqualTo(4));
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void CoordinateCannotStackAndNightPreservesItsProductiveCharges()
        {
            session.TryAdvancePhase(); session.TryAdvancePhase(); session.TryAdvancePhase();
            int before = State(first).effort;
            Assert.That(session.Production.TryAct(ProductionAction.Coordinate), Is.True);
            Assert.That(State(first).effort, Is.EqualTo(before)); Assert.That(session.State.production.coordinationTicks, Is.EqualTo(2));
            session.TryStartNextDay(); string json = JsonUtility.ToJson(session.State);
            Assert.That(session.Production.TryAct(ProductionAction.Coordinate), Is.False); Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(json));
            session.TryAdvancePhase(); Assert.That(session.State.production.coordinationTicks, Is.EqualTo(1));
            session.TryAdvancePhase(); Assert.That(session.State.production.coordinationTicks, Is.Zero);
        }
        [Test] public void RestOrderIsFreeAndPausesOnlyTargetWhenTimePasses()
        {
            var other = Make<CharacterDataSO>(); other.startingStress = 30;
            config.characters = new[] { worker, other };
            production.workers = production.workers.Concat(new[] { new WorkerDefinition { character = other, stream = first.stream, baseRate = 4 } }).ToArray();
            second.owner = other; session = new GameSession(config);
            Assert.That(session.Production.TryOrder(worker.id, StaffOrderKind.Rest), Is.True);
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Morning)); Assert.That(session.State.FindCharacter(worker.id).stress, Is.EqualTo(30));
            Assert.That(session.Production.TryOrder(worker.id, StaffOrderKind.Rest), Is.False);
            Assert.That(session.TryAdvancePhase(), Is.True);
            Assert.That(State(first).effort, Is.Zero); Assert.That(State(second).effort, Is.EqualTo(4));
            Assert.That(session.State.FindCharacter(worker.id).stress, Is.EqualTo(25)); Assert.That(session.State.stamina, Is.EqualTo(100));
        }
        [Test] public void ClarifyRemovesDecisionButDoesNotBypassPrerequisite()
        {
            first.decisionBlocked = true; first.prerequisites = new[] { second }; session = new GameSession(config);
            Assert.That(session.Production.TryAct(ProductionAction.Help, worker.id), Is.False);
            Assert.That(session.Production.TryAct(ProductionAction.Clarify, first.id), Is.True);
            Assert.That(State(first).decisionCleared, Is.True); Assert.That(State(first).effort, Is.Zero);
            Assert.That(session.State.FindCharacter(worker.id).stress, Is.EqualTo(30));
            Assert.That(session.Production.TryPrioritize(second.id), Is.True);
            Assert.That(session.Production.TryAct(ProductionAction.Help, worker.id), Is.True);
        }
        [Test] public void ReviewCreditsOnceAndReorderIsFree()
        {
            Assert.That(session.Production.TryPrioritize(second.id), Is.True); Assert.That(session.State.phase, Is.EqualTo(DayPhase.Morning));
            State(second).effort = 20; Assert.That(session.Production.TryReview(), Is.True);
            Assert.That(session.State.mainProgress, Is.EqualTo(50)); Assert.That(session.Production.Completed(first.stream), Is.EqualTo(20));
            string before = JsonUtility.ToJson(session.State); Assert.That(session.Production.TryReview(), Is.True);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before)); Assert.That(session.State.stamina, Is.EqualTo(100));
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void MissedReviewKeepsEffortAndLaterAcceptanceDoesNotRewriteOldSprint()
        {
            first.effort = 400; first.mainWeight = 400; second.releaseDay = 8; session = new GameSession(config);
            ThroughDay(8); var review = session.State.production.reviews.Single();
            Assert.That(review.quotas[0].completed, Is.Zero); Assert.That(review.carryoverCount, Is.EqualTo(1));
            int partial = State(first).effort; Assert.That(partial, Is.GreaterThan(0));
            State(first).effort = 400; session.Production.TryReview();
            Assert.That(review.quotas[0].completed, Is.Zero); Assert.That(session.Production.Completed(first.stream), Is.EqualTo(400));
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void WorkloadAddsOnlyCodeQuotaOnceAndDoesNotEraseAcceptedScope()
        {
            State(first).effort = 20; session.Production.TryReview(); int progress = session.State.mainProgress;
            Assert.That(session.Production.TryActivate(production.events[0]), Is.True); Assert.That(session.Production.Required(first.stream), Is.EqualTo(64));
            Assert.That(session.Production.TryActivate(production.events[0]), Is.False);
            Assert.That(session.State.mainProgress, Is.EqualTo(progress)); Assert.That(State(first).acceptedDay, Is.EqualTo(1));
            Assert.That(session.Production.IsReleased(State(bug)), Is.True); Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void GenericAbsenceStopsOwnHelpAndExpiresWithoutStoryTrigger()
        {
            production.events[0].absentWorker = worker; production.events[0].absenceDays = 2;
            session = new GameSession(config); session.Production.TryActivate(production.events[0]);
            Assert.That(session.Production.TryAct(ProductionAction.Help, worker.id), Is.False);
            ThroughDay(3); Assert.That(State(first).effort, Is.Zero); Assert.That(session.State.FindCharacter(worker.id).stress, Is.EqualTo(22));
            Assert.That(session.Production.TryAct(ProductionAction.Help, worker.id), Is.True); Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void RejectedActionsAreAtomicAndInvalidStateCannotRestore()
        {
            session.State.stamina = 0; string before = JsonUtility.ToJson(session.State);
            foreach (ProductionAction action in Enum.GetValues(typeof(ProductionAction)))
            { Assert.That(session.Production.TryAct(action, worker.id), Is.False); Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before)); }
            var copy = session.State.Clone(); copy.production.resolvedPhases++; Assert.That(session.TryRestore(copy), Is.False);
            copy = session.State.Clone(); copy.production.tasks[0].effort = 500; Assert.That(session.TryRestore(copy), Is.False);
            copy = session.State.Clone(); copy.production.tasks[1].taskId = "unknown"; Assert.That(session.TryRestore(copy), Is.False);
            copy = session.State.Clone(); copy.production.events.Add(null); Assert.That(session.TryRestore(copy), Is.False);
        }
        [Test] public void SaveV5RoundTripsTasksPlansEventsReviewsAndRejectsChangedCatalogOrLegacy()
        {
            session.Production.TryActivate(production.events[0]); ThroughDay(8);
            session.Production.TryAct(ProductionAction.Coordinate);
            var saves = new SaveStore(directory); Assert.That(saves.Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            string before = JsonUtility.ToJson(session.State); var fresh = new GameSession(config);
            Assert.That(saves.Load(fresh, SaveSlot.Manual), Is.EqualTo(SaveResult.Success)); Assert.That(JsonUtility.ToJson(fresh.State), Is.EqualTo(before));
            production.helpEffort++; fresh = new GameSession(config);
            Assert.That(saves.Load(fresh, SaveSlot.Manual), Is.EqualTo(SaveResult.Invalid)); production.helpEffort--;
            string path = saves.PathFor(session, SaveSlot.Manual), saved = File.ReadAllText(path);
            File.WriteAllText(path, saved.Replace("\"version\": 5", "\"version\": 4"));
            Assert.That(saves.Load(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Incompatible));
        }
        [Test] public void DialogueDeferralRestoresEntireProductionSnapshotAndCompletionUsesSameClock()
        {
            var node = Make<DialogueNodeSO>(); node.lines = new[] { new DialogueLine { lineId = "story", text = "test" } };
            var ev = Make<EventDataSO>(); ev.startDay = 1; ev.endDay = 14; ev.anyPhase = true; ev.entryNode = node; ev.elapsedPhases = 1; ev.staminaCost = 5;
            string before = JsonUtility.ToJson(session.State); Assert.That(session.Dialogue.TryBegin(ev), Is.True);
            Assert.That(session.Production.TryAct(ProductionAction.Help, worker.id), Is.False); Assert.That(session.TryEndDay(), Is.False);
            Assert.That(session.Dialogue.TryDefer(), Is.True); Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
            session.Dialogue.TryBegin(ev); session.Dialogue.TryAdvance(); Assert.That(State(first).effort, Is.EqualTo(4));
            Assert.That(session.State.production.resolvedPhases, Is.EqualTo(1)); Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void RejectsCyclesAndSprintGaps()
        {
            first.prerequisites = new[] { second }; second.prerequisites = new[] { first };
            Assert.Throws<ArgumentException>(() => new GameSession(config)); second.prerequisites = new WorkTaskSO[0];
            production.sprints[1].startDay = 9; Assert.Throws<ArgumentException>(() => new GameSession(config));
        }
        [Test] public void LegacyCloneAndVersionThreeLoadKeepProductionSchemaAbsent()
        {
            config.production = null; var legacy = new GameSession(config);
            var inline = JsonUtility.FromJson<RuntimeGameState>(JsonUtility.ToJson(legacy.State));
            Assert.That(inline.production, Is.Not.Null, "Regression fixture records Unity inline-null behavior");
            Assert.That(legacy.State.Clone().production, Is.Null);
            var saves = new SaveStore(directory); Assert.That(saves.Save(legacy, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            Assert.That(saves.Load(legacy, SaveSlot.Manual), Is.EqualTo(SaveResult.Success)); Assert.That(legacy.State.production, Is.Null);
        }
        [TestCase(false, false)] [TestCase(true, false)] [TestCase(false, true)] [TestCase(true, true)]
        public void DeliveredThirtyDayCampaignHasNoSoftlockAndCanReachAllBaseScope(bool bugEvent, bool dailyTalk)
        {
            var delivered = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameConfigSO>(ProjectLove.EditorTools.StudioPrototypeBuilder.ConfigPath));
            objects.Add(delivered); delivered.events = Array.Empty<EventDataSO>(); // Preserve the four baseline capacity policies; authored events have separate coverage.
            Assert.That(delivered, Is.Not.Null); var run = new GameSession(delivered); string definitions = JsonUtility.ToJson(delivered.production);
            int phases = 0;
            int talkedDay = 0;
            var rest = delivered.rooms.First().hotspots.First(h => h.activity != null && h.activity.staminaRecovery > 0).activity;
            if (bugEvent) Assert.That(run.Production.TryActivate(delivered.production.events[0]), Is.True);
            while (!run.Days.CampaignEnded)
            {
                if (run.State.dayEnded) { Assert.That(run.TryStartNextDay(), Is.True); continue; }
                if (dailyTalk && talkedDay != run.State.day)
                {
                    foreach (var character in delivered.characters)
                    {
                        Assert.That(run.Interactions.TryOpen(run.CurrentRoom.hotspots.First(h => h.character == character)), Is.True);
                        Assert.That(run.Interactions.TryTalk(), Is.True, "First-daily talk must remain affordable");
                        Assert.That(run.Dialogue.TryAdvance(), Is.True); Assert.That(run.Interactions.TryClose(), Is.True);
                    }
                    talkedDay = run.State.day;
                }
                run.Production.TryReview();
                var decision = run.State.production.tasks.FirstOrDefault(t => run.Production.IsReleased(t) && run.Production.Definition(t).decisionBlocked && !t.decisionCleared);
                bool acted = false;
                if (decision != null) acted = run.Production.TryAct(ProductionAction.Clarify, decision.taskId);
                var producerStage = run.Production.ProducerStage;
                bool needsProducer = producerStage != null && (producerStage.startDay <= run.State.day + 3 ||
                    delivered.production.workers.Any(w => run.Production.Active(w.character) == null));
                if (!acted && needsProducer) acted = run.Production.TryAct(ProductionAction.Producer);
                if (!acted)
                {
                    var target = delivered.production.workers.OrderByDescending(w => run.Production.Required(w.stream) - run.Production.Completed(w.stream))
                        .FirstOrDefault(w => run.Production.Preview(ProductionAction.Help, w.character.id).allowed);
                    if (target != null) acted = run.Production.TryAct(ProductionAction.Help, target.character.id);
                }
                if (!acted && run.State.phase == DayPhase.Night && run.State.stamina < delivered.maxStamina)
                    acted = run.TryCompleteActivity(rest, true);
                if (!acted) Assert.That(run.TryAdvancePhase(), Is.True);
                phases++; Assert.That(phases, Is.LessThanOrEqualTo(delivered.totalDays * 4));
                Assert.That(run.CanRestore(run.State), Is.True, "Invalid simulation state on day " + run.State.day);
            }
            Assert.That(run.State.production.resolvedPhases, Is.EqualTo(120)); Assert.That(run.State.production.reviews.Count, Is.EqualTo(5));
            Assert.That(run.State.mainProgress, Is.EqualTo(100), "Trial policy must demonstrate a feasible full-scope route");
            if (dailyTalk) Assert.That(run.State.characters.All(c => c.affection == 60 && c.lastTalkDay == delivered.totalDays), Is.True);
            Assert.That(JsonUtility.ToJson(delivered.production), Is.EqualTo(definitions));
            Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/Session027_30Day_Bug" + bugEvent + "_Talk" + dailyTalk + ".json", JsonUtility.ToJson(run.State, true));
        }
    }
}
