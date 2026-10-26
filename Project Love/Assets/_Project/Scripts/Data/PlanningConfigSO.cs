using System;
using UnityEngine;
namespace ProjectLove.Data
{
    [Serializable] public sealed class PlanningStressBand
    { [Range(0,100)] public int minimum; [Range(.01f,1)] public float mean; [Range(0,.5f)] public float dailySpread; }
    [CreateAssetMenu(menuName="Project Love/Planning Config")]
    public sealed class PlanningConfigSO : IdentifiedDataSO
    {
        [Header("แบบจำลองงาน")]
        public bool randomness = true;
        [Range(0,.5f)] public float taskSpread;
        public PlanningStressBand[] bands = new PlanningStressBand[0];
        [Min(0)] public int normalStress, pressureStress, waitingStress, idleRecovery, overnightRecovery;
        [Range(.1f,1)] public float comfortableLoad;
        [Min(0)] public int sharedAffection;
        [Range(4,64)] public int forecastSamples;
        [Header("การ์ดและการลาก")]
        [Min(1)] public float dragThreshold, autoScrollSpeed;
        [Header("ไทม์ไลน์")]
        [Min(24),Tooltip("ความกว้างต่อหนึ่งช่วงเวลา")] public float timelineCellWidth=32;
        [Min(60),Tooltip("ความสูงต่อการ์ดงาน")] public float timelineRowHeight=76;
    }
}
