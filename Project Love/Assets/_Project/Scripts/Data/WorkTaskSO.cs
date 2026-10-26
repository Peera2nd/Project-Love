using UnityEngine;

namespace ProjectLove.Data
{
    [CreateAssetMenu(menuName = "Project Love/Work Task")]
    public sealed class WorkTaskSO : IdentifiedDataSO
    {
        public string displayName;
        [Tooltip("ต้นแบบแผนงาน: ต้องตรวจรับโดยผู้เล่น แทนส่งต่ออัตโนมัติ")] public bool requiresManualReview;
        public WorkStreamSO stream;
        public CharacterDataSO owner;
        [Min(1), Tooltip("หน่วยงานที่ต้องทำก่อนส่งตรวจ")] public int effort;
        [Min(0), Tooltip("น้ำหนักเป้าหมายหลัก งานแทรกใช้ 0")] public int mainWeight;
        [Min(1)] public int releaseDay;
        [Tooltip("ต้องให้ Producer เคลียร์การตัดสินใจก่อน")] public bool decisionBlocked;
        public WorkTaskSO[] prerequisites = new WorkTaskSO[0];
        [Tooltip("Sprint เจ้าของงาน ใช้ทั้งการปลดล็อคและนับเป้า แม้ทำล่วงหน้า")] public SprintDataSO sprint;
    }
}
