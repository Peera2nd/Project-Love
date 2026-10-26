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
    public static class IsometricPrototypeBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Gameplay/IsometricPrototype.unity";
        public const string ConfigPath = "Assets/_Project/Data/Settings/RoomCampaign.asset";
        private const string Root = "Assets/_Project/";
        private static TMP_FontAsset font;
        [Serializable] private sealed class IdSeed { public string id; }
        [Serializable] private sealed class ConfigSeed
        {
            public string id, mainProgressLabel, startingRoomId;
            public int totalDays, maxActionPointsPerDay, startingMoney, startingSkillPoints, startingProgress, burnoutThreshold;
            public bool usesStamina;
            public int maxStamina, startingStamina, overnightRecovery;
        }
        [Serializable] private sealed class CharacterSeed { public string id, displayName, roleTitle; public int startingAffection, startingStress; }
        [Serializable] private sealed class ItemSeed { public string id, displayName, spritePath; public int price, maxStack, staminaRecovery; }
        [Serializable] private sealed class ActivitySeed
        {
            public string id, displayName; public int actionPointCost, staminaCost, staminaRecovery, elapsedPhases;
            public StatModifier[] successEffects, failureEffects;
        }
        [Serializable] private sealed class ChoiceSeed
        {
            public string choiceId, text, nextNodeId; public int actionPointCost;
            public StatModifier[] effects;
        }
        [Serializable] private sealed class NodeSeed { public string id; public DialogueLine[] lines; public ChoiceSeed[] choices; }
        [Serializable] private sealed class EventSeed
        {
            public string id, displayName, entryNodeId;
            public int startDay, preferredDay, endDay, elapsedPhases, staminaCost;
            public DayPhase triggerPhase; public bool anyPhase, repeatDaily;
        }
        [Serializable] private sealed class PresentationSeed
        {
            public string id; public string[] phaseNames; public float charactersPerSecond, dialogueDimAlpha;
            public PresentationText[] text;
        }
        [Serializable] private sealed class PlacementSeed
        {
            public DayPhase phase; public string spritePath; public Vector2 position, size; public bool visible;
        }
        [Serializable] private sealed class HotspotSeed
        {
            public string id, label, activityId, itemId; public RoomInteractionKind kind;
            public bool character, conversation; public PlacementSeed[] placements;
        }
        [Serializable] private sealed class RoomSeed
        {
            public string id, displayName, dayEndedPath; public string[] backgroundPaths, shopItemIds;
            public int entryPhaseCost; public HotspotSeed[] hotspots;
        }
        [Serializable] private sealed class Seed
        {
            public ConfigSeed config; public CharacterSeed character; public ItemSeed[] items;
            public ActivitySeed[] activities; public NodeSeed[] nodes; public EventSeed @event;
            public PresentationSeed presentation; public RoomSeed[] rooms;
            public string normalSpritePath, smileSpritePath;
        }
        [MenuItem("Tools/Project Love/Create or Open Isometric Prototype")]
        public static void BuildOrOpen()
        {
            if (EditorApplication.isPlaying) { EditorUtility.DisplayDialog("Project Love", "กรุณาหยุด Play ก่อนเปิดฉากต้นแบบ", "ตกลง"); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            { EditorUtility.DisplayDialog("Project Love", "กรุณาบันทึกฉากที่เปิดอยู่ก่อน", "ตกลง"); return; }
            BuildFromCommandLine(); EditorSceneManager.OpenScene(ScenePath);
        }
        public static void BuildFromCommandLine()
        {
            if (File.Exists(ScenePath)) { CharacterInteractionBuilder.Upgrade(); return; }
            foreach (string folder in new[] { "Data/Rooms", "Data/Items", "Data/Settings", "Data/Characters", "Data/Dialogues", "Data/Events", "Prefabs/UI", "Scenes/Gameplay" })
                Directory.CreateDirectory(Root + folder);
            Directory.CreateDirectory("Logs"); AssetDatabase.Refresh();
            var seed = JsonUtility.FromJson<Seed>(File.ReadAllText(Root + "Data/Settings/IsometricRoomSeed.json"));
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "Resources/Fonts/Sarabun/Sarabun SDF.asset");
            if (font == null) throw new InvalidOperationException("Required Thai font is missing.");
            var character = Create<CharacterDataSO>(Root + "Data/Characters/RoomCharacter.asset", seed.character, asset =>
            {
                asset.sprites = new[] { new EmotionSprite { expression = CharacterExpression.Normal, sprite = SpriteAt(seed.normalSpritePath) },
                    new EmotionSprite { expression = CharacterExpression.Smile, sprite = SpriteAt(seed.smileSpritePath) } };
            });
            var items = seed.items.ToDictionary(s => s.id, s => Create<ItemDataSO>(Root + "Data/Items/" + s.id + ".asset", s,
                asset => asset.icon = SpriteAt(s.spritePath)));
            var activities = seed.activities.ToDictionary(s => s.id, s => Create<ActivityDataSO>(Root + "Data/Settings/" + s.id + ".asset", s, asset =>
            {
                BindEffects(asset.successEffects, character); BindEffects(asset.failureEffects, character);
            }));
            var nodes = seed.nodes.ToDictionary(s => s.id, s => Create<DialogueNodeSO>(Root + "Data/Dialogues/" + s.id + ".asset", new IdSeed { id = s.id }, null));
            foreach (var data in seed.nodes)
            {
                var node = nodes[data.id];
                if (node.lines.Length > 0) continue;
                node.lines = data.lines;
                foreach (var line in node.lines) line.speaker = character;
                node.choices = data.choices.Select(c =>
                {
                    BindEffects(c.effects, character);
                    return new DialogueChoice { choiceId = c.choiceId, text = c.text, actionPointCost = c.actionPointCost,
                        effects = c.effects ?? new StatModifier[0], nextNode = string.IsNullOrEmpty(c.nextNodeId) ? null : nodes[c.nextNodeId] };
                }).ToArray();
                EditorUtility.SetDirty(node);
            }
            var conversation = Create<EventDataSO>(Root + "Data/Events/" + seed.@event.id + ".asset", seed.@event,
                asset => asset.entryNode = nodes[seed.@event.entryNodeId]);
            var presentation = Create<RoomPresentationSO>(Root + "Data/Settings/RoomPresentation.asset", seed.presentation, asset => asset.font = font);
            var rooms = seed.rooms.Select(s => Create<RoomDataSO>(Root + "Data/Rooms/" + s.id + ".asset", s, asset =>
            {
                asset.phases = s.backgroundPaths.Select((path, index) => new RoomPhaseArt { phase = (DayPhase)index, background = SpriteAt(path) }).ToArray();
                asset.dayEndedBackground = string.IsNullOrEmpty(s.dayEndedPath) ? null : SpriteAt(s.dayEndedPath);
                asset.shopItems = (s.shopItemIds ?? new string[0]).Select(id => items[id]).ToArray();
                asset.hotspots = s.hotspots.Select(h => new RoomHotspot
                {
                    id = h.id, label = h.label, kind = h.kind,
                    character = h.character ? character : null, conversation = h.conversation ? conversation : null,
                    activity = string.IsNullOrEmpty(h.activityId) ? null : activities[h.activityId],
                    item = string.IsNullOrEmpty(h.itemId) ? null : items[h.itemId],
                    placements = h.placements.Select(p => new RoomPlacement { phase = p.phase, position = p.position,
                        size = p.size, visible = p.visible, sprite = string.IsNullOrEmpty(p.spritePath) ? null : SpriteAt(p.spritePath) }).ToArray()
                }).ToArray();
            })).ToArray();
            var config = Create<GameConfigSO>(ConfigPath, seed.config, asset =>
            { asset.characters = new[] { character }; asset.items = items.Values.ToArray(); asset.rooms = rooms; });
            AssetDatabase.SaveAssets();
            var issues = FoundationValidator.ValidateProject();
            if (issues.Count > 0) throw new InvalidOperationException(string.Join("\n", issues));
            BuildScene(config, presentation);
            AssetDatabase.SaveAssets();
            CharacterInteractionBuilder.Upgrade();
            File.WriteAllText("Logs/IsometricBuild.json", "{\"status\":\"passed\"}");
        }
        private static T Create<T>(string path, object seed, Action<T> initialize) where T : IdentifiedDataSO
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<T>();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(seed), asset);
            initialize?.Invoke(asset);
            AssetDatabase.CreateAsset(asset, path); EditorUtility.SetDirty(asset); return asset;
        }
        private static void BindEffects(StatModifier[] effects, CharacterDataSO character)
        {
            foreach (var effect in effects ?? new StatModifier[0])
                if (effect.stat == StatType.Affection || effect.stat == StatType.Stress) effect.character = character;
        }
        internal static Sprite SpriteAt(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing image: " + path);
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single || !importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true; importer.isReadable = true; importer.sRGBTexture = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.maxTextureSize = 2048;
                importer.npotScale = TextureImporterNPOTScale.None;
                var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidOperationException("Required single sprite is missing: " + path);
            return sprite;
        }
        internal static void BuildScene(GameConfigSO config, RoomPresentationSO presentation, string scenePath = ScenePath)
        {
            font = presentation.font;
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, Application.isBatchMode ? NewSceneMode.Single : NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                var camera = new GameObject("Room Camera").AddComponent<Camera>();
                camera.tag = "MainCamera"; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black; camera.orthographic = true;
                var root = new GameObject("Isometric UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
                var view = root.AddComponent<IsometricRoomView>(); view.config = config; view.presentation = presentation;
                if (config.production != null) root.AddComponent<StudioSprintView>();
                var system = new GameObject("Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
                system.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
                Panel("Black Backdrop", root.transform, 0, 0, 1, 1, Color.black);
                var room = Rect("Room Frame", root.transform, .36f, .47f, .36f, .47f);
                room.sizeDelta = new Vector2(900, 900);
                view.roomImage = room.gameObject.AddComponent<Image>(); view.roomImage.raycastTarget = false;
                view.hotspotRoot = Rect("Hotspots", room, 0, 0, 1, 1);
                var header = Panel("HUD", root.transform, .02f, .90f, .98f, .98f, new Color(.07f, .1f, .12f, .96f));
                view.titleText = Text("Scene Name", header, .015f, .05f, .24f, .95f, 32);
                view.hudText = Text("Resources", header, .25f, .05f, .985f, .95f, 28);
                var panel = Panel("Action Panel", root.transform, .69f, .19f, .98f, .86f, new Color(.06f, .09f, .11f, .96f));
                view.panelTitle = Text("Panel Title", panel, .05f, .87f, .95f, .97f, 30);
                view.panelText = Text("Panel Text", panel, .05f, .61f, .95f, .85f, 25);
                view.panelButtons = List("Panel Buttons", panel, .05f, .05f, .95f, .58f);
                view.travelButton = Button("Travel", root.transform, .69f, .11f, .78f, .17f);
                view.inventoryButton = Button("Inventory", root.transform, .79f, .11f, .88f, .17f);
                view.optionsButton = Button("Options", root.transform, .89f, .11f, .98f, .17f);
                view.waitButton = Button("End Day", root.transform, .69f, .03f, .83f, .09f);
                view.nextDayButton = Button("Next Day", root.transform, .84f, .03f, .98f, .09f);
                view.hintText = Text("Scene Hint", root.transform, .03f, .02f, .65f, .10f, 24);
                var modal = Panel("Dialogue Modal", root.transform, 0, 0, 1, 1, new Color(0, 0, 0, presentation.dialogueDimAlpha));
                view.dialogueModal = modal.gameObject;
                var portrait = Rect("VN Character", modal, .56f, 0, .99f, .96f);
                view.dialoguePortrait = portrait.gameObject.AddComponent<Image>(); view.dialoguePortrait.preserveAspect = true; view.dialoguePortrait.raycastTarget = false;
                var speech = Panel("Dialogue Panel", modal, .04f, .45f, .55f, .85f, new Color(.06f, .09f, .12f, .98f));
                view.speakerText = Text("Speaker", speech, .04f, .80f, .96f, .96f, 32);
                view.dialogueText = Text("Dialogue Text", speech, .04f, .08f, .96f, .77f, 32);
                view.choiceButtons = List("Choices", modal, .04f, .17f, .55f, .40f);
                view.continueButton = Button("Continue", modal, .04f, .06f, .28f, .13f);
                view.deferButton = Button("Defer", modal, .30f, .06f, .55f, .13f);
                var template = Button("Action Template", root.transform, 0, 0, .5f, .08f);
                template.gameObject.SetActive(false);
                string path = Root + "Prefabs/UI/RoomActionButton.prefab";
                if (!File.Exists(path)) PrefabUtility.SaveAsPrefabAsset(template.gameObject, path);
                view.buttonTemplate = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<Button>();
                UnityEngine.Object.DestroyImmediate(template.gameObject);
                EditorSceneManager.SaveScene(scene, scenePath);
            }
            finally
            {
                if (!Application.isBatchMode)
                { EditorSceneManager.CloseScene(scene, true); if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); }
            }
        }
        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1); rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        private static RectTransform Panel(string name, Transform parent, float x0, float y0, float x1, float y1, Color color)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1); rect.gameObject.AddComponent<Image>().color = color; return rect;
        }
        private static TMP_Text Text(string name, Transform parent, float x0, float y0, float x1, float y1, float size)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1); var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.fontSize = size; text.enableAutoSizing = true; text.fontSizeMin = 18; text.fontSizeMax = size; text.color = new Color(.93f, .95f, .97f); text.raycastTarget = false;
            text.fontFeatures = new List<OTL_FeatureTag> { OTL_FeatureTag.kern, OTL_FeatureTag.mark, OTL_FeatureTag.mkmk };
            text.textWrappingMode = TextWrappingModes.Normal; text.alignment = TextAlignmentOptions.MidlineLeft; return text;
        }
        private static Button Button(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = Panel(name, parent, x0, y0, x1, y1, new Color(.16f, .27f, .30f));
            var button = rect.gameObject.AddComponent<Button>(); rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 74;
            var colors = button.colors; colors.disabledColor = new Color(.3f, .3f, .3f); button.colors = colors;
            var text = Text("Label", rect, .025f, .05f, .975f, .95f, 24); text.alignment = TextAlignmentOptions.Center; return button;
        }
        private static RectTransform List(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1); var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12; layout.childControlHeight = true; layout.childControlWidth = true;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = true; return rect;
        }
    }
}
