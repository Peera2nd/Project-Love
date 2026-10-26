using System;
using System.Collections.Generic;
using System.IO;
using ProjectLove.Simulation;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.EditorTools;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.Tests
{
    public sealed class FoundationTests
    {
        private readonly List<ScriptableObject> objects = new List<ScriptableObject>();
        private GameConfigSO config;
        private CharacterDataSO character;
        private GameSession session;
        private string saveDirectory;

        private T Make<T>() where T : IdentifiedDataSO
        {
            var result = ScriptableObject.CreateInstance<T>();
            result.id = Guid.NewGuid().ToString("N");
            objects.Add(result);
            return result;
        }

        [SetUp]
        public void SetUp()
        {
            character = Make<CharacterDataSO>();
            character.startingStress = 35;
            config = Make<GameConfigSO>();
            config.totalDays = 3;
            config.maxActionPointsPerDay = 2;
            config.characters = new[] { character };
            session = new GameSession(config);
            saveDirectory = Path.GetFullPath(Path.Combine("Logs", "SaveTests", Guid.NewGuid().ToString("N")));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var item in objects) UnityEngine.Object.DestroyImmediate(item);
            objects.Clear();
            if (Directory.Exists(saveDirectory)) Directory.Delete(saveDirectory, true);
        }

        private DialogueNodeSO Node()
        {
            var node = Make<DialogueNodeSO>();
            node.lines = new[] { new DialogueLine { lineId = Guid.NewGuid().ToString("N"), text = nameof(DialogueLine), speaker = character } };
            return node;
        }

        private EventDataSO Event(DialogueNodeSO node)
        {
            var result = Make<EventDataSO>();
            result.startDay = 1; result.preferredDay = 1; result.endDay = 3;
            result.entryNode = node;
            return result;
        }

        [Test]
        public void SessionsCopyDefinitionsAndDoNotShareMutableState()
        {
            var second = new GameSession(config);
            var activity = Make<ActivityDataSO>();
            activity.actionPointCost = 1;
            activity.successEffects = new[] { new StatModifier { stat = StatType.Stress, character = character, amount = -10 } };
            Assert.That(session.TryCompleteActivity(activity, true), Is.True);
            Assert.That(session.State.characters[0].stress, Is.EqualTo(25));
            Assert.That(second.State.characters[0].stress, Is.EqualTo(35));
            Assert.That(character.startingStress, Is.EqualTo(35));
            Assert.That(config.maxActionPointsPerDay, Is.EqualTo(2));
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void InvalidOrUnaffordableCostDoesNotSpendAP(int cost)
        {
            Assert.That(session.ActionPoints.TrySpend(cost), Is.False);
            Assert.That(session.State.actionPoints, Is.EqualTo(2));
        }

        [Test]
        public void InvalidEffectTargetRejectsWholeTransaction()
        {
            var unknown = Make<CharacterDataSO>();
            var activity = Make<ActivityDataSO>();
            activity.actionPointCost = 1;
            activity.successEffects = new[]
            {
                new StatModifier { stat = StatType.MainProgress, amount = 20 },
                new StatModifier { stat = StatType.Affection, character = unknown, amount = 5 }
            };
            Assert.That(session.TryCompleteActivity(activity, true), Is.False);
            Assert.That(session.State.actionPoints, Is.EqualTo(2));
            Assert.That(session.State.mainProgress, Is.Zero);
        }

        [Test]
        public void FailedActivityUsesFailureEffectsAndCanBeRecovered()
        {
            var activity = Make<ActivityDataSO>();
            activity.actionPointCost = 1;
            activity.successEffects = new[] { new StatModifier { stat = StatType.MainProgress, amount = 20 } };
            activity.failureEffects = new[] { new StatModifier { stat = StatType.MainProgress, amount = 2 } };
            Assert.That(session.TryCompleteActivity(activity, false), Is.True);
            Assert.That(session.State.mainProgress, Is.EqualTo(2));
            Assert.That(session.TryCompleteActivity(activity, true), Is.True);
            Assert.That(session.State.mainProgress, Is.EqualTo(22));
        }

        [Test]
        public void StatsClampAndDoNotOverflow()
        {
            var activity = Make<ActivityDataSO>();
            activity.successEffects = new[]
            {
                new StatModifier { stat = StatType.Affection, character = character, amount = int.MaxValue },
                new StatModifier { stat = StatType.Stress, character = character, amount = int.MinValue },
                new StatModifier { stat = StatType.Money, amount = int.MaxValue }
            };
            session.TryCompleteActivity(activity, true);
            session.TryCompleteActivity(activity, true);
            Assert.That(session.State.characters[0].affection, Is.EqualTo(100));
            Assert.That(session.State.characters[0].stress, Is.Zero);
            Assert.That(session.State.money, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void DayEndIsIdempotentAndResetsAPOnlyWhenStartingNextDay()
        {
            session.ActionPoints.TrySpend(1);
            Assert.That(session.TryEndDay(), Is.True);
            Assert.That(session.TryEndDay(), Is.False);
            Assert.That(session.State.actionPoints, Is.EqualTo(1));
            Assert.That(session.ActionPoints.TrySpend(0), Is.False);
            Assert.That(session.TryStartNextDay(), Is.True);
            Assert.That(session.TryStartNextDay(), Is.False);
            Assert.That(session.State.day, Is.EqualTo(2));
            Assert.That(session.State.actionPoints, Is.EqualTo(2));
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Morning));
        }

        [Test]
        public void CampaignCannotAdvancePastConfiguredLastDay()
        {
            for (int i = 1; i < config.totalDays; i++) { session.TryEndDay(); Assert.That(session.TryStartNextDay(), Is.True); }
            session.TryEndDay();
            Assert.That(session.Days.CampaignEnded, Is.True);
            Assert.That(session.TryStartNextDay(), Is.False);
            Assert.That(session.State.day, Is.EqualTo(config.totalDays));
        }

        [Test]
        public void PhaseStopsAtNight()
        {
            Assert.That(session.TryAdvancePhase(), Is.True);
            session.TryAdvancePhase(); session.TryAdvancePhase();
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Night));
            Assert.That(session.TryAdvancePhase(), Is.False);
        }

        [Test]
        public void PreferredDateIsNotAHardCutoffButWindowAndPhaseMatter()
        {
            var data = Event(Node());
            session.State.day = 2;
            Assert.That(session.CanStartEvent(data), Is.True);
            session.State.phase = DayPhase.Night;
            Assert.That(session.CanStartEvent(data), Is.False);
            session.State.phase = DayPhase.Morning; session.State.day = 4;
            Assert.That(session.CanStartEvent(data), Is.False);
        }

        [Test]
        public void ChoicesRespectConditionsAndApplyEffectsExactlyOnce()
        {
            var node = Node();
            var choice = new DialogueChoice
            {
                choiceId = Guid.NewGuid().ToString("N"), text = nameof(DialogueChoice), actionPointCost = 1,
                conditions = new[] { new Condition { type = ConditionType.StatAtLeast, stat = StatType.MainProgress, value = 5 } },
                effects = new[] { new StatModifier { stat = StatType.Affection, character = character, amount = 7 } }
            };
            node.choices = new[] { choice };
            var data = Event(node);
            session.Dialogue.TryBegin(data);
            Assert.That(session.TryEndDay(), Is.False);
            session.Dialogue.TryAdvance();
            Assert.That(session.Dialogue.TryChoose(choice), Is.False);
            Assert.That(session.State.actionPoints, Is.EqualTo(2));
            session.State.mainProgress = 5;
            Assert.That(session.Dialogue.TryChoose(choice), Is.True);
            Assert.That(session.Dialogue.TryChoose(choice), Is.False);
            Assert.That(session.State.characters[0].affection, Is.EqualTo(7));
            Assert.That(session.State.chosenOptions, Has.Count.EqualTo(1));
            Assert.That(session.CanStartEvent(data), Is.False);
        }

        [Test]
        public void PersonalArcFlagsCanGateEventsIndependentlyOfAffection()
        {
            var data = Event(Node());
            data.conditions = new[] { new Condition { type = ConditionType.FlagSet, flagId = nameof(PersonalArcFlagsCanGateEventsIndependentlyOfAffection) } };
            session.State.characters[0].affection = 100;
            Assert.That(session.CanStartEvent(data), Is.False);
            session.State.flags.Add(data.conditions[0].flagId);
            Assert.That(session.CanStartEvent(data), Is.True);
        }

        [Test]
        public void RuntimeStateRoundTripsThroughJson()
        {
            session.State.flags.Add(nameof(RuntimeStateRoundTripsThroughJson));
            session.TryEndDay();
            var restored = JsonUtility.FromJson<RuntimeGameState>(JsonUtility.ToJson(session.State));
            Assert.That(restored.dayEnded, Is.True);
            Assert.That(restored.characters[0].characterId, Is.EqualTo(character.id));
            Assert.That(restored.flags, Is.EquivalentTo(session.State.flags));
        }

        [Test]
        public void ZeroApChoicesCanDeferAndReturnNextDayWithoutCompletingEvent()
        {
            var node = Node();
            node.choices = new[] { new DialogueChoice { choiceId = "paid", actionPointCost = 1 } };
            var data = Event(node);
            session.ActionPoints.TrySpend(2);
            string before = JsonUtility.ToJson(session.State);
            Assert.That(session.Dialogue.TryBegin(data), Is.True);
            session.Dialogue.TryAdvance();
            Assert.That(session.Dialogue.HasAvailableChoice, Is.False);
            Assert.That(session.Dialogue.TryDefer(), Is.True);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
            Assert.That(session.TryEndDay(), Is.True);
            session.TryStartNextDay();
            Assert.That(session.Dialogue.TryBegin(data), Is.True);
            session.Dialogue.TryAdvance();
            Assert.That(session.Dialogue.TryChoose(node.choices[0]), Is.True);
            Assert.That(session.State.completedEvents, Does.Contain(data.id));
        }

        [Test]
        public void DeferringLaterBlockedChoiceRollsBackEarlierCostsRewardsFlagsAndHistory()
        {
            var first = Node(); var second = Node();
            first.choices = new[] { new DialogueChoice { choiceId = "first", actionPointCost = 2, nextNode = second,
                effects = new[] { new StatModifier { stat = StatType.Money, amount = 100 } },
                flags = new[] { new FlagChange { flagId = "temporary" } } } };
            second.choices = new[] { new DialogueChoice { choiceId = "second", actionPointCost = 1 } };
            var data = Event(first);
            string before = JsonUtility.ToJson(session.State);
            for (int attempt = 0; attempt < 2; attempt++)
            {
                session.Dialogue.TryBegin(data); session.Dialogue.TryAdvance();
                Assert.That(session.Dialogue.TryChoose(first.choices[0]), Is.True);
                session.Dialogue.TryAdvance();
                Assert.That(session.Dialogue.HasAvailableChoice, Is.False);
                Assert.That(session.Dialogue.TryDefer(), Is.True);
                Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
            }
        }

        [Test]
        public void UnmetConditionsAndMalformedTargetsStillAllowDeferral()
        {
            var node = Node();
            node.choices = new[] { new DialogueChoice { choiceId = "gated", conditions = new[] {
                new Condition { type = ConditionType.FlagSet, flagId = "missing" } } } };
            session.Dialogue.TryBegin(Event(node)); session.Dialogue.TryAdvance();
            Assert.That(session.Dialogue.HasAvailableChoice, Is.False);
            Assert.That(session.Dialogue.TryDefer(), Is.True);
            Assert.That(session.TryAdvancePhase(), Is.True);
        }

        [Test]
        public void SchedulerSortsPriorityThenPreferredDayThenStableIdAndRechecksConditions()
        {
            var a = Event(Node()); a.id = "a"; a.priority = 3;
            var b = Event(Node()); b.id = "b"; b.priority = 3;
            var c = Event(Node()); c.id = "c"; c.priority = 9;
            c.conditions = new[] { new Condition { type = ConditionType.FlagSet, flagId = "ready" } };
            var scheduler = new EventScheduler(session, new[] { b, c, a, a });
            Assert.That(scheduler.Available(), Is.EqualTo(new[] { a, b }));
            session.State.flags.Add("ready");
            Assert.That(scheduler.Available(), Is.EqualTo(new[] { c, a, b }));
            Assert.That(scheduler.TryBeginNext(), Is.True);
            Assert.That(scheduler.Available(), Is.Empty);
            session.Dialogue.TryAdvance();
            Assert.That(scheduler.Available(), Is.EqualTo(new[] { a, b }));
        }

        [Test]
        public void SchedulerKeepsMissedPreferredEventUntilWindowExpires()
        {
            var data = Event(Node());
            var scheduler = new EventScheduler(session, new[] { data });
            session.TryEndDay(); session.TryStartNextDay();
            Assert.That(scheduler.Available(), Does.Contain(data));
            session.TryAdvancePhase();
            Assert.That(scheduler.Available(), Is.Empty);
            session.TryEndDay(); session.TryStartNextDay();
            Assert.That(scheduler.TryBeginNext(), Is.True);
            session.Dialogue.TryAdvance();
            Assert.That(scheduler.Available(), Is.Empty);
        }

        [Test]
        public void DiskSaveRoundTripRestoresCompletedEventsAndPreventsDuplicateReward()
        {
            var node = Node();
            node.choices = new[] { new DialogueChoice { choiceId = "reward", actionPointCost = 1,
                effects = new[] { new StatModifier { stat = StatType.Money, amount = 7 } } } };
            var data = Event(node);
            session.Dialogue.TryBegin(data); session.Dialogue.TryAdvance(); session.Dialogue.TryChoose(node.choices[0]);
            var store = new SaveStore(saveDirectory);
            Assert.That(store.Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            var restored = new GameSession(config);
            Assert.That(store.Load(restored, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            Assert.That(JsonUtility.ToJson(restored.State), Is.EqualTo(JsonUtility.ToJson(session.State)));
            Assert.That(restored.CanStartEvent(data), Is.False);
            Assert.That(restored.State.money, Is.EqualTo(7));
            restored.State.characters[0].affection = 20;
            Assert.That(session.State.characters[0].affection, Is.Zero);
        }

        [Test]
        public void DiskSaveRejectsBusySessionAndDeferralRestoresSafePoint()
        {
            var store = new SaveStore(saveDirectory);
            Assert.That(store.Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            string saved = File.ReadAllText(store.PathFor(session, SaveSlot.Manual));
            session.Dialogue.TryBegin(Event(Node()));
            Assert.That(store.Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Busy));
            Assert.That(store.Load(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Busy));
            Assert.That(File.ReadAllText(store.PathFor(session, SaveSlot.Manual)), Is.EqualTo(saved));
            session.Dialogue.TryDefer();
            Assert.That(store.Load(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
        }

        [Test]
        public void SaveReplacementKeepsLastFileAndSlotsIndependent()
        {
            var store = new SaveStore(saveDirectory);
            store.Save(session, SaveSlot.Manual);
            string previous = File.ReadAllText(store.PathFor(session, SaveSlot.Manual));
            session.State.mainProgress = 20;
            Assert.That(store.Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            Assert.That(File.ReadAllText(store.PathFor(session, SaveSlot.Manual) + ".bak"), Is.EqualTo(previous));
            session.State.mainProgress = 40;
            store.Save(session, SaveSlot.Auto);
            store.Load(session, SaveSlot.Manual);
            Assert.That(session.State.mainProgress, Is.EqualTo(20));
            store.Load(session, SaveSlot.Auto);
            Assert.That(session.State.mainProgress, Is.EqualTo(40));
        }

        [TestCase("")]
        [TestCase("{")]
        [TestCase("{}")]
        public void CorruptOrEmptySaveDoesNotChangeLiveState(string json)
        {
            var store = new SaveStore(saveDirectory);
            Directory.CreateDirectory(saveDirectory);
            File.WriteAllText(store.PathFor(session, SaveSlot.Manual), json);
            string before = JsonUtility.ToJson(session.State);
            Assert.That(store.Load(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Invalid));
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
        }

        [Test]
        public void MissingUnsupportedAndOutOfBoundsSavesDoNotMutateState()
        {
            var store = new SaveStore(saveDirectory);
            Assert.That(store.Load(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Missing));
            store.Save(session, SaveSlot.Manual);
            string path = store.PathFor(session, SaveSlot.Manual);
            File.WriteAllText(path, File.ReadAllText(path).Replace("\"version\": 3", "\"version\": 999"));
            Assert.That(store.Load(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Incompatible));
            session.TryEndDay(); session.TryStartNextDay();
            store.Save(session, SaveSlot.Manual);
            config.totalDays = 1;
            var fresh = new GameSession(config);
            string before = JsonUtility.ToJson(fresh.State);
            Assert.That(store.Load(fresh, SaveSlot.Manual), Is.EqualTo(SaveResult.Invalid));
            Assert.That(JsonUtility.ToJson(fresh.State), Is.EqualTo(before));
        }

        [Test]
        public void InvalidRuntimeSnapshotsAndUnwritableSaveDirectoryAreRejected()
        {
            var invalid = session.State.Clone(); invalid.characters[0].stress = 101;
            Assert.That(session.TryRestore(invalid), Is.False);
            invalid = session.State.Clone(); invalid.flags.Add("duplicate"); invalid.flags.Add("duplicate");
            Assert.That(session.TryRestore(invalid), Is.False);
            Directory.CreateDirectory(saveDirectory);
            string file = Path.Combine(saveDirectory, "file"); File.WriteAllText(file, "occupied");
            Assert.That(new SaveStore(file).Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.IoError));
            Assert.That(session.State.actionPoints, Is.EqualTo(2));
        }

        [Test]
        public void GeneratedSandboxUsesApprovedDurationAndPassesDataValidation()
        {
            var generated = AssetDatabase.LoadAssetAtPath<GameConfigSO>(SandboxProjectBuilder.ConfigPath);
            Assert.That(generated, Is.Not.Null);
            Assert.That(generated.totalDays, Is.EqualTo(30));
            Assert.That(FoundationValidator.ValidateProject(), Is.Empty);
        }
    }
}
