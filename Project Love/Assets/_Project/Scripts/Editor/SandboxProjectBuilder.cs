using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectLove.Data;
using ProjectLove.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace ProjectLove.EditorTools
{
    public static class SandboxProjectBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Sandbox/FoundationSandbox.unity";
        public const string ConfigPath = "Assets/_Project/Data/Settings/SandboxCampaign.asset";
        private const string Root = "Assets/_Project/";
        private static TMP_FontAsset font;
        private static readonly HashSet<IdentifiedDataSO> created = new HashSet<IdentifiedDataSO>();

        [Serializable] private sealed class ConfigSeed
        {
            public string id, mainProgressLabel;
            public int totalDays, maxActionPointsPerDay, startingMoney, startingSkillPoints, startingProgress, burnoutThreshold;
        }
        [Serializable] private sealed class CharacterSeed
        {
            public string id, displayName, roleTitle;
            public int startingAffection, startingStress;
        }
        [Serializable] private sealed class PresentationSeed
        {
            public string id, title, subtitle, dayFormat, statsFormat, characterFormat, narratorLabel, readyText,
                continueLabel, nextPhaseLabel, endDayLabel, nextDayLabel, restartLabel, successLabel, failureLabel,
                activityFormat, unavailableLabel, dayEndedFormat, campaignEndedText, deferLabel, blockedText,
                saveLabel, loadLabel, loadAutoLabel, savedText, loadedText, saveFailedText, loadFailedText, busyText, prototypeEndedText;
            public string[] phaseNames;
            public float charactersPerSecond;
        }
        [Serializable] private sealed class LineSeed { public string lineId, text; public CharacterExpression expression; }
        [Serializable] private sealed class EffectSeed { public StatType stat; public int amount; }
        [Serializable] private sealed class ChoiceSeed
        {
            public string choiceId, text, nextNodeId;
            public int actionPointCost;
            public EffectSeed[] effects;
            public FlagChange[] flags;
            public Condition[] conditions;
        }
        [Serializable] private sealed class NodeSeed { public string id, nextNodeId; public LineSeed[] lines; public ChoiceSeed[] choices; }
        [Serializable] private sealed class EventSeed
        {
            public string id, displayName, entryNodeId;
            public int startDay, endDay, preferredDay, priority, elapsedPhases;
            public bool repeatDaily, anyPhase;
            public Condition[] conditions;
            public DayPhase triggerPhase;
        }
        [Serializable] private sealed class ActivitySeed
        {
            public string id, displayName;
            public int actionPointCost;
            public int elapsedPhases;
            public EffectSeed[] successEffects, failureEffects;
            public FlagChange[] successFlags, failureFlags;
        }
        [Serializable] private sealed class Seed
        {
            public ConfigSeed config;
            public CharacterSeed character;
            public PresentationSeed presentation;
            public NodeSeed[] nodes;
            public EventSeed[] events;
            public ActivitySeed[] activities;
        }

        [MenuItem("Tools/Project Love/Create or Open Foundation Sandbox")]
        public static void BuildOrOpen() => Open(false);

        public const string PrototypeScenePath = "Assets/_Project/Scenes/Sandbox/ThreeDayPrototype.unity";
        public const string PrototypeConfigPath = "Assets/_Project/Data/Settings/PrototypeCampaign.asset";
        [MenuItem("Tools/Project Love/Create or Open Three Day Prototype")]
        public static void BuildOrOpenPrototype() => Open(true);

        private static void Open(bool prototype)
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!Application.isBatchMode && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            {
                EditorUtility.DisplayDialog("Project Love", "กรุณาบันทึกฉากที่เปิดอยู่ก่อนสร้าง Sandbox", "ตกลง");
                return;
            }
            if (prototype) BuildPrototypeFromCommandLine(); else BuildFromCommandLine();
            if (!Application.isBatchMode) EditorSceneManager.OpenScene(prototype ? PrototypeScenePath : ScenePath);
        }

        public static void BuildFromCommandLine() => Generate("SandboxSeed.json", ScenePath, ConfigPath,
            "SandboxCharacter.asset", "SandboxPresentation.asset", 0);

        public static void BuildPrototypeFromCommandLine() => Generate("ThreeDaySeed.json", PrototypeScenePath,
            PrototypeConfigPath, "PrototypeCharacter.asset", "PrototypePresentation.asset", 3);

        private static void Generate(string seedFile, string scenePath, string configPath,
            string characterFile, string presentationFile, int prototypeDays)
        {
            if (File.Exists(scenePath)) return; // Never overwrite an authored scene or its data on repeated clicks.
            created.Clear();
            string json = File.ReadAllText(Root + "Data/Settings/" + seedFile);
            var seed = JsonUtility.FromJson<Seed>(json);
            EnsureFolders();
            CreateTextSettings();
            font = CreateFont(json);
            var character = Create<CharacterDataSO>(Root + "Data/Characters/" + characterFile, JsonUtility.ToJson(seed.character));
            var config = Create<GameConfigSO>(configPath, JsonUtility.ToJson(seed.config));
            if (created.Contains(config)) config.characters = new[] { character };
            var presentation = Create<SandboxPresentationSO>(Root + "Data/Settings/" + presentationFile, JsonUtility.ToJson(seed.presentation));
            if (created.Contains(presentation)) presentation.font = font;
            var nodes = new Dictionary<string, DialogueNodeSO>();
            foreach (var data in seed.nodes)
            {
                var node = Create<DialogueNodeSO>(Root + "Data/Dialogues/" + data.id + ".asset", "{}");
                if (created.Contains(node)) node.id = data.id;
                nodes.Add(data.id, node);
            }
            foreach (var data in seed.nodes)
            {
                var node = nodes[data.id];
                if (!created.Contains(node)) continue;
                node.nextNode = string.IsNullOrEmpty(data.nextNodeId) ? null : nodes[data.nextNodeId];
                node.lines = data.lines.Select(line => new DialogueLine
                { lineId = line.lineId, text = line.text, expression = line.expression, speaker = character }).ToArray();
                node.choices = data.choices.Select(choice => new DialogueChoice
                {
                    choiceId = choice.choiceId, text = choice.text, actionPointCost = choice.actionPointCost,
                    effects = Effects(choice.effects, character), flags = choice.flags ?? new FlagChange[0],
                    conditions = choice.conditions ?? new Condition[0],
                    nextNode = string.IsNullOrEmpty(choice.nextNodeId) ? null : nodes[choice.nextNodeId]
                }).ToArray();
                EditorUtility.SetDirty(node);
            }
            var events = seed.events.Select(data =>
            {
                var result = Create<EventDataSO>(Root + "Data/Events/" + data.id + ".asset", JsonUtility.ToJson(data));
                if (created.Contains(result))
                {
                    result.entryNode = nodes[data.entryNodeId];
                    result.conditions = data.conditions ?? new Condition[0];
                }
                EditorUtility.SetDirty(result);
                return result;
            }).ToArray();
            var activities = seed.activities.Select(data =>
            {
                var result = Create<ActivityDataSO>(Root + "Data/Settings/" + data.id + ".asset", "{}");
                if (!created.Contains(result)) return result;
                result.id = data.id;
                result.displayName = data.displayName;
                result.actionPointCost = data.actionPointCost;
                result.elapsedPhases = data.elapsedPhases;
                result.successEffects = Effects(data.successEffects, character);
                result.failureEffects = Effects(data.failureEffects, character);
                result.successFlags = data.successFlags ?? new FlagChange[0];
                result.failureFlags = data.failureFlags ?? new FlagChange[0];
                EditorUtility.SetDirty(result);
                return result;
            }).ToArray();
            EditorUtility.SetDirty(config);
            EditorUtility.SetDirty(presentation);
            AssetDatabase.SaveAssets();
            var issues = FoundationValidator.ValidateProject();
            if (issues.Count > 0) throw new InvalidOperationException(string.Join("\n", issues));
            BuildScene(config, presentation, events, activities, scenePath, prototypeDays);
            AssetDatabase.SaveAssets();
            File.WriteAllText("Logs/" + (prototypeDays > 0 ? "PrototypeBuild" : "FoundationBuild") + ".json", "{\"status\":\"passed\",\"scene\":\"" + scenePath + "\"}");
        }

        private static StatModifier[] Effects(EffectSeed[] effects, CharacterDataSO character) =>
            (effects ?? new EffectSeed[0]).Select(effect => new StatModifier
            { stat = effect.stat, amount = effect.amount, character = effect.stat == StatType.Affection || effect.stat == StatType.Stress ? character : null }).ToArray();

        private static T Create<T>(string path, string json) where T : IdentifiedDataSO
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null && !string.IsNullOrWhiteSpace(existing.id)) return existing;
            var asset = existing != null ? existing : ScriptableObject.CreateInstance<T>();
            JsonUtility.FromJsonOverwrite(json, asset);
            if (existing == null) AssetDatabase.CreateAsset(asset, path);
            created.Add(asset);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void EnsureFolders()
        {
            foreach (string path in new[] { "Data/Settings", "Data/Characters", "Data/Dialogues", "Data/Events", "Prefabs/UI", "Scenes/Sandbox", "Resources/Fonts/Sarabun" })
                Directory.CreateDirectory(Root + path);
            Directory.CreateDirectory("Logs");
            AssetDatabase.Refresh();
        }

        private static void CreateTextSettings()
        {
            if (Resources.Load<TMP_Settings>("TMP Settings") != null) return;
            var settings = ScriptableObject.CreateInstance<TMP_Settings>();
            var serialized = new SerializedObject(settings);
            serialized.FindProperty("assetVersion").stringValue = "2";
            serialized.FindProperty("m_defaultFontSize").floatValue = 28;
            serialized.FindProperty("m_defaultFontAssetPath").stringValue = "Fonts/";
            var features = serialized.FindProperty("m_ActiveFontFeatures");
            features.arraySize = 3;
            features.GetArrayElementAtIndex(0).intValue = (int)OTL_FeatureTag.kern;
            features.GetArrayElementAtIndex(1).intValue = (int)OTL_FeatureTag.mark;
            features.GetArrayElementAtIndex(2).intValue = (int)OTL_FeatureTag.mkmk;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(settings, Root + "Resources/TMP Settings.asset");
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private static TMP_FontAsset CreateFont(string source)
        {
            string path = Root + "Resources/Fonts/Sarabun/Sarabun SDF.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (existing != null) return existing;
            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "Resources/Fonts/Sarabun/Sarabun-Regular.ttf");
            if (sourceFont == null) throw new InvalidOperationException("Sarabun source font was not imported.");
            if (Shader.Find("TextMeshPro/Mobile/Distance Field") == null)
                throw new InvalidOperationException("TMP Essential Resources SDF shader is missing.");
            var result = TMP_FontAsset.CreateFontAsset(sourceFont, 64, 8, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
            result.name = "Sarabun SDF";
            AssetDatabase.CreateAsset(result, path);
            if (!result.TryAddCharacters(new string(source.Where(c => !char.IsControl(c)).Distinct().ToArray()), out string missing))
                throw new InvalidOperationException("Font is missing seed characters: " + missing);
            AssetDatabase.AddObjectToAsset(result.material, result);
            foreach (var atlas in result.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, result);
            TMP_Settings.defaultFontAsset = result;
            EditorUtility.SetDirty(Resources.Load<TMP_Settings>("TMP Settings"));
            EditorUtility.SetDirty(result);
            AssetDatabase.SaveAssets();
            return result;
        }

        private static void BuildScene(GameConfigSO config, SandboxPresentationSO presentation, EventDataSO[] events, ActivityDataSO[] activities, string scenePath, int prototypeDays)
        {
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                Application.isBatchMode ? NewSceneMode.Single : NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var camera = new GameObject("Sandbox Camera").AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.055f, .07f, .10f);
                camera.orthographic = true;
                camera.tag = "MainCamera";
                var canvasObject = new GameObject("Sandbox UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = .5f;
                var view = canvasObject.AddComponent<SandboxView>();
                view.config = config; view.presentation = presentation; view.events = events; view.activities = activities;
                view.prototypeDays = prototypeDays;
                var eventSystem = new GameObject("Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
                eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

                var header = Panel("Header", canvas.transform, .03f, .87f, .97f, .98f);
                view.titleText = Text("Title", header, .02f, .48f, .65f, 1, 36);
                view.subtitleText = Text("Subtitle", header, .02f, 0, .67f, .5f, 24);
                view.dayText = Text("Day and AP", header, .70f, .05f, .98f, .95f, 29);
                var status = Panel("Status", canvas.transform, .03f, .79f, .97f, .85f);
                view.statsText = Text("Stats", status, .02f, .04f, .64f, .96f, 27);
                view.summaryText = Text("Day Summary", status, .65f, 0, .98f, 1, 21);
                var character = Panel("Character Placeholder", canvas.transform, .03f, prototypeDays > 0 ? .42f : .39f, .33f, .77f);
                var portrait = Panel("Portrait", character, .13f, .43f, .87f, .96f);
                view.portrait = portrait.GetComponent<Image>();
                view.portrait.preserveAspect = true;
                view.characterText = Text("Character Stats", character, .06f, .04f, .94f, .41f, 29);
                var activitiesPanel = Panel("Activities", canvas.transform, .36f, prototypeDays > 0 ? .58f : .49f, .97f, .77f);
                view.activityButtons = List("Activity Buttons", activitiesPanel, .03f, .05f, .97f, .95f, 55);
                var eventsPanel = Panel("Events", canvas.transform, .36f, prototypeDays > 0 ? .42f : .39f, .97f, prototypeDays > 0 ? .56f : .47f);
                view.eventButtons = List("Event Buttons", eventsPanel, .03f, .08f, .97f, .92f, 55);
                var dialogue = Panel("Dialogue", canvas.transform, .03f, prototypeDays > 0 ? .15f : .10f, .97f, prototypeDays > 0 ? .40f : .37f);
                view.speakerText = Text("Speaker", dialogue, .025f, .80f, .97f, .98f, 29);
                view.dialogueText = Text("Dialogue Text", dialogue, .025f, prototypeDays > 0 ? .40f : .36f, .97f, .80f, 30);
                view.choiceButtons = List("Choice Buttons", dialogue, .025f, .025f, .75f, prototypeDays > 0 ? .37f : .34f, 40);
                view.continueButton = Button("Continue", dialogue, .77f, .045f, .975f, .27f);
                var controls = Panel("Day Controls", canvas.transform, .03f, .02f, .97f, .08f);
                view.phaseButton = Button("Next Phase", controls, .005f, .08f, .24f, .92f);
                view.endDayButton = Button("End Day", controls, .255f, .08f, .49f, .92f);
                view.nextDayButton = Button("Next Day", controls, .505f, .08f, .74f, .92f);
                view.restartButton = Button("Restart", controls, .755f, .08f, .99f, .92f);

                if (prototypeDays > 0)
                {
                    var saves = Panel("Save Controls", canvas.transform, .03f, .09f, .97f, .14f);
                    view.saveButton = Button("Save", saves, .005f, .08f, .23f, .92f);
                    view.loadButton = Button("Load Manual", saves, .245f, .08f, .47f, .92f);
                    view.loadAutoButton = Button("Load Auto", saves, .485f, .08f, .71f, .92f);
                    view.feedbackText = Text("Save Feedback", saves, .73f, .02f, .985f, .98f, 20);
                }
                var template = Button("Button Template", canvas.transform, 0, 0, .5f, .05f);
                template.gameObject.SetActive(false);
                string prefabPath = Root + "Prefabs/UI/SandboxButton.prefab";
                if (!File.Exists(prefabPath)) PrefabUtility.SaveAsPrefabAsset(template.gameObject, prefabPath);
                view.buttonTemplate = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath).GetComponent<Button>();
                UnityEngine.Object.DestroyImmediate(template.gameObject);
                EditorSceneManager.SaveScene(scene, scenePath);
            }
            finally
            {
                if (!Application.isBatchMode)
                {
                    EditorSceneManager.CloseScene(scene, true);
                    if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                }
            }
        }

        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static RectTransform Panel(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(.10f, .135f, .17f, 1);
            return rect;
        }

        private static TMP_Text Text(string name, Transform parent, float x0, float y0, float x1, float y1, float size)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontFeatures = new List<OTL_FeatureTag> { OTL_FeatureTag.kern, OTL_FeatureTag.mark, OTL_FeatureTag.mkmk };
            text.fontSize = size;
            text.color = new Color(.90f, .94f, .96f);
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            return text;
        }

        private static Button Button(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = Panel(name, parent, x0, y0, x1, y1);
            rect.GetComponent<Image>().color = new Color(.19f, .30f, .34f);
            var button = rect.gameObject.AddComponent<Button>();
            rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            var colors = button.colors;
            colors.highlightedColor = new Color(.68f, .9f, .88f);
            colors.disabledColor = new Color(.35f, .38f, .42f);
            button.colors = colors;
            var text = Text("Label", rect, .025f, .05f, .975f, .95f, 22);
            text.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static RectTransform List(string name, Transform parent, float x0, float y0, float x1, float y1, float height)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1);
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6;
            layout.childControlHeight = true; layout.childControlWidth = true;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            // Preferred height is supplied by each cloned button's LayoutElement.
            var element = rect.gameObject.AddComponent<LayoutElement>();
            element.minHeight = height;
            return rect;
        }
    }
}
