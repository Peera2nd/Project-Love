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
    public sealed class StudioEventPlayTests : InputTestFixture
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
            directory = Path.GetFullPath(Path.Combine("Logs", "EventPlay", System.Guid.NewGuid().ToString("N"))); view.SetSaveDirectory(directory);
        }
        private static IEnumerator Click(Button button) => SandboxPlayTests.ClickWithMouse(button);
        private Button Choice(int index) => view.choiceButtons.GetComponentsInChildren<Button>().Where(b => b.gameObject.activeInHierarchy).ElementAt(index);
        private void Capture(string suffix)
        { view.RevealAll(); IsometricPlayTests.AssertTextFits(); IsometricPlayTests.Capture(view, "Logs/Session030_" + suffix + ".png", true); }
        private IEnumerator DayTwo()
        { yield return Click(view.waitButton); yield return Click(view.nextDayButton); }
        [UnityTest] public IEnumerator ForcedStartsAtSafePointAndBackSnoozesToClickableCharacterBubble()
        {
            yield return Open(); yield return DayTwo();
            Assert.That(view.Session.Dialogue.ActiveEvent.id, Is.EqualTo("studio_event_brief"));
            Assert.That(view.travelButton.interactable, Is.False); Assert.That(view.Save(SaveSlot.Manual), Is.EqualTo(SaveResult.Busy));
            Capture("Forced"); yield return Click(view.deferButton); yield return null;
            Assert.That(view.Session.Dialogue.IsActive, Is.False); Assert.That(view.Session.Events.HasForcedPending, Is.False);
            Assert.That(view.EventBubbleButtons.Count, Is.EqualTo(3)); Capture("Bubbles");
            Assert.That(view.Save(SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            yield return null; yield return null;
            yield return Click(view.EventBubbleButtons["studio_event_brief"]); yield return Click(view.DialogueTapButton);
            Assert.That(view.Session.State.completedEvents.Contains("studio_event_brief"), Is.True);
            Assert.That(view.Load(SaveSlot.Manual), Is.EqualTo(SaveResult.Success)); yield return null;
            Assert.That(view.Session.Dialogue.IsActive, Is.False); Assert.That(view.EventBubbleButtons.Count, Is.EqualTo(3));
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator BubbleChoicesUpdateRealSprintBoardAndBackRollsEverythingBack()
        {
            yield return Open(); yield return DayTwo(); yield return Click(view.DialogueTapButton);
            Assert.That(view.EventBubbleButtons.Count, Is.EqualTo(2)); string before = JsonUtility.ToJson(view.Session.State);
            yield return Click(view.EventBubbleButtons["studio_event_bug"]); yield return Click(view.DialogueTapButton);
            Capture("BugChoice"); yield return Click(Choice(0));
            Assert.That(view.Session.State.production.events.Count, Is.EqualTo(1)); yield return Click(view.deferButton);
            Assert.That(JsonUtility.ToJson(view.Session.State), Is.EqualTo(before));
            yield return Click(view.EventBubbleButtons["studio_event_bug"]); yield return Click(view.DialogueTapButton); yield return Click(Choice(0));
            var status = view.dialogueModal.GetComponentsInChildren<TMPro.TMP_Text>().Single(t => t.name == "Worker Status");
            Assert.That(status.text, Does.Contain("104")); Assert.That(status.gameObject.activeInHierarchy, Is.True);
            yield return Click(view.DialogueTapButton);
            Assert.That(view.Session.State.production.events.Count, Is.EqualTo(1)); Assert.That(view.EventBubbleButtons.ContainsKey("studio_event_bug"), Is.False);
            var stream = view.config.workStreams.Single(w => w.id == "work_code"); Assert.That(view.Session.Production.Required(stream), Is.EqualTo(104));
            var boardButton = Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b => b.name == "Sprint Board" && b.gameObject.activeInHierarchy);
            yield return Click(boardButton);
            Assert.That(view.GetComponent<StudioSprintView>().Board.GetComponentsInChildren<RectTransform>().Any(r => r.name == "Card studio_bug"), Is.True);
            view.RevealAll(); IsometricPlayTests.AssertTextFits(); IsometricPlayTests.Capture(view, "Logs/Session030_BugBoard.png", false);
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator SeparateCharacterBubblePaysAffectionOnceWithoutConsumingTime()
        {
            yield return Open(); yield return DayTwo(); yield return Click(view.DialogueTapButton);
            int affection = view.Session.State.FindCharacter("studio_plaifah").affection;
            yield return Click(view.EventBubbleButtons["studio_event_art_check"]); yield return Click(view.DialogueTapButton); yield return Click(Choice(0));
            yield return Click(view.DialogueTapButton);
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Morning));
            Assert.That(view.Session.State.FindCharacter("studio_plaifah").affection, Is.EqualTo(affection + 1));
            Assert.That(view.EventBubbleButtons.ContainsKey("studio_event_art_check"), Is.False);
            Assert.That(view.EventBubbleButtons.ContainsKey("studio_event_bug"), Is.True); Capture("AfterEvent");
            Assert.That(view.Session.CanRestore(view.Session.State), Is.True); LogAssert.NoUnexpectedReceived();
        }
    }
}
