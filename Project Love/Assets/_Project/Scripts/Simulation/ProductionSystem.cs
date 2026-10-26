using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ProjectLove.Core;
using ProjectLove.Data;
using UnityEngine;

namespace ProjectLove.Simulation
{
    public enum ProductionAction { Help, Coordinate, Clarify, Care, Producer }
    public enum TaskColumn { Backlog, InProgress, Blocked, Review, Done }
    public sealed class WorkerForecast
    {
        public string characterId, taskId;
        public int before, after, help, team, producer, stressBefore, stressAfter;
    }
    public sealed class ProductionForecast
    {
        public bool allowed;
        public string reason;
        public int cost, moneyGain, producerGain;
        public List<WorkerForecast> workers = new List<WorkerForecast>();
        internal RuntimeGameState after;
    }

    // Preview and commit share the same cloned-state transaction and phase resolver.
    public sealed partial class ProductionSystem
    {
        private readonly GameSession session;
        public ProductionConfigSO Config => session.Config.production;
        public bool Enabled => Config != null;
        public ProductionSystem(GameSession session)
        {
            this.session = session;
            if (!Enabled) return;
            ValidateDefinitions(session.Config);
            session.State.production = new ProductionState { catalog = Fingerprint() };
            if (Config.useProducerStages) foreach (var sprint in Config.sprints)
                session.State.production.producerStages.Add(new ProducerStageState { sprintId = sprint.id });
            for (int i = 0; i < Config.tasks.Length; i++)
                session.State.production.tasks.Add(new TaskState { taskId = Config.tasks[i].id,
                    ownerId = Config.tasks[i].owner.id, priority = i });
            InitializePlanning();
            ActivateAutomatic(session.State);
        }
        public WorkTaskSO Definition(TaskState task) => Config.tasks.First(t => t.id == task.taskId);
        public TaskState Find(string id) => session.State.production?.tasks.FirstOrDefault(t => t.taskId == id);
        public SprintDataSO CurrentSprint => Sprint(session.State);
        private SprintDataSO Sprint(RuntimeGameState state) => Config.sprints.First(s => state.day >= s.startDay && state.day <= s.endDay);
        public int Rate(CharacterDataSO character) => Rate(session.State, Config.workers.First(w => w.character == character));
        private int Rate(RuntimeGameState state, WorkerDefinition worker)
        {
            int stress = state.FindCharacter(worker.character.id).stress;
            var band = Config.stressBands.Where(b => stress >= b.minimum).OrderByDescending(b => b.minimum).First();
            return worker.baseRate * band.outputPercent / 100;
        }
        private bool Absent(RuntimeGameState state, string id) => state.production.absences.Any(a => a.characterId == id && state.day <= a.throughDay);
        private bool Released(RuntimeGameState state, WorkTaskSO task) =>
            (Config.useProducerStages && task.sprint != null ? Unlocked(state, task.sprint) : task.releaseDay <= state.day) &&
            !Config.events.Any(e => e.tasks.Contains(task) && !state.production.events.Any(r => r.eventId == e.id));
        private bool Blocked(RuntimeGameState state, TaskState task)
        {
            var data = Definition(task);
            return (data.decisionBlocked && !task.decisionCleared) ||
                data.prerequisites.Any(p => state.production.tasks.First(t => t.taskId == p.id).acceptedDay == 0);
        }
        private TaskState Active(RuntimeGameState state, string owner) => PlanningEnabled ? PlannedActive(state, owner) : state.production.tasks
            .Where(t => t.ownerId == owner && t.acceptedDay == 0 && t.effort < Definition(t).effort && Released(state, Definition(t)))
            .OrderBy(t => t.priority).FirstOrDefault();
        public TaskState Active(CharacterDataSO character) => Active(session.State, character.id);
        public TaskColumn Column(TaskState task)
        {
            if (task.acceptedDay > 0) return TaskColumn.Done;
            if (task.effort == Definition(task).effort) return TaskColumn.Review;
            if (Released(session.State, Definition(task)) && Blocked(session.State, task)) return TaskColumn.Blocked;
            return task.effort > 0 || Active(session.State, task.ownerId) == task ? TaskColumn.InProgress : TaskColumn.Backlog;
        }
        public bool IsReleased(TaskState task) => Released(session.State, Definition(task));
        public bool IsReady(TaskState task) => task != null && task.acceptedDay == 0 &&
            task.effort < Definition(task).effort && IsReleased(task) && !Blocked(session.State, task);
        public TaskState[] ReadyTasks(CharacterDataSO character) => session.State.production.tasks
            .Where(t => t.ownerId == character.id && IsReady(t)).OrderBy(t => t.priority).ToArray();
        private string Text(string key) => Config.presentation.Get(key);
        public string BlockReason(TaskState task)
        {
            if (!Released(session.State, Definition(task))) return Text("unreleased");
            var data = Definition(task);
            if (data.decisionBlocked && !task.decisionCleared) return Text("decisionBlock");
            if (data.prerequisites.Any(p => Find(p.id).acceptedDay == 0)) return Text("dependencyBlock") + " · " +
                string.Join(", ", data.prerequisites.Where(p => Find(p.id).acceptedDay == 0).Select(p => p.displayName));
            if (Absent(session.State, task.ownerId)) return Text("absent");
            if (Ordered(session.State, task.ownerId, StaffOrderKind.Rest)) return Text("restOrdered");
            return "";
        }
        public ProductionForecast Preview(ProductionAction action, string targetId = null)
        {
            var result = new ProductionForecast { reason = Enabled ? Text("unavailable") : "" };
            if (!Enabled || session.State.dayEnded || session.Dialogue.IsActive ||
                (session.Interactions.IsOpen && (action != ProductionAction.Help || session.Interactions.Character.id != targetId))) return result;
            var state = session.State.Clone();
            var worker = Config.workers.FirstOrDefault(w => w.character.id == targetId);
            bool productive = Config.productivePhases.Contains(state.phase);
            switch (action)
            {
                case ProductionAction.Help:
                    result.cost = Config.helpCost;
                    if (PlanningEnabled)
                    {
                        if (worker == null || Absent(state, targetId)) { result.reason = Text("absent"); return result; }
                        break;
                    }
                    if (!productive && !(state.phase == DayPhase.Night && Ordered(state, targetId, StaffOrderKind.Overtime)))
                    { result.reason = Text("nightOff"); return result; }
                    if (worker == null || Absent(state, targetId)) { result.reason = Text("absent"); return result; }
                    if (Ordered(state, targetId, StaffOrderKind.Rest)) { result.reason = Text("restOrdered"); return result; }
                    var task = Active(state, targetId);
                    if (task == null) { result.reason = Text("noTask"); return result; }
                    if (Blocked(state, task)) { result.reason = BlockReason(Find(task.taskId)); return result; }
                    break;
                case ProductionAction.Coordinate:
                    if (PlanningEnabled) return result;
                    result.cost = Config.coordinateCost;
                    if (state.production.coordinationTicks > 0) { result.reason = Text("planActive"); return result; }
                    break;
                case ProductionAction.Clarify:
                    result.cost = Config.clarifyCost;
                    var blocked = state.production.tasks.FirstOrDefault(t => t.taskId == targetId);
                    if (blocked == null || !Released(state, Definition(blocked)) || !Definition(blocked).decisionBlocked ||
                        blocked.decisionCleared || blocked.acceptedDay > 0) { result.reason = Text("noDecision"); return result; }
                    break;
                case ProductionAction.Care:
                    return result;
                case ProductionAction.Producer:
                    result.cost = Config.producerCost;
                    if (ProducerStage == null) { result.reason = Text("producerFinished"); return result; }
                    result.producerGain = Math.Min(Config.producerEffort, ProducerStage.producerGoal - ProducerCompleted);
                    result.moneyGain = (int)Math.Min(Config.producerMoney, (long)int.MaxValue - state.money);
                    break;
                default: return result;
            }
            if (state.stamina < result.cost) { result.reason = Text("lowEnergy"); return result; }
            state.stamina -= result.cost;
            if (action == ProductionAction.Coordinate) state.production.coordinationTicks = Config.coordinateTicks;
            if (action == ProductionAction.Clarify) state.production.tasks.First(t => t.taskId == targetId).decisionCleared = true;
            if (action == ProductionAction.Producer) AddProducerEffort(state);
            ResolveAndAdvance(state, 1, action, targetId, result.workers);
            result.allowed = true; result.reason = ""; result.after = state;
            return result;
        }
        public bool TryAct(ProductionAction action, string targetId = null)
        {
            if (session.BoardOpen) return false;
            var forecast = Preview(action, targetId);
            if (!forecast.allowed) return false;
            if (session.Interactions.IsOpen) session.Interactions.CloseForWork();
            forecast.after.production.lastReport = string.Format(Text("actionReport"), forecast.cost,
                string.Join("\n", forecast.workers.Select(w => string.Format(Text("workerReport"),
                    session.Config.characters.First(c => c.id == w.characterId).displayName, w.help, w.team, w.stressBefore, w.stressAfter))));
            if (action == ProductionAction.Producer) forecast.after.production.lastReport = string.Format(Text("producerResult"),
                forecast.moneyGain, forecast.producerGain, string.Join(" · ", forecast.workers.Select(w =>
                    session.Config.characters.First(c => c.id == w.characterId).roleTitle + " +" + w.producer))) + "\n" + forecast.after.production.lastReport;
            session.State.CopyFrom(forecast.after); session.NotifyChanged(); return true;
        }
        internal bool Consume(int count)
        {
            if (!session.Days.CanConsumePhases(count)) return false;
            ResolveAndAdvance(session.State, count); return true;
        }
        private void ResolveAndAdvance(RuntimeGameState state, int count, ProductionAction? action = null,
            string target = null, List<WorkerForecast> forecast = null)
        {
            var days = new DayPhaseManager(state, session.Config);
            for (int i = 0; i < count; i++)
            {
                ActivateAutomatic(state);
                bool productive = Config.productivePhases.Contains(state.phase);
                // Select once: neither help nor a completion can spill into another card this tick.
                var targets = Config.workers.ToDictionary(w => w.character.id, w => Active(state, w.character.id));
                if (PlanningEnabled) ResolvePlanningPhase(state, action, target, forecast);
                else foreach (var worker in Config.workers)
                {
                    string id = worker.character.id;
                    var character = state.FindCharacter(id);
                    var task = targets[id];
                    var delta = new WorkerForecast { characterId = id, taskId = task?.taskId,
                        before = task?.effort ?? 0, stressBefore = character.stress };
                    bool overtime = state.phase == DayPhase.Night && Ordered(state, id, StaffOrderKind.Overtime);
                    bool eligible = (productive || overtime) && !Absent(state, id);
                    bool resting = Ordered(state, id, StaffOrderKind.Rest);
                    if (resting) character.stress = Math.Max(0, character.stress - Config.restStressRecovery);
                    if (i == 0 && action == ProductionAction.Producer && !resting && !Absent(state, id) && task != null && !Blocked(state, task) &&
                        !forecast.Any(w => w.producer > 0 && Config.workers.First(v => v.character.id == w.characterId).stream == worker.stream))
                    { delta.producer = Math.Min(Config.producerTeamBonus, Definition(task).effort - task.effort); task.effort += delta.producer; }
                    if (eligible && !resting && task != null && !Blocked(state, task))
                    {
                        int remaining = Definition(task).effort - task.effort;
                        if (i == 0 && action == ProductionAction.Help && target == id)
                        {
                            delta.help = Math.Min(remaining, Config.helpEffort);
                            remaining -= delta.help;
                            if (delta.help > 0) character.stress = Math.Max(0, character.stress - Config.helpStressRecovery);
                        }
                        int rate = Rate(state, worker) + (productive && state.production.coordinationTicks > 0 ? Config.coordinateBonus : 0);
                        delta.team = Math.Min(remaining, rate);
                        task.effort += delta.help + delta.team;
                        if (delta.team > 0 && !overtime) character.stress = Math.Min(100, character.stress + Config.workStress);
                    }
                    if (overtime && delta.help > 0)
                    {
                        character.affection = Math.Min(100, character.affection + Config.sharedNightAffection);
                        state.production.sharedNights.Add(new SharedNightRecord { characterId = id, taskId = task.taskId,
                            day = state.day, help = delta.help, team = delta.team });
                    }
                    delta.after = task?.effort ?? 0; delta.stressAfter = character.stress;
                    forecast?.Add(delta);
                }
                if (productive && state.production.coordinationTicks > 0) state.production.coordinationTicks--;
                if (PlanningEnabled) AcceptReady(state, true);
                state.production.orders.Clear();
                state.production.resolvedPhases++;
                days.TryConsumePhases(1);
                if (state.dayEnded && state.day == Sprint(state).endDay)
                { AcceptReady(state, PlanningEnabled); ReviewDeadline(state); }
                ActivateAutomatic(state);
            }
        }
        internal void StartDay()
        {
            foreach (var worker in Config.workers)
            {
                var character = session.State.FindCharacter(worker.character.id);
                character.stress = Math.Max(0, character.stress - (PlanningEnabled ?
                    (session.State.production.planning.nights.Any(n => n.characterId == worker.character.id && n.day == session.State.day - 1) ? 0 : Config.planning.overnightRecovery) : Config.overnightStressRecovery));
            }
            if (PlanningEnabled) SnapshotPlanningDay(session.State);
            session.State.production.absences.RemoveAll(a => a.throughDay < session.State.day);
            ActivateAutomatic(session.State);
        }
        public int Completed(WorkStreamSO stream) => Completed(session.State, CurrentSprint, stream);
        private int Completed(RuntimeGameState state, SprintDataSO sprint, WorkStreamSO stream) => state.production.tasks
            .Where(t => t.acceptedSprintId == sprint.id && Definition(t).stream == stream).Sum(t => t.effort);
        public int Required(WorkStreamSO stream) => Required(session.State, CurrentSprint, stream);
        private int Required(RuntimeGameState state, SprintDataSO sprint, WorkStreamSO stream) =>
            (sprint.quotas.FirstOrDefault(q => q.stream == stream)?.effort ?? 0) + state.production.events
                .Where(r => r.sprintId == sprint.id).Sum(r => Config.events.First(e => e.id == r.eventId).extraQuotas
                    .Where(q => q.stream == stream).Sum(q => q.effort));
        public bool TryReview()
        {
            if (!Enabled || session.IsBusy || session.State.dayEnded) return false;
            AcceptReady(session.State); session.NotifyChanged(); return true;
        }
        private void AcceptReady(RuntimeGameState state, bool automatic = false)
        {
            foreach (var task in state.production.tasks.Where(t => t.acceptedDay == 0 && t.effort == Definition(t).effort && Released(state, Definition(t)) && (!automatic || !Definition(t).requiresManualReview)))
            { task.acceptedDay = state.day; task.acceptedSprintId = CreditSprint(state, Definition(task)); }
            int total = Config.tasks.Sum(t => t.mainWeight);
            state.mainProgress = total == 0 ? 0 : (int)((long)state.production.tasks.Where(t => t.acceptedDay > 0)
                .Sum(t => Definition(t).mainWeight) * 100 / total);
            foreach (var stream in state.workProgress)
                stream.completed = state.production.tasks.Where(t => t.acceptedDay > 0 && Definition(t).stream.id == stream.streamId).Sum(t => t.effort);
        }
        private void ReviewDeadline(RuntimeGameState state)
        {
            var sprint = Sprint(state);
            if (state.production.reviews.Any(r => r.sprintId == sprint.id)) return;
            var record = new SprintResult { sprintId = sprint.id, reviewedDay = state.day,
                carryoverCount = state.production.tasks.Count(t => t.acceptedDay == 0 && Released(state, Definition(t)) &&
                    (!Config.useProducerStages || Definition(t).sprint == null || Definition(t).sprint.endDay <= sprint.endDay)) };
            foreach (var stream in Config.workers.Select(w => w.stream).Distinct())
                record.quotas.Add(new QuotaResult { streamId = stream.id, completed = DeadlineCompleted(state, sprint, stream), required = Required(state, sprint, stream) });
            state.production.reviews.Add(record);
        }
        public bool TryPrioritize(string taskId)
        {
            if (!Enabled || session.IsBusy || session.State.dayEnded) return false;
            var task = Find(taskId);
            if (task == null || task.acceptedDay > 0 || task.effort == Definition(task).effort || !IsReleased(task)) return false;
            var order = session.State.production.tasks.Where(t => t.ownerId == task.ownerId).OrderBy(t => t.priority).ToList();
            order.Remove(task); order.Insert(0, task);
            var priorities = order.Select(t => t.priority).OrderBy(n => n).ToArray();
            for (int i = 0; i < order.Count; i++) order[i].priority = priorities[i];
            session.NotifyChanged(); return true;
        }
        public bool TryAssign(string taskId, CharacterDataSO owner)
        {
            if (!Enabled || session.IsBusy || session.State.dayEnded) return false;
            var task = Find(taskId);
            if (task == null || task.acceptedDay > 0 || owner == null ||
                !Config.workers.Any(w => w.character == owner && w.stream == Definition(task).stream)) return false;
            task.ownerId = owner.id; session.NotifyChanged(); return true;
        }
        public bool CanActivate(WorkloadEvent data) => Enabled && !session.IsBusy && !session.State.dayEnded &&
            Config.events.Contains(data) && EligibleEvent(session.State, data);
        public bool TryActivate(WorkloadEvent data)
        {
            if (!CanActivate(data)) return false;
            Activate(session.State, data); session.NotifyChanged(); return true;
        }
        internal bool CanApplyEventWorkloads(string[] ids) => ids != null && ids.Distinct().Count() == ids.Length &&
            ids.All(id => Enabled && Config.events.Any(e => e.id == id && !e.automatic && EligibleEvent(session.State, e)));
        internal void ApplyEventWorkloads(string[] ids)
        {
            foreach (var id in ids) Activate(session.State, Config.events.Single(e => e.id == id));
        }
        private bool EligibleEvent(RuntimeGameState state, WorkloadEvent data) => state.day >= data.earliestDay && state.day <= data.latestDay &&
            !state.production.events.Any(r => r.eventId == data.id);
        private void ActivateAutomatic(RuntimeGameState state)
        {
            if (state.dayEnded) return;
            foreach (var data in Config.events.Where(e => e.automatic && EligibleEvent(state, e))) Activate(state, data);
        }
        private void Activate(RuntimeGameState state, WorkloadEvent data)
        {
            state.production.events.Add(new WorkloadRecord { eventId = data.id, activatedDay = state.day, sprintId = Sprint(state).id });
            if (data.absentWorker != null && data.absenceDays > 0)
            {
                var record = state.production.absences.FirstOrDefault(a => a.characterId == data.absentWorker.id);
                int through = Math.Min(session.Config.totalDays, state.day + data.absenceDays - 1);
                if (record == null) state.production.absences.Add(new AbsenceRecord { characterId = data.absentWorker.id, throughDay = through });
                else record.throughDay = Math.Max(record.throughDay, through);
            }
        }
        public string Fingerprint()
        {
            if (!Enabled) return "";
            var b = new StringBuilder();
            void Add(params object[] values) { foreach (var value in values) b.Append(Convert.ToString(value, CultureInfo.InvariantCulture)).Append('|'); }
            Add(Config.id, session.Config.totalDays, Config.workStress, Config.overnightStressRecovery, Config.helpCost, Config.helpEffort,
                Config.helpStressRecovery, Config.coordinateCost, Config.coordinateBonus, Config.coordinateTicks, Config.clarifyCost, Config.careCost, Config.careStressRecovery);
            Add("shared_night_v1", Config.sharedNightAffection);
            Add("staff_producer_v5", Config.useProducerStages, Config.producerCost, Config.producerEffort, Config.producerMoney,
                Config.producerTeamBonus, Config.restStressRecovery, Config.overtimeMoneyCost, Config.overtimeStress);
            AppendPlanningFingerprint(Add);
            foreach (var w in Config.workers) Add(w.character.id, w.stream.id, w.baseRate);
            foreach (var s in Config.stressBands) Add(s.minimum, s.outputPercent);
            foreach (var p in Config.productivePhases) Add(p);
            foreach (var t in Config.tasks) { Add(t.id, t.owner.id, t.stream.id, t.effort, t.mainWeight, t.releaseDay, t.decisionBlocked, t.sprint?.id); foreach (var p in t.prerequisites) Add(p.id); }
            foreach (var s in Config.sprints) { Add(s.id, s.startDay, s.endDay, s.producerGoal); foreach (var q in s.quotas) Add(q.stream.id, q.effort); }
            foreach (var e in Config.events) { Add(e.id, e.earliestDay, e.latestDay, e.automatic, e.absentWorker?.id, e.absenceDays);
                foreach (var t in e.tasks) Add(t.id); foreach (var q in e.extraQuotas) Add(q.stream.id, q.effort); }
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(b.ToString()))).Replace("-", "");
        }
        internal bool CanRestore(RuntimeGameState state)
        {
            if (!Enabled) return state.production == null;
            var p = state.production;
            if (p == null || p.catalog != Fingerprint() || p.tasks == null || p.reviews == null || p.events == null || p.absences == null ||
                p.tasks.Count != Config.tasks.Length || p.tasks.Any(t => t == null || !Config.tasks.Any(d => d.id == t.taskId)) ||
                p.events.Any(r => r == null) || p.absences.Any(a => a == null) || p.tasks.Select(t => t.taskId).Distinct().Count() != p.tasks.Count ||
                p.tasks.Select(t => t.priority).OrderBy(n => n).Where((n, i) => n != i).Any() || p.coordinationTicks < 0 || p.coordinationTicks > Config.coordinateTicks ||
                p.resolvedPhases != (state.day - 1) * 4 + (state.dayEnded ? 4 : (int)state.phase)) return false;
            if (!ValidOrdersAndProducer(state) || !ValidPlanning(state)) return false;
            foreach (var t in p.tasks)
            {
                var d = Config.tasks.FirstOrDefault(x => x.id == t.taskId);
                if (d == null || !Config.workers.Any(w => w.character.id == t.ownerId && w.stream == d.stream) || t.effort < 0 || t.effort > d.effort ||
                    t.acceptedDay < 0 || t.acceptedDay > state.day || (t.acceptedDay == 0 ? !string.IsNullOrEmpty(t.acceptedSprintId) :
                        t.effort != d.effort || !ValidTaskCredit(state, t, d)) ||
                    (t.effort > 0 && !Released(state, d)) || (t.effort > 0 && Blocked(state, t))) return false;
            }
            if (p.events.Any(r => r == null || !Config.events.Any(e => e.id == r.eventId && r.activatedDay >= e.earliestDay && r.activatedDay <= e.latestDay) ||
                r.activatedDay > state.day || !Config.sprints.Any(s => s.id == r.sprintId && r.activatedDay >= s.startDay && r.activatedDay <= s.endDay)) ||
                p.events.Select(r => r.eventId).Distinct().Count() != p.events.Count || p.absences.Any(a => a == null ||
                    !Config.workers.Any(w => w.character.id == a.characterId) || a.throughDay < state.day || a.throughDay > session.Config.totalDays) ||
                p.absences.Select(a => a.characterId).Distinct().Count() != p.absences.Count) return false;
            var expected = Config.sprints.Where(s => s.endDay < state.day || (state.dayEnded && s.endDay == state.day)).ToArray();
            if (p.reviews.Count != expected.Length || p.reviews.Any(r => r == null) || p.reviews.Select(r => r.sprintId).Distinct().Count() != p.reviews.Count) return false;
            foreach (var s in expected)
            {
                var r = p.reviews.FirstOrDefault(x => x.sprintId == s.id);
                var streams = Config.workers.Select(w => w.stream).Distinct().ToArray();
                if (r == null || r.reviewedDay != s.endDay || r.carryoverCount < 0 || r.quotas == null || r.quotas.Count != streams.Length ||
                    r.quotas.Any(q => q == null) || r.quotas.Select(q => q.streamId).Distinct().Count() != streams.Length ||
                    streams.Any(stream => !r.quotas.Any(q => q.streamId == stream.id && q.completed == DeadlineCompleted(state, s, stream) && q.required == Required(state, s, stream)))) return false;
            }
            int total = Config.tasks.Sum(t => t.mainWeight);
            if (state.mainProgress != (total == 0 ? 0 : (int)((long)p.tasks.Where(t => t.acceptedDay > 0).Sum(t => Definition(t).mainWeight) * 100 / total))) return false;
            return state.workProgress.All(w => w.completed == p.tasks.Where(t => t.acceptedDay > 0 && Definition(t).stream.id == w.streamId).Sum(t => t.effort));
        }
        public static void ValidateDefinitions(GameConfigSO game)
        {
            var c = game.production;
            ValidatePlanningDefinitions(game);
            void Require(bool valid) { if (!valid) throw new ArgumentException("Invalid production catalog: " + c.name); }
            Require(game.usesStamina && game.startingProgress == 0 && c.presentation != null && c.workers != null && c.workers.Length > 0 &&
                c.tasks != null && c.tasks.Length > 0 && c.sprints != null && c.sprints.Length > 0 && c.stressBands != null && c.stressBands.Length > 0 &&
                c.productivePhases != null && c.productivePhases.Length > 0 && c.events != null);
            Require(c.workers.All(w => w != null && game.characters.Contains(w.character) && game.workStreams.Contains(w.stream) && w.baseRate > 0) &&
                c.workers.Select(w => w.character).Distinct().Count() == c.workers.Length && c.stressBands.All(b => b != null && b.minimum >= 0 && b.minimum <= 100 && b.outputPercent >= 0 && b.outputPercent <= 100) &&
                c.stressBands.Any(b => b.minimum == 0) && c.stressBands.Select(b => b.minimum).Distinct().Count() == c.stressBands.Length &&
                c.productivePhases.All(p => Enum.IsDefined(typeof(DayPhase), p)) && c.productivePhases.Distinct().Count() == c.productivePhases.Length);
            Require(new[] { c.workStress, c.overnightStressRecovery, c.helpCost, c.helpEffort, c.helpStressRecovery, c.coordinateCost, c.coordinateBonus,
                c.coordinateTicks, c.clarifyCost, c.careCost, c.careStressRecovery }.All(n => n >= 0) && c.coordinateTicks > 0);
            Require(c.tasks.All(t => t != null && !string.IsNullOrWhiteSpace(t.id) && t.effort > 0 && t.mainWeight >= 0 && t.releaseDay >= 1 &&
                t.releaseDay <= game.totalDays && t.prerequisites != null && c.workers.Any(w => w.character == t.owner && w.stream == t.stream)) &&
                c.tasks.Select(t => t.id).Distinct().Count() == c.tasks.Length);
            var visiting = new HashSet<WorkTaskSO>(); var visited = new HashSet<WorkTaskSO>();
            void Visit(WorkTaskSO t) { if (visited.Contains(t)) return; Require(visiting.Add(t));
                foreach (var p in t.prerequisites) { Require(p != null && c.tasks.Contains(p)); Visit(p); }
                visiting.Remove(t); visited.Add(t); }
            foreach (var t in c.tasks) Visit(t);
            bool Quotas(StreamQuota[] quotas) => quotas != null && quotas.All(q => q != null && c.workers.Any(w => w.stream == q.stream) && q.effort >= 0) &&
                quotas.Select(q => q.stream).Distinct().Count() == quotas.Length;
            int next = 1; var sprintIds = new HashSet<string>();
            foreach (var s in c.sprints) { Require(s != null && !string.IsNullOrWhiteSpace(s.id) && sprintIds.Add(s.id) && s.startDay == next && s.endDay >= s.startDay &&
                s.endDay <= game.totalDays && Quotas(s.quotas)); next = s.endDay + 1; }
            Require(next == game.totalDays + 1);
            Require(new[] { c.producerCost, c.producerEffort, c.producerMoney, c.producerTeamBonus,
                c.restStressRecovery, c.overtimeMoneyCost, c.overtimeStress, c.sharedNightAffection }.All(n => n >= 0));
            if (c.useProducerStages) Require(c.producerEffort > 0 && c.sprints.All(s => s.producerGoal > 0) &&
                c.tasks.All(t => t.sprint != null && c.sprints.Contains(t.sprint)));
            Require(c.events.All(e => e != null && !string.IsNullOrWhiteSpace(e.id) && e.earliestDay >= 1 && e.latestDay >= e.earliestDay && e.latestDay <= game.totalDays &&
                e.tasks != null && e.tasks.All(t => c.tasks.Contains(t) && t.mainWeight == 0) && Quotas(e.extraQuotas) && e.absenceDays >= 0 &&
                (e.absentWorker == null ? e.absenceDays == 0 : c.workers.Any(w => w.character == e.absentWorker) && e.absenceDays > 0)) &&
                c.events.Select(e => e.id).Distinct().Count() == c.events.Length && c.events.SelectMany(e => e.tasks).Distinct().Count() == c.events.Sum(e => e.tasks.Length));
        }
    }
}
