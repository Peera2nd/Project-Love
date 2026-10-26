using System;
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
    public sealed class StaffAndProducerTests
    {
        private GameConfigSO config;
        private GameSession session;
        private string directory;
        [SetUp] public void Setup()
        {
            config = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameConfigSO>(StudioPrototypeBuilder.ConfigPath));
            config.events = Array.Empty<EventDataSO>(); // Isolate management rules from authored story scheduling.
            session = new GameSession(config);
            directory = Path.GetFullPath(Path.Combine("Logs", "ManagementSaves", Guid.NewGuid().ToString("N")));
        }
        [TearDown] public void Teardown() { UnityEngine.Object.DestroyImmediate(config); if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        private void Night() { while (session.State.phase != DayPhase.Night) Assert.That(session.TryAdvancePhase(), Is.True); }
        [Test] public void OrdersForMultipleWorkersDoNotAdvanceTimeAndRestResumesNextPhase()
        {
            foreach (var c in config.characters) Assert.That(session.Production.TryOrder(c.id, StaffOrderKind.Rest), Is.True);
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Morning)); Assert.That(session.State.production.resolvedPhases, Is.Zero);
            Assert.That(session.Production.TryAct(ProductionAction.Help, config.characters[0].id), Is.False);
            Assert.That(session.State.characters.All(c => c.stress == 30), Is.True);
            session.TryAdvancePhase(); Assert.That(session.State.production.tasks.All(t => t.effort == 0), Is.True);
            Assert.That(session.State.characters.All(c => c.stress == 25), Is.True); Assert.That(session.State.production.orders, Is.Empty);
            session.TryAdvancePhase(); Assert.That(session.State.production.tasks.Sum(t => t.effort), Is.EqualTo(12));
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void OvertimePaysOncePerPersonAndProducesOnlyAtNightBoundary()
        {
            Night(); int before = session.State.production.tasks.Sum(t => t.effort);
            foreach (var c in config.characters)
            {
                Assert.That(session.Production.TryOrder(c.id, StaffOrderKind.Overtime), Is.True);
                string json = JsonUtility.ToJson(session.State);
                Assert.That(session.Production.TryOrder(c.id, StaffOrderKind.Overtime), Is.False); Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(json));
            }
            Assert.That(session.State.money, Is.EqualTo(config.startingMoney - 3 * config.production.overtimeMoneyCost));
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Night)); Assert.That(session.State.dayEnded, Is.False);
            Assert.That(session.State.production.tasks.Sum(t => t.effort), Is.EqualTo(before));
            Assert.That(session.State.characters.All(c => c.stress == 46), Is.True);
            Assert.That(session.TryAdvancePhase(), Is.True);
            Assert.That(session.State.production.tasks.Sum(t => t.effort), Is.EqualTo(before + 9));
            Assert.That(session.State.characters.All(c => c.stress == 46), Is.True);
            Assert.That(session.TryAdvancePhase(), Is.False); session.TryStartNextDay();
            Assert.That(session.State.characters.All(c => c.stress == 42), Is.True); Assert.That(session.State.production.orders, Is.Empty);
        }
        [Test] public void RejectedOvertimeIsAtomicAndNoOneWorksAtNightByDefault()
        {
            string json = JsonUtility.ToJson(session.State); Assert.That(session.Production.TryOrder(config.characters[0].id, StaffOrderKind.Overtime), Is.False);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(json)); Night();
            session.State.money = 0; json = JsonUtility.ToJson(session.State);
            Assert.That(session.Production.TryOrder(config.characters[0].id, StaffOrderKind.Overtime), Is.False);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(json)); int before = session.State.production.tasks.Sum(t => t.effort);
            session.TryAdvancePhase(); Assert.That(session.State.production.tasks.Sum(t => t.effort), Is.EqualTo(before));
        }
        [TestCase(0, 30, 0)] [TestCase(20, 30, 1)] [TestCase(0, 75, 2)] [TestCase(20, 75, 3)]
        public void OvertimeDialogueUsesAffectionStressAndIndividualCharacter(int affection, int stress, int expected)
        {
            Night();
            foreach (var c in config.characters)
            {
                var state = session.State.FindCharacter(c.id); state.affection = affection; state.stress = stress;
                var target = session.CurrentRoom.hotspots.Single(h => h.character == c);
                Assert.That(session.Interactions.TryOpen(target), Is.True);
                var ordinary = session.Interactions.Conversation;
                Assert.That(session.Production.TryOrder(c.id, StaffOrderKind.Overtime), Is.True);
                var ot = session.Interactions.Conversation;
                Assert.That(ot, Is.Not.SameAs(ordinary)); Assert.That(ot.id, Is.EqualTo(c.id + "_ot_" + expected));
                Assert.That(ot.lines.Single().speaker, Is.SameAs(c));
                Assert.That(session.Interactions.TryTalk(), Is.True); session.Dialogue.TryAdvance();
                int money = session.State.money, stamina = session.State.stamina;
                Assert.That(session.Interactions.TryTalk(), Is.True); session.Dialogue.TryAdvance();
                Assert.That(session.State.stamina, Is.EqualTo(stamina)); Assert.That(session.State.money, Is.EqualTo(money));
                session.Interactions.TryClose();
            }
        }
        [Test] public void ProducerRewardPreviewMatchesMoneyStageAndSharedCardCaps()
        {
            var c = config.characters[0]; var task = session.Production.Active(c); task.effort = session.Production.Definition(task).effort - 1;
            string before = JsonUtility.ToJson(session.State);
            var preview = session.Production.Preview(ProductionAction.Producer);
            Assert.That(preview.allowed, Is.True); Assert.That(preview.workers.First(w => w.characterId == c.id).producer, Is.EqualTo(1));
            Assert.That(preview.workers.First(w => w.characterId == c.id).team, Is.Zero); Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
            Assert.That(session.Production.TryAct(ProductionAction.Producer), Is.True);
            Assert.That(session.State.money, Is.EqualTo(config.startingMoney + config.production.producerMoney));
            Assert.That(session.Production.ProducerCompleted, Is.EqualTo(config.production.producerEffort));
            Assert.That(session.State.production.tasks.Single(t => t.taskId == "studio_w1_grace_2").effort, Is.Zero);
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void ProducerUnlocksNextSprintEarlyAndCreditBelongsToThatSprint()
        {
            var next = config.production.sprints[1]; var task = session.Production.Find("studio_w2_ploy_1");
            Assert.That(session.Production.IsReleased(task), Is.False);
            for (int i = 0; i < 4; i++) Assert.That(session.Production.TryAct(ProductionAction.Producer), Is.True);
            Assert.That(session.State.day, Is.EqualTo(1)); Assert.That(session.Production.StageUnlocked(next), Is.True);
            session.TryStartNextDay(); Assert.That(session.Production.IsReleased(task), Is.True);
            task = session.Production.Find(task.taskId); task.effort = session.Production.Definition(task).effort;
            session.Production.TryReview(); Assert.That(task.acceptedSprintId, Is.EqualTo(next.id));
            Assert.That(task.acceptedDay, Is.LessThan(next.startDay));
            var design = config.workStreams.First(s => s.id == "work_design");
            Assert.That(session.Production.Completed(design), Is.Zero); Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void CalendarDoesNotBypassProducerGateAndLateCreditKeepsDeadlineReportFrozen()
        {
            for (int day = 1; day < 8; day++) { session.TryEndDay(); session.TryStartNextDay(); }
            Assert.That(session.Production.StageUnlocked(config.production.sprints[1]), Is.False);
            var record = JsonUtility.ToJson(session.State.production.reviews.Single());
            foreach (var prerequisite in session.Production.Definition(session.Production.Find("studio_w1_grace_2")).prerequisites)
            {
                Assert.That(prerequisite.prerequisites.All(p => session.Production.Find(p.id).acceptedDay > 0), Is.True);
                session.Production.Find(prerequisite.id).effort = prerequisite.effort;
                session.Production.TryReview();
            }
            var task = session.Production.Find("studio_w1_grace_2"); task.decisionCleared = true; task.effort = session.Production.Definition(task).effort;
            session.Production.TryReview(); Assert.That(task.acceptedSprintId, Is.EqualTo(config.production.sprints[0].id));
            Assert.That(JsonUtility.ToJson(session.State.production.reviews.Single()), Is.EqualTo(record));
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void SaveRestoresPaidNightOrdersAndProducerStageWithoutRepeatingRewards()
        {
            session.Production.TryAct(ProductionAction.Producer); Night();
            Assert.That(session.Production.TryOrder(config.characters[0].id, StaffOrderKind.Overtime), Is.True);
            var saves = new SaveStore(directory); Assert.That(saves.Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            string before = JsonUtility.ToJson(session.State); var loaded = new GameSession(config);
            Assert.That(saves.Load(loaded, SaveSlot.Manual), Is.EqualTo(SaveResult.Success)); Assert.That(JsonUtility.ToJson(loaded.State), Is.EqualTo(before));
            Assert.That(loaded.Production.TryOrder(config.characters[0].id, StaffOrderKind.Overtime), Is.False);
            int money = loaded.State.money; loaded.TryAdvancePhase(); Assert.That(loaded.State.money, Is.EqualTo(money));
            Assert.That(loaded.State.production.orders, Is.Empty); Assert.That(loaded.CanRestore(loaded.State), Is.True);
        }
    }
}
