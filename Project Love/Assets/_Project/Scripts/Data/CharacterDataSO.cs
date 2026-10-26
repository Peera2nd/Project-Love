using System;
using UnityEngine;

namespace ProjectLove.Data
{
    public enum CharacterExpression { Normal, Smile, Angry, Sad, Blush, Surprised }

    [Serializable]
    public sealed class EmotionSprite
    {
        public CharacterExpression expression;
        public Sprite sprite;
    }

    [CreateAssetMenu(menuName = "Project Love/Character")]
    public sealed class CharacterDataSO : IdentifiedDataSO
    {
        public string displayName;
        public string roleTitle;
        [Range(0, 100)] public int startingAffection;
        [Range(0, 100)] public int startingStress;
        public EmotionSprite[] sprites = new EmotionSprite[0];
        [Tooltip("รูปเจ้าของงานบนการ์ด ใช้ภาพ Normal ชั่วคราวหากยังไม่มี")] public Sprite taskAvatar;
        [Tooltip("ข้อความเมื่อกดตัวละคร เลือกตามวัน เวลา ความสนิท และเงื่อนไข")]
        public ConversationVariant[] greetings = new ConversationVariant[0];
        public ConversationVariant[] conversations = new ConversationVariant[0];
        [Min(0), Tooltip("Stamina สำหรับการคุยครั้งแรกของวัน")]
        public int dailyTalkStaminaCost = 3;
        [Range(0, 100)] public int dailyTalkAffection = 2;
        public ActivityDataSO workActivity;
        public DialogueNodeSO neutralGiftResponse;
        [Range(-100, 100)] public int neutralGiftAffection = 1;
        public GiftPreference[] giftPreferences = new GiftPreference[0];

        public Sprite GetSprite(CharacterExpression expression)
        {
            foreach (var entry in sprites)
                if (entry != null && entry.expression == expression) return entry.sprite;
            return null;
        }
    }
}
