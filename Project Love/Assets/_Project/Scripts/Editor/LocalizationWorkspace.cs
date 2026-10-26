using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProjectLove.Core;
using ProjectLove.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.EditorTools
{
    public static class LocalizationCsv
    {
        static readonly string[] Header = { "key", "thai", "english", "context", "sourceRevision", "englishReviewedRevision" };
        static string Cell(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
        public static string Export(LocalizationCatalogSO catalog) => string.Join(",", Header) + "\r\n" + string.Join("\r\n", catalog.entries.Select(e => string.Join(",", new[] { e.key, e.thai, e.english, e.context, e.sourceRevision.ToString(System.Globalization.CultureInfo.InvariantCulture), e.englishReviewedRevision.ToString(System.Globalization.CultureInfo.InvariantCulture) }.Select(Cell)))) + "\r\n";
        public static List<string[]> Parse(string csv)
        {
            var rows = new List<string[]>(); var row = new List<string>(); var cell = new StringBuilder(); bool quoted = false, closed = false;
            csv = (csv ?? "").TrimStart('\uFEFF');
            for (int i = 0; i < csv.Length; i++)
            {
                char c = csv[i];
                if (quoted) { if (c == '"') { if (i + 1 < csv.Length && csv[i + 1] == '"') { cell.Append('"'); i++; } else { quoted = false; closed = true; } } else cell.Append(c); continue; }
                if (c == '"') { if (cell.Length != 0 || closed) throw new FormatException("CSV มีเครื่องหมายคำพูดผิดตำแหน่ง"); quoted = true; }
                else if (c == ',' || c == '\r' || c == '\n')
                {
                    row.Add(cell.ToString()); cell.Clear(); closed = false;
                    if (c != ',') { if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++; rows.Add(row.ToArray()); row.Clear(); }
                }
                else { if (closed) throw new FormatException("CSV มีข้อความหลังปิดคำพูด"); cell.Append(c); }
            }
            if (quoted) throw new FormatException("CSV ปิดคำพูดไม่ครบ");
            if (cell.Length > 0 || row.Count > 0 || closed) { row.Add(cell.ToString()); rows.Add(row.ToArray()); }
            return rows;
        }
        public sealed class ImportPlan
        {
            public readonly List<string> errors = new List<string>();
            public readonly List<string[]> changes = new List<string[]>();
            public string original;
        }
        public static ImportPlan Preview(LocalizationCatalogSO catalog, string csv)
        {
            var plan = new ImportPlan { original = Export(catalog) };
            try
            {
                var rows = Parse(csv);
                if (rows.Count == 0 || !rows[0].SequenceEqual(Header)) throw new FormatException("หัวตารางไม่ตรงไฟล์ที่ส่งออก");
                var seen = new HashSet<string>();
                foreach (var r in rows.Skip(1))
                {
                    if (r.Length != 6) { plan.errors.Add("จำนวนช่องไม่ครบ"); continue; }
                    var e = catalog.Find(r[0]);
                    if (!seen.Add(r[0]) || e == null) { plan.errors.Add("รหัสซ้ำหรือไม่รู้จัก: " + r[0]); continue; }
                    if (r[1] != e.thai || !int.TryParse(r[4], out int revision) || revision != e.sourceRevision) { plan.errors.Add("ต้นฉบับเปลี่ยนแล้ว: " + r[0]); continue; }
                    if (!int.TryParse(r[5], out int reviewed) || reviewed != 0 && reviewed != revision) { plan.errors.Add("รุ่นการตรวจไม่ถูกต้อง: " + r[0]); continue; }
                    string error = LocalizationService.ValidateTranslation(e.thai, r[2]); if (error != null) { plan.errors.Add(r[0] + ": " + error); continue; }
                    if (r[2] != e.english || reviewed != e.englishReviewedRevision) plan.changes.Add(r);
                }
            }
            catch (FormatException e) { plan.errors.Add(e.Message); }
            return plan;
        }
        public static void Apply(LocalizationCatalogSO catalog, ImportPlan plan)
        {
            if (plan == null || plan.errors.Count != 0 || plan.original != Export(catalog)) throw new InvalidOperationException("ข้อมูลเปลี่ยนหลังดูตัวอย่าง กรุณานำเข้าใหม่");
            Undo.RecordObject(catalog, "Import English translations");
            foreach (var r in plan.changes) { var e = catalog.Find(r[0]); e.english = r[2]; e.englishReviewedRevision = int.Parse(r[5]); }
            EditorUtility.SetDirty(catalog);
        }
    }
    public static class LocalizationWorkspace
    {
        static Vector2 scroll, detail;
        static string query = "", selectedKey, thai = "", english = "", notice = "";
        static int filter;
        static bool fonts;
        static LocalizationCatalogSO selectedCatalog;
        static LocalizationCsv.ImportPlan import;
        public static void Select(string key) { selectedKey = key; selectedCatalog = null; query = ""; filter = 0; }
        public static void Invalidate() { selectedCatalog = null; import = null; }
        public static bool SavePending(GameConfigSO game)
        {
            var catalog=game?.localization?.catalog;
            var entry=catalog?.Find(selectedKey);
            if(catalog==null||selectedCatalog!=catalog||entry==null||thai==entry.thai&&english==entry.english)return true;
            return Commit(catalog,entry,false);
        }
        public static void Draw(GameConfigSO game)
        {
            EditorGUILayout.HelpBox("ไทยเป็นต้นฉบับ · อังกฤษที่ยังไม่มีใช้ไทยสำรอง · แก้ที่ตารางนี้หรือหน้าเขียนบท แล้วบันทึกข้อมูล · ภาษาเอกสาร .md ไม่ถูกนำเข้าอัตโนมัติ", MessageType.Info);
            if (game.localization == null)
            {
                EditorGUILayout.HelpBox("แคมเปญนี้ยังใช้ข้อความแบบเดิม ปุ่มด้านล่างจะเพิ่มตารางภาษาโดยรักษารหัส บท เงื่อนไข และไฟล์เดิม", MessageType.Warning);
                if (GUILayout.Button("เริ่มใช้ระบบภาษาไทย–อังกฤษในเกมนี้")) { LocalizationAuthoring.Install(game); notice = "สร้างตารางแล้ว คำแปลอังกฤษยังไม่ครบ"; }
                return;
            }
            var catalog = game.localization.catalog;
            fonts=EditorGUILayout.Foldout(fonts,"ฟอนต์แต่ละภาษา (ว่าง = ใช้ฟอนต์เดิมของ Prefab)",true);
            if(fonts)
            {
                var settings=new SerializedObject(game.localization);settings.Update();
                EditorGUILayout.PropertyField(settings.FindProperty("thaiFont"),new GUIContent("ฟอนต์ภาษาไทย"));
                EditorGUILayout.PropertyField(settings.FindProperty("englishFont"),new GUIContent("ฟอนต์ภาษาอังกฤษ"));
                settings.ApplyModifiedProperties();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("บันทึกคำแปล")&&SavePending(game)) { AssetDatabase.SaveAssets(); notice = "บันทึกตารางแล้ว"; }
                if (GUILayout.Button("ค้นข้อความใหม่เข้าในตาราง")) { LocalizationAuthoring.Install(game); selectedCatalog = null; notice = "เพิ่มเฉพาะรหัสใหม่ ไม่เขียนทับคำที่แก้แล้ว"; }
                if (GUILayout.Button("ส่งออก CSV")&&SavePending(game)) { string path = EditorUtility.SaveFilePanel("ส่งออกคำแปล", "", "Thai-English.csv", "csv"); if (path.Length > 0) try { File.WriteAllText(path, LocalizationCsv.Export(catalog), new UTF8Encoding(true)); } catch(Exception e) when(e is IOException||e is UnauthorizedAccessException) { notice="ส่งออกไม่ได้: "+e.Message; } }
                if (GUILayout.Button("นำเข้า CSV")&&SavePending(game)) { string path = EditorUtility.OpenFilePanel("นำเข้าคำแปลอังกฤษ", "", "csv"); if (path.Length > 0) try { import = LocalizationCsv.Preview(catalog, File.ReadAllText(path, Encoding.UTF8)); } catch(Exception e) when(e is IOException||e is UnauthorizedAccessException) { notice="อ่านไฟล์ไม่ได้: "+e.Message; } }
                if (GUILayout.Button("ดูตัวอย่างสองภาษา")) ArtistPreviewWindow.Open(game);
            }
            EditorGUILayout.LabelField("เกมนี้ · " + catalog.entries.Length + " ข้อความ | ยังไม่แปล " + catalog.entries.Count(e => e.MissingEnglish) + " | ต้องตรวจ " + catalog.entries.Count(e => e.StaleEnglish), EditorStyles.boldLabel);
            query = EditorGUILayout.TextField("ค้นข้อความ / ฉาก / ผู้พูด / รหัส", query);
            filter = GUILayout.Toolbar(filter, new[] { "ทั้งหมด", "ยังไม่แปล", "ต้องตรวจใหม่" });
            if (import != null)
            {
                EditorGUILayout.HelpBox("ตัวอย่างนำเข้า: " + import.changes.Count + " รายการเปลี่ยน · " + import.errors.Count + " ข้อผิดพลาด\n" + string.Join("\n", import.errors.Take(5)), import.errors.Count > 0 ? MessageType.Error : MessageType.Info);
                foreach (var row in import.changes.Take(3)) EditorGUILayout.LabelField(row[0] + " → " + row[2], EditorStyles.wordWrappedMiniLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(import.errors.Count > 0)) if (GUILayout.Button("ยืนยันนำเข้ารายการที่ตรวจแล้ว")) { try { LocalizationCsv.Apply(catalog, import); AssetDatabase.SaveAssetIfDirty(catalog); selectedCatalog = null; notice = "นำเข้าคำแปลแล้ว"; } catch (Exception e) { notice = e.Message; } import = null; }
                    if (GUILayout.Button("ยกเลิกนำเข้า")) import = null;
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(310), GUILayout.MinHeight(350));
                foreach (var e in catalog.entries.Where(e => (filter != 1 || e.MissingEnglish) && (filter != 2 || e.StaleEnglish) && (string.IsNullOrWhiteSpace(query) || (e.key + " " + e.context + " " + e.thai + " " + e.english).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)))
                {
                    var rect = GUILayoutUtility.GetRect(280, 64, GUILayout.ExpandWidth(true));
                    if (selectedKey == e.key) EditorGUI.DrawRect(rect, new Color(.18f, .32f, .43f));
                    if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                    {
                        var previous=catalog.Find(selectedKey);
                        bool dirty=selectedCatalog==catalog&&previous!=null&&(thai!=previous.thai||english!=previous.english);
                        if(!dirty||EditorUtility.DisplayDialog("ยังไม่ได้บันทึกข้อความ", "ต้องการทิ้งการแก้ในช่องนี้แล้วเปลี่ยนข้อความหรือไม่", "เปลี่ยนข้อความ", "กลับไปบันทึก"))
                        {selectedKey = e.key; selectedCatalog = null; GUI.FocusControl(null);}
                    }
                    var style = new GUIStyle(EditorStyles.label) { clipping = TextClipping.Clip, wordWrap = false };
                    GUI.Label(new Rect(rect.x + 5, rect.y + 4, rect.width - 10, 20), e.context, style);
                    GUI.Label(new Rect(rect.x + 5, rect.y + 24, rect.width - 10, 20), (e.thai ?? "").Replace('\n', ' '), style);
                    GUI.Label(new Rect(rect.x + 5, rect.y + 44, rect.width - 10, 18), e.MissingEnglish ? "ยังไม่แปล" : e.StaleEnglish ? "ต้องตรวจใหม่" : "ตรวจแล้ว", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndScrollView();
                detail = EditorGUILayout.BeginScrollView(detail, GUILayout.MinHeight(350));
                var entry = catalog.Find(selectedKey);
                if (entry != null)
                {
                    if (selectedCatalog != catalog) { thai = entry.thai; english = entry.english; selectedCatalog = catalog; }
                    EditorGUILayout.LabelField(entry.context, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.SelectableLabel(entry.key, EditorStyles.wordWrappedMiniLabel, GUILayout.Height(38));
                    EditorGUILayout.LabelField("ไทย · ต้นฉบับ"); thai = EditorGUILayout.TextArea(thai ?? "", GUILayout.MinHeight(110));
                    EditorGUILayout.LabelField("อังกฤษ · เว้นว่างเพื่อใช้ไทยสำรอง"); english = EditorGUILayout.TextArea(english ?? "", GUILayout.MinHeight(110));
                    EditorGUILayout.LabelField("ต้นฉบับรุ่น " + entry.sourceRevision + " · อังกฤษตรวจเทียบรุ่น " + entry.englishReviewedRevision, EditorStyles.miniLabel);
                    if (GUILayout.Button("บันทึกข้อความนี้ / ยังเป็นฉบับร่าง")) Commit(catalog, entry, false);
                    using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(english))) if (GUILayout.Button("บันทึกและยืนยันตรวจคำแปลเทียบต้นฉบับแล้ว")) Commit(catalog, entry, true);
                    if (GUILayout.Button("เลือกไฟล์ต้นทาง")) { var asset = AssetDatabase.LoadMainAssetAtPath(entry.sourcePath); Selection.activeObject = asset; EditorGUIUtility.PingObject(asset); }
                    EditorGUILayout.HelpBox("ช่องข้อความเดิมในไฟล์ต้นทางเก็บไว้เพื่อรองรับรุ่นเก่า ข้อความที่เกมรุ่นนี้ใช้มาจากตารางนี้", MessageType.None);
                }
                else EditorGUILayout.HelpBox("เลือกข้อความด้านซ้ายเพื่อแก้ไทยและอังกฤษ", MessageType.Info);
                EditorGUILayout.EndScrollView();
            }
            if (!string.IsNullOrEmpty(notice)) EditorGUILayout.HelpBox(notice, MessageType.Info);
        }
        static bool Commit(LocalizationCatalogSO catalog, LocalizationEntry entry, bool reviewed)
        {
            try { LocalizationAuthoring.SetText(catalog, entry, thai, english, reviewed); AssetDatabase.SaveAssetIfDirty(catalog); notice = "บันทึกข้อความแล้ว"; return true; }
            catch (ArgumentException e) { notice = e.Message; return false; }
        }
    }
}
