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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjectLove.Tests
{
    public sealed class IsometricPlayTests : InputTestFixture
    {
        private string saveDirectory;
        [TearDown] public override void TearDown()
        {
            if (saveDirectory != null && Directory.Exists(saveDirectory)) Directory.Delete(saveDirectory, true);
            base.TearDown();
        }
        private static Button PanelButton(IsometricRoomView view, int index = 0) =>
            view.panelButtons.GetComponentsInChildren<Button>().Where(b => b.gameObject.activeInHierarchy).ElementAt(index);
        private static Button Choice(IsometricRoomView view) => view.choiceButtons.GetComponentsInChildren<Button>().First(b => b.gameObject.activeInHierarchy);
        private static IEnumerator Click(Button target, bool expect = true) => SandboxPlayTests.ClickWithMouse(target, expect);
        private IEnumerator Open()
        {
            #if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                "Assets/_Project/Scenes/Gameplay/IsometricPrototype.unity",
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            #endif
            yield return null; yield return null;
            var view = Object.FindFirstObjectByType<IsometricRoomView>(); Assert.That(view, Is.Not.Null);
            saveDirectory = Path.GetFullPath(Path.Combine("Logs", "RoomPlaySaves", System.Guid.NewGuid().ToString("N")));
            view.SetSaveDirectory(saveDirectory);
        }
        private static Button Choice(IsometricRoomView view, int index) => view.choiceButtons.GetComponentsInChildren<Button>()
            .Where(b => b.gameObject.activeInHierarchy).ElementAt(index);
        private static IEnumerator Tap(IsometricRoomView view)
        {
            view.RevealAll(); yield return Click(view.DialogueTapButton);
        }
        [UnityTest] public IEnumerator RoomConversationPhaseInventoryShopAndPersistenceWorkThroughUi()
        {
            yield return Open(); var view = Object.FindFirstObjectByType<IsometricRoomView>();
            string definitions = JsonUtility.ToJson(view.config.characters[0]);
            Assert.That(view.roomImage.sprite.rect.size, Is.EqualTo(new Vector2(1024, 1024)));
            var morningSprite = view.roomImage.sprite;
            var morningPosition = ((RectTransform)view.HotspotButtons["room_person"].transform).anchorMin;
            AssertTextFits(); Capture(view, "Logs/Session022_Room.png");
            yield return Click(view.HotspotButtons["room_person"]);
            Assert.That(view.dialogueModal.activeInHierarchy, Is.True); Assert.That(view.Session.Dialogue.IsActive, Is.False);
            Assert.That(view.Session.Interactions.IsOpen, Is.True); Assert.That(view.continueButton.gameObject.activeSelf, Is.False);
            Assert.That(((RectTransform)view.deferButton.transform).anchorMin.x, Is.GreaterThan(.9f));
            string before = JsonUtility.ToJson(view.Session.State);
            yield return Click(view.HotspotButtons["room_desk"], false);
            Assert.That(JsonUtility.ToJson(view.Session.State), Is.EqualTo(before));
            Assert.That(view.travelButton.interactable, Is.False); AssertTextFits();
            view.RevealAll(); Capture(view, "Logs/Session022_CharacterMenu.png");
            yield return Click(Choice(view, 1)); Assert.That(view.Session.Dialogue.IsActive, Is.True);
            Assert.That(view.Session.State.stamina, Is.EqualTo(97));
            yield return Click(view.deferButton);
            Assert.That(view.Session.State.stamina, Is.EqualTo(100)); Assert.That(view.Session.Interactions.RewardAvailable, Is.True);
            yield return Click(Choice(view, 1)); view.RevealAll(); Capture(view, "Logs/Session022_Talk.png");
            yield return Tap(view);
            Assert.That(view.Session.State.characters[0].affection, Is.EqualTo(2)); Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Morning));
            yield return Click(Choice(view, 1)); yield return Tap(view);
            Assert.That(view.Session.State.characters[0].affection, Is.EqualTo(2)); Assert.That(view.Session.State.stamina, Is.EqualTo(97));
            yield return Click(view.deferButton); Assert.That(view.dialogueModal.activeSelf, Is.False);
            yield return Click(view.HotspotButtons["room_desk"]); yield return Click(PanelButton(view));
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Daytime)); Assert.That(view.Session.State.stamina, Is.EqualTo(77));
            Assert.That(view.roomImage.sprite, Is.Not.EqualTo(morningSprite));
            Assert.That(((RectTransform)view.HotspotButtons["room_person"].transform).anchorMin, Is.Not.EqualTo(morningPosition));
            yield return Click(view.HotspotButtons["room_sofa"]); yield return Click(PanelButton(view));
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Evening)); Assert.That(view.Session.State.stamina, Is.EqualTo(100));
            yield return Click(view.HotspotButtons["room_pickup_water"]);
            Assert.That(view.Session.ItemCount(view.config.items[0]), Is.EqualTo(1));
            yield return Click(view.travelButton); yield return Click(PanelButton(view, 1));
            yield return Click(view.HotspotButtons["room_clerk"]); AssertTextFits();
            yield return Click(PanelButton(view)); Assert.That(view.Session.ItemCount(view.config.items[0]), Is.EqualTo(2));
            Assert.That(view.Session.State.money, Is.EqualTo(20));
            yield return Click(view.travelButton); yield return Click(PanelButton(view));
            yield return Click(view.optionsButton); yield return Click(PanelButton(view));
            yield return Click(view.HotspotButtons["room_desk"]); yield return Click(PanelButton(view));
            yield return Click(view.HotspotButtons["room_desk"]); yield return Click(PanelButton(view));
            Assert.That(view.Session.State.dayEnded, Is.True); Assert.That(view.Session.State.stamina, Is.EqualTo(60));
            Assert.That(view.roomImage.sprite.name, Does.Contain("LateNight"));
            yield return Click(view.optionsButton); yield return Click(PanelButton(view, 1));
            Assert.That(view.Session.State.dayEnded, Is.False); Assert.That(view.Session.State.stamina, Is.EqualTo(100));
            yield return Click(view.optionsButton); yield return Click(PanelButton(view, 2));
            Assert.That(view.Session.State.dayEnded, Is.True); Assert.That(view.Session.State.stamina, Is.EqualTo(60));
            yield return Click(view.nextDayButton); Assert.That(view.Session.State.day, Is.EqualTo(2));
            Assert.That(view.Session.State.stamina, Is.EqualTo(100)); Assert.That(view.HotspotButtons.ContainsKey("room_pickup_water"), Is.False);
            yield return Click(view.HotspotButtons["room_person"]); Assert.That(view.Session.Interactions.RewardAvailable, Is.True);
            yield return Click(view.deferButton);
            yield return Click(view.HotspotButtons["room_desk"]); yield return Click(PanelButton(view));
            yield return Click(view.inventoryButton); yield return Click(PanelButton(view));
            Assert.That(view.Session.State.stamina, Is.EqualTo(95)); Assert.That(view.Session.ItemCount(view.config.items[0]), Is.EqualTo(1));
            AssertTextFits(); Assert.That(JsonUtility.ToJson(view.config.characters[0]), Is.EqualTo(definitions)); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator ZeroEnergyCanBackOutAndRestWhileRepeatTalkSurvivesSaveAndRefreshesTomorrow()
        {
            yield return Open(); var view = Object.FindFirstObjectByType<IsometricRoomView>();
            view.Session.State.stamina = 0; view.Refresh(); yield return null;
            yield return Click(view.HotspotButtons["room_person"]); Assert.That(Choice(view, 1).interactable, Is.False);
            yield return Click(view.deferButton); yield return Click(view.HotspotButtons["room_sofa"]); yield return Click(PanelButton(view));
            Assert.That(view.Session.State.stamina, Is.EqualTo(30));
            yield return Click(view.HotspotButtons["room_person"]); yield return Click(Choice(view, 1)); yield return Tap(view);
            Assert.That(view.Session.State.stamina, Is.EqualTo(27)); Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Daytime));
            yield return Click(view.deferButton); Assert.That(view.Save(SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            view.Restart(); Assert.That(view.Load(SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            view.Session.State.stamina = 0; view.Refresh(); yield return null;
            yield return Click(view.HotspotButtons["room_person"]); Assert.That(view.Session.Interactions.RewardAvailable, Is.False);
            Assert.That(Choice(view, 1).interactable, Is.True); yield return Click(Choice(view, 1)); yield return Tap(view);
            Assert.That(view.Session.State.stamina, Is.Zero); Assert.That(view.Session.State.characters[0].affection, Is.EqualTo(2));
            yield return Click(view.deferButton); yield return Click(view.waitButton); yield return Click(view.nextDayButton);
            yield return Click(view.HotspotButtons["room_person"]); Assert.That(view.Session.Interactions.RewardAvailable, Is.True);
            AssertTextFits(); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator CharacterWorkGiftInventoryAndContextSelectionWorkThroughRealClicks()
        {
            yield return Open(); var view = Object.FindFirstObjectByType<IsometricRoomView>();
            yield return Click(view.HotspotButtons["room_person"]); yield return Click(Choice(view, 0));
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Daytime));
            Assert.That(view.Session.WorkCompleted(view.config.workStreams.Single(s => s.id == "work_art")), Is.EqualTo(6));
            Assert.That(view.Session.WorkCompleted(view.config.workStreams.Single(s => s.id == "work_producer")), Is.Zero);
            yield return Click(view.HotspotButtons["room_pickup_water"]);
            yield return Click(view.travelButton); yield return Click(PanelButton(view, 1));
            yield return Click(view.HotspotButtons["room_clerk"]); yield return Click(PanelButton(view, 1));
            yield return Click(PanelButton(view, 2));
            yield return Click(view.travelButton); yield return Click(PanelButton(view));
            yield return Click(view.HotspotButtons["room_person"]); yield return Click(Choice(view, 2));
            AssertTextFits(); view.RevealAll(); Capture(view, "Logs/Session022_Gifts.png");
            var pudding = view.config.items.Single(i => i.id == "room_pudding");
            yield return Click(Choice(view, 1)); Assert.That(view.Session.ItemCount(pudding), Is.EqualTo(1));
            yield return Click(view.deferButton); Assert.That(view.Session.ItemCount(pudding), Is.EqualTo(1));
            yield return Click(Choice(view, 2)); yield return Click(Choice(view, 1)); yield return Tap(view);
            Assert.That(view.Session.ItemCount(pudding), Is.Zero); Assert.That(view.Session.State.characters[0].affection, Is.EqualTo(5));
            yield return Click(Choice(view, 2)); yield return Click(Choice(view, 0)); yield return Tap(view);
            Assert.That(view.Session.State.characters[0].affection, Is.EqualTo(6));
            yield return Click(Choice(view, 2)); yield return Click(Choice(view, 0)); yield return Tap(view);
            Assert.That(view.Session.State.characters[0].affection, Is.EqualTo(4)); Assert.That(view.Session.State.stamina, Is.EqualTo(80));
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Daytime));
            yield return Click(Choice(view, 2)); Assert.That(view.choiceButtons.GetComponentsInChildren<Button>().Length, Is.Zero);
            yield return Click(view.deferButton); yield return Click(view.deferButton);
            view.Session.State.day = 2; view.Session.State.phase = DayPhase.Evening;
            view.Session.State.flags.Add("prototype_shared_task_done"); view.Refresh(); yield return null;
            view.TextSpeed = 1;
            yield return Click(view.HotspotButtons["room_person"]); yield return Click(Choice(view, 1));
            Assert.That(view.dialogueText.text, Does.Contain("เรื่องงานที่เราช่วยกันแก้"));
            Assert.That(view.IsRevealing, Is.True);
            yield return Click(view.DialogueTapButton);
            Assert.That(view.IsRevealing, Is.False); Assert.That(view.Session.Dialogue.IsActive, Is.True);
            AssertTextFits(); Capture(view, "Logs/Session022_ContextTalk.png");
            yield return Click(view.DialogueTapButton);
            Assert.That(view.Session.Dialogue.IsActive, Is.False); Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Evening));
            LogAssert.NoUnexpectedReceived();
        }
        internal static void AssertTextFits()
        {
            Canvas.ForceUpdateCanvases();
            foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                text.ForceMeshUpdate();
                Assert.That(text.isTextOverflowing, Is.False, text.name + " | " + text.text + " | " + text.rectTransform.rect.size);
                string printable = new string(text.text.Where(c => !char.IsControl(c)).ToArray());
                Assert.That(text.font.HasCharacters(printable, out uint[] missing, true, true), Is.True,
                    text.name + ": missing " + string.Join(",", missing ?? new uint[0]));
            }
        }
        internal static void Capture(IsometricRoomView view, string path, bool checkBackdrop = true, int width = 1920, int height = 1080)
        {
            Directory.CreateDirectory("Logs");
            var canvas = view.GetComponent<Canvas>(); var camera = Camera.main; var rt = new RenderTexture(width, height, 24); rt.Create();
            var oldMode = canvas.renderMode; var oldCamera = canvas.worldCamera; var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            Texture2D image = null;
            try
            {
                camera.targetTexture = rt; canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = rt });
                RenderTexture.active = rt; image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
                if (checkBackdrop && !view.Session.Dialogue.IsActive)
                { var corner = image.GetPixel(4, 4); Assert.That(corner.maxColorComponent, Is.LessThan(.02f), "Outer backdrop must be black."); }
                File.WriteAllBytes(path, image.EncodeToPNG()); Assert.That(File.Exists(path), Is.True);
                var arrow = view.deferButton.GetComponentInChildren<BackArrowGraphic>();
                if (arrow != null && arrow.isActiveAndEnabled && width == 1920 && height == 1080)
                {
                    Assert.That(arrow.canvasRenderer.GetMesh().vertexCount, Is.EqualTo(12), "Back arrow mesh");
                    int whitePixels = 0;
                    for (int y = 34; y < 116; y++) for (int x = 1768; x < 1880; x++)
                    {
                        var pixel = image.GetPixel(x, y);
                        if (pixel.r > .75f && pixel.g > .75f && pixel.b > .75f) whitePixels++;
                    }
                    Assert.That(whitePixels, Is.GreaterThan(20), "Back arrow must visibly render");
                }
            }
            finally
            {
                canvas.renderMode = oldMode; canvas.worldCamera = oldCamera; camera.targetTexture = oldTarget; RenderTexture.active = oldActive;
                if (image != null) Object.Destroy(image); rt.Release(); Object.Destroy(rt);
            }
        }
    }
}
