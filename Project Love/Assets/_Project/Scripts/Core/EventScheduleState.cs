using System;
using System.Collections.Generic;
using ProjectLove.Data;

namespace ProjectLove.Core
{
    [Serializable] public sealed class EventDeferral
    {
        public string eventId;
        public int day;
        public DayPhase phase;
    }

    [Serializable] public sealed class EventScheduleState
    {
        public string catalog;
        public List<EventDeferral> deferred = new List<EventDeferral>();
    }
}
