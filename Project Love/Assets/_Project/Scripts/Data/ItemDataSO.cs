using UnityEngine;
namespace ProjectLove.Data
{
    [CreateAssetMenu(menuName = "Project Love/Item")]
    public sealed class ItemDataSO : IdentifiedDataSO
    {
        public string displayName;
        public Sprite icon;
        [Min(0)] public int price;
        [Min(1)] public int maxStack = 99;
        [Min(0)] public int staminaRecovery;
        [Tooltip("สามารถเลือกมอบให้ตัวละครได้")]
        public bool giftable;
    }
}
