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
    public sealed class SharedNightTests
    {
        private GameConfigSO config;
        private GameSession session;
        private string directory;
        [SetUp] public void Setup()
        {
            config = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameConfigSO>(StudioPrototypeBuilder.ConfigPath));
            session = new GameSession(config);
            directory = Path.GetFullPath(Path.Combine("Logs", "NightSaves", Guid.NewGuid().ToString("N")));
        }
        [TearDown] public void Teardown()
        { UnityEngine.Object.DestroyImmediate(config); if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        private CharacterDataSO Grace => config.characters.Single(c => c.id == "studio_grace");
        private EventDataSO Offer(string person = "studio_grace")
        {
            var bubbles = session.Events.BubbleEvents();
            Assert.That(bubbles.Any(e => e.repeatGroup == person + "_volunteer"), Is.True,
                "Expected actionable offer; visible bubbles: " + string.Join(",", bubbles.Select(e => e.id)) +
                "; can volunteer: " + session.Production.CanVolunteer(config.characters.Single(c => c.id == person), 2));
            return bubbles.Single(e => e.repeatGroup == person + "_volunteer");
        }
        private void Finish()
        {
            int guard = 0;
            while (session.Dialogue.IsActive)
            {
                Assert.That(++guard, Is.LessThan(20));
                if (session.Dialogue.ChoicesReady)
                    Assert.That(session.Dialogue.TryChoose(session.Dialogue.Node.choices.Last()), Is.True);
                else Assert.That(session.Dialogue.TryAdvance(), Is.True);
            }
        }
        private void ReachNight(int day = 5)
        {
            while (session.State.day < day || session.State.phase != DayPhase.Night)
            {
                if (session.State.dayEnded) { Assert.That(session.TryStartNextDay(), Is.True); continue; }
                if (session.Events.TryBeginForced()) { Finish(); continue; }
                var regular = session.Events.BubbleEvents().FirstOrDefault(e => !e.repeatDaily);
                if (regular != null)
                {
                    Assert.That(session.Dialogue.TryBegin(regular), Is.True);
                    while (session.Dialogue.IsActive)
                        if (session.Dialogue.ChoicesReady) Assert.That(session.Dialogue.TryChoose(session.Dialogue.Node.choices.First(session.Dialogue.CanChoose)), Is.True);
                        else Assert.That(session.Dialogue.TryAdvance(), Is.True);
                    continue;
                }
                session.Production.TryReview();
                var ready = session.Production.ReadyTasks(Grace).FirstOrDefault();
                if (ready != null && session.State.day == 5 && session.State.phase == DayPhase.Evening)
                    session.Production.TryPrioritize(ready.taskId);
                Assert.That(session.TryAdvancePhase(), Is.True);
            }
        }
        private DialogueChoice OpenOffer(string person = "studio_grace", int choice = 0)
        {
            Assert.That(session.Dialogue.TryBegin(Offer(person)), Is.True);
            Assert.That(session.Dialogue.TryAdvance(), Is.True);
            return session.Dialogue.Node.choices[choice];
        }
        [Test] public void VolunteerRequiresNightReadyWorkPressureAffordableCompensationAndStressRange()
        {
            Assert.That(session.Events.BubbleEvents().Any(e => !string.IsNullOrEmpty(e.repeatGroup)), Is.False);
            ReachNight(); var e = Offer(); Assert.That(e.character, Is.SameAs(Grace));
            session.State.money = 0; Assert.That(session.Events.BubbleEvents().Any(x => x.character == Grace && x.repeatDaily), Is.False);
            session.State.money = 100; session.State.FindCharacter(Grace.id).stress = 66;
            Assert.That(session.Events.BubbleEvents().Any(x => x.character == Grace && x.repeatDaily), Is.False);
            session.State.FindCharacter(Grace.id).stress = 30;
            session.Production.TryPrioritize("studio_w1_grace_2");
            Assert.That(session.Events.BubbleEvents().Any(x => x.character == Grace && x.repeatDaily), Is.False);
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [TestCase(0, 30, 0)] [TestCase(20, 30, 1)] [TestCase(0, 55, 2)] [TestCase(20, 55, 3)]
        public void VolunteerVoiceTracksAffectionAndStressWithoutOverlappingPerCharacter(int affection, int stress, int variant)
        {
            ReachNight(); var c = session.State.FindCharacter(Grace.id); c.affection = affection; c.stress = stress;
            Assert.That(Offer().entryNode.id, Is.EqualTo("studio_grace_volunteer_" + variant));
            Assert.That(session.Events.BubbleEvents().GroupBy(e => e.character.id).All(g => g.Count() == 1), Is.True);
        }
        [Test] public void WorkTogetherProducesRealCappedTaskEffortOneNightAndOtherOrderedWorkersOnly()
        {
            ReachNight(); var work = session.Production; var task = work.Active(Grace);
            int before = task.effort, phases = session.State.production.resolvedPhases, money = session.State.money, stamina = session.State.stamina;
            var choice = OpenOffer(); var forecast = work.PreviewDialogueWork(choice);
            Assert.That(forecast.allowed, Is.True); Assert.That(forecast.moneyCost, Is.EqualTo(config.production.overtimeMoneyCost));
            Assert.That(session.Dialogue.TryChoose(choice), Is.True);
            Assert.That(session.State.dayEnded, Is.True); Assert.That(session.State.production.resolvedPhases, Is.EqualTo(phases + 1));
            Assert.That(work.Find(task.taskId).effort, Is.EqualTo(before + forecast.help + forecast.team));
            Assert.That(session.State.production.sharedNights.Single().taskId, Is.EqualTo(task.taskId));
            Assert.That(session.State.money, Is.EqualTo(money - forecast.moneyCost)); Assert.That(session.State.stamina, Is.EqualTo(stamina - forecast.staminaCost));
            Assert.That(session.State.FindCharacter(Grace.id).affection, Is.EqualTo(forecast.affectionGain));
            Assert.That(session.State.production.orders, Is.Empty); Finish(); Assert.That(session.CanRestore(session.State), Is.True);
            Assert.That(session.TryAdvancePhase(), Is.False); Assert.That(session.TryStartNextDay(), Is.True);
            Assert.That(session.Events.BubbleEvents().Any(e => e.exclusiveGroup == Grace.id + "_night_callback"), Is.True);
        }
        [Test] public void BackAfterSharedWorkRevertsOrdersMoneyEnergyTaskReviewRelationshipAndCooldown()
        {
            ReachNight(); string before = JsonUtility.ToJson(session.State);
            var choice = OpenOffer(); Assert.That(session.Dialogue.TryChoose(choice), Is.True);
            Assert.That(session.State.dayEnded, Is.True); Assert.That(session.Dialogue.TryDefer(), Is.True);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before)); Assert.That(Offer(), Is.Not.Null);
            choice = OpenOffer(); Assert.That(session.Dialogue.TryChoose(choice), Is.True); Finish();
            Assert.That(session.State.production.sharedNights.Count, Is.EqualTo(1));
        }
        [Test] public void AcceptKeepsNightOpenForOtherOrdersAndNormalTalkThenHelpDoesNotChargeAgain()
        {
            ReachNight(); var choice = OpenOffer(choice: 1); int money = session.State.money;
            Assert.That(session.Dialogue.TryChoose(choice), Is.True); Finish();
            Assert.That(session.State.dayEnded, Is.False); Assert.That(session.Production.HasOrder(Grace.id, StaffOrderKind.Overtime), Is.True);
            Assert.That(session.State.production.sharedNights, Is.Empty);
            Assert.That(session.Interactions.TryOpen(session.CurrentRoom.hotspots.Single(h => h.character == Grace)), Is.True);
            Assert.That(session.Interactions.Conversation.id, Does.Contain("_ot_"));
            Assert.That(session.Production.TryAct(ProductionAction.Help, Grace.id), Is.True);
            Assert.That(session.State.money, Is.EqualTo(money - config.production.overtimeMoneyCost));
            Assert.That(session.State.production.sharedNights.Count, Is.EqualTo(1)); Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void DeclineCostsNothingAndCooldownIsSharedAcrossVoiceVariants()
        {
            ReachNight(); int phases = session.State.production.resolvedPhases, money = session.State.money;
            var choice = OpenOffer(choice: 2); Assert.That(session.Dialogue.TryChoose(choice), Is.True); Finish();
            Assert.That(session.State.production.resolvedPhases, Is.EqualTo(phases)); Assert.That(session.State.money, Is.EqualTo(money));
            session.State.FindCharacter(Grace.id).affection = 20;
            Assert.That(session.Events.BubbleEvents().Any(e => e.character == Grace && e.repeatDaily), Is.False);
            Assert.That(session.CanRestore(session.State), Is.True);
            ReachNight(6); Assert.That(session.Events.BubbleEvents().Any(e => e.character == Grace && e.repeatDaily), Is.False);
            ReachNight(8); Assert.That(session.Production.TryPrioritize("studio_bug"), Is.True);
            Assert.That(Offer(), Is.Not.Null); // Three days elapsed, including voice changes; overdue ready work remains eligible.
        }
        [Test] public void InsufficientEnergyAndInvalidTargetsCannotPartiallyChargeOrSetEffects()
        {
            ReachNight(); var choice = OpenOffer(); session.State.stamina = config.production.helpCost - 1;
            string before = JsonUtility.ToJson(session.State);
            Assert.That(session.Dialogue.CanChoose(choice), Is.False); Assert.That(session.Dialogue.TryChoose(choice), Is.False);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
            Assert.That(session.Dialogue.CanChoose(session.Dialogue.Node.choices[1]), Is.True);
            Assert.That(session.Dialogue.CanChoose(session.Dialogue.Node.choices[2]), Is.True);
        }
        [Test] public void ExistingPaidOrderAndOtherWorkerTicksSurviveSharedChoiceWithoutExtraNightTick()
        {
            ReachNight(); var work = session.Production; var ploy = config.characters.Single(c => c.id == "studio_ploy");
            Assert.That(work.TryOrder(ploy.id, StaffOrderKind.Overtime), Is.True);
            var ployTask = work.Active(ploy); int before = ployTask.effort; int phases = session.State.production.resolvedPhases;
            var choice = OpenOffer(); var preview = work.PreviewDialogueWork(choice); Assert.That(preview.allowed, Is.True);
            Assert.That(session.Dialogue.TryChoose(choice), Is.True); Finish();
            Assert.That(work.Find(ployTask.taskId).effort, Is.GreaterThan(before));
            Assert.That(session.State.production.resolvedPhases, Is.EqualTo(phases + 1)); Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void SharedNightSaveRoundTripKeepsCallbackLedgerAndRejectsTampering()
        {
            ReachNight(); var choice = OpenOffer(); session.Dialogue.TryChoose(choice); Finish();
            var store = new SaveStore(directory); Assert.That(store.Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            var fresh = new GameSession(config); Assert.That(store.Load(fresh, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            Assert.That(JsonUtility.ToJson(fresh.State), Is.EqualTo(JsonUtility.ToJson(session.State)));
            var bad = fresh.State.Clone(); bad.production.sharedNights[0].day++; Assert.That(fresh.CanRestore(bad), Is.False);
            bad = fresh.State.Clone(); bad.production.sharedNights.Add(bad.production.sharedNights[0]); Assert.That(fresh.CanRestore(bad), Is.False);
            bad = fresh.State.Clone(); bad.production.sharedNights[0].help = config.production.helpEffort + 1; Assert.That(fresh.CanRestore(bad), Is.False);
        }
        [Test] public void CallbackIsCompletionOnlyOneTimeAndNotAnExtraAffectionFarm()
        {
            ReachNight(); var choice = OpenOffer(); session.Dialogue.TryChoose(choice); Finish(); session.TryStartNextDay();
            var callback = session.Events.BubbleEvents().Single(e => e.exclusiveGroup == Grace.id + "_night_callback");
            int affection = session.State.FindCharacter(Grace.id).affection;
            Assert.That(session.Dialogue.TryBegin(callback), Is.True); Assert.That(session.Dialogue.TryDefer(), Is.True);
            Assert.That(session.Events.BubbleEvents(), Does.Contain(callback)); session.Dialogue.TryBegin(callback); Finish();
            Assert.That(session.State.FindCharacter(Grace.id).affection, Is.EqualTo(affection));
            Assert.That(session.Events.BubbleEvents().Any(e => e.exclusiveGroup == callback.exclusiveGroup), Is.False);
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [TestCase("studio_grace")] [TestCase("studio_plaifah")] [TestCase("studio_ploy")]
        public void EveryWorkerCanVolunteerAndReceiveActualSharedWorkWithIndividualVoice(string id)
        {
            ReachNight(); var character = session.State.FindCharacter(id); character.affection = 20; character.stress = 55;
            var choice = OpenOffer(id); Assert.That(choice.workCharacter.id, Is.EqualTo(id));
            Assert.That(session.Dialogue.CurrentLine, Is.Null);
            var preview = session.Production.PreviewDialogueWork(choice); Assert.That(preview.allowed, Is.True);
            Assert.That(session.Dialogue.TryChoose(choice), Is.True);
            Assert.That(session.Dialogue.CurrentLine.speaker.id, Is.EqualTo(id));
            Finish(); var record = session.State.production.sharedNights.Single();
            Assert.That(record.characterId, Is.EqualTo(id)); Assert.That(record.help, Is.GreaterThan(0));
            Assert.That(session.State.FindCharacter(id).affection, Is.EqualTo(23));
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void SharedHelpCapsAtLastUnitWithoutSpillingAndStillOnlyResolvesOneNight()
        {
            ReachNight(); var task = session.Production.Active(Grace); task.effort = session.Production.Definition(task).effort - 1;
            var choice = OpenOffer(); var forecast = session.Production.PreviewDialogueWork(choice);
            Assert.That(forecast.help, Is.EqualTo(1)); Assert.That(forecast.team, Is.Zero);
            int phases = session.State.production.resolvedPhases;
            Assert.That(session.Dialogue.TryChoose(choice), Is.True); Finish();
            Assert.That(session.State.production.resolvedPhases, Is.EqualTo(phases + 1));
            Assert.That(session.State.production.sharedNights.Single().help, Is.EqualTo(1));
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void CommandValidationRejectsExtraEventTimeAndInteractiveEpilogues()
        {
            var original = config.events.First(e => e.repeatDaily);
            var altered = UnityEngine.Object.Instantiate(original);
            config.events = new[] { altered }; altered.elapsedPhases = 1;
            Assert.That(EventScheduler.ValidateCatalog(config).Any(i => i.Contains("invalid work command")), Is.True);
            UnityEngine.Object.DestroyImmediate(altered);
            altered = UnityEngine.Object.Instantiate(original);
            var entry = UnityEngine.Object.Instantiate(original.entryNode);
            // A command must leave only a linear, choice-free epilogue after the phase boundary.
            var choice = new DialogueChoice { choiceId = "invalid", text = "ทดสอบ", workAction = DialogueWorkAction.WorkTogether,
                workCharacter = Grace, nextNode = original.entryNode };
            entry.choices = new[] { choice }; altered.entryNode = entry; config.events = new[] { altered };
            Assert.That(EventScheduler.ValidateCatalog(config).Any(i => i.Contains("invalid work command")), Is.True);
            UnityEngine.Object.DestroyImmediate(entry); UnityEngine.Object.DestroyImmediate(altered);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void ThirtyDayPoliciesCanJoinAcceptOrDeclineVolunteerWorkAndKeepValidRecovery(int policy)
        {
            int guard = 0, talked = 0;
            var rest = config.rooms.First().hotspots.First(h => h.activity != null && h.activity.staminaRecovery > 0).activity;
            while (!session.Days.CampaignEnded)
            {
                Assert.That(++guard, Is.LessThan(600));
                if (session.State.dayEnded) { Assert.That(session.TryStartNextDay(), Is.True); continue; }
                bool forced = session.Events.TryBeginForced(); var bubble = forced ? null : session.Events.BubbleEvents().FirstOrDefault();
                if (forced || bubble != null)
                {
                    if (!forced) Assert.That(session.Dialogue.TryBegin(bubble), Is.True);
                    bool volunteer = !string.IsNullOrEmpty(session.Dialogue.ActiveEvent.repeatGroup);
                    while (session.Dialogue.IsActive)
                    {
                        if (!session.Dialogue.ChoicesReady) { Assert.That(session.Dialogue.TryAdvance(), Is.True); continue; }
                        var choices = session.Dialogue.Node.choices;
                        var choice = volunteer && session.Dialogue.CanChoose(choices[policy]) ? choices[policy] : choices.First(session.Dialogue.CanChoose);
                        Assert.That(session.Dialogue.TryChoose(choice), Is.True);
                    }
                    Assert.That(session.CanRestore(session.State), Is.True); continue;
                }
                if (talked != session.State.day)
                {
                    foreach (var c in config.characters)
                    {
                        if (!session.Interactions.TryOpen(session.CurrentRoom.hotspots.First(h => h.character == c))) continue;
                        if (session.Interactions.TryTalk()) Finish();
                        session.Interactions.TryClose();
                    }
                    talked = session.State.day;
                }
                var work = session.Production; work.TryReview();
                foreach (var worker in config.production.workers)
                {
                    var active = work.Active(worker.character);
                    if (active != null && !work.IsReady(active))
                    {
                        var ready = work.ReadyTasks(worker.character).FirstOrDefault();
                        if (ready != null) work.TryPrioritize(ready.taskId);
                    }
                }
                var decision = session.State.production.tasks.FirstOrDefault(t => work.IsReleased(t) && work.Definition(t).decisionBlocked && !t.decisionCleared);
                bool acted = decision != null && work.TryAct(ProductionAction.Clarify, decision.taskId);
                var stage = work.ProducerStage;
                if (!acted && stage != null && (stage.startDay <= session.State.day + 3 || config.production.workers.Any(w => work.Active(w.character) == null)))
                    acted = work.TryAct(ProductionAction.Producer);
                if (!acted)
                {
                    var target = config.production.workers.OrderByDescending(w => work.Required(w.stream) - work.Completed(w.stream))
                        .FirstOrDefault(w => work.Preview(ProductionAction.Help, w.character.id).allowed);
                    if (target != null) acted = work.TryAct(ProductionAction.Help, target.character.id);
                }
                if (!acted && session.State.stamina < config.maxStamina) acted = session.TryCompleteActivity(rest, true);
                if (!acted) Assert.That(session.TryAdvancePhase(), Is.True);
                Assert.That(session.CanRestore(session.State), Is.True);
            }
            Assert.That(session.State.mainProgress, Is.EqualTo(100)); Assert.That(session.State.production.resolvedPhases, Is.EqualTo(120));
            Assert.That(session.State.production.producerStages.All(s => s.completedDay > 0), Is.True);
            Assert.That(session.State.production.reviews.Count, Is.EqualTo(5));
            if (policy == 2) Assert.That(session.State.production.sharedNights, Is.Empty);
            Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/Session032_30Day_Policy" + policy + ".json", JsonUtility.ToJson(session.State, true));
        }
    }
}
