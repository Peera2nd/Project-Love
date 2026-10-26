using System;
using System.Collections.Generic;
using ProjectLove.Data;
using UnityEngine;

namespace ProjectLove.Core
{
    [Serializable]
    public sealed class CharacterRuntimeState
    {
        public string characterId;
        public int affection;
        public int stress;
        public int lastTalkDay;
    }

    [Serializable] public sealed class DailyInteractionState { public string eventId; public int day; }
    [Serializable] public sealed class InventoryEntry { public string itemId; public int quantity; }
    [Serializable] public sealed class WorkProgressEntry { public string streamId; public int completed; }

    [Serializable]
    public sealed class RuntimeGameState
    {
        public int day = 1;
        public DayPhase phase;
        public int actionPoints;
        public int stamina;
        public string roomId;
        public List<DailyInteractionState> dailyInteractions = new List<DailyInteractionState>();
        public List<InventoryEntry> inventory = new List<InventoryEntry>();
        public List<string> collectedPickups = new List<string>();
        public List<WorkProgressEntry> workProgress = new List<WorkProgressEntry>();
        public ProductionState production;
        public EventScheduleState eventSchedule;
        public RomanceState romance;
        public int money;
        public int skillPoints;
        public int mainProgress;
        public bool dayEnded;
        public int dayStartProgress;
        public List<CharacterRuntimeState> characters = new List<CharacterRuntimeState>();
        public List<string> flags = new List<string>();
        public List<string> completedEvents = new List<string>();
        public List<string> readLines = new List<string>();
        public List<string> chosenOptions = new List<string>();

        public RuntimeGameState Clone()
        {
            var copy = JsonUtility.FromJson<RuntimeGameState>(JsonUtility.ToJson(this));
            // Unity inline class serialization does not preserve null as a schema boundary.
            if (production == null) copy.production = null;
            else if (production.planning == null) copy.production.planning = null;
            if (eventSchedule == null) copy.eventSchedule = null;
            if (romance == null) copy.romance = null;
            return copy;
        }

        internal void CopyFrom(RuntimeGameState source)
        {
            var copy = source.Clone();
            day = copy.day; phase = copy.phase; actionPoints = copy.actionPoints;
            stamina = copy.stamina; roomId = copy.roomId;
            dailyInteractions = copy.dailyInteractions; inventory = copy.inventory; collectedPickups = copy.collectedPickups;
            workProgress = copy.workProgress;
            production = copy.production;
            eventSchedule = copy.eventSchedule;
            romance = copy.romance;
            money = copy.money; skillPoints = copy.skillPoints; mainProgress = copy.mainProgress;
            dayEnded = copy.dayEnded; dayStartProgress = copy.dayStartProgress;
            characters = copy.characters; flags = copy.flags; completedEvents = copy.completedEvents;
            readLines = copy.readLines; chosenOptions = copy.chosenOptions;
        }

        public CharacterRuntimeState FindCharacter(string id) => characters.Find(c => c.characterId == id);
    }
}
