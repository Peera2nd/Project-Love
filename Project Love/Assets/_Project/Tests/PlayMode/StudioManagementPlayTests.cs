using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjectLove.Tests
{
    public sealed class StudioManagementPlayTests : InputTestFixture
    {
        private IsometricRoomView view;
        private StudioSprintView studio;
        private string directory;
        private GameConfigSO isolatedConfig;
        [TearDown] public override void TearDown()
        { if (isolatedConfig != null) Object.Destroy(isolatedConfig); if (directory != null && Directory.Exists(directory)) Directory.Delete(directory, true); base.TearDown(); }
        private IEnumerator Open()
        {
            #if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/Gameplay/StudioSprintPrototype.unity",
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            #endif
            yield return null; yield return null; yield return null;
            view = Object.FindFirstObjectByType<IsometricRoomView>(); studio = view.GetComponent<StudioSprintView>(); view.TextSpeed = 0;
            directory = Path.GetFullPath(Path.Combine("Logs", "ManagementPlay", System.Guid.NewGuid().ToString("N"))); view.SetSaveDirectory(directory);
            isolatedConfig = Object.Instantiate(view.config); isolatedConfig.events = System.Array.Empty<EventDataSO>();
            view.config = isolatedConfig; view.Restart(); // Test management presentation independently of the event-content fixture.
            yield return null; yield return null;
        }
        private static Button Named(string name) => Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b => b.name == name && b.gameObject.activeInHierarchy);
        private Button Choice(int index) => view.choiceButtons.GetComponentsInChildren<Button>().Where(b => b.gameObject.activeInHierarchy).ElementAt(index);
        private IEnumerator Click(Button button)
        {
            var scroll = button.GetComponentInParent<ScrollRect>();
            if (scroll != null)
            {
                Canvas.ForceUpdateCanvases();
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.content, button.transform);
                float y = Mathf.Clamp(-scroll.viewport.rect.height * .5f - bounds.center.y, 0, Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height));
                scroll.StopMovement(); scroll.content.anchoredPosition = new Vector2(0, y); yield return null; yield return null;
            }
            yield return SandboxPlayTests.ClickWithMouse(button);
        }
        private void Capture(string suffix)
        { view.RevealAll(); IsometricPlayTests.AssertTextFits(); IsometricPlayTests.Capture(view, "Logs/Session027_" + suffix + ".png", !studio.IsOpen); }
        [UnityTest] public IEnumerator ProducerBoardHasNoWorkerBreakAndShowsWorkStageAndColoredCards()
        {
            yield return Open(); yield return Click(Named("Sprint Board"));
            Assert.That(studio.Board.GetComponentsInChildren<Button>().Any(b => b.name.StartsWith("Care ")), Is.False);
            Assert.That(Named("Producer Work").interactable, Is.True); Capture("Board");
            yield return Click(Named("Producer Work"));
            Assert.That(view.Session.State.money, Is.EqualTo(30)); Assert.That(view.Session.Production.ProducerCompleted, Is.EqualTo(10));
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Daytime));
            Assert.That(studio.Board.GetComponentsInChildren<Image>().Any(i => i.name == "Fill" && i.color == view.config.production.workers[0].stream.tint), Is.True);
            Capture("Producer"); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator CharacterRestOrdersStayInMenuAndOnlyResolveWhenPhasePasses()
        {
            yield return Open(); yield return Click(view.HotspotButtons["studio_grace"]); yield return Click(Choice(3));
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Morning)); Assert.That(view.Session.Interactions.IsOpen, Is.True);
            Assert.That(view.Session.State.FindCharacter("studio_grace").stress, Is.EqualTo(30)); Assert.That(Choice(0).interactable, Is.False);
            Capture("Rest"); yield return Click(view.deferButton); yield return Click(Named("Pass Phase"));
            Assert.That(view.Session.Production.Find("studio_w1_grace_1").effort, Is.Zero);
            Assert.That(view.Session.Production.Find("studio_w1_plaifah_1").effort, Is.EqualTo(4));
            Assert.That(view.Session.State.FindCharacter("studio_grace").stress, Is.EqualTo(25));
            Assert.That(view.Session.CanRestore(view.Session.State), Is.True); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator ThreeNightOvertimeOrdersAndContextTalkWorkWithoutPassingTime()
        {
            yield return Open(); for (int i = 0; i < 3; i++) yield return Click(Named("Pass Phase"));
            int work = view.Session.State.production.tasks.Sum(t => t.effort);
            foreach (var c in view.config.characters)
            {
                yield return Click(view.HotspotButtons[c.id]); yield return Click(Choice(3));
                Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Night)); Assert.That(Choice(3).interactable, Is.False);
                yield return Click(Choice(1)); Assert.That(view.Session.Dialogue.Node.id, Does.Contain("_ot_"));
                Capture("OT_" + c.id); yield return Click(view.DialogueTapButton); yield return Click(view.deferButton);
            }
            Assert.That(view.Session.State.production.orders.Count, Is.EqualTo(3)); Assert.That(view.Session.State.money, Is.EqualTo(5));
            Assert.That(view.Session.State.production.tasks.Sum(t => t.effort), Is.EqualTo(work));
            yield return Click(Named("Pass Phase")); Assert.That(view.Session.State.production.tasks.Sum(t => t.effort), Is.EqualTo(work + 9));
            Assert.That(view.Session.State.dayEnded, Is.True); Assert.That(view.Session.State.production.orders, Is.Empty);
            Assert.That(view.Session.CanRestore(view.Session.State), Is.True); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator ProducerUnlocksS2EarlyAndMouseReviewCreditsItsOwnGoal()
        {
            yield return Open(); yield return Click(Named("Sprint Board"));
            for (int i = 0; i < 4; i++) yield return Click(Named("Producer Work"));
            Assert.That(view.Session.State.day, Is.EqualTo(1)); Assert.That(view.Session.State.dayEnded, Is.True);
            Assert.That(view.Session.Production.StageUnlocked(view.config.production.sprints[1]), Is.True);
            yield return Click(Named("Close Board")); yield return Click(view.nextDayButton);
            yield return Click(Named("Sprint Board")); yield return Click(Named("Prioritize studio_w2_ploy_1")); yield return Click(Named("Close Board"));
            for (int i = 0; i < 3; i++)
            { yield return Click(view.HotspotButtons["studio_ploy"]); yield return Click(Choice(0)); }
            yield return Click(Named("Sprint Board")); yield return Click(Named("Review Ready"));
            var task = view.Session.Production.Find("studio_w2_ploy_1"); Assert.That(task.acceptedSprintId, Is.EqualTo("studio_sprint_2"));
            Assert.That(task.acceptedDay, Is.EqualTo(2)); Assert.That(view.Session.CanRestore(view.Session.State), Is.True);
            Capture("Early_S2"); LogAssert.NoUnexpectedReceived();
        }
    }
}
