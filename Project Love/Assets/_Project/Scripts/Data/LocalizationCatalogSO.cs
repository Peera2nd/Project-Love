using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ProjectLove.Data
{
    [Serializable]
    public struct LocalizedTextRef
    {
        public LocalizationCatalogSO catalog;
        public string key;
        public string Source(string fallback) => catalog != null ? catalog.Find(key)?.thai ?? fallback : fallback;
    }

    [Serializable]
    public sealed class LocalizationEntry
    {
        public string key, context, sourcePath, legacy;
        [TextArea(2, 8)] public string thai, english;
        public int sourceRevision = 1, englishReviewedRevision;
        public bool MissingEnglish => string.IsNullOrWhiteSpace(english);
        public bool StaleEnglish => !MissingEnglish && englishReviewedRevision != sourceRevision;
    }

    [CreateAssetMenu(menuName = "Project Love/Localization Catalog")]
    public sealed class LocalizationCatalogSO : IdentifiedDataSO
    {
        public LocalizationEntry[] entries = Array.Empty<LocalizationEntry>();
        public LocalizationEntry Find(string key) => entries.FirstOrDefault(e => e.key == key);
    }
}
