using System;
using System.IO;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectLove.EditorTools
{
    public static class StudioPrototypeBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Gameplay/StudioSprintPrototype.unity";
        public const string ConfigPath = "Assets/_Project/Data/Studio/StudioCampaign.asset";
        private const string Root = "Assets/_Project/";
        [Serializable] private sealed class QuotaSeed { public string streamId; public int effort; }
        [Serializable] private sealed class TaskSeed { public string id, displayName, streamId, ownerId; public int effort, mainWeight, releaseDay; public bool decisionBlocked; public string[] prerequisiteIds; }
        [Serializable] private sealed class SprintSeed { public string id, displayName; public int startDay, endDay; public QuotaSeed[] quotas; }
        [Serializable] private sealed class EventSeed { public string id, displayName; public int earliestDay, latestDay, absenceDays; public bool automatic; public string[] taskIds; public QuotaSeed[] extraQuotas; }
        [Serializable] private sealed class WorkerSeed
        {
            public string id, artId, displayName, roleTitle, streamId, likedItemId;
            public int startingAffection, startingStress, baseRate, dailyTalkStaminaCost, dailyTalkAffection, likedAffection, neutralGiftAffection, dislikedAffection, closeAffection;
            public string greeting, nightGreeting, talk, closeTalk, neutralGift, likedGift, dislikedGift;
            public string[] roomSprites; public Vector2[] positions; public Vector2 size;
        }
        [Serializable] private sealed class Seed
        {
            public string configId, productionId, workspaceId, storeId, presentationId, restId, restName, deskLabel;
            public int restCost, restRecovery, restPhases;
            public WorkerSeed[] workers; public TaskSeed[] tasks; public SprintSeed[] sprints; public EventSeed[] events;
            public StressBand[] stressBands; public DayPhase[] productivePhases; public PresentationText[] text;
            public int workStress, overnightStressRecovery, helpCost, helpEffort, helpStressRecovery, coordinateCost, coordinateBonus, coordinateTicks, clarifyCost, careCost, careStressRecovery;
        }
        [MenuItem("Tools/Project Love/Create or Open Studio Sprint Prototype")]
        public static void BuildOrOpen()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            { EditorUtility.DisplayDialog("Project Love", "กรุณาบันทึกฉากที่เปิดอยู่ก่อน", "ตกลง"); return; }
            BuildFromCommandLine(); EditorSceneManager.OpenScene(ScenePath); EnsureController();
        }
        public static void BuildFromCommandLine()
        {
            if (File.Exists(ScenePath))
            {
                StudioManagementUpdate.Apply(); StudioEventUpdate.Apply(); StudioUiUpdate.Apply(); StudioNightUpdate.Apply();
                if (Application.isBatchMode) { EditorSceneManager.OpenScene(ScenePath); EnsureController(); }
                return;
            }
            IsometricPrototypeBuilder.BuildFromCommandLine();
            Directory.CreateDirectory(Root + "Data/Studio"); Directory.CreateDirectory(Root + "Data/Studio/Tasks");
            Directory.CreateDirectory(Root + "Data/Studio/Dialogues"); AssetDatabase.Refresh();
            var seed = JsonUtility.FromJson<Seed>(File.ReadAllText(Root + "Data/Settings/StudioSprintSeed.json"));
            var source = AssetDatabase.LoadAssetAtPath<GameConfigSO>(IsometricPrototypeBuilder.ConfigPath);
            var sourceViewScene = AssetDatabase.LoadAssetAtPath<RoomPresentationSO>(Root + "Data/Settings/RoomPresentation.asset");
            if (sourceViewScene == null)
                sourceViewScene = AssetDatabase.FindAssets("t:RoomPresentationSO").Select(g => AssetDatabase.LoadAssetAtPath<RoomPresentationSO>(AssetDatabase.GUIDToAssetPath(g))).First();
            var presentation = Create<RoomPresentationSO>("StudioPresentation", seed.presentationId, () =>
            {
                var p = UnityEngine.Object.Instantiate(sourceViewScene);
                p.text = p.text.Where(t => !seed.text.Any(s => s.key == t.key)).Concat(seed.text).ToArray(); return p;
            });
            WorkStreamSO Stream(string id) => source.workStreams.First(s => s.id == id);
            StreamQuota[] Quotas(QuotaSeed[] list) => list.Select(q => new StreamQuota { stream = Stream(q.streamId), effort = q.effort }).ToArray();
            var characters = seed.workers.ToDictionary(w => w.id, w => Create<CharacterDataSO>(w.id, w.id, () =>
            {
                var c = ScriptableObject.CreateInstance<CharacterDataSO>();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(w), c);
                c.sprites = new[] { CharacterExpression.Normal, CharacterExpression.Smile }.Select(e => new EmotionSprite { expression = e,
                    sprite = IsometricPrototypeBuilder.SpriteAt(Root + "Art/Characters/Sprites/Mockup/Dialogue/chr_" + w.artId + "_" + e + ".png") }).ToArray(); return c;
            }));
            foreach (var w in seed.workers)
            {
                var c = characters[w.id];
                if (c.greetings.Length > 0) continue;
                DialogueNodeSO Node(string suffix, string text, CharacterExpression expression = CharacterExpression.Normal) =>
                    Create<DialogueNodeSO>("Dialogues/" + w.id + "_" + suffix, w.id + "_" + suffix, () =>
                    {
                        var n = ScriptableObject.CreateInstance<DialogueNodeSO>(); n.lines = new[] { new DialogueLine { lineId = w.id + "_" + suffix + "_line", speaker = c, expression = expression, text = text } }; return n;
                    });
                var normal = Node("greeting", w.greeting); var night = Node("night", w.nightGreeting);
                var talk = Node("talk", w.talk); var close = Node("close", w.closeTalk, CharacterExpression.Smile);
                c.greetings = new[] { new ConversationVariant { id = "default", node = normal, endDay = source.totalDays },
                    new ConversationVariant { id = "night", priority = 10, anyPhase = false, phase = DayPhase.Night, node = night, endDay = source.totalDays } };
                c.conversations = new[] { new ConversationVariant { id = "default", node = talk, endDay = source.totalDays },
                    new ConversationVariant { id = "close", priority = 10, minimumAffection = w.closeAffection, node = close, endDay = source.totalDays } };
                c.neutralGiftResponse = Node("neutral_gift", w.neutralGift);
                c.giftPreferences = new[] { new GiftPreference { item = source.items.First(i => i.id == w.likedItemId), reaction = GiftReaction.Liked,
                        affectionChange = w.likedAffection, response = Node("liked_gift", w.likedGift, CharacterExpression.Smile) },
                    new GiftPreference { item = source.items.First(i => i.id == "room_trial_gift"), reaction = GiftReaction.Disliked,
                        affectionChange = w.dislikedAffection, response = Node("disliked_gift", w.dislikedGift) } };
                EditorUtility.SetDirty(c);
            }
            var tasks = seed.tasks.ToDictionary(t => t.id, t => Create<WorkTaskSO>("Tasks/" + t.id, t.id, () =>
            {
                var d = ScriptableObject.CreateInstance<WorkTaskSO>(); JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(t), d);
                d.stream = Stream(t.streamId); d.owner = characters[t.ownerId]; return d;
            }));
            foreach (var t in seed.tasks)
            {
                if (tasks[t.id].prerequisites.Length == 0 && t.prerequisiteIds.Length > 0)
                { tasks[t.id].prerequisites = t.prerequisiteIds.Select(id => tasks[id]).ToArray(); EditorUtility.SetDirty(tasks[t.id]); }
            }
            var sprints = seed.sprints.Select(s => Create<SprintDataSO>(s.id, s.id, () =>
            {
                var d = ScriptableObject.CreateInstance<SprintDataSO>(); JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(s), d); d.quotas = Quotas(s.quotas); return d;
            })).ToArray();
            var production = Create<ProductionConfigSO>("StudioProduction", seed.productionId, () =>
            {
                var d = ScriptableObject.CreateInstance<ProductionConfigSO>(); JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(seed), d);
                d.tasks = tasks.Values.ToArray(); d.sprints = sprints; d.presentation = presentation;
                d.workers = seed.workers.Select(w => new WorkerDefinition { character = characters[w.id], stream = Stream(w.streamId), baseRate = w.baseRate }).ToArray();
                d.events = seed.events.Select(e => new WorkloadEvent { id = e.id, displayName = e.displayName, earliestDay = e.earliestDay,
                    latestDay = e.latestDay, automatic = e.automatic, tasks = e.taskIds.Select(id => tasks[id]).ToArray(), extraQuotas = Quotas(e.extraQuotas) }).ToArray(); return d;
            });
            var rest = Create<ActivityDataSO>(seed.restId, seed.restId, () =>
            { var a = ScriptableObject.CreateInstance<ActivityDataSO>(); a.displayName = seed.restName; a.staminaCost = seed.restCost; a.staminaRecovery = seed.restRecovery; a.elapsedPhases = seed.restPhases; return a; });
            var workspace = Create<RoomDataSO>(seed.workspaceId, seed.workspaceId, () =>
            {
                var r = UnityEngine.Object.Instantiate(source.rooms.First());
                r.hotspots = r.hotspots.Where(h => h.kind != RoomInteractionKind.Character).ToArray();
                foreach (var h in r.hotspots)
                {
                    if (h.activity != null && h.activity.staminaRecovery > 0) h.activity = rest;
                    else if (h.activity != null) { h.kind = RoomInteractionKind.Producer; h.activity = null; h.label = seed.deskLabel; }
                }
                r.hotspots = r.hotspots.Concat(seed.workers.Select(w => new RoomHotspot { id = w.id, label = w.displayName,
                    kind = RoomInteractionKind.Character, character = characters[w.id], conversation = source.rooms.First().hotspots.First(h => h.character != null).conversation,
                    placements = Enumerable.Range(0, 4).Select(i => new RoomPlacement { phase = (DayPhase)i, visible = true, position = w.positions[i], size = w.size,
                        sprite = IsometricPrototypeBuilder.SpriteAt(Root + "Art/Characters/Sprites/Mockup/Room/" + w.roomSprites[i == 0 ? 0 : 1]) }).ToArray() })).ToArray(); return r;
            });
            var store = Create<RoomDataSO>(seed.storeId, seed.storeId, () => UnityEngine.Object.Instantiate(source.rooms.Last()));
            var config = Create<GameConfigSO>("StudioCampaign", seed.configId, () =>
            {
                var c = UnityEngine.Object.Instantiate(source); c.characters = characters.Values.ToArray();
                c.production = production; c.rooms = new[] { workspace, store }; c.startingRoomId = workspace.id; return c;
            });
            AssetDatabase.SaveAssets();
            var issues = FoundationValidator.ValidateProject();
            if (issues.Count > 0) throw new InvalidOperationException(string.Join("\n", issues));
            IsometricPrototypeBuilder.BuildScene(config, presentation, ScenePath);
            if (Application.isBatchMode) EnsureController();
            AssetDatabase.SaveAssets(); Directory.CreateDirectory("Logs");
            StudioManagementUpdate.Apply();
            StudioEventUpdate.Apply();
            StudioUiUpdate.Apply(); StudioNightUpdate.Apply();
            File.WriteAllText("Logs/StudioBuild.json", "{\"status\":\"passed\",\"tasks\":" + tasks.Count + "}");
        }
        public static void ReimportMockupsFromCommandLine()
        {
            var seed = JsonUtility.FromJson<Seed>(File.ReadAllText(Root + "Data/Settings/StudioSprintSeed.json"));
            foreach (var w in seed.workers)
            {
                foreach (var name in w.roomSprites) IsometricPrototypeBuilder.SpriteAt(Root + "Art/Characters/Sprites/Mockup/Room/" + name);
                foreach (var expression in new[] { CharacterExpression.Normal, CharacterExpression.Smile })
                    IsometricPrototypeBuilder.SpriteAt(Root + "Art/Characters/Sprites/Mockup/Dialogue/chr_" + w.artId + "_" + expression + ".png");
            }
            AssetDatabase.SaveAssets(); BuildFromCommandLine();
        }
        private static T Create<T>(string name, string id, Func<T> factory) where T : IdentifiedDataSO
        {
            string path = Root + "Data/Studio/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path); if (existing != null) return existing;
            var value = factory(); value.id = id; value.name = Path.GetFileName(name); AssetDatabase.CreateAsset(value, path); EditorUtility.SetDirty(value); return value;
        }
        private static void EnsureController()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath) throw new InvalidOperationException("Expected Studio prototype scene.");
            var view = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<IsometricRoomView>(true)).Single();
            if (view.config == null || view.config.production == null) throw new InvalidOperationException("Studio scene requires a production config.");
            if (view.GetComponent<StudioSprintView>() == null)
            { Undo.AddComponent<StudioSprintView>(view.gameObject); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); }
        }
    }
}
