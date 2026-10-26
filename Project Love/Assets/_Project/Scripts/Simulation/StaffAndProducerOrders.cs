using System;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;

namespace ProjectLove.Simulation
{
    public sealed class StaffOrderForecast
    {
        public bool allowed;
        public string reason;
        public int moneyCost, stressBefore, stressAfter, effort;
        internal RuntimeGameState after;
    }
    public sealed partial class ProductionSystem
    {
        public bool HasOrder(string characterId, StaffOrderKind kind) => Enabled &&
            session.State.production.orders.Any(o => o.characterId == characterId && o.kind == kind &&
                o.day == session.State.day && o.phase == session.State.phase && !session.State.dayEnded);
        private bool Ordered(RuntimeGameState state, string id, StaffOrderKind kind) => state.production.orders
            .Any(o => o.characterId == id && o.kind == kind && o.day == state.day && o.phase == state.phase);
        public StaffOrderForecast PreviewOrder(string characterId, StaffOrderKind kind)
        {
            var result = new StaffOrderForecast { reason = Enabled ? Text("unavailable") : "" };
            if (!Enabled || session.State.dayEnded || session.Dialogue.IsActive ||
                (session.Interactions.IsOpen && session.Interactions.Character.id != characterId)) return result;
            return PreviewOrder(session.State, characterId, kind);
        }
        private StaffOrderForecast PreviewOrder(RuntimeGameState source, string characterId, StaffOrderKind kind)
        {
            var result = new StaffOrderForecast { reason = Text("unavailable") };
            if (source.dayEnded) return result;
            var state = source.Clone();
            var worker = Config.workers.FirstOrDefault(w => w.character.id == characterId);
            if (worker == null || Absent(state, characterId)) { result.reason = Text("absent"); return result; }
            if (state.production.orders.Any(o => o.characterId == characterId)) { result.reason = Text("orderAlready"); return result; }
            var character = state.FindCharacter(characterId);
            result.stressBefore = character.stress;
            if (kind == StaffOrderKind.Rest)
            {
                if (!Config.productivePhases.Contains(state.phase)) { result.reason = Text("nightOff"); return result; }
                if (character.stress == 0) { result.reason = Text("noStress"); return result; }
                result.stressAfter = Math.Max(0, character.stress - Config.restStressRecovery);
            }
            else if (kind == StaffOrderKind.Overtime)
            {
                if (state.phase != DayPhase.Night) { result.reason = Text("nightOnly"); return result; }
                var task = Active(state, characterId);
                if (task == null) { result.reason = Text("noTask"); return result; }
                if (Blocked(state, task)) { result.reason = BlockReason(Find(task.taskId)); return result; }
                result.moneyCost = Config.overtimeMoneyCost;
                if (state.money < result.moneyCost) { result.reason = Text("lowMoney"); return result; }
                state.money -= result.moneyCost;
                character.stress = Math.Min(100, character.stress + Config.overtimeStress);
                result.stressAfter = character.stress;
                result.effort = Math.Min(Definition(task).effort - task.effort, Rate(state, worker));
                if (result.effort == 0) return result;
            }
            else return result;
            state.production.orders.Add(new StaffOrder { characterId = characterId, kind = kind, day = state.day, phase = state.phase });
            result.after = state; result.allowed = true; result.reason = ""; return result;
        }
        public bool TryOrder(string characterId, StaffOrderKind kind)
        {
            if (session.BoardOpen) return false;
            var preview = PreviewOrder(characterId, kind);
            if (!preview.allowed) return false;
            // Orders keep the character menu open and do not enter the phase resolver.
            session.State.CopyFrom(preview.after); session.NotifyChanged(); return true;
        }
        public SprintDataSO ProducerStage => !Enabled || !Config.useProducerStages ? null : Config.sprints.FirstOrDefault(s =>
            session.State.production.producerStages.First(p => p.sprintId == s.id).completedDay == 0 &&
            (!PlanningEnabled || s.startDay <= session.State.day));
        public int ProducerCompleted => ProducerStage == null ? 0 : session.State.production.producerStages.First(s => s.sprintId == ProducerStage.id).effort;
        public bool StageUnlocked(SprintDataSO sprint) => !Config.useProducerStages || Unlocked(session.State, sprint);
        private bool Unlocked(RuntimeGameState state, SprintDataSO sprint)
        {
            if (PlanningEnabled) return Config.sprints.Contains(sprint) && sprint.startDay <= state.day;
            int index = Array.IndexOf(Config.sprints, sprint);
            return index >= 0 && (index == 0 || state.production.producerStages[index - 1].completedDay > 0);
        }
        private bool AddProducerEffort(RuntimeGameState state)
        {
            if (!Config.useProducerStages) return false;
            var stage = state.production.producerStages.FirstOrDefault(p => p.completedDay == 0);
            if (stage == null) return false;
            var definition = Config.sprints.First(s => s.id == stage.sprintId);
            stage.effort = Math.Min(definition.producerGoal, stage.effort + Config.producerEffort);
            if (stage.effort == definition.producerGoal)
            { stage.completedDay = state.day; stage.completedPhase = state.phase; }
            state.money = (int)Math.Min(int.MaxValue, (long)state.money + Config.producerMoney);
            return true;
        }
        private string CreditSprint(RuntimeGameState state, WorkTaskSO task) =>
            Config.useProducerStages && task.sprint != null ? task.sprint.id : Sprint(state).id;
        private bool ValidTaskCredit(RuntimeGameState state, TaskState task, WorkTaskSO definition)
        {
            if (Config.useProducerStages && definition.sprint != null)
                return task.acceptedSprintId == definition.sprint.id && Unlocked(state, definition.sprint);
            return Config.sprints.Any(s => s.id == task.acceptedSprintId && task.acceptedDay >= s.startDay && task.acceptedDay <= s.endDay);
        }
        private int DeadlineCompleted(RuntimeGameState state, SprintDataSO sprint, WorkStreamSO stream) => state.production.tasks
            .Where(t => t.acceptedSprintId == sprint.id && t.acceptedDay <= sprint.endDay && Definition(t).stream == stream).Sum(t => t.effort);
        private bool ValidOrdersAndProducer(RuntimeGameState state)
        {
            var p = state.production;
            if (!ValidSharedNights(state) || p.orders == null || p.producerStages == null || p.orders.Any(o => o == null || !Config.workers.Any(w => w.character.id == o.characterId) ||
                state.dayEnded || o.day != state.day || o.phase != state.phase ||
                (o.kind == StaffOrderKind.Overtime ? o.phase != DayPhase.Night : o.kind != StaffOrderKind.Rest || !Config.productivePhases.Contains(o.phase))) ||
                p.orders.Select(o => o.characterId).Distinct().Count() != p.orders.Count) return false;
            if (!Config.useProducerStages) return p.producerStages.Count == 0;
            if (p.producerStages.Count != Config.sprints.Length || p.producerStages.Any(s => s == null)) return false;
            int previousCursor = -1;
            bool unfinished = false;
            for (int i = 0; i < Config.sprints.Length; i++)
            {
                var s = p.producerStages[i]; var d = Config.sprints[i];
                if (s.sprintId != d.id || s.effort < 0 || s.effort > d.producerGoal || s.completedDay < 0 || s.completedDay > state.day ||
                    s.completedDay == 0 && s.effort == d.producerGoal || s.completedDay > 0 && s.effort != d.producerGoal ||
                    unfinished && s.effort > 0) return false;
                if (s.completedDay == 0) { unfinished = true; continue; }
                int cursor = (s.completedDay - 1) * 4 + (int)s.completedPhase;
                if (!Enum.IsDefined(typeof(DayPhase), s.completedPhase) || cursor <= previousCursor || cursor >= p.resolvedPhases) return false;
                previousCursor = cursor;
            }
            return true;
        }
    }
}
