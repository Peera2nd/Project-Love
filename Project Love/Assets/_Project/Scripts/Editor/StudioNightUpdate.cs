using System;
using System.IO;
using System.Linq;
using ProjectLove.Data;
using ProjectLove.Simulation;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.EditorTools
{
    // Append only once; later author edits to the catalog and dialogue are preserved.
    public static class StudioNightUpdate
    {
        private const string Root = "Assets/_Project/Data/Studio/Events/";
        [Serializable] private sealed class ChoiceSeed { public string id, text, action, nextId; }
        [Serializable] private sealed class NodeSeed { public string id, characterId, text; public ChoiceSeed[] choices; }
        [Serializable] private sealed class EventSeed
        {
            public string id, characterId, entryId, group;
            public bool callback;
            public int minimumAffection, maximumAffection, minimumStress, maximumStress, priority;
        }
        [Serializable] private sealed class Seed
        { public int version, sharedNightAffection, deadlineDays, cooldownDays; public NodeSeed[] nodes; public EventSeed[] events; public PresentationText[] text; }
        [MenuItem("Tools/Project Love/Apply Studio Night Prototype")]
        public static void Apply()
        {
            var game = AssetDatabase.LoadAssetAtPath<GameConfigSO>(StudioPrototypeBuilder.ConfigPath);
            if (game == null) return;
            var seed = JsonUtility.FromJson<Seed>(File.ReadAllText("Assets/_Project/Data/Settings/StudioNightSeed.json"));
            if (game.production.nightContentVersion >= seed.version) return;
            Directory.CreateDirectory(Root); AssetDatabase.Refresh();
            CharacterDataSO Person(string id) => game.characters.Single(c => c.id == id);
            T Asset<T>(string id) where T : IdentifiedDataSO
            {
                string path = Root + id + ".asset";
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset == null) { asset = ScriptableObject.CreateInstance<T>(); asset.id = id; AssetDatabase.CreateAsset(asset, path); }
                Undo.RecordObject(asset, "Seed shared night content"); return asset;
            }
            var nodes = seed.nodes.ToDictionary(n => n.id, n => Asset<DialogueNodeSO>(n.id));
            foreach (var n in seed.nodes)
            {
                var node = nodes[n.id]; var person = Person(n.characterId);
                node.lines = new[] { new DialogueLine { lineId = n.id + "_line", speaker = person, text = n.text } };
                node.choices = n.choices.Select(c => new DialogueChoice { choiceId = c.id, text = c.text,
                    workAction = (DialogueWorkAction)Enum.Parse(typeof(DialogueWorkAction), c.action),
                    workCharacter = c.action == "None" ? null : person,
                    nextNode = string.IsNullOrEmpty(c.nextId) ? null : nodes[c.nextId] }).ToArray();
                EditorUtility.SetDirty(node);
            }
            var events = seed.events.Select(e =>
            {
                var data = Asset<EventDataSO>(e.id); var person = Person(e.characterId);
                data.displayName = person.displayName + (e.callback ? " · หลังคืนที่ช่วยกัน" : " · อาสาอยู่ต่อ");
                data.character = person; data.room = game.rooms.Single(r => r.id == game.startingRoomId);
                data.startDay = data.preferredDay = 2; data.endDay = game.totalDays; data.priority = e.priority;
                data.delivery = EventDelivery.CharacterBubble; data.anyPhase = false;
                data.triggerPhase = e.callback ? DayPhase.Morning : DayPhase.Night;
                data.repeatDaily = !e.callback; data.minimumRepeatDays = e.callback ? 0 : seed.cooldownDays;
                data.repeatGroup = e.callback ? "" : e.group; data.exclusiveGroup = e.callback ? e.group : "";
                data.entryNode = nodes[e.entryId];
                data.conditions = new[] {
                    new Condition { type = e.callback ? ConditionType.CharacterSharedNight : ConditionType.CharacterCanVolunteer, character = person, value = seed.deadlineDays },
                    new Condition { type = ConditionType.StatAtLeast, stat = StatType.Affection, character = person, value = e.minimumAffection },
                    new Condition { type = ConditionType.StatAtMost, stat = StatType.Affection, character = person, value = e.maximumAffection },
                    new Condition { type = ConditionType.StatAtLeast, stat = StatType.Stress, character = person, value = e.minimumStress },
                    new Condition { type = ConditionType.StatAtMost, stat = StatType.Stress, character = person, value = e.maximumStress }
                };
                EditorUtility.SetDirty(data); return data;
            }).ToArray();
            Undo.RecordObject(game, "Append night events"); game.events = game.events.Where(e => !events.Contains(e)).Concat(events).ToArray();
            var issues = EventScheduler.ValidateCatalog(game); if (issues.Count > 0) throw new ArgumentException(string.Join("\n", issues));
            var config = game.production; var presentation = config.presentation;
            Undo.RecordObject(config, "Shared night tuning"); config.sharedNightAffection = seed.sharedNightAffection; config.nightContentVersion = seed.version;
            Undo.RecordObject(presentation, "Night UI copy");
            presentation.text = presentation.text.Where(t => !seed.text.Any(s => s.key == t.key)).Concat(seed.text).ToArray();
            EditorUtility.SetDirty(game); EditorUtility.SetDirty(config); EditorUtility.SetDirty(presentation); AssetDatabase.SaveAssets();
        }
    }
}
