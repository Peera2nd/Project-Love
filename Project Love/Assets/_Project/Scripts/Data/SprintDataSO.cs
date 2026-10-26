using System;
using UnityEngine;

namespace ProjectLove.Data
{
    [Serializable] public sealed class StreamQuota { public WorkStreamSO stream; [Min(0)] public int effort; }
    [CreateAssetMenu(menuName = "Project Love/Sprint")]
    public sealed class SprintDataSO : IdentifiedDataSO
    {
        public string displayName;
        [Min(1)] public int startDay, endDay;
        public StreamQuota[] quotas = new StreamQuota[0];
        [Min(0), Tooltip("เป้างาน Producer ของขั้นนี้ เมื่อเต็มปลดล็อคขั้นถัดไป")] public int producerGoal;
    }
}
