using UnityEngine;

namespace ProjectLove.Data
{
    [CreateAssetMenu(menuName = "Project Love/Game Config")]
    public sealed class GameConfigSO : IdentifiedDataSO
    {
        [Min(1), Tooltip("จำนวนวันทั้งหมดของรอบเกม ปรับผ่านข้อมูล")]
        public int totalDays = 1;
        [Min(1), Tooltip("AP ที่ได้รับเมื่อเริ่มวันใหม่")]
        public int maxActionPointsPerDay = 1;
        [Min(0)] public int startingMoney;
        [Min(0)] public int startingSkillPoints;
        [Range(0, 100)] public int startingProgress;
        [Range(0, 100)] public int burnoutThreshold;
        [Tooltip("ชื่อสถานะเป้าหมายหลักที่แสดงในเกม")]
        public string mainProgressLabel;
        [Tooltip("ใช้พลังงานแทน AP สำหรับฉากรุ่นใหม่") ] public bool usesStamina;
        [Min(1)] public int maxStamina = 100;
        [Min(0)] public int startingStamina = 100;
        [Min(0)] public int overnightRecovery = 40;
        public ItemDataSO[] items = new ItemDataSO[0];
        public RoomDataSO[] rooms = new RoomDataSO[0];
        public string startingRoomId;
        public CharacterDataSO[] characters = new CharacterDataSO[0];
        public WorkStreamSO[] workStreams = new WorkStreamSO[0];
        public ProductionConfigSO production;
        public RomanceConfigSO romance;
        [Tooltip("ระบบภาษาไทย–อังกฤษ เว้นว่างเพื่อใช้ข้อมูลเดิม")]
        public LocalizationSettingsSO localization;
        [Tooltip("อีเว้นของรอบเกมนี้ เรียงคิวด้วย Priority แล้ววันหมดเขต ไม่ใช้ลำดับในรายการ")]
        public EventDataSO[] events = new EventDataSO[0];
        [HideInInspector] public int eventContentVersion;
    }
}
