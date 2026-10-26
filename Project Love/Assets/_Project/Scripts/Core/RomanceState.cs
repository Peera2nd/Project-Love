using System;
using System.Collections.Generic;
using ProjectLove.Data;
namespace ProjectLove.Core
{
    [Serializable] public sealed class CharacterMoodState { public string characterId; public CharacterMood mood; }
    [Serializable] public sealed class AffectionSnapshot { public string characterId; public int value; }
    [Serializable] public sealed class WeeklyFocusState { public int week; public string characterId; public bool claimed; }
    [Serializable] public sealed class RomanceDayReport
    {
        public int day, projectBefore, projectAfter, teamGain, moneyBefore, moneyAfter;
        public List<AffectionSnapshot> affectionBefore=new List<AffectionSnapshot>(), affectionAfter=new List<AffectionSnapshot>();
        public List<string> newTopics=new List<string>();
    }
    [Serializable] public sealed class RomanceReceipt
    {
        public string title, characterId, topicFlag, titleKey;
        public int project, affection, money;
    }
    [Serializable] public sealed class RomanceState
    {
        public string catalog;
        public int dayStartMoney, acknowledgedDay;
        public List<CharacterMoodState> moods=new List<CharacterMoodState>();
        public List<WeeklyFocusState> focuses=new List<WeeklyFocusState>();
        public List<AffectionSnapshot> dayStartAffection=new List<AffectionSnapshot>();
        public List<string> dayStartTopics=new List<string>();
        public List<RomanceDayReport> reports=new List<RomanceDayReport>();
        public RomanceReceipt receipt;
    }
}
