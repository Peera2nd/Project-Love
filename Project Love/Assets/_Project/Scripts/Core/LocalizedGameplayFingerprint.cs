using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ProjectLove.Data;

namespace ProjectLove.Core
{
    // Includes serialized mechanics and graph links, excluding presentation and text revisions.
    public static class LocalizedGameplayFingerprint
    {
        static readonly HashSet<string> TextFields = new HashSet<string> { "text", "displayName", "roleTitle", "label", "title", "workTitle", "topicTitle", "dailyHints", "phaseNames", "mainProgressLabel", "description", "localization", "localizedText", "localizationId", "uiContentVersion", "eventContentVersion" };
        public static string Compute(GameConfigSO game)
        {
            var b = new StringBuilder("localized-gameplay-1:");
            var seen = new HashSet<IdentifiedDataSO>();
            void Add(string s) { s = s ?? ""; b.Append(s.Length).Append(':').Append(s); }
            void Walk(object value)
            {
                if (value == null) { Add("null"); return; }
                var type = value.GetType();
                if (value is RoomPresentationSO || value is LocalizationSettingsSO || value is LocalizationCatalogSO || value is LocalizedTextRef) return;
                if (value is IdentifiedDataSO asset)
                {
                    Add(type.FullName); Add(asset.id);
                    if (!seen.Add(asset)) return;
                }
                else if (value is UnityEngine.Object) return;
                else if (value is string s) { Add(s); return; }
                else if (type.IsPrimitive || type.IsEnum || value is decimal)
                { Add(Convert.ToString(value, CultureInfo.InvariantCulture)); return; }
                else if (value is IEnumerable sequence)
                { foreach (var child in sequence) Walk(child); Add("end-array"); return; }
                else if (type.Namespace == "UnityEngine") return;
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(f => f.Name, StringComparer.Ordinal))
                {
                    if (TextFields.Contains(field.Name) || field.IsNotSerialized) continue;
                    Add(field.Name); Walk(field.GetValue(value));
                }
            }
            Walk(game);
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(b.ToString())));
        }
    }
}
