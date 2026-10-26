using System;
using System.Collections.Generic;
namespace ProjectLove.Core
{
    [Serializable] public sealed class TaskPlanState
    {
        public string taskId;
        public int start, due, started = -1, finished = -1;
        public float fraction;
    }
    [Serializable] public sealed class WorkerDayState { public string characterId; public int day, stress; }
    [Serializable] public sealed class SharedTimeState
    { public string characterId, taskId; public int cursor, affection; public bool nightWork; }
    [Serializable] public sealed class NightWorkerState { public string characterId; public int day; }
    [Serializable] public sealed class PlanningState
    {
        public string seed;
        public int revision;
        public List<TaskPlanState> tasks = new List<TaskPlanState>();
        public List<WorkerDayState> workers = new List<WorkerDayState>();
        public List<SharedTimeState> shared = new List<SharedTimeState>();
        public List<NightWorkerState> nights = new List<NightWorkerState>();
    }
}
