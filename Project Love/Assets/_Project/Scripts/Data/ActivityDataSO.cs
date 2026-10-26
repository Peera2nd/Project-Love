using UnityEngine;

namespace ProjectLove.Data
{
    [CreateAssetMenu(menuName = "Project Love/Activity")]
    public sealed class ActivityDataSO : IdentifiedDataSO
    {
        public string displayName;
        [Min(0)] public int actionPointCost;
        [Min(0)] public int staminaCost;
        [Min(0)] public int staminaRecovery;
        [Range(0, 4), Tooltip("จำนวนช่วงเวลาที่ผ่านเมื่อกิจกรรมเสร็จ") ] public int elapsedPhases;
        public Condition[] conditions = new Condition[0];
        public StatModifier[] successEffects = new StatModifier[0];
        public StatModifier[] failureEffects = new StatModifier[0];
        public FlagChange[] successFlags = new FlagChange[0];
        public FlagChange[] failureFlags = new FlagChange[0];
        public WorkOutput[] successWork = new WorkOutput[0];
        public WorkOutput[] failureWork = new WorkOutput[0];
    }
}
