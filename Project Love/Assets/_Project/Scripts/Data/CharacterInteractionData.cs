using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectLove.Data
{
    public enum GiftReaction { Neutral, Liked, Disliked }

    [Serializable]
    public sealed class ConversationVariant
    {
        public string id;
        public string displayName;
        public int priority;
        [Min(1)] public int startDay = 1;
        [Min(1)] public int endDay = 30;
        public bool anyPhase = true;
        public DayPhase phase;
        [Range(0, 100)] public int minimumAffection;
        [Range(0, 100)] public int maximumAffection = 100;
        [Range(0, 100)] public int minimumStress;
        [Range(0, 100)] public int maximumStress = 100;
        [Tooltip("ใช้เมื่อยืนยัน OT ในคืนนี้แล้วเท่านั้น")] public bool overtimeOnly;
        public Condition[] conditions = new Condition[0];
        public DialogueNodeSO node;
        [Tooltip("บททางเลือกในชุดเดียวกัน รวมบทหลักเป็นพูล หมุนตามวันและช่วงเวลาโดยไม่เปลี่ยนเมื่อรีเฟรช")]
        public DialogueNodeSO[] pool = Array.Empty<DialogueNodeSO>();
        public IEnumerable<DialogueNodeSO> Nodes => new[] { node }.Concat(pool ?? Array.Empty<DialogueNodeSO>()).Where(n=>n!=null).Distinct();
    }

    [Serializable]
    public sealed class GiftPreference
    {
        public ItemDataSO item;
        public GiftReaction reaction;
        [Range(-100, 100)] public int affectionChange;
        public DialogueNodeSO response;
    }
}
