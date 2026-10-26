using System;
using UnityEngine;

namespace ProjectLove.Data
{
    [Serializable] public sealed class WorkerDefinition
    {
        public CharacterDataSO character;
        public WorkStreamSO stream;
        [Min(1)] public int baseRate;
    }
    [Serializable] public sealed class StressBand
    {
        [Range(0, 100)] public int minimum;
        [Range(0, 100)] public int outputPercent;
    }
    [Serializable] public sealed class WorkloadEvent
    {
        public string id, displayName;
        [Min(1)] public int earliestDay, latestDay;
        public bool automatic;
        public WorkTaskSO[] tasks = new WorkTaskSO[0];
        public StreamQuota[] extraQuotas = new StreamQuota[0];
        public CharacterDataSO absentWorker;
        [Min(0)] public int absenceDays;
    }
    [CreateAssetMenu(menuName = "Project Love/Production Config")]
    public sealed class ProductionConfigSO : IdentifiedDataSO
    {
        public PlanningConfigSO planning;
        public WorkerDefinition[] workers = new WorkerDefinition[0];
        public StressBand[] stressBands = new StressBand[0];
        public DayPhase[] productivePhases = new DayPhase[0];
        public WorkTaskSO[] tasks = new WorkTaskSO[0];
        public SprintDataSO[] sprints = new SprintDataSO[0];
        public WorkloadEvent[] events = new WorkloadEvent[0];
        [Min(0)] public int workStress, overnightStressRecovery;
        [Min(0)] public int helpCost, helpEffort, helpStressRecovery;
        [Min(0)] public int coordinateCost, coordinateBonus, coordinateTicks;
        [Min(0)] public int clarifyCost, careCost, careStressRecovery;
        public RoomPresentationSO presentation;
        [Tooltip("ปลดล็อค Sprint ตามงาน Producer แทนการรอวัน")] public bool useProducerStages;
        [Min(0)] public int producerCost, producerEffort, producerMoney, producerTeamBonus;
        [Min(0)] public int restStressRecovery, overtimeMoneyCost, overtimeStress;
        [Min(0), Tooltip("ความสัมพันธ์เมื่อช่วยงานกลางคืนจริงหนึ่งช่วง")]
        public int sharedNightAffection;
        [HideInInspector] public int nightContentVersion;
        [HideInInspector] public int managementVersion;
        [HideInInspector] public int dependencyContentVersion;
    }
}
