using System.Linq;
using ProjectLove.Data;
using UnityEditor;
using UnityEngine;
namespace ProjectLove.EditorTools
{
    public static class RoomPlacementEditor
    {
        static string selected;static Vector2 offset;static bool resizing;
        public static void Draw(RoomDataSO room,DayPhase phase)
        {
            EditorGUILayout.HelpBox("กดเลือกแล้วลากตัวละคร/จุดกด · ลากมุมขวาล่างเพื่อย่อขยาย · เลือกช่วงเวลาก่อนแก้ · บันทึกห้องเมื่อพอใจ",MessageType.Info);
            var background=room.phases.FirstOrDefault(p=>p.phase==phase);
            if(background!=null)
            {
                var sprite=(Sprite)EditorGUILayout.ObjectField("ภาพห้องช่วงนี้",background.background,typeof(Sprite),false);
                if(sprite!=background.background){Undo.RecordObject(room,"Set room background");background.background=sprite;EditorUtility.SetDirty(room);}
            }
            var board=GUILayoutUtility.GetAspectRect(1,GUILayout.MaxWidth(620));EditorGUI.DrawRect(board,Color.black);if(background?.background!=null)Sprite(board,background.background);
            var placements=room.hotspots.Select(h=>new{hotspot=h,place=h.placements.FirstOrDefault(p=>p.phase==phase)}).Where(v=>v.place!=null).ToArray();
            foreach(var item in placements)
            {
                var p=item.place;var rect=Bounds(board,p);var old=GUI.color;GUI.color=p.visible?Color.white:new Color(1,1,1,.3f);if(p.sprite!=null)Sprite(rect,p.sprite);else EditorGUI.DrawRect(rect,new Color(.3f,.6f,.7f,.4f));GUI.color=old;
                if(item.hotspot.id==selected){Outline(rect,Color.cyan);EditorGUI.DrawRect(new Rect(rect.xMax-7,rect.yMax-7,14,14),Color.cyan);}
            }
            int id=GUIUtility.GetControlID(FocusType.Passive);var e=Event.current;var hit=placements.FirstOrDefault(v=>v.hotspot.id==selected);
            if(e.type==EventType.MouseDown&&e.button==0&&board.Contains(e.mousePosition))
            {
                if(hit==null||!new Rect(Bounds(board,hit.place).xMax-10,Bounds(board,hit.place).yMax-10,20,20).Contains(e.mousePosition))hit=placements.Reverse().FirstOrDefault(v=>Bounds(board,v.place).Contains(e.mousePosition));
                if(hit!=null){selected=hit.hotspot.id;var rect=Bounds(board,hit.place);resizing=new Rect(rect.xMax-10,rect.yMax-10,20,20).Contains(e.mousePosition);offset=Point(board,e.mousePosition)-hit.place.position;Undo.RecordObject(room,"Arrange room placement");GUIUtility.hotControl=id;e.Use();}
            }
            if(e.type==EventType.MouseDrag&&GUIUtility.hotControl==id&&hit!=null)
            {
                var pt=Point(board,e.mousePosition);if(resizing)hit.place.size=new Vector2(Mathf.Clamp(Mathf.Abs(pt.x-hit.place.position.x)*2,.02f,1),Mathf.Clamp(Mathf.Abs(pt.y-hit.place.position.y)*2,.02f,1));else hit.place.position=new Vector2(Mathf.Clamp01(pt.x-offset.x),Mathf.Clamp01(pt.y-offset.y));
                EditorUtility.SetDirty(room);GUI.changed=true;e.Use();
            }
            if(e.type==EventType.MouseUp&&GUIUtility.hotControl==id){GUIUtility.hotControl=0;e.Use();}
            var current=placements.FirstOrDefault(v=>v.hotspot.id==selected);if(current!=null)
            {
                EditorGUILayout.LabelField(current.hotspot.character!=null?current.hotspot.character.displayName:current.hotspot.label,EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();bool visible=EditorGUILayout.Toggle("แสดงในช่วงเวลานี้",current.place.visible);var sprite=(UnityEngine.Sprite)EditorGUILayout.ObjectField("ท่าทาง / ภาพ",current.place.sprite,typeof(UnityEngine.Sprite),false);var pos=EditorGUILayout.Vector2Field("ตำแหน่ง",current.place.position);var size=EditorGUILayout.Vector2Field("ขนาด",current.place.size);
                if(EditorGUI.EndChangeCheck()){Undo.RecordObject(room,"Edit room placement");current.place.visible=visible;current.place.sprite=sprite;current.place.position=pos;current.place.size=new Vector2(Mathf.Max(.01f,size.x),Mathf.Max(.01f,size.y));EditorUtility.SetDirty(room);}
            }
            if(GUILayout.Button("บันทึกห้องนี้"))AssetDatabase.SaveAssetIfDirty(room);
        }
        public static void SetPlacement(RoomDataSO room,string id,DayPhase phase,Vector2 position,Vector2 size)
        {Undo.RecordObject(room,"Arrange room placement");var p=room.hotspots.First(h=>h.id==id).placements.First(p=>p.phase==phase);p.position=position;p.size=size;EditorUtility.SetDirty(room);}
        static Vector2 Point(Rect board,Vector2 p)=>new Vector2((p.x-board.x)/board.width,1-(p.y-board.y)/board.height);
        static Rect Bounds(Rect board,RoomPlacement p)=>new Rect(board.x+(p.position.x-p.size.x/2)*board.width,board.y+(1-p.position.y-p.size.y/2)*board.height,p.size.x*board.width,p.size.y*board.height);
        static void Sprite(Rect rect,Sprite s)
        {float scale=Mathf.Min(rect.width/s.rect.width,rect.height/s.rect.height);var dst=new Rect(rect.center.x-s.rect.width*scale/2,rect.center.y-s.rect.height*scale/2,s.rect.width*scale,s.rect.height*scale);GUI.DrawTextureWithTexCoords(dst,s.texture,new Rect(s.textureRect.x/s.texture.width,s.textureRect.y/s.texture.height,s.textureRect.width/s.texture.width,s.textureRect.height/s.texture.height),true);}
        static void Outline(Rect r,Color c){EditorGUI.DrawRect(new Rect(r.x,r.y,r.width,2),c);EditorGUI.DrawRect(new Rect(r.x,r.yMax-2,r.width,2),c);EditorGUI.DrawRect(new Rect(r.x,r.y,2,r.height),c);EditorGUI.DrawRect(new Rect(r.xMax-2,r.y,2,r.height),c);}
    }
}
