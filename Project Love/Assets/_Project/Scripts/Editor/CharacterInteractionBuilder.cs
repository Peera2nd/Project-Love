using System;
using System.IO;
using System.Linq;
using ProjectLove.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.EditorTools
{
    public static class CharacterInteractionBuilder
    {
        private const string Root = "Assets/_Project/";
        [Serializable] private sealed class StreamSeed { public string id, displayName; }
        [Serializable] private sealed class ItemSeed { public string id, displayName; public int price, maxStack; public bool giftable; }
        [Serializable] private sealed class NodeSeed { public string id, text; public CharacterExpression expression; }
        [Serializable] private sealed class VariantSeed
        {
            public string id, nodeId, requiredFlag;
            public int priority, startDay = 1, endDay = 30, minimumAffection, maximumAffection = 100;
            public bool anyPhase = true;
            public DayPhase phase;
        }
        [Serializable] private sealed class GiftSeed { public string itemId, nodeId; public GiftReaction reaction; public int affectionChange; }
        [Serializable] private sealed class Seed
        {
            public StreamSeed[] streams;
            public ItemSeed[] items;
            public NodeSeed[] nodes;
            public VariantSeed[] greetings, conversations;
            public GiftSeed[] gifts;
            public PresentationText[] text;
            public int talkStaminaCost, talkAffection;
            public string artActivityId, artStreamId, producerStreamId, neutralGiftNode;
            public int workAmount, neutralGiftAffection;
        }

        [MenuItem("Tools/Project Love/Apply Character Interaction Update")]
        public static void Upgrade()
        {
            if (EditorApplication.isPlaying) return;
            var config = AssetDatabase.LoadAssetAtPath<GameConfigSO>(IsometricPrototypeBuilder.ConfigPath);
            if (config == null) return;
            var character = config.characters[0];
            // Reopening an authored prototype must not reset its content or tuning.
            if (character.conversations.Length > 0 && character.workActivity != null) return;
            var seed = JsonUtility.FromJson<Seed>(File.ReadAllText(Root + "Data/Settings/CharacterInteractionsSeed.json"));
            Directory.CreateDirectory(Root + "Data/WorkStreams"); AssetDatabase.Refresh();
            var streams = seed.streams.ToDictionary(s => s.id, s => Create<WorkStreamSO>("Data/WorkStreams/" + s.id + ".asset", s));
            var items = seed.items.ToDictionary(s => s.id, s => Create<ItemDataSO>("Data/Items/" + s.id + ".asset", s));
            foreach (var item in items.Values) { Undo.RecordObject(item, "Enable character gifts"); item.giftable = true; EditorUtility.SetDirty(item); }
            var nodes = seed.nodes.ToDictionary(s => s.id, s =>
            {
                string path = Root + "Data/Dialogues/" + s.id + ".asset";
                var existing = AssetDatabase.LoadAssetAtPath<DialogueNodeSO>(path);
                if (existing != null) return existing;
                var node = ScriptableObject.CreateInstance<DialogueNodeSO>(); node.id = s.id;
                node.lines = new[] { new DialogueLine { lineId = s.id + "_line", speaker = character, expression = s.expression, text = s.text } };
                AssetDatabase.CreateAsset(node, path); return node;
            });
            var work = AssetDatabase.LoadAssetAtPath<ActivityDataSO>(Root + "Data/Settings/room_work.asset");
            string activityPath = Root + "Data/Settings/" + seed.artActivityId + ".asset";
            var artWork = AssetDatabase.LoadAssetAtPath<ActivityDataSO>(activityPath);
            if (artWork == null)
            {
                artWork = UnityEngine.Object.Instantiate(work); artWork.id = seed.artActivityId;
                artWork.successWork = new[] { new WorkOutput { stream = streams[seed.artStreamId], amount = seed.workAmount } };
                AssetDatabase.CreateAsset(artWork, activityPath);
            }
            Undo.RecordObject(work, "Bind producer work");
            if (work.successWork.Length == 0) work.successWork = new[] { new WorkOutput { stream = streams[seed.producerStreamId], amount = seed.workAmount } };
            EditorUtility.SetDirty(work);
            Undo.RecordObject(character, "Update character interactions");
            character.workActivity = artWork; character.dailyTalkStaminaCost = seed.talkStaminaCost; character.dailyTalkAffection = seed.talkAffection;
            character.greetings = seed.greetings.Select(v => Variant(v, nodes)).ToArray();
            character.conversations = seed.conversations.Select(v => Variant(v, nodes)).ToArray();
            character.neutralGiftResponse = nodes[seed.neutralGiftNode]; character.neutralGiftAffection = seed.neutralGiftAffection;
            character.giftPreferences = seed.gifts.Select(g => new GiftPreference { item = items[g.itemId], response = nodes[g.nodeId],
                reaction = g.reaction, affectionChange = g.affectionChange }).ToArray(); EditorUtility.SetDirty(character);
            Undo.RecordObject(config, "Update prototype catalogs");
            config.items = config.items.Concat(items.Values).Distinct().ToArray();
            config.workStreams = config.workStreams.Concat(streams.Values).Distinct().ToArray(); EditorUtility.SetDirty(config);
            foreach (var room in config.rooms.Where(r => r.shopItems.Length > 0))
            {
                Undo.RecordObject(room, "Add prototype gifts"); room.shopItems = room.shopItems.Concat(items.Values).Distinct().ToArray(); EditorUtility.SetDirty(room);
            }
            var presentation = AssetDatabase.LoadAssetAtPath<RoomPresentationSO>(Root + "Data/Settings/RoomPresentation.asset");
            Undo.RecordObject(presentation, "Update interaction copy");
            presentation.text = presentation.text.Where(t => !seed.text.Any(s => s.key == t.key)).Concat(seed.text).ToArray(); EditorUtility.SetDirty(presentation);
            AssetDatabase.SaveAssets();
            var issues = FoundationValidator.ValidateProject();
            if (issues.Count > 0) throw new InvalidOperationException(string.Join("\n", issues));
        }
        private static ConversationVariant Variant(VariantSeed data, System.Collections.Generic.Dictionary<string, DialogueNodeSO> nodes) =>
            new ConversationVariant { id = data.id, priority = data.priority, startDay = data.startDay, endDay = data.endDay,
                anyPhase = data.anyPhase, phase = data.phase, minimumAffection = data.minimumAffection, maximumAffection = data.maximumAffection,
                node = nodes[data.nodeId], conditions = string.IsNullOrEmpty(data.requiredFlag) ? new Condition[0] :
                    new[] { new Condition { type = ConditionType.FlagSet, flagId = data.requiredFlag } } };
        private static T Create<T>(string path, object data) where T : IdentifiedDataSO
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(Root + path);
            if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<T>(); JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data), asset);
            AssetDatabase.CreateAsset(asset, Root + path); return asset;
        }
    }
}
