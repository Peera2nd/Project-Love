using System;
using System.IO;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.EditorTools
{
    public static class StudioManagementUpdate
    {
        private const string Root = "Assets/_Project/";
        [Serializable] private sealed class StageSeed { public string sprintId; public int goal; }
        [Serializable] private sealed class BindingSeed { public string taskId, sprintId; }
        [Serializable] private sealed class ColorSeed { public string streamId, color; }
        [Serializable] private sealed class VariantSeed
        { public string id, characterId, text; public int minimumAffection, maximumAffection, minimumStress, maximumStress; }
        [Serializable] private sealed class Seed
        {
            public int version, producerCost, producerEffort, producerMoney, producerTeamBonus, restStressRecovery, overtimeMoneyCost, overtimeStress;
            public StageSeed[] stages; public BindingSeed[] taskBindings; public ColorSeed[] colors;
            public VariantSeed[] variants; public PresentationText[] text; public string[] icons;
        }
        public static void Apply()
        {
            var game = AssetDatabase.LoadAssetAtPath<GameConfigSO>(StudioPrototypeBuilder.ConfigPath);
            if (game == null) return;
            var config = game.production;
            var seed = JsonUtility.FromJson<Seed>(File.ReadAllText(Root + "Data/Settings/StudioManagementSeed.json"));
            if (config.managementVersion >= seed.version) return;
            Undo.RecordObject(config, "Update studio management rules");
            config.useProducerStages = true; config.producerCost = seed.producerCost; config.producerEffort = seed.producerEffort;
            config.producerMoney = seed.producerMoney; config.producerTeamBonus = seed.producerTeamBonus;
            config.restStressRecovery = seed.restStressRecovery; config.overtimeMoneyCost = seed.overtimeMoneyCost; config.overtimeStress = seed.overtimeStress;
            foreach (var entry in seed.stages)
            {
                var stage = config.sprints.Single(s => s.id == entry.sprintId); Undo.RecordObject(stage, "Set producer goal");
                stage.producerGoal = entry.goal; EditorUtility.SetDirty(stage);
            }
            foreach (var entry in seed.taskBindings)
            {
                var task = config.tasks.Single(t => t.id == entry.taskId); Undo.RecordObject(task, "Bind task sprint");
                task.sprint = config.sprints.Single(s => s.id == entry.sprintId); EditorUtility.SetDirty(task);
            }
            foreach (var entry in seed.colors)
            {
                var stream = game.workStreams.Single(s => s.id == entry.streamId); Undo.RecordObject(stream, "Set discipline tint");
                if (!ColorUtility.TryParseHtmlString(entry.color, out stream.tint)) throw new ArgumentException("Invalid discipline tint.");
                EditorUtility.SetDirty(stream);
            }
            var presentation = config.presentation; Undo.RecordObject(presentation, "Update studio UI copy");
            presentation.text = presentation.text.Where(t => !seed.text.Any(s => s.key == t.key)).Concat(seed.text).ToArray();
            presentation.icons = seed.icons.Select(k => new PresentationIcon { key = k, sprite = presentation.Icon(k) }).ToArray();
            EditorUtility.SetDirty(presentation);
            foreach (var group in seed.variants.GroupBy(v => v.characterId))
            {
                var character = game.characters.Single(c => c.id == group.Key); Undo.RecordObject(character, "Add OT conversations");
                var variants = group.Select(v =>
                {
                    string path = Root + "Data/Studio/Dialogues/" + v.id + ".asset";
                    var node = AssetDatabase.LoadAssetAtPath<DialogueNodeSO>(path);
                    if (node == null)
                    {
                        node = ScriptableObject.CreateInstance<DialogueNodeSO>(); node.id = v.id;
                        node.lines = new[] { new DialogueLine { lineId = v.id + "_line", speaker = character, text = v.text } };
                        AssetDatabase.CreateAsset(node, path);
                    }
                    return new ConversationVariant { id = v.id, priority = 100, anyPhase = false, phase = DayPhase.Night,
                        startDay = 1, endDay = game.totalDays, minimumAffection = v.minimumAffection, maximumAffection = v.maximumAffection,
                        minimumStress = v.minimumStress, maximumStress = v.maximumStress, overtimeOnly = true, node = node };
                }).ToArray();
                character.conversations = character.conversations.Where(v => !variants.Any(n => n.id == v.id)).Concat(variants).ToArray();
                character.greetings = character.greetings.Where(v => !variants.Any(n => n.id == v.id)).Concat(variants).ToArray();
                EditorUtility.SetDirty(character);
            }
            foreach (var folder in new[] { "Icons", "Portraits", "Bars", "Panels" }) Directory.CreateDirectory(Root + "Art/UI/Mockup/" + folder);
            AssetDatabase.Refresh();
            _ = new GameSession(game);
            config.managementVersion = seed.version; EditorUtility.SetDirty(config); AssetDatabase.SaveAssets();
        }
        public static void ApplyFromCommandLine()
        {
            Apply();
            var issues = FoundationValidator.ValidateProject();
            if (issues.Count > 0) throw new InvalidOperationException(string.Join("\n", issues));
            Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/StudioManagementBuild.json", "{\"status\":\"passed\"}");
        }
    }
}
