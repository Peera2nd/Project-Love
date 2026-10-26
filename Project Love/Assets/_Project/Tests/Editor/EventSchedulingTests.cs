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
    public sealed class EventSchedulingTests
    {
        private readonly List<ScriptableObject> objects = new List<ScriptableObject>();
        private readonly List<EventDataSO> events = new List<EventDataSO>();
        private GameConfigSO config;
        private CharacterDataSO first, second;
        private RoomDataSO room, other;
        private GameSession session;
        private string directory;
        private T Make<T>(string id) where T : IdentifiedDataSO
        { var x = ScriptableObject.CreateInstance<T>(); x.id = x.name = id; objects.Add(x); return x; }
        [SetUp] public void Setup()
        {
            config = Make<GameConfigSO>("event_test"); config.totalDays = 7; config.usesStamina = true;
            first = Make<CharacterDataSO>("first"); second = Make<CharacterDataSO>("second"); config.characters = new[] { first, second };
            room = Make<RoomDataSO>("room"); other = Make<RoomDataSO>("other");
            room.hotspots = new[] { first, second }.Select(c => new RoomHotspot { id = c.id, character = c, kind = RoomInteractionKind.Character,
                placements = Enumerable.Range(0, 4).Select(i => new RoomPlacement { phase = (DayPhase)i }).ToArray() }).ToArray();
            config.rooms = new[] { room, other }; config.startingRoomId = room.id;
            directory = Path.GetFullPath(Path.Combine("Logs", "EventSaves", Guid.NewGuid().ToString("N")));
        }
        [TearDown] public void Teardown()
        { foreach (var o in objects) UnityEngine.Object.DestroyImmediate(o); objects.Clear(); events.Clear(); if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        private EventDataSO Event(string id, EventDelivery delivery, int priority = 0, CharacterDataSO character = null)
        {
            var n = Make<DialogueNodeSO>(id + "_node"); n.lines = new[] { new DialogueLine { lineId = id + "_line", text = "บททดสอบ", speaker = character } };
            var e = Make<EventDataSO>(id); e.displayName = id; e.entryNode = n; e.endDay = 7; e.anyPhase = true; e.delivery = delivery;
            e.priority = priority; e.character = character; events.Add(e); return e;
        }
        private void Start() { config.events = events.ToArray(); session = new GameSession(config); }
        private void Finish() { int guard = 0; while (session.Dialogue.IsActive) { Assert.That(session.Dialogue.TryAdvance(), Is.True); Assert.That(++guard, Is.LessThan(30)); } }
        [Test] public void ForcedOrderUsesPriorityDeadlinePreferredDateAndStableIdRegardlessOfCatalogOrder()
        {
            var last = Event("z", EventDelivery.Forced, 10); last.endDay = 5;
            var earlier = Event("b", EventDelivery.Forced, 10); earlier.endDay = 4;
            var tie = Event("a", EventDelivery.Forced, 10); tie.endDay = 4;
            var high = Event("high", EventDelivery.Forced, 11); Start();
            Assert.That(session.Events.NextForced, Is.EqualTo(high));
            session.Events.TryBeginForced(); Assert.That(session.Events.TryBeginForced(), Is.False); Finish();
            Assert.That(session.Events.NextForced, Is.EqualTo(tie)); session.Events.TryBeginForced(); Finish();
            Assert.That(session.Events.NextForced, Is.EqualTo(earlier)); session.Events.TryBeginForced(); Finish();
            Assert.That(session.Events.NextForced, Is.EqualTo(last));
        }
        [Test] public void ForcedPendingBlocksOrdinaryActionsAndManualEventsUntilResolved()
        {
            var bubble = Event("bubble", EventDelivery.CharacterBubble, 1000, first);
            var forced = Event("forced", EventDelivery.Forced, 0); Start();
            Assert.That(session.IsBusy, Is.True); Assert.That(session.TryEndDay(), Is.False);
            Assert.That(session.TryTravel(other), Is.False); Assert.That(session.Dialogue.TryBegin(bubble), Is.False);
            Assert.That(session.Events.BubbleEvents(), Is.Empty); Assert.That(new SaveStore(directory).Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Busy));
            Assert.That(session.Events.TryBeginForced(), Is.True); Assert.That(session.Dialogue.ActiveEvent, Is.EqualTo(forced)); Finish();
            Assert.That(session.Events.BubbleEvents(), Is.EqualTo(new[] { bubble }));
        }
        [Test] public void OneBubblePerCharacterKeepsOtherCharactersAvailableAndRechecksAfterCompletion()
        {
            var low = Event("low", EventDelivery.CharacterBubble, 1, first);
            var high = Event("high", EventDelivery.CharacterBubble, 2, first);
            var parallel = Event("parallel", EventDelivery.CharacterBubble, 0, second); Start();
            Assert.That(session.Events.BubbleEvents(), Is.EqualTo(new[] { high, parallel }));
            Assert.That(session.Dialogue.TryBegin(low), Is.False); session.Dialogue.TryBegin(high);
            Assert.That(session.Dialogue.TryBegin(parallel), Is.False); Finish();
            Assert.That(session.Events.BubbleEvents(), Is.EqualTo(new[] { low, parallel }));
        }
        [Test] public void ExclusiveGroupOnlySuppressesAlternativesAfterCompletionAndPersists()
        {
            var a = Event("a", EventDelivery.CharacterBubble, 2, first); var b = Event("b", EventDelivery.CharacterBubble, 1, second);
            a.exclusiveGroup = b.exclusiveGroup = "route_choice"; Start();
            session.Dialogue.TryBegin(a); session.Dialogue.TryDefer(); Assert.That(session.Events.BubbleEvents().Length, Is.EqualTo(2));
            session.Dialogue.TryBegin(b); Finish(); Assert.That(session.Events.BubbleEvents(), Is.Empty);
            var copy = session.State.Clone(); copy.completedEvents.Add(a.id); Assert.That(session.CanRestore(copy), Is.False);
            var saves = new SaveStore(directory); Assert.That(saves.Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            var loaded = new GameSession(config); Assert.That(saves.Load(loaded, SaveSlot.Manual), Is.EqualTo(SaveResult.Success)); Assert.That(loaded.Events.BubbleEvents(), Is.Empty);
        }
        [Test] public void DeferringForcedRollsBackAndSnoozesThroughSaveLoadThenRearmsNextPhase()
        {
            var e = Event("forced", EventDelivery.Forced, 0, first); Start(); var before = session.State.Clone();
            session.Events.TryBeginForced(); session.State.money += 99; session.Dialogue.TryDefer();
            Assert.That(session.State.money, Is.EqualTo(before.money)); Assert.That(session.Events.HasForcedPending, Is.False);
            Assert.That(session.Events.BubbleEvents(), Is.EqualTo(new[] { e }));
            var saves = new SaveStore(directory); Assert.That(saves.Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            var loaded = new GameSession(config); Assert.That(saves.Load(loaded, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            Assert.That(loaded.Events.HasForcedPending, Is.False); Assert.That(loaded.TryAdvancePhase(), Is.True); Assert.That(loaded.Events.NextForced, Is.EqualTo(e));
        }
        [Test] public void DeferredGlobalForcedHasManualRetryWithoutCharacter()
        {
            var e = Event("global", EventDelivery.Forced); Start(); session.Events.TryBeginForced(); session.Dialogue.TryDefer();
            Assert.That(session.Events.DeferredGlobalEvents(), Is.EqualTo(new[] { e })); Assert.That(session.Dialogue.TryBegin(e), Is.True); Finish();
            Assert.That(session.Events.DeferredGlobalEvents(), Is.Empty);
        }
        [Test] public void ModalIsNeverInterruptedAndEligibilityIsRecheckedAfterItCloses()
        {
            var forced = Event("forced", EventDelivery.Forced); forced.conditions = new[] { new Condition { type = ConditionType.FlagSet, flagId = "ready" } };
            var manual = Event("manual", EventDelivery.Manual); Start(); session.Dialogue.TryBegin(manual);
            session.State.flags.Add("ready"); Assert.That(session.Events.TryBeginForced(), Is.False); Finish();
            Assert.That(session.Events.NextForced, Is.EqualTo(forced)); session.State.flags.Clear(); Assert.That(session.Events.NextForced, Is.Null);
        }
        [Test] public void WrongRoomInvisibleCharacterAndExpiredWindowDoNotLeakBubbles()
        {
            var e = Event("bubble", EventDelivery.CharacterBubble, 0, first); e.room = room; e.endDay = 2; Start();
            Assert.That(session.Events.BubbleEvents().Length, Is.EqualTo(1)); session.TryTravel(other); Assert.That(session.Events.BubbleEvents(), Is.Empty);
            session.TryTravel(room); room.hotspots[0].placements[0].visible = false; Assert.That(session.Events.BubbleEvents(), Is.Empty);
            session.TryEndDay(); session.TryStartNextDay(); session.TryEndDay(); session.TryStartNextDay(); Assert.That(session.Events.BubbleEvents(), Is.Empty);
        }
        [Test] public void EndDayStopsAtForcedPhaseAndDoesNotConsumeItsTimeOrSkipIt()
        {
            var e = Event("night", EventDelivery.Forced); e.anyPhase = false; e.triggerPhase = DayPhase.Night; Start();
            Assert.That(session.TryEndDay(), Is.True); Assert.That(session.State.phase, Is.EqualTo(DayPhase.Night)); Assert.That(session.State.dayEnded, Is.False);
            session.Events.TryBeginForced(); Finish(); Assert.That(session.TryEndDay(), Is.True); Assert.That(session.State.dayEnded, Is.True);
        }
        [Test] public void CompletionRechecksDependentEventsAndDoesNotRepeatOrRewardTwice()
        {
            var a = Event("a", EventDelivery.Forced); var b = Event("b", EventDelivery.Forced);
            b.conditions = new[] { new Condition { type = ConditionType.EventCompleted, eventId = a.id } }; Start();
            session.Events.TryBeginForced(); Finish(); Assert.That(session.Events.NextForced, Is.EqualTo(b));
            session.Events.TryBeginForced(); Finish(); Assert.That(session.Events.TryBeginForced(), Is.False);
            Assert.That(session.State.completedEvents, Is.EqualTo(new[] { a.id, b.id }));
        }
        [Test] public void DailyBubbleReopensNextDayWithoutLosingItsDailyGate()
        {
            var e = Event("daily", EventDelivery.CharacterBubble, 0, first); e.repeatDaily = true; Start();
            session.Dialogue.TryBegin(e); Finish(); Assert.That(session.Events.BubbleEvents(), Is.Empty);
            session.TryEndDay(); session.TryStartNextDay(); Assert.That(session.Events.BubbleEvents(), Is.EqualTo(new[] { e }));
        }
        [Test] public void SaveCatalogChangeAndInvalidDeferralAreRejectedWithoutMutatingLiveState()
        {
            Event("forced", EventDelivery.Forced); Start(); session.Events.TryBeginForced(); session.Dialogue.TryDefer();
            var saves = new SaveStore(directory); saves.Save(session, SaveSlot.Manual);
            var invalid = session.State.Clone(); invalid.eventSchedule.deferred[0].day = 8; Assert.That(session.TryRestore(invalid), Is.False);
            events[0].priority++; var changed = new GameSession(config); string before = JsonUtility.ToJson(changed.State);
            Assert.That(saves.Load(changed, SaveSlot.Manual), Is.EqualTo(SaveResult.Invalid)); Assert.That(JsonUtility.ToJson(changed.State), Is.EqualTo(before));
        }
        [Test] public void InvalidDuplicateIdsMissingBubbleTargetAndDailyExclusiveGroupAreReported()
        {
            var a = Event("same", EventDelivery.CharacterBubble); var b = Event("same", EventDelivery.Forced); b.repeatDaily = true; b.exclusiveGroup = "bad";
            config.events = events.ToArray(); Assert.That(EventScheduler.ValidateCatalog(config).Count, Is.GreaterThanOrEqualTo(2)); Assert.Throws<ArgumentException>(() => new GameSession(config));
        }
        [Test] public void DialogueCycleIsRejectedAtAuthoringBoundary()
        {
            var e = Event("cycle", EventDelivery.Forced); e.entryNode.nextNode = e.entryNode;
            config.events = events.ToArray(); Assert.That(EventScheduler.ValidateCatalog(config).Any(i => i.Contains("cycle")), Is.True);
        }
        [Test] public void DeliveredBugChoiceAddsWorkloadAndRollbackRestoresWholeSnapshot()
        {
            var delivered = FoundationStudio(); session = new GameSession(delivered);
            session.TryEndDay(); session.TryStartNextDay(); Assert.That(session.Events.TryBeginForced(), Is.True); Finish();
            var bug = delivered.events.Single(e => e.id == "studio_event_bug"); string before = JsonUtility.ToJson(session.State);
            Assert.That(session.Dialogue.TryBegin(bug), Is.True); session.Dialogue.TryAdvance();
            Assert.That(session.Dialogue.TryChoose(bug.entryNode.choices[0]), Is.True);
            Assert.That(session.State.production.events.Single().eventId, Is.EqualTo("studio_bug_demo"));
            Assert.That(session.Production.Required(delivered.workStreams.Single(w => w.id == "work_code")), Is.EqualTo(104));
            Assert.That(session.Dialogue.TryDefer(), Is.True); Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
            session.Dialogue.TryBegin(bug); session.Dialogue.TryAdvance(); session.Dialogue.TryChoose(bug.entryNode.choices[0]); Finish();
            Assert.That(session.State.production.events.Count, Is.EqualTo(1)); Assert.That(session.Dialogue.TryBegin(bug), Is.False);
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void CircularEventRequirementsAndMissingWorkloadConditionsAreRejected()
        {
            var a = Event("a", EventDelivery.Forced); var b = Event("b", EventDelivery.Forced);
            a.conditions = new[] { new Condition { type = ConditionType.EventCompleted, eventId = b.id } };
            b.conditions = new[] { new Condition { type = ConditionType.EventCompleted, eventId = a.id } };
            config.events = events.ToArray(); Assert.That(EventScheduler.ValidateCatalog(config).Any(i => i.Contains("circular")), Is.True);
            a.conditions = new[] { new Condition { type = ConditionType.WorkloadNotActivated, targetId = "unknown" } };
            Assert.That(EventScheduler.ValidateCatalog(config).Any(i => i.Contains("invalid schedule")), Is.True);
        }
        [Test] public void DailyExclusiveGroupCannotBeAuthoredAsPermanentRouteLock()
        {
            var e = Event("daily", EventDelivery.CharacterBubble, 0, first); e.repeatDaily = true; e.exclusiveGroup = "route";
            config.events = events.ToArray(); Assert.Throws<ArgumentException>(() => new GameSession(config));
        }
        [Test] public void InvalidWorkloadChoiceCannotPartiallySpendResourcesOrSetFlags()
        {
            var e = Event("event", EventDelivery.Forced);
            e.entryNode.choices = new[] { new DialogueChoice { choiceId = "choice", text = "เลือก", staminaCost = 5,
                flags = new[] { new FlagChange { flagId = "changed" } } } }; Start(); session.Events.TryBeginForced(); session.Dialogue.TryAdvance();
            e.entryNode.choices[0].activateWorkloads = new[] { "unknown" }; string before = JsonUtility.ToJson(session.State);
            Assert.That(session.Dialogue.TryChoose(e.entryNode.choices[0]), Is.False); Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
        }
        [Test] public void AuthoredEventDurationUsesProductionBoundaryExactlyOnce()
        {
            var e = Event("duration", EventDelivery.Forced); e.elapsedPhases = 1;
            var delivered = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameConfigSO>(StudioPrototypeBuilder.ConfigPath)); objects.Add(delivered);
            delivered.events = new[] { e }; session = new GameSession(delivered); session.Events.TryBeginForced(); Finish();
            Assert.That(session.State.production.resolvedPhases, Is.EqualTo(1)); Assert.That(session.State.production.tasks.Sum(t => t.effort), Is.EqualTo(12));
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Daytime)); Assert.That(session.Events.TryBeginForced(), Is.False);
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        private GameConfigSO FoundationStudio()
        {
            var delivered = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameConfigSO>(StudioPrototypeBuilder.ConfigPath));
            objects.Add(delivered);
            var ids = new[] { "studio_event_brief", "studio_event_bug", "studio_event_art_check", "studio_event_bug_followup" };
            delivered.events = delivered.events.Where(e => ids.Contains(e.id)).ToArray();
            return delivered;
        }
        [Test] public void FirstSprintCanPlayThroughAllAuthoredEventsAndKeepDeadlineReportAndStateValid()
        {
            var delivered = FoundationStudio(); session = new GameSession(delivered);
            int guard = 0;
            while (session.State.day <= 7)
            {
                Assert.That(++guard, Is.LessThan(100));
                if (session.State.dayEnded) { Assert.That(session.TryStartNextDay(), Is.True); continue; }
                if (session.Events.TryBeginForced()) { Finish(); continue; }
                var bubble = session.Events.BubbleEvents().FirstOrDefault();
                if (bubble != null)
                {
                    Assert.That(session.Dialogue.TryBegin(bubble), Is.True);
                    while (session.Dialogue.IsActive)
                        if (session.Dialogue.ChoicesReady) Assert.That(session.Dialogue.TryChoose(session.Dialogue.Node.choices[0]), Is.True);
                        else Assert.That(session.Dialogue.TryAdvance(), Is.True);
                    continue;
                }
                session.Production.TryReview();
                if (!session.Production.TryAct(ProductionAction.Producer)) Assert.That(session.TryAdvancePhase(), Is.True);
                Assert.That(session.CanRestore(session.State), Is.True);
            }
            Assert.That(delivered.events.All(e => session.State.completedEvents.Contains(e.id)), Is.True);
            Assert.That(session.State.production.reviews.Count, Is.EqualTo(1)); Assert.That(session.State.production.events.Count, Is.EqualTo(1));
            Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/Session030_FirstSprint.json", JsonUtility.ToJson(session.State, true));
        }
        [TestCase(false)] [TestCase(true)]
        public void DeliveredThirtyDayCampaignWithEventsAllowsRecoveryAndDailyConversation(bool dailyTalk)
        {
            var delivered = FoundationStudio(); session = new GameSession(delivered);
            int guard = 0, talkedDay = 0;
            var rest = delivered.rooms.First().hotspots.First(h => h.activity != null && h.activity.staminaRecovery > 0).activity;
            while (!session.Days.CampaignEnded)
            {
                Assert.That(++guard, Is.LessThan(200));
                if (session.State.dayEnded) { Assert.That(session.TryStartNextDay(), Is.True); continue; }
                bool forced = session.Events.TryBeginForced(); var bubble = forced ? null : session.Events.BubbleEvents().FirstOrDefault();
                if (forced || bubble != null)
                {
                    if (!forced) Assert.That(session.Dialogue.TryBegin(bubble), Is.True);
                    while (session.Dialogue.IsActive)
                        if (session.Dialogue.ChoicesReady) Assert.That(session.Dialogue.TryChoose(session.Dialogue.Node.choices[0]), Is.True);
                        else Assert.That(session.Dialogue.TryAdvance(), Is.True);
                    continue;
                }
                if (dailyTalk && talkedDay != session.State.day)
                {
                    foreach (var c in delivered.characters)
                    {
                        Assert.That(session.Interactions.TryOpen(session.CurrentRoom.hotspots.First(h => h.character == c)), Is.True);
                        Assert.That(session.Interactions.TryTalk(), Is.True); Finish(); Assert.That(session.Interactions.TryClose(), Is.True);
                    }
                    talkedDay = session.State.day;
                }
                session.Production.TryReview();
                var decision = session.State.production.tasks.FirstOrDefault(t => session.Production.IsReleased(t) && session.Production.Definition(t).decisionBlocked && !t.decisionCleared);
                bool acted = decision != null && session.Production.TryAct(ProductionAction.Clarify, decision.taskId);
                var stage = session.Production.ProducerStage;
                bool needsProducer = stage != null && (stage.startDay <= session.State.day + 3 || delivered.production.workers.Any(w => session.Production.Active(w.character) == null));
                if (!acted && needsProducer) acted = session.Production.TryAct(ProductionAction.Producer);
                if (!acted)
                {
                    var target = delivered.production.workers.OrderByDescending(w => session.Production.Required(w.stream) - session.Production.Completed(w.stream))
                        .FirstOrDefault(w => session.Production.Preview(ProductionAction.Help, w.character.id).allowed);
                    if (target != null) acted = session.Production.TryAct(ProductionAction.Help, target.character.id);
                }
                if (!acted && session.State.phase == DayPhase.Night && session.State.stamina < delivered.maxStamina) acted = session.TryCompleteActivity(rest, true);
                if (!acted) Assert.That(session.TryAdvancePhase(), Is.True);
                Assert.That(session.CanRestore(session.State), Is.True);
            }
            Assert.That(session.State.mainProgress, Is.EqualTo(100)); Assert.That(session.State.production.resolvedPhases, Is.EqualTo(120));
            Assert.That(session.State.production.reviews.Count, Is.EqualTo(5)); Assert.That(delivered.events.All(e => session.State.completedEvents.Contains(e.id)), Is.True);
            Assert.That(session.State.production.events.Count, Is.EqualTo(1));
            foreach (var c in session.State.characters) Assert.That(c.affection, Is.EqualTo((dailyTalk ? 60 : 0) + (c.characterId == "studio_plaifah" ? 1 : 0)));
            Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/Session030_30Day_Events_Talk" + dailyTalk + ".json", JsonUtility.ToJson(session.State, true));
        }
    }
}
