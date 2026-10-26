using System;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;

namespace ProjectLove.Simulation
{
    public sealed class DialogueWorkForecast
    {
        public bool allowed;
        public string reason;
        public int moneyCost, staminaCost, help, team, stressBefore, stressAfter, affectionGain;
        public string taskId;
        internal RuntimeGameState after;
    }
    public sealed partial class ProductionSystem
    {
        public bool CanVolunteer(CharacterDataSO character, int deadlineDays)
        {
            if (!Enabled || character == null || deadlineDays < 0 || session.State.phase != DayPhase.Night) return false;
            var task = Active(session.State, character.id);
            if (task == null) return false;
            var sprint = Definition(task).sprint;
            return sprint != null && sprint.endDay - session.State.day <= deadlineDays &&
                PreviewOrder(session.State, character.id, StaffOrderKind.Overtime).allowed;
        }
        // A command choice is evaluated on a clone, including ordinary costs/effects, before any commit.
        public DialogueWorkForecast PreviewDialogueWork(DialogueChoice choice)
        {
            var result = new DialogueWorkForecast { reason = Enabled ? Text("unavailable") : "" };
            if (!Enabled || choice == null || choice.workAction == DialogueWorkAction.None ||
                !Enum.IsDefined(typeof(DialogueWorkAction), choice.workAction) || choice.workCharacter == null ||
                !Config.workers.Any(w => w.character == choice.workCharacter) || session.State.dayEnded ||
                choice.activateWorkloads == null || choice.activateWorkloads.Length != 0 ||
                !session.CanPay(choice.actionPointCost, choice.staminaCost) ||
                !session.CanApplyTransaction(choice.effects, choice.flags)) return result;
            var state = session.State.Clone();
            state.stamina -= choice.staminaCost;
            string id = choice.workCharacter.id;
            var character = state.FindCharacter(id);
            result.stressBefore = character.stress;
            bool already = Ordered(state, id, StaffOrderKind.Overtime);
            if (choice.workAction == DialogueWorkAction.Overtime || !already)
            {
                var order = PreviewOrder(state, id, StaffOrderKind.Overtime);
                if (!order.allowed) { result.reason = order.reason; return result; }
                result.moneyCost = order.moneyCost;
                state.CopyFrom(order.after);
            }
            result.staminaCost = choice.staminaCost;
            if (choice.workAction == DialogueWorkAction.WorkTogether)
            {
                if (state.phase != DayPhase.Night || state.stamina < Config.helpCost)
                { result.reason = Text("lowEnergy"); return result; }
                var task = Active(state, id);
                if (Absent(state, id) || task == null || Blocked(state, task))
                { result.reason = task == null ? Text("noTask") : BlockReason(Find(task.taskId)); return result; }
                if (!PlanningEnabled && Config.helpEffort <= 0) return result;
                state.stamina -= Config.helpCost; result.staminaCost += Config.helpCost;
                var workers = new System.Collections.Generic.List<WorkerForecast>();
                ResolveAndAdvance(state, 1, ProductionAction.Help, id, workers);
                var delta = workers.Single(w => w.characterId == id);
                if (!PlanningEnabled && delta.help <= 0) return result;
                result.taskId = delta.taskId; result.help = delta.help; result.team = delta.team;
                result.affectionGain = state.FindCharacter(id).affection - session.State.FindCharacter(id).affection;
                state.production.lastReport = string.Format(Text("sharedNightResult"), choice.workCharacter.displayName,
                    Definition(Find(delta.taskId)).displayName, delta.help, delta.team, result.staminaCost, result.moneyCost);
            }
            else
            {
                var task = Active(state, id); result.taskId = task?.taskId;
                result.team = task == null ? 0 : Math.Min(Definition(task).effort - task.effort,
                    Rate(state, Config.workers.Single(w => w.character.id == id)));
            }
            new StatAndRelationshipManager(state).ApplyValidated(choice.effects, choice.flags);
            result.affectionGain = state.FindCharacter(id).affection - session.State.FindCharacter(id).affection;
            result.stressAfter = state.FindCharacter(id).stress;
            result.allowed = true; result.reason = ""; result.after = state;
            return result;
        }
        internal bool TryDialogueWork(DialogueChoice choice)
        {
            var preview = PreviewDialogueWork(choice);
            if (!preview.allowed) return false;
            session.State.CopyFrom(preview.after); return true;
        }
        public string DialogueWorkLabel(DialogueChoice choice)
        {
            if (choice.workAction == DialogueWorkAction.None) return choice.text;
            var preview = PreviewDialogueWork(choice);
            if (!preview.allowed) return choice.text + "\n" + preview.reason;
            return choice.text + "\n" + string.Format(Text(choice.workAction == DialogueWorkAction.WorkTogether ? "sharedNightChoice" : "volunteerChoice"),
                preview.moneyCost, preview.staminaCost, preview.help, preview.team, preview.stressBefore, preview.stressAfter, preview.affectionGain);
        }
        private bool ValidSharedNights(RuntimeGameState state)
        {
            var records = state.production.sharedNights;
            if (records == null || records.Any(r => r == null || r.day < 1 || r.day > state.day ||
                r.day == state.day && !state.dayEnded || r.help <= 0 || r.help > Config.helpEffort || r.team < 0 ||
                !Config.workers.Any(w => w.character.id == r.characterId && Config.tasks.Any(t => t.id == r.taskId && t.stream == w.stream)))) return false;
            if (records.GroupBy(r => r.day).Any(g => g.Count() > 1)) return false;
            return records.GroupBy(r => r.taskId).All(g => g.Sum(r => (long)r.help + r.team) <=
                state.production.tasks.Single(t => t.taskId == g.Key).effort);
        }
    }
}
