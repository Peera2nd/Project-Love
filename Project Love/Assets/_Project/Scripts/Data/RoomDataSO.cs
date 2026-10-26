using System;
using UnityEngine;
namespace ProjectLove.Data
{
    public enum RoomInteractionKind { Character, Activity, Pickup, Shop, Producer }
    [Serializable] public sealed class RoomPhaseArt
    {
        public DayPhase phase;
        public Sprite background;
    }
    [Serializable] public sealed class RoomPlacement
    {
        public DayPhase phase;
        public Sprite sprite;
        public Vector2 position = new Vector2(.5f, .5f);
        public Vector2 size = new Vector2(.2f, .3f);
        public bool visible = true;
    }
    [Serializable] public sealed class RoomHotspot
    {
        public string id, label;
        public RoomInteractionKind kind;
        public RoomPlacement[] placements = new RoomPlacement[0];
        public CharacterDataSO character;
        public EventDataSO conversation;
        public ActivityDataSO activity;
        public ItemDataSO item;
    }
    [CreateAssetMenu(menuName = "Project Love/Room")]
    public sealed class RoomDataSO : IdentifiedDataSO
    {
        public string displayName;
        public RoomPhaseArt[] phases = new RoomPhaseArt[0];
        public Sprite dayEndedBackground;
        public RoomHotspot[] hotspots = new RoomHotspot[0];
        public ItemDataSO[] shopItems = new ItemDataSO[0];
        [Range(0, 4)] public int entryPhaseCost;
        public Sprite Background(DayPhase phase, bool ended)
        {
            if (ended && dayEndedBackground != null) return dayEndedBackground;
            foreach (var entry in phases) if (entry.phase == phase) return entry.background;
            return phases.Length > 0 ? phases[0].background : null;
        }
    }
}
