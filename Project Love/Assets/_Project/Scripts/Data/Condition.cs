using System;

namespace ProjectLove.Data
{
    public enum DayPhase { Morning, Daytime, Evening, Night }
    public enum StatType { MainProgress, Money, SkillPoints, Affection, Stress }
    public enum ConditionType { StatAtLeast, StatAtMost, FlagSet, FlagUnset, EventCompleted, EventNotCompleted,
        WorkloadActivated, WorkloadNotActivated, TaskAccepted, TaskNotAccepted, CharacterOvertime,
        CharacterCanVolunteer, CharacterSharedNight }

    [Serializable]
    public sealed class Condition
    {
        public ConditionType type;
        public StatType stat;
        public CharacterDataSO character;
        public int value;
        public string flagId;
        public string eventId;
        [UnityEngine.Tooltip("รหัส Workload หรือการ์ดงาน ตามชนิดเงื่อนไข")]
        public string targetId;
    }

    [Serializable]
    public sealed class StatModifier
    {
        public StatType stat;
        public CharacterDataSO character;
        public int amount;
    }

    [Serializable]
    public sealed class FlagChange
    {
        public string flagId;
        public bool value = true;
    }
}
