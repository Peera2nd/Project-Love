using System;
using UnityEngine;

namespace ProjectLove.Data
{
    public enum DialogueWorkAction { None, Overtime, WorkTogether }
    [Serializable]
    public sealed class DialogueLine
    {
        public string lineId;
        public CharacterDataSO speaker;
        public CharacterExpression expression;
        [TextArea(2, 8)] public string text;
        public LocalizedTextRef localizedText;
        public string SourceText => localizedText.Source(text);
    }

    [Serializable]
    public sealed class DialogueChoice
    {
        public string choiceId;
        [TextArea] public string text;
        public LocalizedTextRef localizedText;
        public string SourceText => localizedText.Source(text);
        [Min(0)] public int actionPointCost;
        [Min(0)] public int staminaCost;
        public Condition[] conditions = new Condition[0];
        public StatModifier[] effects = new StatModifier[0];
        public FlagChange[] flags = new FlagChange[0];
        [Tooltip("รหัส Workload ที่เพิ่มเมื่อเลือก คืนค่าทั้งหมดหากย้อนออกจากอีเว้น")]
        public string[] activateWorkloads = new string[0];
        [Tooltip("คำสั่งงานแบบธุรกรรมเดียว คืนค่าทั้งหมดเมื่อย้อนออก")]
        public DialogueWorkAction workAction;
        public CharacterDataSO workCharacter;
        public DialogueNodeSO nextNode;
    }

    [CreateAssetMenu(menuName = "Project Love/Dialogue Node")]
    public sealed class DialogueNodeSO : IdentifiedDataSO
    {
        [Tooltip("ชื่อฉากสำหรับค้นหาในเครื่องมือ ไม่เปลี่ยนรหัสบท") ] public string displayName;
        public DialogueLine[] lines = new DialogueLine[0];
        public DialogueChoice[] choices = new DialogueChoice[0];
        public DialogueNodeSO nextNode;
    }
}
