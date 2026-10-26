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
    public sealed class StudioPlayTests : InputTestFixture
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
            view = Object.FindFirstObjectByType<IsometricRoomView>(); studio = view.GetComponent<StudioSprintView>();
            directory = Path.GetFullPath(Path.Combine("Logs", "StudioPlaySaves", System.Guid.NewGuid().ToString("N"))); view.SetSaveDirectory(directory);
            isolatedConfig = Object.Instantiate(view.config); isolatedConfig.events = System.Array.Empty<EventDataSO>();
            view.config = isolatedConfig; view.Restart(); // Test management presentation independently of the event-content fixture.
            yield return null; yield return null;
            view.TextSpeed = 0;
        }
        private static IEnumerator Click(Button button) => SandboxPlayTests.ClickWithMouse(button);
        private static Button Named(string name) => Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b => b.name == name && b.gameObject.activeInHierarchy);
        private Button Choice(int index) => view.choiceButtons.GetComponentsInChildren<Button>().Where(b => b.gameObject.activeInHierarchy).ElementAt(index);
        private Button Panel(int index) => view.panelButtons.GetComponentsInChildren<Button>().Where(b => b.gameObject.activeInHierarchy).ElementAt(index);
        private IEnumerator BoardClick(string name)
        {
            var button = Named(name); var scroll = button.GetComponentInParent<ScrollRect>();
            if (scroll != null)
            {
                Canvas.ForceUpdateCanvases();
                var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.content, button.transform);
                float y = Mathf.Clamp(-scroll.viewport.rect.height * .5f - bounds.center.y, 0, Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height));
                scroll.StopMovement(); scroll.content.anchoredPosition = new Vector2(0, y);
                yield return null; yield return null;
            }
            yield return Click(button);
        }
        private void Capture(string suffix)
        { IsometricPlayTests.AssertTextFits(); IsometricPlayTests.Capture(view, "Logs/Session026_" + suffix + ".png", !studio.IsOpen); }
        [UnityTest] public IEnumerator ThreeWorkersShowStatusAndAccurateHelpThroughRealMouse()
        {
            yield return Open(); Assert.That(view.config.characters.Length, Is.EqualTo(3));
            foreach (var h in view.Session.CurrentRoom.hotspots.Where(h => h.kind == RoomInteractionKind.Character))
                Assert.That(h.placements.All(p => p.sprite != null), Is.True, h.id + " must have art in every phase");
            Capture("Room");
            string source = JsonUtility.ToJson(view.config.production);
            foreach (var character in view.config.characters)
            {
                yield return Click(view.HotspotButtons[character.id]);
                Assert.That(view.Session.Interactions.Character, Is.SameAs(character));
                var status = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Single(t => t.name == "Worker Status");
                Assert.That(status.text, Does.Contain("เครียด 30/100")); Assert.That(status.text, Does.Contain("ช่วย +6 / ทีม +4"));
                view.RevealAll(); Capture("VN_" + character.id);
                Assert.That(view.dialoguePortrait.sprite, Is.SameAs(character.GetSprite(CharacterExpression.Normal)));
                Assert.That(view.dialoguePortrait.sprite, Is.Not.Null); Assert.That(character.GetSprite(CharacterExpression.Smile), Is.Not.Null);
                yield return Click(view.deferButton);
            }
            yield return Click(view.HotspotButtons["studio_grace"]); yield return Click(Choice(0));
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Daytime)); Assert.That(view.Session.State.stamina, Is.EqualTo(80));
            Assert.That(view.Session.Production.Find("studio_w1_grace_1").effort, Is.EqualTo(10));
            Assert.That(view.Session.Production.Find("studio_w1_plaifah_1").effort, Is.EqualTo(4));
            Assert.That(view.Session.State.FindCharacter("studio_grace").stress, Is.EqualTo(29));
            Assert.That(view.panelText.text, Does.Contain("ช่วย +6 / ทีม +4")); Capture("After_Help");
            Assert.That(view.Session.CanRestore(view.Session.State), Is.True);
            Assert.That(JsonUtility.ToJson(view.config.production), Is.EqualTo(source)); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator BoardBlocksWorldAndProducerReorderReviewAndBugWorkThroughMouse()
        {
            yield return Open(); yield return Click(Named("Sprint Board"));
            Assert.That(studio.IsOpen, Is.True); Assert.That(view.Session.IsBusy, Is.True);
            Assert.That(view.Session.TryEndDay(), Is.False); Assert.That(view.Save(SaveSlot.Manual), Is.EqualTo(SaveResult.Busy));
            Capture("Board");
            yield return BoardClick("Coordinate"); Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Daytime));
            Assert.That(view.Session.State.stamina, Is.EqualTo(85)); Assert.That(view.Session.State.production.coordinationTicks, Is.EqualTo(1));
            Assert.That(view.Session.Production.Find("studio_w1_grace_1").effort, Is.EqualTo(5));
            yield return BoardClick("Clarify studio_w1_grace_2");
            Assert.That(view.Session.Production.Find("studio_w1_grace_2").decisionCleared, Is.True);
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Evening));
            int cursor = view.Session.State.production.resolvedPhases;
            yield return BoardClick("Prioritize studio_w1_grace_3");
            Assert.That(view.Session.Production.Active(view.config.characters[0]).taskId, Is.EqualTo("studio_w1_grace_3"));
            Assert.That(view.Session.State.production.resolvedPhases, Is.EqualTo(cursor));
            yield return BoardClick("Review Ready"); Assert.That(view.Session.State.production.resolvedPhases, Is.EqualTo(cursor));
            yield return BoardClick("Event studio_bug_demo");
            var code = view.config.workStreams.First(s => s.id == "work_code");
            Assert.That(view.Session.Production.Required(code), Is.EqualTo(104)); Capture("Board_Bug");
            yield return Click(Named("Close Board")); yield return Click(Named("Pass Phase"));
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Night));
            yield return Click(view.HotspotButtons["studio_ploy"]); Assert.That(Choice(0).interactable, Is.False);
            Assert.That(view.Session.Interactions.CanTalk, Is.True); view.RevealAll(); Capture("Night");
            yield return Click(view.deferButton); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator DailyConversationGiftCancellationAndSaveRemainIndependentOfProduction()
        {
            yield return Open(); yield return Click(view.HotspotButtons["room_pickup_water"]);
            yield return Click(view.HotspotButtons["studio_grace"]); yield return Click(Choice(1));
            yield return Click(view.DialogueTapButton);
            Assert.That(view.Session.State.stamina, Is.EqualTo(97)); Assert.That(view.Session.State.FindCharacter("studio_grace").affection, Is.EqualTo(2));
            yield return Click(Choice(1)); yield return Click(view.DialogueTapButton);
            Assert.That(view.Session.State.stamina, Is.EqualTo(97));
            yield return Click(Choice(2)); yield return Click(Choice(0));
            yield return Click(view.deferButton); Assert.That(view.Session.State.inventory.Single().quantity, Is.EqualTo(1));
            yield return Click(Choice(2)); yield return Click(Choice(0)); yield return Click(view.DialogueTapButton);
            Assert.That(view.Session.State.inventory.Count, Is.Zero); Assert.That(view.Session.State.FindCharacter("studio_grace").affection, Is.EqualTo(6));
            Assert.That(view.Session.State.production.resolvedPhases, Is.Zero); yield return Click(view.deferButton);
            yield return Click(view.optionsButton); yield return Click(Panel(0));
            string before = JsonUtility.ToJson(view.Session.State);
            yield return Click(Named("Pass Phase")); yield return Click(view.optionsButton); yield return Click(Panel(1));
            Assert.That(JsonUtility.ToJson(view.Session.State), Is.EqualTo(before)); Assert.That(view.Session.CanRestore(view.Session.State), Is.True);
            yield return Click(view.waitButton); yield return Click(view.nextDayButton);
            yield return Click(view.HotspotButtons["studio_grace"]); Assert.That(view.Session.Interactions.RewardAvailable, Is.True);
            Capture("Day2"); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator MissedSprintCarriesCardsAndCanContinueNextWeekThroughMouse()
        {
            yield return Open();
            for (int day = 1; day <= 7; day++)
            {
                yield return Click(view.waitButton); Assert.That(view.Session.State.dayEnded, Is.True);
                yield return Click(view.nextDayButton);
            }
            Assert.That(view.Session.State.day, Is.EqualTo(8)); var result = view.Session.State.production.reviews.Single();
            Assert.That(result.quotas.Any(q => q.completed < q.required), Is.True); Assert.That(result.carryoverCount, Is.GreaterThan(0));
            Assert.That(view.Session.CanRestore(view.Session.State), Is.True);
            yield return Click(Named("Sprint Board")); Capture("Review_Day8");
            yield return Click(Named("Close Board")); yield return Click(view.optionsButton); yield return Click(Panel(0));
            yield return Click(Named("Pass Phase")); yield return Click(view.optionsButton); yield return Click(Panel(1));
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Morning)); Assert.That(view.Session.State.production.reviews.Count, Is.EqualTo(1));
            LogAssert.NoUnexpectedReceived();
        }
    }
}
