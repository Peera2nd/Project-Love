using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectLove.UI;
using ProjectLove.Core;
using ProjectLove.Data;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Utilities;

namespace ProjectLove.Tests
{
    public sealed class SandboxPlayTests : InputTestFixture
    {
        private readonly List<string> saveDirectories = new List<string>();
        private void IsolateSaves(SandboxView view)
        {
            string directory = Path.GetFullPath(Path.Combine("Logs", "PlaySaves", System.Guid.NewGuid().ToString("N")));
            saveDirectories.Add(directory);
            view.SetSaveDirectory(directory);
        }
        [TearDown]
        public override void TearDown()
        {
            foreach (string directory in saveDirectories) if (Directory.Exists(directory)) Directory.Delete(directory, true);
            saveDirectories.Clear();
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator PlayThroughChoicesActivitiesAndDayEndWithoutMutatingAssets()
        {
            #if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                "Assets/_Project/Scenes/Sandbox/FoundationSandbox.unity",
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            #endif
            yield return null;
            yield return null;
            var view = Object.FindFirstObjectByType<SandboxView>();
            Assert.That(view, Is.Not.Null);
            IsolateSaves(view);
            string configBefore = JsonUtility.ToJson(view.config);
            string characterBefore = JsonUtility.ToJson(view.config.characters[0]);
            var node = view.events[0].entryNode;
            string dialogueBefore = JsonUtility.ToJson(node);
            var eventButton = view.eventButtons.GetComponentsInChildren<Button>().Single();
            yield return ClickWithMouse(eventButton);
            yield return null;
            Assert.That(view.Session.Dialogue.IsActive, Is.True);
            Assert.That(view.IsRevealing, Is.True);
            view.continueButton.onClick.Invoke();
            Assert.That(view.IsRevealing, Is.False);
            Assert.That(view.dialogueText.text, Is.EqualTo(node.lines[0].text));
            Assert.That(view.dialogueText.textInfo.characterCount, Is.GreaterThan(0));
            view.continueButton.onClick.Invoke();
            yield return null;
            view.RevealAll();
            Assert.That(view.dialogueText.text, Does.Contain("<color="));
            Assert.That(view.dialogueText.maxVisibleCharacters, Is.EqualTo(int.MaxValue));
            view.continueButton.onClick.Invoke();
            yield return null;
            var choices = view.choiceButtons.GetComponentsInChildren<Button>().Where(b => b.gameObject.activeInHierarchy).ToArray();
            Assert.That(choices, Has.Length.EqualTo(2));
            choices[0].onClick.Invoke();
            yield return null;
            view.RevealAll();
            view.continueButton.onClick.Invoke();
            yield return null;
            Assert.That(view.Session.Dialogue.IsActive, Is.False);
            Assert.That(view.Session.State.actionPoints, Is.EqualTo(3));
            Assert.That(view.Session.State.characters[0].affection, Is.EqualTo(8));
            Assert.That(view.Session.State.characters[0].stress, Is.EqualTo(30));

            view.activityButtons.GetComponentsInChildren<Button>().Where(b => b.gameObject.activeInHierarchy).ElementAt(1).onClick.Invoke();
            yield return null;
            Assert.That(view.Session.State.mainProgress, Is.EqualTo(2));
            view.activityButtons.GetComponentsInChildren<Button>().First(b => b.gameObject.activeInHierarchy).onClick.Invoke();
            yield return null;
            Assert.That(view.Session.State.mainProgress, Is.EqualTo(17));
            Assert.That(view.Session.State.actionPoints, Is.EqualTo(1));
            view.endDayButton.onClick.Invoke();
            yield return null;
            Assert.That(view.Session.State.dayEnded, Is.True);
            Assert.That(view.nextDayButton.gameObject.activeSelf, Is.True);
            view.nextDayButton.onClick.Invoke();
            yield return null;
            Assert.That(view.Session.State.day, Is.EqualTo(2));
            Assert.That(view.Session.State.actionPoints, Is.EqualTo(4));
            Assert.That(JsonUtility.ToJson(view.config), Is.EqualTo(configBefore));
            Assert.That(JsonUtility.ToJson(view.config.characters[0]), Is.EqualTo(characterBefore));
            Assert.That(JsonUtility.ToJson(node), Is.EqualTo(dialogueBefore));
            view.restartButton.onClick.Invoke();
            yield return null;
            Assert.That(view.Session.State.day, Is.EqualTo(1));
            Assert.That(view.Session.State.characters[0].affection, Is.Zero);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ThaiFontAndLayoutHaveNoMissingGlyphsOrOverflow()
        {
            #if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                "Assets/_Project/Scenes/Sandbox/FoundationSandbox.unity",
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            #endif
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            var overflow = new List<string>();
            foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                text.ForceMeshUpdate();
                string printable = new string(text.text.Where(c => !char.IsControl(c)).ToArray());
                Assert.That(text.font.HasCharacters(printable, out uint[] missing, true, true), Is.True,
                    text.name + ": missing " + string.Join(",", missing ?? new uint[0]));
                if (text.isTextOverflowing) overflow.Add(text.transform.parent.name + "/" + text.name + " " + text.rectTransform.rect.size);
            }
            Directory.CreateDirectory("Logs");
            CapturePreview();
            Assert.That(File.Exists("Logs/FoundationSandbox.png"), Is.True);
            Assert.That(overflow, Is.Empty, string.Join("; ", overflow));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ZeroApOldSceneProvidesClickableDeferralAndNextDayRecovery()
        {
            #if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                "Assets/_Project/Scenes/Sandbox/FoundationSandbox.unity",
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            #endif
            yield return null; yield return null;
            var view = Object.FindFirstObjectByType<SandboxView>();
            IsolateSaves(view);
            // Exhaust the budget without consuming all phases, then verify the zero-AP fallback.
            view.Session.ActionPoints.TrySpend(view.config.maxActionPointsPerDay);
            Assert.That(view.Session.State.actionPoints, Is.Zero);
            view.eventButtons.GetComponentsInChildren<Button>().Single().onClick.Invoke();
            while (view.Session.Dialogue.CurrentLine != null) { view.RevealAll(); view.Continue(); yield return null; }
            Assert.That(view.Session.Dialogue.HasAvailableChoice, Is.False);
            Assert.That(view.continueButton.gameObject.activeInHierarchy, Is.True);
            Assert.That(view.continueButton.interactable, Is.True);
            Assert.That(view.dialogueText.text, Is.EqualTo(view.presentation.blockedText));
            Assert.That(view.continueButton.GetComponentInChildren<TMP_Text>().text, Is.EqualTo(view.presentation.deferLabel));
            Canvas.ForceUpdateCanvases();
            AssertTextFits();
            CapturePreview("Logs/ZeroApDeferral.png");
            yield return ClickWithMouse(view.continueButton);
            Assert.That(view.Session.Dialogue.IsActive, Is.False);
            Assert.That(view.Session.State.completedEvents, Is.Empty);
            Assert.That(view.endDayButton.interactable, Is.True);
            view.endDayButton.onClick.Invoke(); view.nextDayButton.onClick.Invoke();
            yield return null;
            Assert.That(view.Session.State.day, Is.EqualTo(2));
            Assert.That(view.Session.State.actionPoints, Is.EqualTo(view.config.maxActionPointsPerDay));
            Assert.That(view.eventButtons.GetComponentsInChildren<Button>().Single().interactable, Is.True);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ThreeDayPrototypeSupportsCatchupRecoveryPersistenceAndDaySummary()
        {
            #if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                "Assets/_Project/Scenes/Sandbox/ThreeDayPrototype.unity",
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            #endif
            yield return null; yield return null;
            var view = Object.FindFirstObjectByType<SandboxView>();
            IsolateSaves(view);
            Assert.That(view.config.totalDays, Is.EqualTo(30));
            string definitions = JsonUtility.ToJson(view.config) + JsonUtility.ToJson(view.config.characters[0]);
            Canvas.ForceUpdateCanvases(); AssertTextFits();
            CapturePreview("Logs/ThreeDayPrototype_Start.png");
            yield return ClickWithMouse(view.eventButtons.GetComponentsInChildren<Button>().Single());
            view.RevealAll(); view.Continue(); yield return null;
            Canvas.ForceUpdateCanvases(); AssertTextFits();
            view.choiceButtons.GetComponentsInChildren<Button>().First(b => b.gameObject.activeInHierarchy).onClick.Invoke();
            yield return null;
            Assert.That(view.Session.State.flags, Does.Contain("prototype_supported"));
            view.activityButtons.GetComponentsInChildren<Button>().Where(b => b.gameObject.activeInHierarchy).ElementAt(1).onClick.Invoke();
            yield return null;
            Assert.That(view.Session.State.mainProgress, Is.EqualTo(2));
            Assert.That(view.Save(SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            int savedAp = view.Session.State.actionPoints;
            view.activityButtons.GetComponentsInChildren<Button>().First(b => b.gameObject.activeInHierarchy).onClick.Invoke();
            yield return null;
            Assert.That(view.Session.State.mainProgress, Is.EqualTo(17));
            yield return ClickWithMouse(view.loadButton);
            Assert.That(view.Session.State.mainProgress, Is.EqualTo(2));
            Assert.That(view.Session.State.actionPoints, Is.EqualTo(savedAp));
            Assert.That(view.Session.State.completedEvents, Does.Contain("prototype_intro_event"));
            view.endDayButton.onClick.Invoke(); view.nextDayButton.onClick.Invoke(); yield return null;
            Assert.That(view.Session.State.day, Is.EqualTo(2));
            // Skip the preferred Day 2 events; their authored window still permits Day 3.
            view.endDayButton.onClick.Invoke(); view.nextDayButton.onClick.Invoke(); yield return null;
            Assert.That(view.Session.State.day, Is.EqualTo(3));
            view.activityButtons.GetComponentsInChildren<Button>().First(b => b.gameObject.activeInHierarchy).onClick.Invoke(); yield return null;
            Assert.That(view.Session.State.mainProgress, Is.EqualTo(17));
            Assert.That(view.Session.State.phase, Is.EqualTo(DayPhase.Daytime));
            Assert.That(view.Scheduler.Available().Select(e => e.id), Is.EqualTo(new[] { "prototype_recovery_event", "prototype_followup_event" }));
            view.eventButtons.GetComponentsInChildren<Button>().First(b => b.gameObject.activeInHierarchy).onClick.Invoke();
            view.RevealAll(); view.Continue(); yield return null;
            Canvas.ForceUpdateCanvases(); AssertTextFits();
            view.choiceButtons.GetComponentsInChildren<Button>().First(b => b.gameObject.activeInHierarchy).onClick.Invoke(); yield return null;
            Assert.That(view.Session.State.flags, Does.Contain("prototype_recovered"));
            view.eventButtons.GetComponentsInChildren<Button>().First(b => b.gameObject.activeInHierarchy).onClick.Invoke();
            view.RevealAll(); view.Continue(); yield return null;
            Canvas.ForceUpdateCanvases(); AssertTextFits();
            view.choiceButtons.GetComponentsInChildren<Button>().First(b => b.gameObject.activeInHierarchy).onClick.Invoke(); yield return null;
            Assert.That(view.Session.State.completedEvents.Count, Is.EqualTo(3));
            Assert.That(view.Save(SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            view.Restart();
            Assert.That(view.Load(SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            Assert.That(view.Session.State.completedEvents.Count, Is.EqualTo(3));
            Assert.That(view.Scheduler.Available(), Is.Empty);
            view.phaseButton.onClick.Invoke(); yield return null;
            view.eventButtons.GetComponentsInChildren<Button>().Single().onClick.Invoke();
            view.RevealAll(); view.Continue(); yield return null;
            view.endDayButton.onClick.Invoke(); yield return null;
            Assert.That(view.summaryText.text, Is.EqualTo(view.presentation.prototypeEndedText));
            Assert.That(view.nextDayButton.gameObject.activeSelf, Is.False);
            Assert.That(view.Load(SaveSlot.Auto), Is.EqualTo(SaveResult.Success));
            Assert.That(view.Session.State.dayEnded, Is.True);
            Assert.That(JsonUtility.ToJson(view.config) + JsonUtility.ToJson(view.config.characters[0]), Is.EqualTo(definitions));
            Canvas.ForceUpdateCanvases();
            AssertTextFits();
            CapturePreview("Logs/ThreeDayPrototype.png");
            LogAssert.NoUnexpectedReceived();
        }

        private static void AssertTextFits()
        {
            foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                text.ForceMeshUpdate();
                Assert.That(text.isTextOverflowing, Is.False, text.transform.parent.name + "/" + text.name + " | " + text.text + " | " + text.rectTransform.rect.size + " | " + text.fontSize);
                string printable = new string(text.text.Where(c => !char.IsControl(c)).ToArray());
                Assert.That(text.font.HasCharacters(printable, out uint[] missing, true, true), Is.True,
                    text.name + ": missing " + string.Join(",", missing ?? new uint[0]));
            }
        }

        internal static IEnumerator ClickWithMouse(Button target, bool expectTarget = true)
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            var previousBehavior = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            #if UNITY_EDITOR
            var previousEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            #endif
            InputSystemUIInputModule module = null;
            ReadOnlyArray<InputDevice>? previousDevices = null;
            try
            {
                Canvas.ForceUpdateCanvases();
                var system = EventSystem.current;
                Assert.That(system, Is.Not.Null);
                module = system.currentInputModule as InputSystemUIInputModule;
                Assert.That(module, Is.Not.Null);
                previousDevices = module.actionsAsset.devices;
                module.actionsAsset.devices = new InputDevice[] { mouse };
                Assert.That(module.point.action.enabled, Is.True);
                Assert.That(module.leftClick.action.enabled, Is.True);
                var rect = (RectTransform)target.transform;
                Vector2 position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
                var graphic = target.GetComponent<Image>();
                if (graphic != null && graphic.alphaHitTestMinimumThreshold > 0 && !graphic.IsRaycastLocationValid(position, null))
                {
                    bool found = false;
                    for (int y = 1; y < 10 && !found; y++)
                        for (int x = 1; x < 10 && !found; x++)
                        {
                            Vector2 candidate = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(new Vector2(
                                Mathf.Lerp(rect.rect.xMin, rect.rect.xMax, x / 10f), Mathf.Lerp(rect.rect.yMin, rect.rect.yMax, y / 10f))));
                            if (graphic.IsRaycastLocationValid(candidate, null)) { position = candidate; found = true; }
                        }
                    Assert.That(found, Is.True, "No opaque click point: " + target.name);
                }
                var hits = new List<RaycastResult>();
                system.RaycastAll(new PointerEventData(system) { position = position }, hits);
                Assert.That(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == target, Is.EqualTo(expectTarget),
                    "Target " + target.name + " | active=" + target.gameObject.activeInHierarchy + " depth=" + graphic.depth + " cull=" + graphic.canvasRenderer.cull +
                    " | Room=" + (Object.FindFirstObjectByType<IsometricRoomView>() != null ? JsonUtility.ToJson(Object.FindFirstObjectByType<IsometricRoomView>().Session.State) : "none") +
                    " | Raycast " + position + ": " + string.Join(",", hits.Select(hit => hit.gameObject.name)));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
                InputSystem.Update();
                yield return null;
                yield return null;
                Assert.That(mouse.position.ReadValue(), Is.EqualTo(position), "Simulated mouse did not update.");
                Assert.That(module.point.action.ReadValue<Vector2>(), Is.EqualTo(position), "UI point action did not bind to the test device.");
                InputSystem.QueueStateEvent(mouse, new MouseState { position = position }.WithButton(MouseButton.Left));
                InputSystem.Update();
                yield return null;
                yield return null;
                InputSystem.QueueStateEvent(mouse, new MouseState { position = position });
                InputSystem.Update();
                yield return null;
                yield return null;
                yield return null;
            }
            finally
            {
                InputSystem.settings.backgroundBehavior = previousBehavior;
                #if UNITY_EDITOR
                InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorBehavior;
                #endif
                if (module != null) module.actionsAsset.devices = previousDevices;
                InputSystem.RemoveDevice(mouse);
            }
        }

        private static void CapturePreview(string path = "Logs/FoundationSandbox.png")
        {
            var view = Object.FindFirstObjectByType<SandboxView>();
            var canvas = view.GetComponent<Canvas>();
            var camera = Camera.main;
            var target = new RenderTexture(1920, 1080, 24);
            target.Create();
            var originalMode = canvas.renderMode;
            var originalCamera = canvas.worldCamera;
            var originalTarget = camera.targetTexture;
            var originalActive = RenderTexture.active;
            Texture2D image = null;
            try
            {
                camera.targetTexture = target;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1;
                Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture.active = target;
                image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
                Assert.That(File.Exists(path), Is.True);
            }
            finally
            {
                canvas.renderMode = originalMode;
                canvas.worldCamera = originalCamera;
                camera.targetTexture = originalTarget;
                RenderTexture.active = originalActive;
                if (image != null) Object.Destroy(image);
                target.Release();
                Object.Destroy(target);
            }
        }
    }
}
