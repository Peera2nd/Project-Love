using System;
using TMPro;
using UnityEngine;
namespace ProjectLove.Data
{
    [Serializable] public sealed class PresentationText { public string key; [TextArea] public string value; }
    [Serializable] public sealed class PresentationIcon { public string key; public Sprite sprite; }
    [CreateAssetMenu(menuName = "Project Love/Room Presentation")]
    public sealed class RoomPresentationSO : IdentifiedDataSO
    {
        public TMP_FontAsset font;
        public string[] phaseNames = new string[0];
        public PresentationText[] text = new PresentationText[0];
        public PresentationIcon[] icons = new PresentationIcon[0];
        public Sprite Icon(string key) => System.Array.Find(icons, i => i != null && i.key == key)?.sprite;
        [Header("ชุด UI สตูดิโอ")]
        public bool useStudioSkin;
        public Sprite panelSprite, taskCardSprite, avatarFrameSprite, selectionRingSprite;
        public Sprite barTrack, barFill, barFrame, barForecast;
        public Color affectionTint = new Color(.95f, .48f, .63f);
        public Color staminaTint = new Color(.38f, .82f, .61f);
        public Color stressTint = new Color(.96f, .61f, .32f);
        public Color producerTint = new Color(.34f, .75f, .67f);
        public Color progressTint = new Color(.40f, .73f, .90f);
        [HideInInspector] public int uiContentVersion;
        [Min(0)] public float charactersPerSecond = 32;
        [Range(0, 1)] public float dialogueDimAlpha = .7f;
        public string Get(string key)
        {
            foreach (var entry in text) if (entry.key == key) return entry.value;
            return "";
        }
    }
}
