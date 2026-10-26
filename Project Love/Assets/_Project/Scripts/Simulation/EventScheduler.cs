using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.VN;

namespace ProjectLove.Simulation
{
    // Pending events are derived, not latched: every safe point rechecks current conditions.
    public sealed class EventScheduler
    {
        private readonly GameSession session;
        private readonly EventDataSO[] catalog;
        private readonly string fingerprint;
        public bool Enabled => session.Config.events.Length > 0;
        public EventScheduler(GameSession session, IEnumerable<EventDataSO> catalog)
        {
            this.session = session;
            this.catalog = (catalog ?? Array.Empty<EventDataSO>()).Where(e => e != null).Distinct().ToArray();
            if (session.Config.events == null) throw new ArgumentException("Null event catalog.");
            if (!Enabled) return;
            var issues = ValidateCatalog(session.Config);
            if (issues.Count > 0) throw new ArgumentException(string.Join("\n", issues));
            fingerprint = Fingerprint();
            session.State.eventSchedule = new EventScheduleState { catalog = fingerprint };
        }
        private IOrderedEnumerable<EventDataSO> Ordered(IEnumerable<EventDataSO> events) => events
            .OrderByDescending(e => e.priority).ThenBy(e => e.endDay).ThenBy(e => e.preferredDay)
            .ThenBy(e => e.id, StringComparer.Ordinal);
        private bool Eligible(EventDataSO data) => session.CanEvaluateEvent(data) &&
            (data.room == null || data.room.id == session.State.roomId) &&
            (string.IsNullOrWhiteSpace(data.exclusiveGroup) || !catalog.Any(other => other != data &&
                other.exclusiveGroup == data.exclusiveGroup && session.State.completedEvents.Contains(other.id)));
        public bool IsCoolingDown(EventDataSO data) => data.repeatDaily && !string.IsNullOrWhiteSpace(data.repeatGroup) &&
            catalog.Where(e => e.repeatDaily && e.repeatGroup == data.repeatGroup).Any(e =>
                session.State.dailyInteractions.Any(r => r.eventId == e.id && session.State.day - r.day < Math.Max(1, data.minimumRepeatDays)));
        private bool Snoozed(EventDataSO data) => session.State.eventSchedule != null &&
            session.State.eventSchedule.deferred.Any(d => d.eventId == data.id && d.day == session.State.day && d.phase == session.State.phase);
        public EventDataSO NextForced => !Enabled || session.HasActiveModal ? null : Ordered(catalog.Where(e =>
            e.delivery == EventDelivery.Forced && Eligible(e) && !Snoozed(e))).FirstOrDefault();
        public bool HasForcedPending => NextForced != null;
        public bool TryBeginForced() => NextForced != null && session.Dialogue.TryBegin(NextForced);
        private bool VisibleCharacter(EventDataSO data) => data.character != null && session.CurrentRoom != null &&
            session.CurrentRoom.hotspots.Any(h => h.character == data.character && h.placements.Any(p => p.phase == session.State.phase && p.visible));
        public EventDataSO[] BubbleEvents()
        {
            if (!Enabled || session.IsBusy) return Array.Empty<EventDataSO>();
            return Ordered(catalog.Where(e => Eligible(e) && VisibleCharacter(e) &&
                (e.delivery == EventDelivery.CharacterBubble || e.delivery == EventDelivery.Forced && Snoozed(e))))
                .GroupBy(e => e.character.id).Select(g => g.First()).ToArray();
        }
        // Deferred global forced events need an explicit retry entry even without a character hotspot.
        public EventDataSO[] DeferredGlobalEvents() => !Enabled || session.IsBusy ? Array.Empty<EventDataSO>() :
            Ordered(catalog.Where(e => e.delivery == EventDelivery.Forced && !VisibleCharacter(e) && Eligible(e) && Snoozed(e))).ToArray();
        public bool CanBegin(EventDataSO data)
        {
            var forced = NextForced;
            if (forced != null) return forced == data;
            if (!Enabled || !catalog.Contains(data)) return true;
            if (!Eligible(data)) return false;
            if (data.delivery == EventDelivery.Manual) return true;
            if (data.delivery == EventDelivery.Forced && !Snoozed(data)) return false;
            if (data.delivery == EventDelivery.Forced && !VisibleCharacter(data)) return true;
            if (data.character == null) return data.delivery == EventDelivery.Forced;
            return BubbleEvents().Contains(data);
        }
        // Explicitly supplied legacy catalogs retain their original ordering and manual start API.
        public EventDataSO[] Available() => !Enabled ? catalog.Where(session.CanStartEvent)
            .OrderByDescending(e => e.priority).ThenBy(e => e.preferredDay).ThenBy(e => e.id, StringComparer.Ordinal).ToArray() :
            Ordered(catalog.Where(session.CanStartEvent)).OrderBy(e => e.delivery == EventDelivery.Forced ? 0 : 1).ToArray();
        public bool TryBeginNext()
        {
            var next = Available().FirstOrDefault();
            return next != null && session.Dialogue.TryBegin(next);
        }
        public void Defer(EventDataSO data)
        {
            if (!Enabled || data == null || !catalog.Contains(data) || data.delivery != EventDelivery.Forced) return;
            session.State.eventSchedule.deferred.RemoveAll(d => d.eventId == data.id);
            session.State.eventSchedule.deferred.Add(new EventDeferral { eventId = data.id, day = session.State.day, phase = session.State.phase });
        }
        public bool CanRestore(RuntimeGameState state)
        {
            if (!Enabled) return state.eventSchedule == null;
            var schedule = state.eventSchedule;
            return schedule != null && schedule.catalog == fingerprint && schedule.deferred != null &&
                schedule.deferred.All(d => d != null && catalog.Any(e => e.id == d.eventId && e.delivery == EventDelivery.Forced) &&
                    d.day >= 1 && d.day <= state.day && Enum.IsDefined(typeof(DayPhase), d.phase) &&
                    (d.day < state.day || d.phase <= state.phase)) &&
                schedule.deferred.Select(d => d.eventId).Distinct().Count() == schedule.deferred.Count &&
                catalog.Where(e => !string.IsNullOrWhiteSpace(e.exclusiveGroup) && state.completedEvents.Contains(e.id))
                    .GroupBy(e => e.exclusiveGroup).All(g => g.Count() <= 1);
        }
        private string Fingerprint()
        {
            var text = new StringBuilder("event-schedule-v1");
            void Add(params object[] values) { foreach (var v in values) { var s = v?.ToString() ?? ""; text.Append(s.Length).Append(':').Append(s); } }
            foreach (var e in catalog.OrderBy(e => e.id, StringComparer.Ordinal))
            {
                Add(e.id, e.delivery, e.priority, e.startDay, e.endDay, e.preferredDay, e.anyPhase, e.triggerPhase,
                    e.elapsedPhases, e.staminaCost, e.repeatDaily, e.minimumRepeatDays, e.repeatGroup, e.character?.id, e.room?.id, e.exclusiveGroup);
                foreach (var c in e.conditions) Add(c.type, c.stat, c.character?.id, c.value, c.flagId, c.eventId, c.targetId);
                var visited = new HashSet<DialogueNodeSO>();
                void Node(DialogueNodeSO n)
                {
                    if (n == null || !visited.Add(n)) return;
                    Add(n.id, n.nextNode?.id);
                    foreach (var l in n.lines) Add(l.lineId);
                    foreach (var c in n.choices)
                    {
                        Add(c.choiceId, c.actionPointCost, c.staminaCost, c.nextNode?.id, c.workAction, c.workCharacter?.id);
                        foreach (var condition in c.conditions) Add(condition.type, condition.stat, condition.character?.id, condition.value, condition.flagId, condition.eventId, condition.targetId);
                        foreach (var effect in c.effects) Add(effect.stat, effect.character?.id, effect.amount);
                        foreach (var flag in c.flags) Add(flag.flagId, flag.value);
                        foreach (var id in c.activateWorkloads) Add(id);
                        Node(c.nextNode);
                    }
                    Node(n.nextNode);
                }
                Node(e.entryNode);
            }
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-", "");
        }
        private static bool LinearEpilogue(DialogueNodeSO node)
        {
            var visited = new HashSet<DialogueNodeSO>();
            while (node != null)
            {
                if (!visited.Add(node) || node.choices == null || node.choices.Length != 0) return false;
                node = node.nextNode;
            }
            return true;
        }
        public static List<string> ValidateCatalog(GameConfigSO config)
        {
            var issues = new List<string>();
            if (config.events == null) { issues.Add("Null event catalog."); return issues; }
            bool ValidCondition(Condition c)
            {
                if (c == null || !Enum.IsDefined(typeof(ConditionType), c.type)) return false;
                switch (c.type)
                {
                    case ConditionType.WorkloadActivated: case ConditionType.WorkloadNotActivated:
                        return config.production != null && config.production.events.Any(w => w.id == c.targetId);
                    case ConditionType.TaskAccepted: case ConditionType.TaskNotAccepted:
                        return config.production != null && config.production.tasks.Any(t => t.id == c.targetId);
                    case ConditionType.CharacterCanVolunteer:
                        if (c.value < 0) return false;
                        goto case ConditionType.CharacterOvertime;
                    case ConditionType.CharacterSharedNight:
                    case ConditionType.CharacterOvertime:
                        return config.production != null && c.character != null && config.production.workers.Any(w => w.character == c.character);
                    case ConditionType.EventCompleted: case ConditionType.EventNotCompleted:
                        return !string.IsNullOrWhiteSpace(c.eventId);
                    case ConditionType.FlagSet: case ConditionType.FlagUnset:
                        return !string.IsNullOrWhiteSpace(c.flagId);
                    default:
                        return Enum.IsDefined(typeof(StatType), c.stat) &&
                            (c.stat != StatType.Affection && c.stat != StatType.Stress || config.characters.Contains(c.character));
                }
            }
            var ids = new HashSet<string>();
            foreach (var e in config.events)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.id) || !ids.Add(e.id)) { issues.Add("Missing/duplicate event ID."); continue; }
                if (!Enum.IsDefined(typeof(EventDelivery), e.delivery) || !Enum.IsDefined(typeof(DayPhase), e.triggerPhase) ||
                    e.startDay < 1 || e.startDay > e.preferredDay || e.preferredDay > e.endDay || e.endDay > config.totalDays ||
                    e.elapsedPhases < 0 || e.elapsedPhases > 4 || e.staminaCost < 0 || e.minimumRepeatDays < 0 ||
                    !e.repeatDaily && (e.minimumRepeatDays != 0 || !string.IsNullOrWhiteSpace(e.repeatGroup)) || e.conditions == null || e.conditions.Any(c => !ValidCondition(c)) ||
                    e.repeatDaily && !string.IsNullOrWhiteSpace(e.exclusiveGroup) ||
                    e.room != null && !config.rooms.Contains(e.room) || e.character != null && !config.characters.Contains(e.character) ||
                    e.delivery == EventDelivery.CharacterBubble && (e.character == null || !config.rooms.Any(r => r.hotspots.Any(h => h.character == e.character))))
                    issues.Add(e.name + ": invalid schedule, group or room/character target.");
                var visited = new HashSet<DialogueNodeSO>();
                var path = new HashSet<DialogueNodeSO>();
                void Node(DialogueNodeSO n)
                {
                    if (n == null) return;
                    if (path.Contains(n)) { issues.Add(e.name + ": dialogue cycle; use another event for repeatable content."); return; }
                    if (!visited.Add(n)) return;
                    if (!DialogueRunner.IsPlayable(n)) { issues.Add(e.name + ": invalid dialogue node."); return; }
                    path.Add(n);
                    if (n.nextNode != null && n.choices.Length > 0) issues.Add(n.name + ": ambiguous next link.");
                    foreach (var c in n.choices)
                    {
                        if (c == null || string.IsNullOrWhiteSpace(c.choiceId) || string.IsNullOrWhiteSpace(c.SourceText) || c.actionPointCost < 0 || c.staminaCost < 0 ||
                            c.conditions == null || c.conditions.Any(x => !ValidCondition(x)) || c.effects == null || c.effects.Any(x => x == null ||
                                !Enum.IsDefined(typeof(StatType), x.stat) || x.stat == StatType.MainProgress && config.production != null ||
                                (x.stat == StatType.Affection || x.stat == StatType.Stress) && !config.characters.Contains(x.character)) ||
                            c.flags == null || c.flags.Any(x => x == null || string.IsNullOrWhiteSpace(x.flagId)) || c.activateWorkloads == null || c.activateWorkloads.Distinct().Count() != c.activateWorkloads.Length)
                        { issues.Add(n.name + ": invalid choice transaction."); continue; }
                        foreach (var id in c.activateWorkloads)
                            if (config.production == null || !config.production.events.Any(w => w.id == id && !w.automatic)) issues.Add(n.name + ": missing/manual workload target " + id);
                        if (!Enum.IsDefined(typeof(DialogueWorkAction), c.workAction) ||
                            c.workAction != DialogueWorkAction.None && (config.production == null || c.workCharacter == null ||
                                !config.production.workers.Any(w => w.character == c.workCharacter) || c.activateWorkloads.Length > 0 ||
                                e.elapsedPhases != 0 || !LinearEpilogue(c.nextNode)))
                            issues.Add(n.name + ": invalid work command or non-linear epilogue.");
                        Node(c.nextNode);
                    }
                    Node(n.nextNode); path.Remove(n);
                }
                if (e.entryNode == null) issues.Add(e.name + ": missing entry node."); else Node(e.entryNode);
            }
            var finished = new HashSet<EventDataSO>();
            var visiting = new HashSet<EventDataSO>();
            void Visit(EventDataSO e)
            {
                if (visiting.Contains(e)) { issues.Add(e.name + ": circular EventCompleted requirements."); return; }
                if (!finished.Add(e)) return;
                visiting.Add(e);
                foreach (var condition in e.conditions ?? Array.Empty<Condition>())
                    if (condition != null && condition.type == ConditionType.EventCompleted)
                    {
                        var prerequisite = config.events.FirstOrDefault(x => x != null && x.id == condition.eventId);
                        if (prerequisite != null)
                        {
                            if (!string.IsNullOrWhiteSpace(e.exclusiveGroup) && e.exclusiveGroup == prerequisite.exclusiveGroup)
                                issues.Add(e.name + ": completed prerequisite is mutually exclusive with this event.");
                            Visit(prerequisite);
                        }
                    }
                visiting.Remove(e);
            }
            foreach (var e in config.events.Where(e => e != null)) Visit(e);
            return issues;
        }
    }
}
