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
    public sealed class StudioUiDependencyPlayTests : InputTestFixture
    {
        private IsometricRoomView view;
        private StudioSprintView studio;
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
            view = Object.FindFirstObjectByType<IsometricRoomView>(); studio = view.GetComponent<StudioSprintView>(); view.TextSpeed = 0;
            directory = Path.GetFullPath(Path.Combine("Logs", "UiDependency", System.Guid.NewGuid().ToString("N"))); view.SetSaveDirectory(directory);
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
        { view.RevealAll(); IsometricPlayTests.Capture(view, "Logs/Session031_" + suffix + ".png", !studio.IsOpen); IsometricPlayTests.AssertTextFits(); }
        [UnityTest] public IEnumerator SuppliedSkinShowsExactHudWorkerForecastSharedBarsAndOwnerAvatars()
        {
            yield return Open(); Assert.That(view.presentation.useStudioSkin, Is.True);
            Assert.That(view.hudText.gameObject.activeInHierarchy, Is.False);
            Assert.That(Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Count(i => i.sprite == view.presentation.Icon("stamina")), Is.GreaterThan(0));
            Capture("Room"); IsometricPlayTests.Capture(view, "Logs/Session031_Room_1280.png", true, 1280, 720); yield return Click(view.HotspotButtons["studio_grace"]);
            Assert.That(Choice(0).transform.Find("Action Icon").GetComponent<Image>().sprite, Is.SameAs(view.presentation.Icon("work")));
            var forecast = view.dialogueModal.GetComponentsInChildren<RectTransform>().Single(r => r.name == "Task Forecast");
            var segment = forecast.GetComponentsInChildren<RectTransform>().Single(r => r.name == "Forecast Clip");
            Assert.That(segment.anchorMin.x, Is.Zero); Assert.That(segment.anchorMax.x, Is.EqualTo(10f / 28).Within(.0001f));
            var image = segment.GetComponentInChildren<Image>(); Assert.That(image.sprite, Is.SameAs(view.presentation.barForecast));
            Assert.That(image.material, Is.SameAs(Graphic.defaultGraphicMaterial)); Capture("Worker");
            yield return Click(Choice(0)); Assert.That(view.Session.State.stamina, Is.EqualTo(80));
            var stamina = Object.FindObjectsByType<RectTransform>(FindObjectsSortMode.None).Single(r => r.name == "Stamina");
            Assert.That(stamina.GetComponentsInChildren<TMP_Text>().Single().text, Does.Contain("80/100"));
            yield return Click(Named("Sprint Board"));
            var avatar = studio.Board.GetComponentsInChildren<Image>().First(i => i.name == "Owner Avatar");
            Assert.That(view.config.characters.Any(c => c.taskAvatar == avatar.sprite), Is.True);
            Assert.That(studio.Board.GetComponentsInChildren<Image>().Where(i => i.name == "Fill").All(i => i.sprite == view.presentation.barFill), Is.True);
            Capture("Board"); IsometricPlayTests.Capture(view, "Logs/Session031_Board_1280.png", false, 1280, 720); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator InspectorFollowsDesignArtCodeAndSwitchesFromBlockedCardToReadyWork()
        {
            yield return Open(); yield return Click(Named("Sprint Board")); yield return Click(Named("Prioritize studio_w1_grace_2"));
            Assert.That(view.Session.Production.Active(view.config.characters.First(c => c.id == "studio_grace")).taskId, Is.EqualTo("studio_w1_grace_2"));
            yield return Click(Named("Inspect studio_w1_plaifah_2")); Capture("ArtDependency");
            Assert.That(studio.FocusedTaskId, Is.EqualTo("studio_w1_plaifah_2"));
            yield return Click(Named("Dependency Before studio_w1_ploy_1")); Assert.That(studio.FocusedTaskId, Is.EqualTo("studio_w1_ploy_1"));
            yield return Click(Named("Dependency After studio_w1_plaifah_2")); yield return Click(Named("Dependency After studio_w1_grace_2"));
            Assert.That(studio.FocusedTaskId, Is.EqualTo("studio_w1_grace_2")); Capture("CodeDependency");
            yield return Click(Named("Dependency Ready studio_w1_grace_1")); yield return Click(Named("Prioritize studio_w1_grace_1"));
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Morning)); Assert.That(view.Session.State.stamina, Is.EqualTo(100));
            yield return Click(Named("Close Board")); yield return Click(view.HotspotButtons["studio_grace"]);
            Assert.That(Choice(0).interactable, Is.True); yield return Click(Choice(0));
            Assert.That(view.Session.Production.Find("studio_w1_grace_1").effort, Is.EqualTo(10));
            Assert.That(view.Session.Production.Find("studio_w1_grace_2").effort, Is.Zero); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator ReviewUnlocksArtAndRestBadgeIsVisibleWithoutReopeningConversation()
        {
            yield return Open(); var work = view.Session.Production; var brief = work.Find("studio_w1_ploy_1");
            brief.effort = work.Definition(brief).effort; view.Refresh(); yield return null; yield return null;
            Assert.That(work.IsReady(work.Find("studio_w1_plaifah_2")), Is.False);
            yield return Click(Named("Sprint Board")); yield return Click(Named("Inspect studio_w1_plaifah_2"));
            Assert.That(Named("Dependency Before studio_w1_ploy_1").GetComponentInChildren<TMP_Text>().text, Does.Contain(view.presentation.Get("dependencyReview")));
            yield return Click(Named("Review Ready")); Assert.That(work.IsReady(work.Find("studio_w1_plaifah_2")), Is.True);
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Morning)); yield return Click(Named("Close Board"));
            yield return Click(view.HotspotButtons["studio_plaifah"]); yield return Click(Choice(3)); yield return Click(view.deferButton);
            var badge = Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Single(i => i.name == "Order Badge studio_plaifah");
            Assert.That(badge.sprite, Is.SameAs(view.presentation.Icon("rest"))); Capture("RestBadge");
            Assert.That(view.Session.CanRestore(view.Session.State), Is.True); LogAssert.NoUnexpectedReceived();
        }
    }
}
