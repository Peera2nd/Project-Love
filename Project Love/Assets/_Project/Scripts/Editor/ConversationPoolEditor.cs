using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.VN;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.EditorTools
{
    public static class ConversationPoolEditor
    {
        static int kind,day=1,love;
        static DayPhase phase;
        static string flags="",completed="",result="";
        static DialogueNodeSO selected;
        static bool contextAdvanced;
        static CharacterDataSO owner;
        static readonly HashSet<string> collapsed=new HashSet<string>();
        public static void SelectKind(bool greeting){kind=greeting?0:1;result="";selected=null;}
        static ConversationVariant[] Groups(CharacterDataSO c,bool greeting)=>greeting?c.greetings:c.conversations;
        static void SetGroups(CharacterDataSO c,bool greeting,ConversationVariant[] groups){if(greeting)c.greetings=groups;else c.conversations=groups;}
        public static ConversationVariant AddGroup(GameConfigSO game,CharacterDataSO character,bool greeting)
        {
            var groups=Groups(character,greeting);var v=new ConversationVariant{id=Guid.NewGuid().ToString("N"),displayName="ชุดใหม่",endDay=game.totalDays,priority=groups.Length==0?0:groups.Max(g=>g.priority)+1};
            Undo.RecordObject(character,"Add conversation group");SetGroups(character,greeting,groups.Concat(new[]{v}).ToArray());
            AddAlternative(character,v,greeting);return v;
        }
        public static DialogueNodeSO AddAlternative(CharacterDataSO character,ConversationVariant group,bool greeting)
        {
            string parent=System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(character)).Replace('\\','/');string folder=parent+"/Dialogues";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder(parent,"Dialogues");
            var node=ScriptableObject.CreateInstance<DialogueNodeSO>();node.id="dialogue_"+Guid.NewGuid().ToString("N");node.displayName=character.displayName+" · "+(greeting?"ทักทาย":"คุยเล่น")+" · "+group.displayName+" · แบบ "+(group.Nodes.Count()+1);
            node.lines=new[]{new DialogueLine{lineId=Guid.NewGuid().ToString("N"),speaker=character,text=""}};
            AssetDatabase.CreateAsset(node,folder+"/"+node.id+".asset");Undo.RecordObject(character,"Add conversation alternative");
            if(group.node==null)group.node=node;else group.pool=(group.pool??Array.Empty<DialogueNodeSO>()).Concat(new[]{node}).ToArray();
            EditorUtility.SetDirty(character);AssetDatabase.SaveAssetIfDirty(character);return node;
        }
        public static string Probe(GameConfigSO game,CharacterDataSO character,bool greeting,int affection,int testDay,DayPhase testPhase,string testFlags,string testEvents,out DialogueNodeSO node)
        {
            var s=new GameSession(game);s.State.day=Mathf.Clamp(testDay,1,game.totalDays);s.State.phase=testPhase;s.State.FindCharacter(character.id).affection=Mathf.Clamp(affection,0,100);
            foreach(string f in testFlags.Split(',').Select(v=>v.Trim()).Where(v=>v.Length>0))if(!s.State.flags.Contains(f))s.State.flags.Add(f);
            foreach(string e in testEvents.Split(',').Select(v=>v.Trim()).Where(v=>v.Length>0))if(!s.State.completedEvents.Contains(e))s.State.completedEvents.Add(e);
            var groups=Groups(character,greeting);node=s.Interactions.SelectFor(character,groups);
            bool topic=!greeting&&s.Romance.Enabled&&s.State.flags.Contains(s.Romance.Cast(character.id).topicFlag);if(topic)node=s.Romance.Cast(character.id).focusTalk;
            var lines=new List<string>{node==null?"ไม่มีบทที่พร้อมใช้ในเงื่อนไขนี้":"บทที่เลือก: "+(string.IsNullOrEmpty(node.displayName)?node.name:node.displayName)};
            if(topic)lines.Add("ใช้บทหัวข้อที่ปลดล็อกจากการสนับสนุนก่อนบทคุยเล่นทั่วไป");
            foreach(var g in groups)
            {
                var reasons=new List<string>();
                if(affection<g.minimumAffection||affection>g.maximumAffection)reasons.Add("ความสนิทต้องอยู่ที่ "+g.minimumAffection+"–"+g.maximumAffection);
                if(testDay<g.startDay||testDay>g.endDay)reasons.Add("วันที่ต้องอยู่ที่ "+g.startDay+"–"+g.endDay);
                if(!g.anyPhase&&testPhase!=g.phase)reasons.Add("ช่วงเวลาไม่ตรง");
                if(!s.Stats.Meets(g.conditions))reasons.Add("เงื่อนไขเสริมยังไม่ผ่าน");
                if(!g.Nodes.Any(DialogueRunner.IsPlayable))reasons.Add("ยังไม่มีบทที่เขียนพร้อมเล่น");
                bool match=s.Interactions.MatchesVariant(character,g)&&g.Nodes.Any(DialogueRunner.IsPlayable);
                lines.Add((string.IsNullOrEmpty(g.displayName)?g.id:g.displayName)+": "+(match?"ผ่าน · ลำดับ "+g.priority:string.Join(" / ",reasons.Count>0?reasons:new List<string>{"เงื่อนไขเดิมไม่ผ่าน"})));
            }
            return string.Join("\n",lines);
        }
        public static void Draw(GameConfigSO game,CharacterDataSO character,Action<DialogueNodeSO> open,Action changed)
        {
            if(owner!=character){owner=character;result="";selected=null;}
            EditorGUILayout.Space();EditorGUILayout.LabelField("ชุดบทและเงื่อนไขความสัมพันธ์",EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();kind=GUILayout.Toolbar(kind,new[]{"ทักทาย","คุยเล่น"});if(EditorGUI.EndChangeCheck()){result="";selected=null;}
            bool greeting=kind==0;
            EditorGUILayout.HelpBox("แต่ละชุดกำหนดช่วงความสนิทได้ ใส่หลายแบบในพูลเพื่อหมุนบทตามวัน/ช่วงเวลา บทไม่เปลี่ยนเมื่อรีเฟรช\nหากหลายชุดผ่าน ใช้ลำดับสูงก่อน หากเท่ากันใช้รหัสคงที่",MessageType.Info);
            var so=new SerializedObject(character);so.Update();var groups=so.FindProperty(greeting?"greetings":"conversations");
            for(int i=0;i<groups.arraySize;i++)
            {
                var g=groups.GetArrayElementAtIndex(i);var group=Groups(character,greeting)[i];
                using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    string title=(string.IsNullOrEmpty(group.displayName)?"ชุด "+(i+1):group.displayName)+" · ♥ "+group.minimumAffection+"–"+group.maximumAffection+" · "+group.Nodes.Count()+" แบบ";
                    bool expanded=!collapsed.Contains(group.id);bool next=EditorGUILayout.Foldout(expanded,title,true);if(next)collapsed.Remove(group.id);else collapsed.Add(group.id);if(!next)continue;
                    EditorGUILayout.PropertyField(g.FindPropertyRelative("displayName"),new GUIContent("ชื่อชุด"));
                    using(new EditorGUILayout.HorizontalScope())
                    {
                        var min=g.FindPropertyRelative("minimumAffection");var max=g.FindPropertyRelative("maximumAffection");
                        min.intValue=Mathf.Clamp(EditorGUILayout.IntField("ความสนิทตั้งแต่",min.intValue),0,100);max.intValue=Mathf.Clamp(EditorGUILayout.IntField("ถึง",max.intValue),0,100);
                        if(min.intValue>max.intValue)EditorGUILayout.LabelField("ช่วงกลับด้าน",GUILayout.Width(65));
                    }
                    EditorGUILayout.PropertyField(g.FindPropertyRelative("priority"),new GUIContent("ลำดับความสำคัญ"));
                    EditorGUILayout.PropertyField(g.FindPropertyRelative("startDay"),new GUIContent("เริ่มพบวันที่"));EditorGUILayout.PropertyField(g.FindPropertyRelative("endDay"),new GUIContent("พบได้ถึงวันที่"));
                    var any=g.FindPropertyRelative("anyPhase");EditorGUILayout.PropertyField(any,new GUIContent("ทุกช่วงเวลา"));if(!any.boolValue){var p=g.FindPropertyRelative("phase");p.enumValueIndex=EditorGUILayout.Popup("เฉพาะช่วง",p.enumValueIndex,new[]{"เช้า","บ่าย","เย็น","กลางคืน"});}
                    EditorGUILayout.PropertyField(g.FindPropertyRelative("conditions"),new GUIContent("เงื่อนไขเสริม / ความทรงจำ"),true);
                    EditorGUILayout.LabelField("บทในพูล",EditorStyles.boldLabel);var pool=g.FindPropertyRelative("pool");
                    for(int n=0;n<=pool.arraySize;n++)
                    {
                        var reference=n==0?g.FindPropertyRelative("node"):pool.GetArrayElementAtIndex(n-1);
                        using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.PropertyField(reference,new GUIContent("บทที่ "+(n+1)));var node=reference.objectReferenceValue as DialogueNodeSO;
                            if(node!=null)
                            {
                                LocalizationAuthoring.BindNode(game,node);var text=new SerializedObject(node);text.Update();EditorGUILayout.PropertyField(text.FindProperty("displayName"),new GUIContent("ชื่อบท"));var rows=text.FindProperty("lines");
                                if(rows.arraySize>0){var line=rows.GetArrayElementAtIndex(0);EditorGUILayout.LabelField(greeting?"ประโยคทักทาย":"ประโยคแรก (เปิดบทเต็มเพื่อเพิ่มบรรทัด)");LocalizationAuthoring.DrawText(game,node,LocalizationService.LineKey(node,line.FindPropertyRelative("lineId").stringValue),line.FindPropertyRelative("text"),"บทพูด");}
                                text.ApplyModifiedProperties();if(!DialogueRunner.IsPlayable(node))EditorGUILayout.HelpBox("ยังเขียนไม่ครบ: บทนี้จะไม่ถูกเลือก",MessageType.Warning);
                            }
                            using(new EditorGUILayout.HorizontalScope())
                            {
                                using(new EditorGUI.DisabledScope(node==null))if(GUILayout.Button("เปิดบทเต็ม")){so.ApplyModifiedProperties();changed();open(node);GUIUtility.ExitGUI();}
                                if(GUILayout.Button("ถอดจากชุด")){reference.objectReferenceValue=null;if(n>0)pool.DeleteArrayElementAtIndex(n-1);so.ApplyModifiedProperties();changed();GUIUtility.ExitGUI();}
                            }
                        }
                    }
                    if(GUILayout.Button("+ เพิ่มบทอีกแบบในพูล")){so.ApplyModifiedProperties();AddAlternative(character,group,greeting);changed();GUIUtility.ExitGUI();}
                    if(GUILayout.Button("ถอดชุดนี้ (เก็บไฟล์บทไว้)")){groups.DeleteArrayElementAtIndex(i);so.ApplyModifiedProperties();changed();GUIUtility.ExitGUI();}
                }
            }
            if(so.ApplyModifiedProperties()){result="";selected=null;changed();}
            if(GUILayout.Button("+ เพิ่มชุดตามระดับความสัมพันธ์")){AddGroup(game,character,greeting);changed();GUIUtility.ExitGUI();}
            if(GUILayout.Button("บันทึกชุดบทของตัวละครนี้")){AssetDatabase.SaveAssets();changed();}
            EditorGUILayout.Space();EditorGUILayout.LabelField("ลองเช็คว่าระบบจะเลือกบทไหน",EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();love=EditorGUILayout.IntSlider("ความสนิททดสอบ",love,0,100);day=EditorGUILayout.IntSlider("วันที่ทดสอบ",day,1,game.totalDays);phase=(DayPhase)EditorGUILayout.Popup("ช่วงเวลาทดสอบ",(int)phase,new[]{"เช้า","บ่าย","เย็น","กลางคืน"});
            contextAdvanced=EditorGUILayout.Foldout(contextAdvanced,"บริบทความทรงจำ / อีเว้นที่ผ่าน",true);if(contextAdvanced){flags=EditorGUILayout.TextField("รหัสความทรงจำ (คั่น ,)",flags);completed=EditorGUILayout.TextField("รหัสอีเว้นที่ผ่าน (คั่น ,)",completed);}
            if(EditorGUI.EndChangeCheck()){result="";selected=null;}
            if(GUILayout.Button("เช็คเงื่อนไขและบทที่เลือก"))result=Probe(game,character,greeting,love,day,phase,flags,completed,out selected);
            if(!string.IsNullOrEmpty(result)){EditorGUILayout.HelpBox(result,MessageType.Info);if(selected!=null&&GUILayout.Button("เปิดบทที่ระบบเลือก"))open(selected);}
            EditorGUILayout.LabelField("ตรวจเฉพาะการเลือกบท ไม่แตะเซฟหรือสถานะเกมจริง · ทักทายใช้ประโยคแรกของแต่ละแบบ",EditorStyles.wordWrappedMiniLabel);
        }
    }
}
