using System.Linq;
using ProjectLove.Data;
using ProjectLove.UI;
using UnityEditor;
using UnityEngine;
namespace ProjectLove.EditorTools
{
    [CustomEditor(typeof(LocalizationCatalogSO))]
    public sealed class LocalizationCatalogInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("แก้ไทย–อังกฤษในแท็บภาษาและคำแปล เพื่อเก็บรุ่นต้นฉบับ การตรวจคำแปล และ Undo ให้ถูกต้อง",MessageType.Info);
            if(GUILayout.Button("เปิดภาษาและคำแปล"))
            {
                var game=AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath);
                DesignerDashboard.OpenTranslation(game,((LocalizationCatalogSO)target).entries.FirstOrDefault()?.key);
            }
            using(new EditorGUI.DisabledScope(true))DrawDefaultInspector();
        }
    }
    [CustomEditor(typeof(DialogueNodeSO))]
    public sealed class DialogueAuthoringInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("แก้บทแบบอ่านง่ายและค้นหาฉากที่เกี่ยวข้องได้ในหน้าทำงาน",MessageType.Info);
            if(GUILayout.Button("เปิดในหน้าทำงานเขียนบท"))DesignerDashboard.OpenNode((DialogueNodeSO)target);
            using(new EditorGUI.DisabledScope(EditorApplication.isPlaying || ((DialogueNodeSO)target).lines.Any(l=>l.localizedText.catalog!=null)))DrawDefaultInspector();
        }
    }
    [CustomEditor(typeof(AuthoredRomanceLayout))]
    public sealed class AuthoredLayoutInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("ตำแหน่ง ขนาด ฟอนต์ และสีอยู่ที่ชิ้น UI/Prefab จริง ใช้ Rect Tool และบันทึกตามปกติ ไม่ต้องแก้ตัวเลขในโค้ด ช่องด้านล่างเป็นการผูกข้อมูลของระบบ",MessageType.Info);
            if(GUILayout.Button("เปิดหน้าทำงานจัด UI"))DesignerDashboard.Open();
            if(GUILayout.Button("ดูตัวอย่างจาก Prefab")){var a=(AuthoredRomanceLayout)target;if(a.view?.config!=null)ArtistPreviewWindow.Open(a.view.config);}
            DrawDefaultInspector();
        }
    }
}
