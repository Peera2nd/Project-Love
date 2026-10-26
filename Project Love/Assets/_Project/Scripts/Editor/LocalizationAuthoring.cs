using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.EditorTools
{
    public static class LocalizationAuthoring
    {
        public sealed class Binding
        {
            public IdentifiedDataSO owner;
            public string key, source, path, context;
        }
        public static List<Binding> Inventory(GameConfigSO game)
        {
            var bindings = new List<Binding>();
            void Add(IdentifiedDataSO owner, string field, string path, string value, string context = null)
            { bindings.Add(new Binding { owner = owner, key = LocalizationService.Key(owner, field), path = path, source = value ?? "", context = context ?? owner.name + " · " + field }); }
            var assets = AuthoringCatalog.Reachable(game).Where(a => !(a is LocalizationSettingsSO) && !(a is LocalizationCatalogSO)).ToArray();
            foreach (var a in assets)
            {
                var so = new SerializedObject(a);
                foreach (string field in new[] { "displayName", "roleTitle", "mainProgressLabel" })
                { var p = so.FindProperty(field); if (p != null && p.propertyType == SerializedPropertyType.String) Add(a, field, field, p.stringValue); }
                if (a is DialogueNodeSO node)
                {
                    for (int i = 0; i < node.lines.Length; i++) bindings.Add(new Binding { owner = node, key = LocalizationService.LineKey(node, node.lines[i].lineId), path = "lines.Array.data[" + i + "].text", source = node.lines[i].text ?? "", context = (node.displayName ?? node.name) + " · บรรทัด " + (i + 1) + " · " + node.lines[i].speaker?.displayName });
                    for (int i = 0; i < node.choices.Length; i++) bindings.Add(new Binding { owner = node, key = LocalizationService.ChoiceKey(node, node.choices[i].choiceId), path = "choices.Array.data[" + i + "].text", source = node.choices[i].text ?? "", context = (node.displayName ?? node.name) + " · ตัวเลือก " + (i + 1) });
                }
                if (a is RoomPresentationSO pso)
                {
                    for (int i = 0; i < pso.text.Length; i++) Add(a, "ui/" + pso.text[i].key, "text.Array.data[" + i + "].value", pso.text[i].value);
                    for (int i = 0; i < pso.phaseNames.Length; i++) Add(a, "phase/" + (DayPhase)i, "phaseNames.Array.data[" + i + "]", pso.phaseNames[i]);
                }
            }
            var r = game.romance;
            if (r != null)
            {
                for (int i = 0; i < r.cast.Length; i++)
                    foreach (string f in new[] { "workTitle", "topicTitle" }) Add(r, "cast/" + r.cast[i].character.id + "/" + f, "cast.Array.data[" + i + "]." + f, f == "workTitle" ? r.cast[i].workTitle : r.cast[i].topicTitle);
                for (int i = 0; i < r.moods.Length; i++) Add(r, "mood/" + r.moods[i].mood, "moods.Array.data[" + i + "].label", r.moods[i].label);
                for (int i = 0; i < r.dailyHints.Length; i++) Add(r, "day/" + (i + 1) + "/hint", "dailyHints.Array.data[" + i + "]", r.dailyHints[i]);
                for (int i = 0; i < r.milestones.Length; i++)
                {
                    if (string.IsNullOrEmpty(r.milestones[i].localizationId)) { Undo.RecordObject(r, "Assign milestone text identity"); r.milestones[i].localizationId = Guid.NewGuid().ToString("N"); EditorUtility.SetDirty(r); }
                    Add(r, "milestone/" + r.milestones[i].localizationId, "milestones.Array.data[" + i + "].title", r.milestones[i].title);
                }
            }
            var duplicate = bindings.GroupBy(b => b.key).FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null) throw new InvalidOperationException("รหัสข้อความซ้ำ: " + duplicate.Key);
            return bindings;
        }
        public static LocalizationEntry Ensure(GameConfigSO game, IdentifiedDataSO owner, string key, string source, string context = "")
        {
            var catalog = game.localization?.catalog; if (catalog == null) return null;
            var entry = catalog.Find(key); if (entry != null) return entry;
            Undo.RecordObject(catalog, "Add localized text");
            entry = new LocalizationEntry { key = key, thai = source ?? "", legacy = source ?? "", sourcePath = AssetDatabase.GetAssetPath(owner), context = context };
            catalog.entries = catalog.entries.Concat(new[] { entry }).ToArray(); EditorUtility.SetDirty(catalog); return entry;
        }
        public static void SetText(LocalizationCatalogSO catalog, LocalizationEntry entry, string thai, string english, bool reviewed)
        {
            string error = LocalizationService.ValidateTranslation(thai, english); if (error != null) throw new ArgumentException(error);
            Undo.RecordObject(catalog, "Edit localized text");
            if (entry.thai != thai) { entry.thai = thai; entry.sourceRevision++; }
            entry.english = english; entry.englishReviewedRevision = reviewed ? entry.sourceRevision : 0; EditorUtility.SetDirty(catalog);
        }
        public static void DrawText(GameConfigSO game, IdentifiedDataSO owner, string key, SerializedProperty raw, string label, bool multiline = true)
        {
            if (game.localization == null) { if (multiline) { EditorGUILayout.LabelField(label); raw.stringValue = EditorGUILayout.TextArea(raw.stringValue, GUILayout.MinHeight(64)); } else raw.stringValue = EditorGUILayout.TextField(label, raw.stringValue); return; }
            var entry = Ensure(game, owner, key, raw.stringValue, owner.name + " · " + label);
            EditorGUILayout.LabelField(label + " · ไทย (ต้นฉบับตารางคำแปล)", EditorStyles.wordWrappedMiniLabel);
            string value = multiline ? EditorGUILayout.TextArea(entry.thai ?? "", GUILayout.MinHeight(64)) : EditorGUILayout.TextField(entry.thai ?? "");
            if (value != entry.thai) { Undo.RecordObject(game.localization.catalog, "Edit Thai source"); entry.thai = value; entry.sourceRevision++; EditorUtility.SetDirty(game.localization.catalog); }
            EditorGUILayout.LabelField("อังกฤษ: " + (entry.MissingEnglish ? "ยังไม่แปล · ใช้ไทยสำรอง" : entry.StaleEnglish ? "ต้องตรวจใหม่" : "ตรวจแล้ว"), EditorStyles.miniLabel);
            if (GUILayout.Button("เปิดคำแปลของข้อความนี้", GUILayout.MaxWidth(240))) DesignerDashboard.OpenTranslation(game, key);
        }
        public static void BindNode(GameConfigSO game, DialogueNodeSO node)
        {
            if (game.localization == null) return;
            if (node.lines.All(l => l.localizedText.catalog == game.localization.catalog && l.localizedText.key == LocalizationService.LineKey(node, l.lineId)) &&
                node.choices.All(c => c.localizedText.catalog == game.localization.catalog && c.localizedText.key == LocalizationService.ChoiceKey(node, c.choiceId))) return;
            Undo.RecordObject(node, "Bind dialogue localization");
            foreach (var l in node.lines) { string key = LocalizationService.LineKey(node, l.lineId); Ensure(game, node, key, l.text, node.displayName + " · " + l.speaker?.displayName); l.localizedText = new LocalizedTextRef { key = key, catalog = game.localization.catalog }; }
            foreach (var c in node.choices) { string key = LocalizationService.ChoiceKey(node, c.choiceId); Ensure(game, node, key, c.text, node.displayName + " · ตัวเลือก"); c.localizedText = new LocalizedTextRef { key = key, catalog = game.localization.catalog }; }
            EditorUtility.SetDirty(node);
        }
        public static void DrawRomanceTextSettings(GameConfigSO game, SerializedObject so)
        {
            var r=game.romance;
            if(game.localization==null)
            {
                foreach(string field in new[]{"dailyHints","milestones","moods"})EditorGUILayout.PropertyField(so.FindProperty(field),true);
                return;
            }
            EditorGUILayout.LabelField("ข้อความจากตารางภาษา · ค่าตัวเลขและไอคอนยังแก้ที่นี่ได้",EditorStyles.wordWrappedMiniLabel);
            var hints=so.FindProperty("dailyHints");
            hints.arraySize=Mathf.Max(0,EditorGUILayout.IntField("จำนวนคำแนะนำรายวัน",hints.arraySize));
            for(int i=0;i<hints.arraySize;i++)DrawText(game,r,LocalizationService.Key(r,"day/"+(i+1)+"/hint"),hints.GetArrayElementAtIndex(i),"คำแนะนำวันที่ "+(i+1),false);
            var goals=so.FindProperty("milestones");
            goals.arraySize=Mathf.Max(0,EditorGUILayout.IntField("จำนวนช่วงเป้าหมาย",goals.arraySize));
            for(int i=0;i<goals.arraySize;i++)
            {
                var m=goals.GetArrayElementAtIndex(i);var id=m.FindPropertyRelative("localizationId");if(string.IsNullOrEmpty(id.stringValue))id.stringValue=Guid.NewGuid().ToString("N");
                EditorGUILayout.LabelField("เป้าหมายช่วง "+(i+1),EditorStyles.boldLabel);
                foreach(string f in new[]{"startDay","endDay","target"})EditorGUILayout.PropertyField(m.FindPropertyRelative(f));
                DrawText(game,r,LocalizationService.Key(r,"milestone/"+id.stringValue),m.FindPropertyRelative("title"),"ชื่อเป้าหมาย",false);
            }
            var moods=so.FindProperty("moods");
            for(int i=0;i<moods.arraySize;i++)
            {
                var m=moods.GetArrayElementAtIndex(i);EditorGUILayout.PropertyField(m.FindPropertyRelative("mood"));EditorGUILayout.PropertyField(m.FindPropertyRelative("icon"));
                DrawText(game,r,LocalizationService.Key(r,"mood/"+(CharacterMood)m.FindPropertyRelative("mood").enumValueIndex),m.FindPropertyRelative("label"),"ชื่ออารมณ์",false);
            }
        }
        public static void Install(GameConfigSO game)
        {
            if (game == null || game.romance == null) throw new ArgumentException("เลือกแคมเปญโรแมนซ์ก่อน");
            var baseline = new GameSession(game);
            var bindings = Inventory(game);
            if (game.localization == null)
            {
                const string folder = "Assets/_Project/Data/Localization";
                if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/_Project/Data", "Localization");
                var catalog = ScriptableObject.CreateInstance<LocalizationCatalogSO>(); catalog.id = game.id + "_texts";
                var settings = ScriptableObject.CreateInstance<LocalizationSettingsSO>(); settings.id = game.id + "_localization"; settings.catalog = catalog;
                AssetDatabase.CreateAsset(catalog, AssetDatabase.GenerateUniqueAssetPath(folder + "/" + catalog.id + ".asset"));
                AssetDatabase.CreateAsset(settings, AssetDatabase.GenerateUniqueAssetPath(folder + "/" + settings.id + ".asset"));
                settings.legacyRomanceFingerprint = baseline.Romance.LegacyFingerprint();
                Undo.RecordObject(game, "Enable Thai English localization"); game.localization = settings; EditorUtility.SetDirty(game);
            }
            foreach (var b in bindings) Ensure(game, b.owner, b.key, b.source, b.context);
            foreach (var node in bindings.Select(b => b.owner).OfType<DialogueNodeSO>().Distinct()) BindNode(game, node);
            var p = game.romance.presentation;
            var th = Ensure(game, p, LocalizationService.Key(p, "ui/languageThai"), "ภาษา: ไทย", "เมนูเลือกภาษา");
            var en = Ensure(game, p, LocalizationService.Key(p, "ui/languageEnglish"), "ภาษา: อังกฤษ", "เมนูเลือกภาษา");
            if (string.IsNullOrEmpty(th.english)) { th.english = "Language: Thai"; th.englishReviewedRevision = 0; }
            if (string.IsNullOrEmpty(en.english)) { en.english = "Language: English"; en.englishReviewedRevision = 0; }
            if (string.IsNullOrEmpty(game.localization.migrationGameplayFingerprint)) { game.localization.migrationGameplayFingerprint = LocalizedGameplayFingerprint.Compute(game); EditorUtility.SetDirty(game.localization); }
            AssetDatabase.SaveAssets();
        }
        public static void InstallFromCommandLine()
        {
            Install(AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath));
            Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/Session064_Installed.txt", "Localization enabled on existing Romance campaign; Thai seeded, English incomplete.");
        }
    }
}
