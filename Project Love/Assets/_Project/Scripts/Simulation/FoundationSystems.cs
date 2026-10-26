using System;
using ProjectLove.Core;
using ProjectLove.Data;

namespace ProjectLove.Simulation
{
    public sealed class ActionPointSystem
    {
        private readonly RuntimeGameState state;
        public ActionPointSystem(RuntimeGameState state) { this.state = state; }
        public bool CanSpend(int cost) => cost >= 0 && !state.dayEnded && state.actionPoints >= cost;
        public bool TrySpend(int cost)
        {
            if (!CanSpend(cost)) return false;
            state.actionPoints -= cost;
            return true;
        }
    }

    public sealed class DayPhaseManager
    {
        private readonly RuntimeGameState state;
        private readonly GameConfigSO config;
        public DayPhaseManager(RuntimeGameState state, GameConfigSO config) { this.state = state; this.config = config; }
        public bool CampaignEnded => state.dayEnded && state.day >= config.totalDays;
        public bool TryAdvancePhase()
        {
            if (state.dayEnded || state.phase == DayPhase.Night) return false;
            state.phase = (DayPhase)((int)state.phase + 1);
            return true;
        }
        public bool CanConsumePhases(int cost) => !state.dayEnded && cost >= 0 && cost <= (int)DayPhase.Night - (int)state.phase + 1;
        public bool TryConsumePhases(int cost)
        {
            if (!CanConsumePhases(cost)) return false;
            int target = (int)state.phase + cost;
            if (target > (int)DayPhase.Night) { state.phase = DayPhase.Night; state.dayEnded = true; }
            else state.phase = (DayPhase)target;
            return true;
        }
        public bool TryEndDay()
        {
            if (state.dayEnded) return false;
            state.dayEnded = true;
            return true;
        }
        public bool TryStartNextDay()
        {
            if (!state.dayEnded || CampaignEnded) return false;
            state.day++;
            state.dayStartProgress = state.mainProgress;
            state.phase = DayPhase.Morning;
            state.actionPoints = config.usesStamina ? 0 : config.maxActionPointsPerDay;
            if (config.usesStamina) state.stamina = (int)Math.Min(config.maxStamina, (long)state.stamina + config.overnightRecovery);
            state.dayEnded = false;
            return true;
        }
    }

    public sealed class StatAndRelationshipManager
    {
        private readonly RuntimeGameState state;
        private readonly Func<Condition, bool> productionCondition;
        public StatAndRelationshipManager(RuntimeGameState state, Func<Condition, bool> productionCondition = null)
        { this.state = state; this.productionCondition = productionCondition; }

        public bool TryGet(StatType stat, CharacterDataSO character, out int value)
        {
            value = 0;
            switch (stat)
            {
                case StatType.MainProgress: value = state.mainProgress; return true;
                case StatType.Money: value = state.money; return true;
                case StatType.SkillPoints: value = state.skillPoints; return true;
                case StatType.Affection:
                case StatType.Stress:
                    if (stat == StatType.Stress && state.romance != null) return false;
                    var target = character != null ? state.FindCharacter(character.id) : null;
                    if (target == null) return false;
                    value = stat == StatType.Affection ? target.affection : target.stress;
                    return true;
                default: return false;
            }
        }

        public bool CanApply(StatModifier[] effects, FlagChange[] flags)
        {
            if (effects == null || flags == null) return false;
            foreach (var effect in effects)
                if (effect == null || !TryGet(effect.stat, effect.character, out _)) return false;
            foreach (var flag in flags)
                if (flag == null || string.IsNullOrWhiteSpace(flag.flagId)) return false;
            return true;
        }

        public void ApplyValidated(StatModifier[] effects, FlagChange[] flags)
        {
            foreach (var effect in effects)
            {
                TryGet(effect.stat, effect.character, out int current);
                int max = effect.stat == StatType.Money || effect.stat == StatType.SkillPoints ? int.MaxValue : 100;
                int value = (int)Math.Max(0L, Math.Min(max, (long)current + effect.amount));
                switch (effect.stat)
                {
                    case StatType.MainProgress: state.mainProgress = value; break;
                    case StatType.Money: state.money = value; break;
                    case StatType.SkillPoints: state.skillPoints = value; break;
                    case StatType.Affection: state.FindCharacter(effect.character.id).affection = value; break;
                    case StatType.Stress: state.FindCharacter(effect.character.id).stress = value; break;
                }
            }
            foreach (var flag in flags)
            {
                if (flag.value && !state.flags.Contains(flag.flagId)) state.flags.Add(flag.flagId);
                if (!flag.value) state.flags.Remove(flag.flagId);
            }
        }

        public bool Meets(Condition[] conditions)
        {
            if (conditions == null) return false;
            foreach (var condition in conditions)
            {
                if (condition == null) return false;
                if (condition.type == ConditionType.WorkloadActivated || condition.type == ConditionType.WorkloadNotActivated)
                {
                    if (state.production == null || string.IsNullOrWhiteSpace(condition.targetId)) return false;
                    bool active = state.production.events.Exists(e => e.eventId == condition.targetId);
                    if (active != (condition.type == ConditionType.WorkloadActivated)) return false;
                    continue;
                }
                if (condition.type == ConditionType.TaskAccepted || condition.type == ConditionType.TaskNotAccepted)
                {
                    var task = state.production?.tasks.Find(t => t.taskId == condition.targetId);
                    if (task == null || (task.acceptedDay > 0) != (condition.type == ConditionType.TaskAccepted)) return false;
                    continue;
                }
                if (condition.type == ConditionType.CharacterCanVolunteer)
                {
                    if (productionCondition == null || !productionCondition(condition)) return false;
                    continue;
                }
                if (condition.type == ConditionType.CharacterSharedNight)
                {
                    if (state.production?.planning != null)
                    {
                        if (condition.character == null || !state.production.planning.shared.Exists(r => r.characterId == condition.character.id && r.nightWork)) return false;
                        continue;
                    }
                    if (condition.character == null || state.production?.sharedNights == null ||
                        !state.production.sharedNights.Exists(r => r.characterId == condition.character.id)) return false;
                    continue;
                }
                if (condition.type == ConditionType.CharacterOvertime)
                {
                    if (state.production == null || condition.character == null || !state.production.orders.Exists(o =>
                        o.characterId == condition.character.id && o.day == state.day && o.phase == state.phase && o.kind == StaffOrderKind.Overtime)) return false;
                    continue;
                }
                if (condition.type == ConditionType.EventCompleted || condition.type == ConditionType.EventNotCompleted)
                {
                    if (string.IsNullOrWhiteSpace(condition.eventId)) return false;
                    if (state.completedEvents.Contains(condition.eventId) != (condition.type == ConditionType.EventCompleted)) return false;
                    continue;
                }
                if (condition.type == ConditionType.FlagSet || condition.type == ConditionType.FlagUnset)
                {
                    if (string.IsNullOrWhiteSpace(condition.flagId)) return false;
                    bool present = state.flags.Contains(condition.flagId);
                    if (present != (condition.type == ConditionType.FlagSet)) return false;
                }
                else
                {
                    if (!TryGet(condition.stat, condition.character, out int value)) return false;
                    if (condition.type == ConditionType.StatAtLeast && value < condition.value) return false;
                    if (condition.type == ConditionType.StatAtMost && value > condition.value) return false;
                    if (condition.type != ConditionType.StatAtLeast && condition.type != ConditionType.StatAtMost) return false;
                }
            }
            return true;
        }
    }
}
