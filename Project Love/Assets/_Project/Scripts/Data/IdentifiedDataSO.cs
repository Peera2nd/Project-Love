using System;
using UnityEngine;

namespace ProjectLove.Data
{
    public abstract class IdentifiedDataSO : ScriptableObject
    {
        [Tooltip("รหัสถาวร ห้ามเปลี่ยนเมื่อเริ่มใช้ในข้อมูลหรือเซฟแล้ว")]
        public string id;

        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id)) id = Guid.NewGuid().ToString("N");
        }
    }
}
