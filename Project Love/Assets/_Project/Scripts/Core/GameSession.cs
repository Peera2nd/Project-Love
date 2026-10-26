using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Data;
using ProjectLove.Simulation;
using ProjectLove.VN;

namespace ProjectLove.Core
{
    public sealed class GameSession
    {
        public GameConfigSO Config { get; }
        public RuntimeGameState State { get; }
        public ActionPointSystem ActionPoints { get; }
        public DayPhaseManager Days { get; }
        public StatAndRelationshipManager Stats { get; }
        public DialogueRunner Dialogue { get; }
        public CharacterInteractions Interactions { get; }
        public ProductionSystem Production { get; }
        public EventScheduler Events { get; }
        public RomanceSystem Romance { get; }
        public LocalizationService Localization { get; }
        public bool BoardOpen { get; internal set; }
        public bool HasActiveModal => BoardOpen || (Dialogue != null && Dialogue.IsActive) || (Interactions != null && Interactions.IsOpen);
        public bool IsBusy => HasActiveModal || (Events != null && Events.HasForcedPending);
        public RoomDataSO CurrentRoom => Config.rooms.FirstOrDefault(r => r.id == State.roomId);
        public event Action Changed;

        public GameSession(GameConfigSO config)
        {
            if (config == null || config.totalDays < 1 || config.maxActionPointsPerDay < 1 ||
                config.startingMoney < 0 || config.startingSkillPoints < 0 ||
                config.startingProgress < 0 || config.startingProgress > 100 || config.characters == null ||
                config.items == null || config.rooms == null || config.workStreams == null || config.maxStamina < 1 ||
                config.startingStamina < 0 || config.startingStamina > config.maxStamina || config.overnightRecovery < 0)
                throw new ArgumentException("Invalid game configuration.", nameof(config));
            Config = config;
            Localization = new LocalizationService(config.localization);
            State = new RuntimeGameState
            {
                actionPoints = config.usesStamina ? 0 : config.maxActionPointsPerDay,
                stamina = config.usesStamina ? config.startingStamina : 0,
                roomId = config.startingRoomId,
                money = config.startingMoney, skillPoints = config.startingSkillPoints,
                mainProgress = config.startingProgress, dayStartProgress = config.startingProgress
            };
            var ids = new HashSet<string>();
            foreach (var character in config.characters)
            {
                if (character == null || string.IsNullOrWhiteSpace(character.id) || !ids.Add(character.id) ||
                    character.startingAffection < 0 || character.startingAffection > 100 ||
                    character.startingStress < 0 || character.startingStress > 100)
                    throw new ArgumentException("Invalid or duplicate configured character.", nameof(config));
                State.characters.Add(new CharacterRuntimeState { characterId = character.id,
                    affection = character.startingAffection, stress = character.startingStress });
            }
            ids.Clear();
            foreach (var item in config.items)
                if (item == null || string.IsNullOrWhiteSpace(item.id) || !ids.Add(item.id) || item.price < 0 ||
                    item.maxStack < 1 || item.staminaRecovery < 0) throw new ArgumentException("Invalid item catalog.", nameof(config));
            ids.Clear();
            foreach (var room in config.rooms)
                if (room == null || string.IsNullOrWhiteSpace(room.id) || !ids.Add(room.id) ||
                    room.hotspots == null || room.shopItems == null || room.phases == null || room.entryPhaseCost < 0 || room.entryPhaseCost > 4)
                    throw new ArgumentException("Invalid room catalog.", nameof(config));
            if (config.rooms.Length > 0 && !config.rooms.Any(r => r.id == config.startingRoomId))
                throw new ArgumentException("Missing starting room.", nameof(config));
            ids.Clear();
            foreach (var stream in config.workStreams)
            {
                if (stream == null || string.IsNullOrWhiteSpace(stream.id) || !ids.Add(stream.id))
                    throw new ArgumentException("Invalid work stream catalog.", nameof(config));
                State.workProgress.Add(new WorkProgressEntry { streamId = stream.id });
            }
            ActionPoints = new ActionPointSystem(State);
            Days = new DayPhaseManager(State, config);
            Stats = new StatAndRelationshipManager(State, c => Production != null && Production.CanVolunteer(c.character, c.value));
            Dialogue = new DialogueRunner(this);
            Interactions = new CharacterInteractions(this);
            Production = new ProductionSystem(this);
            Romance = new RomanceSystem(this);
            Events = new EventScheduler(this, config.events);
        }

        public bool CanRestore(RuntimeGameState value)
        {
            if (value == null || value.day < 1 || value.day > Config.totalDays ||
                !Enum.IsDefined(typeof(DayPhase), value.phase) || value.actionPoints < 0 || value.actionPoints > Config.maxActionPointsPerDay ||
                value.stamina < 0 || value.stamina > Config.maxStamina || (Config.usesStamina && value.actionPoints != 0) ||
                value.money < 0 || value.skillPoints < 0 || value.mainProgress < 0 || value.mainProgress > 100 ||
                value.dayStartProgress < 0 || value.dayStartProgress > 100 || value.characters == null ||
                value.characters.Count != Config.characters.Length || !ValidIds(value.flags) || !ValidIds(value.completedEvents) ||
                !ValidIds(value.readLines) || !ValidIds(value.chosenOptions) || !ValidIds(value.collectedPickups) ||
                value.inventory == null || value.dailyInteractions == null || value.workProgress == null ||
                value.workProgress.Count != Config.workStreams.Length ||
                (Config.rooms.Length > 0 && !Config.rooms.Any(r => r.id == value.roomId))) return false;
            var ids = new HashSet<string>();
            foreach (var character in value.characters)
                if (character == null || !ids.Add(character.characterId ?? "") || !Config.characters.Any(c => c.id == character.characterId) ||
                    character.affection < 0 || character.affection > 100 || character.stress < 0 || character.stress > 100 ||
                    character.lastTalkDay < 0 || character.lastTalkDay > value.day) return false;
            ids.Clear();
            foreach (var entry in value.inventory)
            {
                var item = entry != null ? Config.items.FirstOrDefault(i => i.id == entry.itemId) : null;
                if (item == null || !ids.Add(item.id) || entry.quantity < 1 || entry.quantity > item.maxStack) return false;
            }
            ids.Clear();
            foreach (var entry in value.dailyInteractions)
                if (entry == null || string.IsNullOrWhiteSpace(entry.eventId) || !ids.Add(entry.eventId) || entry.day < 1 || entry.day > value.day) return false;
            ids.Clear();
            foreach (var entry in value.workProgress)
                if (entry == null || !ids.Add(entry.streamId ?? "") || !Config.workStreams.Any(s => s.id == entry.streamId) || entry.completed < 0) return false;
            return Production.CanRestore(value) && Romance.CanRestore(value) && Events.CanRestore(value);
        }
        private static bool ValidIds(List<string> values) => values != null &&
            values.All(v => !string.IsNullOrWhiteSpace(v)) && values.Count == new HashSet<string>(values).Count;
        public bool TryRestore(RuntimeGameState value)
        {
            if (HasActiveModal || !CanRestore(value)) return false;
            State.CopyFrom(value); NotifyChanged(); return true;
        }
        public bool CanPay(int ap, int stamina) => !State.dayEnded && ap >= 0 && stamina >= 0 &&
            (Romance.Enabled ? ap == 0 && stamina == 0 : Config.usesStamina ? ap == 0 && State.stamina >= stamina : stamina == 0 && ActionPoints.CanSpend(ap));
        internal bool TryPay(int ap, int stamina)
        {
            if (!CanPay(ap, stamina)) return false;
            if (Config.usesStamina) State.stamina -= stamina; else ActionPoints.TrySpend(ap);
            return true;
        }
        public bool CanStartEvent(EventDataSO data) => !HasActiveModal && CanEvaluateEvent(data) && Events.CanBegin(data);
        internal bool CanEvaluateEvent(EventDataSO data) => data != null && !State.dayEnded &&
            State.day >= data.startDay && State.day <= data.endDay && (data.anyPhase || State.phase == data.triggerPhase) &&
            !string.IsNullOrWhiteSpace(data.id) && (data.repeatDaily ? !State.dailyInteractions.Any(e => e.eventId == data.id && State.day - e.day < Math.Max(1, data.minimumRepeatDays))
                : !State.completedEvents.Contains(data.id)) && Days.CanConsumePhases(data.elapsedPhases) &&
            CanPay(0, data.staminaCost) && (Events == null || !Events.IsCoolingDown(data)) && Stats.Meets(data.conditions) && DialogueRunner.IsPlayable(data.entryNode);
        internal void CompleteEvent(EventDataSO data, RuntimeGameState before = null)
        {
            if (data.repeatDaily)
            {
                var record = State.dailyInteractions.FirstOrDefault(e => e.eventId == data.id);
                if (record == null) State.dailyInteractions.Add(new DailyInteractionState { eventId = data.id, day = State.day });
                else record.day = State.day;
            }
            else if (!State.completedEvents.Contains(data.id)) State.completedEvents.Add(data.id);
            Romance.CompleteStory(data, before);
            ConsumePhases(data.elapsedPhases);
        }
        public bool CanCompleteActivity(ActivityDataSO activity, bool succeeded) => CanCompleteActivity(activity, succeeded, false);
        internal bool CanCompleteActivity(ActivityDataSO activity, bool succeeded, bool fromCharacter) => activity != null &&
            !BoardOpen && !Dialogue.IsActive && (fromCharacter || !Interactions.IsOpen) &&
            (!Production.Enabled || (activity.successWork.Length == 0 && activity.failureWork.Length == 0 &&
                !activity.successEffects.Concat(activity.failureEffects).Any(e => e.stat == StatType.MainProgress))) &&
            CanPay(activity.actionPointCost, activity.staminaCost) && activity.staminaRecovery >= 0 &&
            Days.CanConsumePhases(activity.elapsedPhases) && Stats.Meets(activity.conditions) &&
            CanApplyWork(succeeded ? activity.successWork : activity.failureWork) &&
            Stats.CanApply(succeeded ? activity.successEffects : activity.failureEffects, succeeded ? activity.successFlags : activity.failureFlags);
        public bool TryCompleteActivity(ActivityDataSO activity, bool succeeded)
        {
            if (!CanCompleteActivity(activity, succeeded)) return false;
            TryCommit(activity.actionPointCost, succeeded ? activity.successEffects : activity.failureEffects,
                succeeded ? activity.successFlags : activity.failureFlags, activity.staminaCost);
            if (Config.usesStamina) State.stamina = (int)Math.Min(Config.maxStamina, (long)State.stamina + activity.staminaRecovery);
            foreach (var output in succeeded ? activity.successWork : activity.failureWork)
            {
                var entry = State.workProgress.First(e => e.streamId == output.stream.id);
                entry.completed = (int)Math.Min(int.MaxValue, (long)entry.completed + output.amount);
            }
            ConsumePhases(activity.elapsedPhases);
            NotifyChanged(); return true;
        }
        internal bool TryCommit(int cost, StatModifier[] effects, FlagChange[] flags, int stamina = 0)
        {
            if (!CanApplyTransaction(effects, flags) || !TryPay(cost, stamina)) return false;
            Stats.ApplyValidated(effects, flags); return true;
        }
        internal bool CanApplyTransaction(StatModifier[] effects, FlagChange[] flags) => Stats.CanApply(effects, flags) &&
            (!Production.Enabled || !effects.Any(e => e.stat == StatType.MainProgress));
        private bool CanApplyWork(WorkOutput[] outputs) => outputs != null && outputs.All(o => o != null && o.amount >= 0 &&
            o.stream != null && Config.workStreams.Contains(o.stream));
        public int WorkCompleted(WorkStreamSO stream) => stream == null ? 0 : State.workProgress.FirstOrDefault(p => p.streamId == stream.id)?.completed ?? 0;
        public int ItemCount(ItemDataSO item) => item == null ? 0 : State.inventory.FirstOrDefault(e => e.itemId == item.id)?.quantity ?? 0;
        private bool CanAdd(ItemDataSO item) => item != null && Config.items.Contains(item) && ItemCount(item) < item.maxStack;
        private void Add(ItemDataSO item)
        {
            var entry = State.inventory.FirstOrDefault(e => e.itemId == item.id);
            if (entry == null) State.inventory.Add(new InventoryEntry { itemId = item.id, quantity = 1 }); else entry.quantity++;
        }
        public bool CanCollect(RoomHotspot hotspot) => !State.dayEnded && !IsBusy && CurrentRoom != null &&
            hotspot != null && CurrentRoom.hotspots.Contains(hotspot) && hotspot.kind == RoomInteractionKind.Pickup &&
            !string.IsNullOrWhiteSpace(hotspot.id) && !State.collectedPickups.Contains(hotspot.id) && CanAdd(hotspot.item);
        public bool TryCollect(RoomHotspot hotspot)
        {
            if (!CanCollect(hotspot)) return false;
            Add(hotspot.item); State.collectedPickups.Add(hotspot.id); NotifyChanged(); return true;
        }
        public bool CanBuy(ItemDataSO item) => !State.dayEnded && !IsBusy && CurrentRoom != null &&
            CurrentRoom.shopItems.Contains(item) && CanAdd(item) && item.price >= 0 && State.money >= item.price;
        public bool TryBuy(ItemDataSO item)
        {
            if (!CanBuy(item)) return false;
            State.money -= item.price; Add(item); NotifyChanged(); return true;
        }
        public bool CanUse(ItemDataSO item) => !State.dayEnded && !IsBusy && Config.usesStamina &&
            item != null && Config.items.Contains(item) && item.staminaRecovery > 0 && ItemCount(item) > 0 && State.stamina < Config.maxStamina;
        public bool TryUse(ItemDataSO item)
        {
            if (!CanUse(item)) return false;
            var entry = State.inventory.First(e => e.itemId == item.id); entry.quantity--;
            if (entry.quantity == 0) State.inventory.Remove(entry);
            State.stamina = (int)Math.Min(Config.maxStamina, (long)State.stamina + item.staminaRecovery);
            NotifyChanged(); return true;
        }
        public bool CanTravel(RoomDataSO room) => !IsBusy && !State.dayEnded && room != null &&
            Config.rooms.Contains(room) && room.id != State.roomId && Days.CanConsumePhases(room.entryPhaseCost);
        public bool TryTravel(RoomDataSO room)
        {
            if (!CanTravel(room)) return false;
            State.roomId = room.id; ConsumePhases(room.entryPhaseCost); NotifyChanged(); return true;
        }
        internal bool ConsumePhases(int count) => Romance.Enabled ? Romance.Consume(count) : Production.Enabled ? Production.Consume(count) : Days.TryConsumePhases(count);
        public bool TryAdvancePhase() => ChangeDayState(() => Production.Enabled || Romance.Enabled ? ConsumePhases(1) : Days.TryAdvancePhase());
        public bool TryEndDay() => ChangeDayState(() =>
        {
            if (!Events.Enabled) return Production.Enabled || Romance.Enabled ? ConsumePhases(4 - (int)State.phase) : Days.TryEndDay();
            bool changed = false;
            // Stop at a newly eligible forced event instead of skipping its phase with End Day.
            while (!State.dayEnded && !Events.HasForcedPending)
            { if (!ConsumePhases(1)) break; changed = true; }
            return changed;
        });
        public bool TryStartNextDay() => ChangeDayState(() =>
        {
            if (!Days.TryStartNextDay()) return false;
            if (Production.Enabled) Production.StartDay();
            if (Romance.Enabled) Romance.StartDay();
            State.roomId = Config.startingRoomId; return true;
        });
        private bool ChangeDayState(Func<bool> change)
        {
            if (IsBusy || !change()) return false;
            NotifyChanged(); return true;
        }
        internal void NotifyChanged() => Changed?.Invoke();
    }
}
