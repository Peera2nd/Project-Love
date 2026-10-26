using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjectLove.Tests
{
    public sealed class SharedNightPlayTests : InputTestFixture
    {
        private IsometricRoomView view;
        private string directory;
        [TearDown] public override void TearDown()
        { if (directory != null && Directory.Exists(directory)) Directory.Delete(directory, true); base.TearDown(); }
        private IEnumerator Open()
        {
            #if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/Gameplay/StudioSprintPrototype.unity",
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            #endif
            yield return null; yield return null; yield return null;
            view = Object.FindFirstObjectByType<IsometricRoomView>(); view.TextSpeed = 0;
            directory = Path.GetFullPath(Path.Combine("Logs", "NightPlay", System.Guid.NewGuid().ToString("N"))); view.SetSaveDirectory(directory);
            // Prepare Day 5 through domain boundaries; actual new UI flow below uses mouse raycasts.
            var session = view.Session; var grace = view.config.characters.Single(c => c.id == "studio_grace");
            while (session.State.day < 5 || session.State.phase != DayPhase.Night)
            {
                if (session.State.dayEnded) { session.TryStartNextDay(); continue; }
                if (session.Events.TryBeginForced())
                {
                    while (session.Dialogue.IsActive) session.Dialogue.TryAdvance();
                    continue;
                }
                var regular = session.Events.BubbleEvents().FirstOrDefault(e => !e.repeatDaily);
                if (regular != null)
                {
                    Assert.That(session.Dialogue.TryBegin(regular), Is.True);
                    while (session.Dialogue.IsActive)
                        if (session.Dialogue.ChoicesReady) Assert.That(session.Dialogue.TryChoose(session.Dialogue.Node.choices.First(session.Dialogue.CanChoose)), Is.True);
                        else Assert.That(session.Dialogue.TryAdvance(), Is.True);
                    continue;
                }
                session.Production.TryReview(); var ready = session.Production.ReadyTasks(grace).FirstOrDefault();
                if (ready != null && session.State.day == 5 && session.State.phase == DayPhase.Evening)
                    session.Production.TryPrioritize(ready.taskId);
                Assert.That(session.TryAdvancePhase(), Is.True);
            }
            view.Refresh(); yield return null; yield return null;
        }
        private Button Choice(int index) => view.choiceButtons.GetComponentsInChildren<Button>().Where(b => b.gameObject.activeInHierarchy).ElementAt(index);
        private IEnumerator Click(Button button)
        {
            var scroll = button.GetComponentInParent<ScrollRect>();
            if (scroll != null)
            {
                Canvas.ForceUpdateCanvases(); var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.content, button.transform);
                scroll.StopMovement(); scroll.content.anchoredPosition = new Vector2(0,
                    Mathf.Clamp(-scroll.viewport.rect.height * .5f - bounds.center.y, 0, Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height)));
                yield return null; yield return null;
            }
            yield return SandboxPlayTests.ClickWithMouse(button);
        }
        private void Capture(string name)
        {
            view.RevealAll(); IsometricPlayTests.Capture(view, "Logs/Session032_" + name + ".png", true);
            IsometricPlayTests.AssertTextFits();
        }
        private string OfferId => view.Session.Events.BubbleEvents().Single(e => e.repeatGroup == "studio_grace_volunteer").id;
        [UnityTest] public IEnumerator VolunteerBubbleSharedWorkBackAndMorningCallbackUseRealMouseAndOneNight()
        {
            yield return Open(); string id = OfferId; string before = JsonUtility.ToJson(view.Session.State);
            Capture("NightBubbles"); yield return Click(view.EventBubbleButtons[id]); yield return Click(view.DialogueTapButton);
            Canvas.ForceUpdateCanvases();
            var viewport = Choice(0).GetComponentInParent<ScrollRect>().viewport;
            var frame = new Vector3[4]; viewport.GetWorldCorners(frame);
            foreach (var button in view.choiceButtons.GetComponentsInChildren<Button>().Where(b => b.gameObject.activeInHierarchy))
            {
                var corners = new Vector3[4]; ((RectTransform)button.transform).GetWorldCorners(corners);
                Assert.That(corners[0].y, Is.GreaterThanOrEqualTo(frame[0].y - .1f), "All three options must fit without scrolling");
                Assert.That(corners[2].y, Is.LessThanOrEqualTo(frame[2].y + .1f));
            }
            Capture("VolunteerChoice"); IsometricPlayTests.Capture(view, "Logs/Session032_VolunteerChoice_1280.png", true, 1280, 720);
            var choice = view.Session.Dialogue.Node.choices[0]; var forecast = view.Session.Production.PreviewDialogueWork(choice);
            Assert.That(Choice(0).GetComponentInChildren<TMPro.TMP_Text>().text, Does.Contain("+" + forecast.help));
            yield return Click(Choice(0)); Assert.That(view.Session.State.dayEnded, Is.True);
            Assert.That(view.Session.State.production.sharedNights.Count, Is.EqualTo(1)); Capture("SharedNight");
            yield return Click(view.deferButton); Assert.That(JsonUtility.ToJson(view.Session.State), Is.EqualTo(before));
            yield return Click(view.EventBubbleButtons[id]); yield return Click(view.DialogueTapButton); yield return Click(Choice(0));
            yield return Click(view.DialogueTapButton); Assert.That(view.Session.Dialogue.IsActive, Is.False);
            Assert.That(view.Session.State.production.resolvedPhases, Is.EqualTo(20));
            Assert.That(view.Save(SaveSlot.Manual), Is.EqualTo(SaveResult.Success)); yield return null; yield return null;
            yield return Click(view.nextDayButton);
            var callback = view.Session.Events.BubbleEvents().Single(e => e.exclusiveGroup == "studio_grace_night_callback");
            yield return Click(view.EventBubbleButtons[callback.id]); Capture("MorningCallback");
            yield return Click(view.DialogueTapButton); Assert.That(view.Session.State.completedEvents, Does.Contain(callback.id));
            Assert.That(view.Session.CanRestore(view.Session.State), Is.True); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator AcceptedVolunteerChangesNormalTalkAndCharacterWorkCanJoinOrderedNight()
        {
            yield return Open(); int money = view.Session.State.money;
            yield return Click(view.EventBubbleButtons[OfferId]); yield return Click(view.DialogueTapButton); yield return Click(Choice(1));
            yield return Click(view.DialogueTapButton); Assert.That(view.Session.State.dayEnded, Is.False);
            Assert.That(view.Session.State.production.sharedNights, Is.Empty);
            yield return Click(view.HotspotButtons["studio_grace"]); Capture("OrderedNightHelp");
            yield return Click(Choice(1)); Assert.That(view.Session.Dialogue.Node.id, Does.Contain("_ot_"));
            yield return Click(view.DialogueTapButton); int affection = view.Session.State.FindCharacter("studio_grace").affection;
            yield return Click(Choice(0)); Assert.That(view.Session.State.dayEnded, Is.True);
            Assert.That(view.Session.State.money, Is.EqualTo(money - view.config.production.overtimeMoneyCost));
            Assert.That(view.Session.State.FindCharacter("studio_grace").affection, Is.EqualTo(affection + view.config.production.sharedNightAffection));
            Assert.That(view.Session.State.production.sharedNights.Count, Is.EqualTo(1)); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator LowEnergyDisablesJoiningButDeclineRemainsFreeAndDoesNotConsumeNight()
        {
            yield return Open(); view.Session.State.stamina = 0; view.Refresh(); yield return null; yield return null;
            int money = view.Session.State.money, phases = view.Session.State.production.resolvedPhases;
            yield return Click(view.EventBubbleButtons[OfferId]); yield return Click(view.DialogueTapButton);
            Assert.That(Choice(0).interactable, Is.False); Assert.That(Choice(1).interactable, Is.True); Assert.That(Choice(2).interactable, Is.True);
            Capture("LowEnergyChoice"); yield return Click(Choice(2)); yield return Click(view.DialogueTapButton);
            Assert.That(view.Session.State.production.resolvedPhases, Is.EqualTo(phases)); Assert.That(view.Session.State.money, Is.EqualTo(money));
            Assert.That(view.Session.State.dayEnded, Is.False); Assert.That(view.Session.Events.BubbleEvents().Any(e => e.repeatGroup == "studio_grace_volunteer"), Is.False);
            Assert.That(view.Session.CanRestore(view.Session.State), Is.True); LogAssert.NoUnexpectedReceived();
        }
    }
}
