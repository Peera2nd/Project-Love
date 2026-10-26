using System;
using System.IO;
using System.Linq;
using ProjectLove.Data;
using ProjectLove.Simulation;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.EditorTools
{
    // One-time seed import. Subsequent opening preserves author edits to event assets.
    public static class StudioEventUpdate
    {
        private const string Root = "Assets/_Project/Data/Studio/Events/";
        [Serializable] private sealed class EffectSeed { public string stat, characterId; public int amount; }
        [Serializable] private sealed class ChoiceSeed
        { public string id, text, nextId; public string[] workloads; public FlagChange[] flags; public EffectSeed[] effects; }
        [Serializable] private sealed class NodeSeed { public string id, characterId, text, nextId; public ChoiceSeed[] choices; }
        [Serializable] private sealed class ConditionSeed { public string type, targetId, eventId, flagId; }
        [Serializable] private sealed class EventSeed
        { public string id, displayName, characterId, entryId, delivery; public int startDay, endDay, priority; public ConditionSeed[] conditions; }
        [Serializable] private sealed class Seed { public int version; public NodeSeed[] nodes; public EventSeed[] events; public PresentationText[] text; }
        [MenuItem("Tools/Project Love/Apply Studio Event Prototype")]
        public static void Apply()
        {
            var game = AssetDatabase.LoadAssetAtPath<GameConfigSO>(StudioPrototypeBuilder.ConfigPath);
            if (game == null) return;
            var seed = JsonUtility.FromJson<Seed>(File.ReadAllText("Assets/_Project/Data/Settings/StudioEventSeed.json"));
            if (game.eventContentVersion >= seed.version) return;
            Directory.CreateDirectory(Root); AssetDatabase.Refresh();
            CharacterDataSO Person(string id) => game.characters.Single(c => c.id == id);
            T Asset<T>(string id) where T : IdentifiedDataSO
            {
                string path = Root + id + ".asset";
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset == null) { asset = ScriptableObject.CreateInstance<T>(); asset.id = id; AssetDatabase.CreateAsset(asset, path); }
                Undo.RecordObject(asset, "Seed studio event"); return asset;
            }
            var nodes = seed.nodes.ToDictionary(n => n.id, n => Asset<DialogueNodeSO>(n.id));
            DialogueNodeSO Node(string id) => string.IsNullOrEmpty(id) ? null : nodes[id];
            foreach (var n in seed.nodes)
            {
                var node = nodes[n.id];
                node.lines = new[] { new DialogueLine { lineId = n.id + "_line", speaker = Person(n.characterId), text = n.text } };
                node.nextNode = Node(n.nextId);
                node.choices = n.choices.Select(c => new DialogueChoice { choiceId = c.id, text = c.text, nextNode = Node(c.nextId),
                    activateWorkloads = c.workloads, flags = c.flags,
                    effects = c.effects.Select(e => new StatModifier { stat = (StatType)Enum.Parse(typeof(StatType), e.stat), character = Person(e.characterId), amount = e.amount }).ToArray() }).ToArray();
                EditorUtility.SetDirty(node);
            }
            var events = seed.events.Select(e =>
            {
                var data = Asset<EventDataSO>(e.id); data.displayName = e.displayName;
                data.character = Person(e.characterId); data.room = game.rooms.Single(r => r.id == game.startingRoomId);
                data.startDay = data.preferredDay = e.startDay; data.endDay = e.endDay; data.priority = e.priority;
                data.delivery = (EventDelivery)Enum.Parse(typeof(EventDelivery), e.delivery); data.anyPhase = true;
                data.entryNode = nodes[e.entryId]; data.conditions = e.conditions.Select(c => new Condition {
                    type = (ConditionType)Enum.Parse(typeof(ConditionType), c.type), targetId = c.targetId, eventId = c.eventId, flagId = c.flagId }).ToArray();
                EditorUtility.SetDirty(data); return data;
            }).ToArray();
            Undo.RecordObject(game, "Bind studio events"); game.events = events;
            var issues = EventScheduler.ValidateCatalog(game);
            if (issues.Count > 0) throw new ArgumentException(string.Join("\n", issues));
            var presentation = game.production.presentation; Undo.RecordObject(presentation, "Add event UI copy");
            presentation.text = presentation.text.Where(t => !seed.text.Any(s => s.key == t.key)).Concat(seed.text).ToArray();
            game.eventContentVersion = seed.version; EditorUtility.SetDirty(game); EditorUtility.SetDirty(presentation); AssetDatabase.SaveAssets();
        }
    }
}
