using System;
using System.Collections.Generic;

namespace ProjectLove.Core
{
    [Serializable] public sealed class TaskState
    {
        public string taskId, ownerId, acceptedSprintId;
        public int effort, priority, acceptedDay;
        public bool decisionCleared;
    }
    [Serializable] public sealed class SprintResult
    {
        public string sprintId;
        public int reviewedDay, carryoverCount;
        public List<QuotaResult> quotas = new List<QuotaResult>();
    }
    [Serializable] public sealed class QuotaResult { public string streamId; public int completed, required; }
    [Serializable] public sealed class WorkloadRecord { public string eventId, sprintId; public int activatedDay; }
    [Serializable] public sealed class AbsenceRecord { public string characterId; public int throughDay; }
    public enum StaffOrderKind { Rest, Overtime }
    [Serializable] public sealed class SharedNightRecord
    { public string characterId, taskId; public int day, help, team; }
    [Serializable] public sealed class StaffOrder { public string characterId; public int day; public ProjectLove.Data.DayPhase phase; public StaffOrderKind kind; }
    [Serializable] public sealed class ProducerStageState { public string sprintId; public int effort, completedDay; public ProjectLove.Data.DayPhase completedPhase; }
    [Serializable] public sealed class ProductionState
    {
        public string catalog;
        public PlanningState planning;
        public int resolvedPhases, coordinationTicks;
        public List<TaskState> tasks = new List<TaskState>();
        public List<SprintResult> reviews = new List<SprintResult>();
        public List<WorkloadRecord> events = new List<WorkloadRecord>();
        public List<AbsenceRecord> absences = new List<AbsenceRecord>();
        public string lastReport;
        public List<StaffOrder> orders = new List<StaffOrder>();
        public List<SharedNightRecord> sharedNights = new List<SharedNightRecord>();
        public List<ProducerStageState> producerStages = new List<ProducerStageState>();
    }
}
