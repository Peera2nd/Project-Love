using UnityEngine;
using TMPro;
namespace ProjectLove.Data
{
    [CreateAssetMenu(menuName = "Project Love/Localization Settings")]
    public sealed class LocalizationSettingsSO : IdentifiedDataSO
    {
        [Tooltip("ตารางคำแปลของเกมนี้ ไทยเป็นต้นฉบับและภาษาสำรอง")]
        public LocalizationCatalogSO catalog;
        [Tooltip("เว้นว่างเพื่อใช้ฟอนต์ที่ศิลปินจัดไว้ใน Prefab")]
        public TMP_FontAsset thaiFont, englishFont;
        [HideInInspector] public string legacyRomanceFingerprint, migrationGameplayFingerprint;
    }
}
