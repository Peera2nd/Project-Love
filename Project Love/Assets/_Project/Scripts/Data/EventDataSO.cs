using UnityEngine;

namespace ProjectLove.Data
{
    public enum EventDelivery { Manual, Forced, CharacterBubble }
    [CreateAssetMenu(menuName = "Project Love/Event")]
    public sealed class EventDataSO : IdentifiedDataSO
    {
        public string displayName;
        [Min(1)] public int startDay = 1;
        [Min(1)] public int endDay = 1;
        [Min(1), Tooltip("วันเป้าหมาย ไม่ใช่วันเดียวที่เข้าถึงได้")]
        public int preferredDay = 1;
        public DayPhase triggerPhase;
        [Tooltip("ลำดับความสำคัญ ค่ามากแสดงก่อน") ] public int priority;
        [Tooltip("บทคุยทั่วไป กลับมาใช้ได้วันถัดไป") ] public bool repeatDaily;
        [Min(0), Tooltip("เว้นอย่างน้อยกี่วันหลังจบครั้งก่อน ใช้กับอีเว้นรายวัน")]
        public int minimumRepeatDays;
        [Tooltip("ใช้ระยะเว้นร่วมกันระหว่างบทอาสาหลายรูปแบบ") ] public string repeatGroup;
        public bool anyPhase;
        [Range(0, 4)] public int elapsedPhases;
        [Min(0)] public int staminaCost;
        public Condition[] conditions = new Condition[0];
        public DialogueNodeSO entryNode;
        [Tooltip("Manual = ระบบเดิม, Forced = เปิดเมื่อว่าง, CharacterBubble = กดเหนือหัว")]
        public EventDelivery delivery;
        [Tooltip("เจ้าของบับเบิล/ผู้ที่เกี่ยวข้องกับอีเว้น")]
        public CharacterDataSO character;
        [Tooltip("เว้นว่างได้สำหรับอีเว้นบังคับที่เล่นได้ทุกห้อง")]
        public RoomDataSO room;
        [Tooltip("กลุ่มทางเลือก: จบเรื่องหนึ่งแล้วเรื่องอื่นในกลุ่มจะไม่เกิด ไม่ใช้กับอีเว้นรายวัน")]
        public string exclusiveGroup;
    }
}
