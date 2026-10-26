using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Data;
using UnityEditor;

namespace ProjectLove.EditorTools
{
    public static class FoundationValidator
    {
        public static List<string> ValidateProject()
        {
            var assets = AssetDatabase.FindAssets("t:IdentifiedDataSO", new[] { "Assets/_Project" })
                .Select(g => AssetDatabase.LoadAssetAtPath<IdentifiedDataSO>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
            var issues = new List<string>();
            var ids = new HashSet<string>();
            var lineIds = new HashSet<string>();
            var choiceIds = new HashSet<string>();
            foreach (var asset in assets)
            {
                if (string.IsNullOrWhiteSpace(asset.id) || !ids.Add(asset.GetType().Name + ":" + asset.id))
                    issues.Add(asset.name + ": missing or duplicate stable ID.");
                if (asset is DialogueNodeSO node)
                {
                    if (!VN.DialogueRunner.IsPlayable(node)) issues.Add(node.name + ": requires valid lines with IDs and text.");
                    if (node.lines != null)
                        foreach (var line in node.lines)
                            if (line != null && !lineIds.Add(line.lineId ?? "")) issues.Add(node.name + ": duplicate line ID.");
                    if (node.choices == null) { issues.Add(node.name + ": null choices."); continue; }
                    if (node.choices.Length > 0 && node.nextNode != null) issues.Add(node.name + ": choose either choices or a default next link.");
                    foreach (var choice in node.choices)
                    {
                        if (choice == null) { issues.Add(node.name + ": null choice."); continue; }
                        if (string.IsNullOrWhiteSpace(choice.choiceId) || !choiceIds.Add(choice.choiceId)) issues.Add(node.name + ": missing or duplicate choice ID.");
                        if (string.IsNullOrWhiteSpace(choice.SourceText) || choice.actionPointCost < 0) issues.Add(node.name + ": invalid choice text/cost.");
                    }
                }
                if (asset is CharacterDataSO character && string.IsNullOrWhiteSpace(character.displayName)) issues.Add(character.name + ": missing display name.");
                if (asset is CharacterDataSO person)
                {
                    ValidateVariants(person.name, person.greetings, issues);
                    ValidateVariants(person.name, person.conversations, issues);
                    if (person.dailyTalkStaminaCost < 0 || person.dailyTalkAffection < 0 || person.dailyTalkAffection > 100 ||
                        person.giftPreferences == null || person.giftPreferences.Any(g => g == null || g.item == null ||
                            !Enum.IsDefined(typeof(GiftReaction), g.reaction) || g.affectionChange < -100 || g.affectionChange > 100 ||
                            !VN.DialogueRunner.IsPlayable(g.response ?? person.neutralGiftResponse)) ||
                        person.giftPreferences.Where(g => g != null).Select(g => g.item).Distinct().Count() != person.giftPreferences.Length)
                        issues.Add(person.name + ": invalid social costs/gift preferences.");
                }
                if (asset is ActivityDataSO work && (work.successWork == null || work.failureWork == null ||
                    work.successWork.Concat(work.failureWork).Any(o => o == null || o.stream == null || o.amount < 0)))
                    issues.Add(work.name + ": invalid work output.");
                if (asset is ActivityDataSO activity && (activity.actionPointCost < 0 || activity.staminaCost < 0 || activity.staminaRecovery < 0 || activity.elapsedPhases < 0 || activity.elapsedPhases > 4))
                    issues.Add(activity.name + ": invalid activity costs/duration.");
                if (asset is EventDataSO ev && (ev.staminaCost < 0 || ev.elapsedPhases < 0 || ev.elapsedPhases > 4 || !Enum.IsDefined(typeof(DayPhase), ev.triggerPhase)))
                    issues.Add(ev.name + ": invalid event cost/phase.");
                if (asset is RoomDataSO room)
                {
                    var hotspotIds = new HashSet<string>();
                    if (room.phases == null || room.phases.Length == 0 || room.phases.Any(p => p == null || p.background == null) ||
                        room.phases.Select(p => p.phase).Distinct().Count() != room.phases.Length) issues.Add(room.name + ": missing/duplicate room art phase.");
                    if (room.hotspots == null) { issues.Add(room.name + ": null hotspots."); continue; }
                    foreach (var hotspot in room.hotspots)
                    {
                        if (hotspot == null || string.IsNullOrWhiteSpace(hotspot.id) || !hotspotIds.Add(hotspot.id))
                        { issues.Add(room.name + ": missing/duplicate hotspot ID."); continue; }
                        if (hotspot.placements == null || hotspot.placements.Length == 0 || hotspot.placements.Any(p => p == null || p.size.x <= 0 || p.size.y <= 0 ||
                            !Enum.IsDefined(typeof(DayPhase), p.phase)) || hotspot.placements.Select(p => p.phase).Distinct().Count() != hotspot.placements.Length)
                            issues.Add(room.name + ": invalid hotspot placements.");
                        if ((hotspot.kind == RoomInteractionKind.Character && (hotspot.character == null || hotspot.conversation == null)) ||
                            (hotspot.kind == RoomInteractionKind.Activity && hotspot.activity == null) ||
                            (hotspot.kind == RoomInteractionKind.Pickup && hotspot.item == null)) issues.Add(room.name + ": missing hotspot target.");
                    }
                }
            }
            foreach (var config in assets.OfType<GameConfigSO>())
            {
                try { _ = new Core.GameSession(config); }
                catch (System.ArgumentException e) { issues.Add(config.name + ": " + e.Message); }
                foreach (var data in config.events.Concat(config.rooms.SelectMany(r => r.hotspots).Where(h => h.conversation != null).Select(h => h.conversation)).Distinct())
                    if (data.startDay < 1 || data.startDay > data.preferredDay || data.preferredDay > data.endDay || data.endDay > config.totalDays)
                        issues.Add(data.name + ": event window outside " + config.name + ".");
            }
            foreach (var data in assets.OfType<EventDataSO>())
                if (!VN.DialogueRunner.IsPlayable(data.entryNode)) issues.Add(data.name + ": missing or invalid entry node.");
            return issues;
        }
        private static void ValidateVariants(string name, ConversationVariant[] variants, List<string> issues)
        {
            if (variants == null) { issues.Add(name + ": null conversation variants."); return; }
            var ids = new HashSet<string>();
            foreach (var variant in variants)
                if (variant == null || string.IsNullOrWhiteSpace(variant.id) || !ids.Add(variant.id) || variant.startDay < 1 ||
                    variant.endDay < variant.startDay || variant.minimumAffection < 0 || variant.maximumAffection > 100 ||
                    variant.minimumAffection > variant.maximumAffection || !Enum.IsDefined(typeof(DayPhase), variant.phase) ||
                    variant.minimumStress < 0 || variant.maximumStress > 100 || variant.minimumStress > variant.maximumStress ||
                    variant.conditions == null || !variant.Nodes.Any() || variant.Nodes.Any(n=>!VN.DialogueRunner.IsPlayable(n))) issues.Add(name + ": invalid conversation variant.");
        }
    }
}
