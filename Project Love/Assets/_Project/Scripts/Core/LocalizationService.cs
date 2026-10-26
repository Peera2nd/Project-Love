using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ProjectLove.Data;
using UnityEngine;

namespace ProjectLove.Core
{
    public struct LocalizationResult
    {
        public string text;
        public bool fallback, missing, stale;
    }

    // Per-session lookup. Preview instances never read/write user preferences.
    public sealed class LocalizationService
    {
        public const string PreferenceKey = "ProjectLove.Locale";
        readonly LocalizationSettingsSO settings;
        readonly Dictionary<string, LocalizationEntry> entries = new Dictionary<string, LocalizationEntry>(StringComparer.Ordinal);
        public string Locale { get; private set; } = "th";
        public bool Enabled => settings != null;
        public int Revision { get; private set; }
        public event Action Changed;
        public LocalizationService(LocalizationSettingsSO settings)
        {
            this.settings = settings;
            if (settings == null) return;
            if (settings.catalog == null) throw new ArgumentException("Missing localization catalog.");
            foreach (var e in settings.catalog.entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.key) || entries.ContainsKey(e.key))
                    throw new ArgumentException("Invalid or duplicate localization key.");
                entries.Add(e.key, e);
            }
        }
        public static string Key(IdentifiedDataSO owner, string field) => "content/" + owner.GetType().Name + "/" + owner.id + "/" + field;
        public static string LineKey(DialogueNodeSO node, string lineId) => "dialogue/" + node.id + "/line/" + lineId;
        public static string ChoiceKey(DialogueNodeSO node, string choiceId) => "dialogue/" + node.id + "/choice/" + choiceId;
        public void SetLocale(string locale, bool persist = false)
        {
            locale = locale == "en" ? "en" : "th";
            if (persist && Enabled) { PlayerPrefs.SetString(PreferenceKey, locale); PlayerPrefs.Save(); }
            if (Locale == locale) return;
            Locale = locale; Revision++; Changed?.Invoke();
        }
        public void LoadPreference() { if (Enabled) SetLocale(PlayerPrefs.GetString(PreferenceKey, "th")); }
        public LocalizationResult Resolve(string key, string legacy = "")
        {
            if (!Enabled) return new LocalizationResult { text = legacy ?? "" };
            entries.TryGetValue(key ?? "", out var e);
            bool translated = Locale == "en" && e != null && !e.MissingEnglish && ValidateTranslation(e.thai, e.english) == null;
            string value = translated ? e.english : !string.IsNullOrWhiteSpace(e?.thai) ? e.thai : legacy;
            bool missing = string.IsNullOrWhiteSpace(value);
            return new LocalizationResult { text = missing ? "[missing text]" : value,
                fallback = e == null || Locale == "en" && !translated, missing = missing, stale = translated && e.StaleEnglish };
        }
        public string Text(string key, string legacy = "") => Resolve(key, legacy).text;
        public string Content(IdentifiedDataSO owner, string field, string legacy) => Text(Key(owner, field), legacy);
        public string Line(DialogueNodeSO node, DialogueLine line) => Text(LineKey(node, line.lineId), line.SourceText);
        public string Choice(DialogueNodeSO node, DialogueChoice choice) => Text(ChoiceKey(node, choice.choiceId), choice.SourceText);
        public string Presentation(RoomPresentationSO source, string key) => Content(source, "ui/" + key, source.Get(key));
        public TMPro.TMP_FontAsset Font => Locale == "en" ? settings?.englishFont : settings?.thaiFont;
        public static string Format(string template, params object[] arguments)
        {
            try { return string.Format(CultureInfo.CurrentCulture,template??"",arguments); }
            catch(FormatException) { return "[invalid text format]"; }
        }

        public static string ValidateTranslation(string source, string translation)
        {
            if (string.IsNullOrEmpty(translation)) return null;
            try
            {
                string[] Signature(string s) => Regex.Matches(s ?? "", @"(?<!\{)\{\d+(?:,-?\d+)?(?::[^{}]+)?\}(?!\})").Cast<Match>().Select(m => m.Value).OrderBy(v => v, StringComparer.Ordinal).ToArray();
                if (!Signature(source).SequenceEqual(Signature(translation))) return "รูปแบบตัวแทนค่า {0} ไม่ตรงต้นฉบับ";
                string Tags(string s) => string.Join("|", Regex.Matches(s ?? "", @"</?([A-Za-z]+)(?:=[^>]+)?>").Cast<Match>().Select(m => m.Groups[1].Value + (m.Value.StartsWith("</", StringComparison.Ordinal) ? "/" : "")).OrderBy(v => v, StringComparer.Ordinal));
                if (Tags(source) != Tags(translation)) return "แท็กจัดรูปแบบข้อความไม่ตรงต้นฉบับ";
                int max = Regex.Matches(source ?? "", @"(?<!\{)\{(\d+)").Cast<Match>().Select(m => int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture)).DefaultIfEmpty(-1).Max();
                var args = Enumerable.Repeat<object>(1, max + 1).ToArray();
                string.Format(CultureInfo.InvariantCulture, source ?? "", args);
                string.Format(CultureInfo.InvariantCulture, translation, args);
                return null;
            }
            catch (Exception e) when (e is FormatException || e is OverflowException) { return "รูปแบบข้อความไม่ถูกต้อง"; }
        }
    }
}
