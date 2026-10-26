using System;
using UnityEngine;
namespace ProjectLove.Data
{
    public enum CharacterMood { Calm, Cheerful, Tired, NeedsSupport }
    public enum RomanceActivity { WorkTogether, PersonalTime, Producer }
    [Serializable] public sealed class MoodDefinition { public CharacterMood mood; public string label, icon; }
    [Serializable] public sealed class RomanceCast
    {
        public CharacterDataSO character;
        public string workTitle, topicTitle, topicFlag;
        public int warmAffection;
        public CharacterMood initialMood, sharedMood, personalMood;
        public DialogueNodeSO sharedScene, personalScene, comfortScene, warmScene, focusTalk;
    }
    [Serializable] public sealed class RomanceMilestone { public int startDay, endDay, target; public string title; [HideInInspector] public string localizationId; }
    [Serializable] public sealed class MoodSchedule { public int day; public CharacterDataSO character; public CharacterMood mood; }
    [Serializable] public sealed class MoodOutcome { public string eventId, requiredFlag; public CharacterDataSO character; public CharacterMood mood; }
    [CreateAssetMenu(menuName="Project Love/Romance Rules")]
    public sealed class RomanceConfigSO : IdentifiedDataSO
    {
        [Header("วงจรเล่นแบบเน้นความสัมพันธ์")]
        [Min(0)] public int teamDailyProgress, sharedProgress, sharedAffection, personalAffection, producerProgress, producerMoney, focusAffection;
        [Min(1)] public int daysPerFocus;
        public DialogueNodeSO producerScene;
        public RomanceCast[] cast = Array.Empty<RomanceCast>();
        public MoodDefinition[] moods = Array.Empty<MoodDefinition>();
        public RomanceMilestone[] milestones = Array.Empty<RomanceMilestone>();
        public MoodSchedule[] moodSchedule = Array.Empty<MoodSchedule>();
        public MoodOutcome[] moodOutcomes = Array.Empty<MoodOutcome>();
        public RoomPresentationSO presentation;
        [TextArea] public string[] dailyHints = Array.Empty<string>();
    }
}
