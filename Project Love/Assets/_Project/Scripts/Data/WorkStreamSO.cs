using System;
using UnityEngine;

namespace ProjectLove.Data
{
    [CreateAssetMenu(menuName = "Project Love/Work Stream")]
    public sealed class WorkStreamSO : IdentifiedDataSO
    {
        public string displayName;
        [Tooltip("สีการ์ดและหลอดของฝ่ายนี้")] public Color tint = Color.white;
        public Sprite icon;
    }
    [Serializable]
    public sealed class WorkOutput
    {
        public WorkStreamSO stream;
        [Min(0)] public int amount;
    }
}
