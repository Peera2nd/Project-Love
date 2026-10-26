using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Data;
using ProjectLove.UI;
using UnityEditor;
using UnityEngine;
namespace ProjectLove.EditorTools
{
    public sealed class DesignerDashboard : EditorWindow
    {
        [SerializeField]GameConfigSO campaign;
        [SerializeField]DialogueNodeSO selectedNode;
        [SerializeField]EventDataSO selectedEvent;
        [SerializeField]string query="",focusLine,uiQuery="";
        List<AuthoringEntry> entries=new List<AuthoringEntry>();
        Vector2 listScroll,detailScroll;int tab,personFilter,kindFilter,person,roomIndex;DayPhase roomPhase;
        string[] kinds=Array.Empty<string>();string message="",newName="";bool advanced,legacy;UnityEditor.Editor inspector;
        [MenuItem("Tools/Project Love/Dashboard")]
        [MenuItem("Tools/Project Love/Workspace")]
        public static void OpenTranslation(GameConfigSO game,string key)
        {var w=GetWindow<DesignerDashboard>("Project Love · ทำงาน");w.campaign=game;w.tab=4;LocalizationWorkspace.Select(key);w.Show();w.Repaint();}
        public static void Open()=>GetWindow<DesignerDashboard>("Project Love · ทำงาน").Show();
        public static void OpenNode(DialogueNodeSO node,string lineId=null)
        {var w=GetWindow<DesignerDashboard>("Project Love · ทำงาน");w.selectedNode=node;w.selectedEvent=w.entries.FirstOrDefault(e=>e.node==node)?.story;w.focusLine=lineId;w.query="";w.personFilter=w.kindFilter=0;w.tab=0;w.Show();}
        void OnEnable(){minSize=new Vector2(950,650);if(campaign==null)campaign=AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath);Refresh();Undo.undoRedoPerformed+=OnUndo;}
        void OnDisable(){Undo.undoRedoPerformed-=OnUndo;if(inspector!=null)DestroyImmediate(inspector);}
        void OnUndo(){LocalizationWorkspace.Invalidate();Refresh();Repaint();}
        void Refresh(){entries=campaign!=null?AuthoringCatalog.Entries(campaign):new List<AuthoringEntry>();kinds=new[]{"ทุกประเภท"}.Concat(entries.Select(e=>e.category).Distinct()).ToArray();kindFilter=Mathf.Clamp(kindFilter,0,kinds.Length-1);}
        void OnGUI()
        {
            EditorGUILayout.LabelField("Project Love · เขียนบทและจัดหน้าจอ",EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();campaign=(GameConfigSO)EditorGUILayout.ObjectField("เกมที่กำลังแก้",campaign,typeof(GameConfigSO),false);if(EditorGUI.EndChangeCheck()){selectedNode=null;selectedEvent=null;Refresh();}
            using(new EditorGUILayout.HorizontalScope())
            {
                if(GUILayout.Button("เปิดเกม"))ArtistWorkspaceBuilder.BuildOrOpen();
                using(new EditorGUI.DisabledScope(EditorApplication.isPlaying))if(GUILayout.Button("ดูตัวอย่างก่อนเล่น")&&campaign!=null)ArtistPreviewWindow.Open(campaign);
                if(GUILayout.Button("บทที่กำลังแสดง"))OpenPlayingLine();
                if(GUILayout.Button("รีเฟรช"))Refresh();
                using(new EditorGUI.DisabledScope(EditorApplication.isPlaying))if(GUILayout.Button("บันทึกข้อมูล")){if(LocalizationWorkspace.SavePending(campaign)){AssetDatabase.SaveAssets();Refresh();message="บันทึกข้อมูลแล้ว";}else message="คำแปลยังบันทึกไม่ได้ ตรวจรูปแบบในแท็บภาษาและคำแปล";}
            }
            if(campaign==null||campaign.romance==null){EditorGUILayout.HelpBox("เลือกข้อมูลเกมโรแมนซ์ปัจจุบันก่อน",MessageType.Info);return;}
            EditorGUILayout.LabelField("แสดงเฉพาะข้อมูลที่เกมนี้ใช้ · "+entries.Select(e=>e.node).Distinct().Count()+" ฉากบทพูด · ต้นแบบเก่าอยู่ด้านล่าง",EditorStyles.miniLabel);
            if(EditorApplication.isPlaying)EditorGUILayout.HelpBox("กำลังเล่น: เลือกดูต้นฉบับได้ หยุด Play ก่อนแก้และบันทึกข้อมูล",MessageType.Info);
            tab=GUILayout.Toolbar(tab,new[]{"เขียนบท / อีเว้น","ตัวละคร","ฉากและ UI","ตั้งค่าเกม","ภาษาและคำแปล"});
            using(new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                if(tab==0)DrawWriting();
                else {detailScroll=EditorGUILayout.BeginScrollView(detailScroll);if(tab==1)DrawCharacter();else if(tab==2)DrawScenes();else if(tab==4)LocalizationWorkspace.Draw(campaign);else DrawSettings();EditorGUILayout.EndScrollView();}
            }
            if(!string.IsNullOrEmpty(message))EditorGUILayout.HelpBox(message,MessageType.Info);
            if(GUI.changed)Repaint();
            legacy=EditorGUILayout.Foldout(legacy,"ต้นแบบเก่า / ตรวจระบบขั้นสูง",true);
            if(legacy)
            {
                using(new EditorGUI.DisabledScope(EditorApplication.isPlaying))using(new EditorGUILayout.HorizontalScope())
                {if(GUILayout.Button("Planning"))PlanningPrototypeBuilder.BuildOrOpen();if(GUILayout.Button("Studio"))StudioPrototypeBuilder.BuildOrOpen();if(GUILayout.Button("Room"))IsometricPrototypeBuilder.BuildOrOpen();if(GUILayout.Button("ตรวจข้อมูลทั้งหมด"))message=string.Join("\n",FoundationValidator.ValidateProject());}
                EditorGUILayout.LabelField("ไฟล์เดิมยังอยู่ครบ การแก้ข้อมูลเกมปัจจุบันใช้แท็บด้านบน",EditorStyles.miniLabel);
            }
        }
        void DrawWriting()
        {
            query=EditorGUILayout.TextField("ค้นประโยค / ชื่อฉาก",query);
            using(new EditorGUILayout.HorizontalScope())
            {
                personFilter=EditorGUILayout.Popup("ตัวละคร",Mathf.Clamp(personFilter,0,campaign.characters.Length),new[]{"ทุกคน"}.Concat(campaign.characters.Select(c=>c.displayName)).ToArray());kindFilter=EditorGUILayout.Popup("หมวด",kindFilter,kinds);
            }
            using(new EditorGUILayout.HorizontalScope())
            {
                listScroll=EditorGUILayout.BeginScrollView(listScroll,GUILayout.Width(Mathf.Clamp(position.width*.31f,240,390)));
                foreach(var e in AuthoringCatalog.Search(entries,query,personFilter==0?null:campaign.characters[personFilter-1],kindFilter==0?null:kinds[kindFilter]))
                {
                    if(DrawSceneEntry(e.title,e.category,e.node.lines.FirstOrDefault()?.SourceText??"",selectedNode==e.node))Select(e.node,e.story);
                }
                foreach(var draft in AuthoringCatalog.Drafts(campaign))if(DrawSceneEntry(draft.displayName,"ร่างยังไม่เข้าเกม",draft.entryNode?.lines.FirstOrDefault()?.SourceText??"",selectedEvent==draft))Select(draft.entryNode,draft);
                EditorGUILayout.Space();EditorGUILayout.LabelField("ชื่ออีเว้นใหม่");newName=EditorGUILayout.TextField(newName);if(GUILayout.Button("สร้างอีเว้นฉบับร่าง")){var d=AuthoringCatalog.CreateDraft(campaign,string.IsNullOrWhiteSpace(newName)?"อีเว้นใหม่":newName,personFilter==0?campaign.characters[0]:campaign.characters[personFilter-1]);Select(d.entryNode,d);Refresh();}
                EditorGUILayout.EndScrollView();
                detailScroll=EditorGUILayout.BeginScrollView(detailScroll);
                if(selectedNode==null)EditorGUILayout.HelpBox("ค้นข้อความหรือเลือกตัวละครและประเภทบทด้านซ้าย แล้วแก้บทได้ที่นี่",MessageType.Info);
                else
                {
                    if(selectedEvent!=null)DrawEvent();
                    DrawNode();
                }
                EditorGUILayout.EndScrollView();
            }
        }
        static bool DrawSceneEntry(string title,string category,string snippet,bool selected)
        {
            // Explicit rows prevent miniButton's fixed metrics/content alignment from clipping multiline Thai.
            var row=GUILayoutUtility.GetRect(GUIContent.none,GUIStyle.none,GUILayout.Height(82),GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(row,selected?new Color(.19f,.33f,.45f):EditorGUIUtility.isProSkin?new Color(.21f,.21f,.21f):new Color(.85f,.85f,.85f));
            bool clicked=GUI.Button(row,GUIContent.none,GUIStyle.none);
            if(selected)EditorGUI.DrawRect(new Rect(row.x,row.y,3,row.height),new Color(.35f,.7f,1));
            var heading=new GUIStyle(EditorStyles.boldLabel){alignment=TextAnchor.MiddleLeft,wordWrap=false,clipping=TextClipping.Clip,fontSize=13,richText=false};
            var body=new GUIStyle(EditorStyles.label){alignment=TextAnchor.MiddleLeft,wordWrap=false,clipping=TextClipping.Clip,fontSize=12,richText=false};
            var metadata=new GUIStyle(body){fontSize=11};
            float width=Mathf.Max(0,row.width-22);
            GUI.Label(new Rect(row.x+10,row.y+7,width,22),SingleLine(title),heading);
            GUI.Label(new Rect(row.x+10,row.y+30,width,18),SingleLine(category),metadata);
            GUI.Label(new Rect(row.x+10,row.y+51,width,23),SingleLine(snippet),body);
            GUILayout.Space(5);return clicked;
        }
        static string SingleLine(string text)=>(text??"").Replace('\r',' ').Replace('\n',' ').Replace('\t',' ');
        void Select(DialogueNodeSO node,EventDataSO story=null){selectedNode=node;selectedEvent=story;focusLine=null;detailScroll=Vector2.zero;GUI.FocusControl(null);}
        void DrawNode()
        {
            var context=Entry();
            if(context?.character!=null&&(context.category=="ทักทาย"||context.category=="คุยเล่น"))
            {
                if(GUILayout.Button("จัดการพูล / เงื่อนไขความสัมพันธ์ของบทนี้"))
                {person=Array.IndexOf(campaign.characters,context.character);ConversationPoolEditor.SelectKind(context.category=="ทักทาย");tab=1;detailScroll=Vector2.zero;GUIUtility.ExitGUI();}
            }
            EditorGUILayout.LabelField("ต้นฉบับ · "+AssetDatabase.GetAssetPath(selectedNode),EditorStyles.wordWrappedMiniLabel);
            using(new EditorGUILayout.HorizontalScope())
            {if(GUILayout.Button("ดูตัวอย่างบทนี้"))ArtistPreviewWindow.Open(campaign,selectedNode);if(GUILayout.Button("เลือกไฟล์ต้นฉบับ")){Selection.activeObject=selectedNode;EditorGUIUtility.PingObject(selectedNode);}if(GUILayout.Button("บันทึกบทนี้")){AssetDatabase.SaveAssets();Refresh();message="บันทึกบทแล้ว";}}
            LocalizationAuthoring.BindNode(campaign,selectedNode);var so=new SerializedObject(selectedNode);so.Update();EditorGUILayout.PropertyField(so.FindProperty("displayName"),new GUIContent("ชื่อฉากสำหรับค้นหา"));
            var lines=so.FindProperty("lines");
            for(int i=0;i<lines.arraySize;i++)
            {
                var line=lines.GetArrayElementAtIndex(i);using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using(new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("บรรทัด "+(i+1)+(line.FindPropertyRelative("lineId").stringValue==focusLine?" · กำลังแสดงในเกม":""),EditorStyles.boldLabel);
                        if(GUILayout.Button("↑",GUILayout.Width(25))&&i>0)lines.MoveArrayElement(i,i-1);
                        if(GUILayout.Button("↓",GUILayout.Width(25))&&i+1<lines.arraySize)lines.MoveArrayElement(i,i+1);
                        if(GUILayout.Button("ลบ",GUILayout.Width(36))){lines.DeleteArrayElementAtIndex(i);break;}
                    }
                    Speaker(line.FindPropertyRelative("speaker"));Expression(line.FindPropertyRelative("expression"));
                    var text=line.FindPropertyRelative("text");LocalizationAuthoring.DrawText(campaign,selectedNode,Core.LocalizationService.LineKey(selectedNode,line.FindPropertyRelative("lineId").stringValue),text,"บทพูด");
                }
            }
            if(GUILayout.Button("เพิ่มบรรทัด")){so.ApplyModifiedProperties();AuthoringCatalog.AddLine(selectedNode,Entry()?.character??selectedEvent?.character);return;}
            EditorGUILayout.Space();EditorGUILayout.LabelField("ตัวเลือกของผู้เล่น",EditorStyles.boldLabel);var choices=so.FindProperty("choices");
            for(int i=0;i<choices.arraySize;i++)
            {
                var choice=choices.GetArrayElementAtIndex(i);using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    var t=choice.FindPropertyRelative("text");LocalizationAuthoring.DrawText(campaign,selectedNode,Core.LocalizationService.ChoiceKey(selectedNode,choice.FindPropertyRelative("choiceId").stringValue),t,"ตัวเลือก "+(i+1),false);
                    Link(choice.FindPropertyRelative("nextNode"),"ไปฉาก",so);
                    EditorGUILayout.PropertyField(choice.FindPropertyRelative("effects"),new GUIContent("ผลของตัวเลือก"),true);EditorGUILayout.PropertyField(choice.FindPropertyRelative("flags"),new GUIContent("สิ่งที่เกมจดจำ"),true);
                    if(advanced)EditorGUILayout.PropertyField(choice.FindPropertyRelative("conditions"),new GUIContent("เงื่อนไขตัวเลือก"),true);
                    if(GUILayout.Button("ลบตัวเลือกนี้")){choices.DeleteArrayElementAtIndex(i);break;}
                }
            }
            if(GUILayout.Button("เพิ่มตัวเลือก"))
            {
                so.ApplyModifiedProperties();Undo.RecordObject(selectedNode,"Add dialogue choice");selectedNode.choices=selectedNode.choices.Concat(new[]{new DialogueChoice{choiceId=Guid.NewGuid().ToString("N"),text="",nextNode=selectedNode.nextNode}}).ToArray();selectedNode.nextNode=null;EditorUtility.SetDirty(selectedNode);return;
            }
            if(choices.arraySize==0)Link(so.FindProperty("nextNode"),"บทต่อไป",so);
            else EditorGUILayout.HelpBox("เมื่อมีตัวเลือก ให้กำหนดบทถัดไปที่แต่ละตัวเลือก",MessageType.None);
            advanced=EditorGUILayout.Foldout(advanced,"ข้อมูลขั้นสูง / รหัสคงที่",true);if(advanced)using(new EditorGUI.DisabledScope(true))EditorGUILayout.PropertyField(so.FindProperty("id"),new GUIContent("รหัสฉาก (ไม่เปลี่ยนเมื่อย้ายบรรทัด)"));
            if(so.ApplyModifiedProperties()){EditorUtility.SetDirty(selectedNode);Repaint();}
        }
        AuthoringEntry Entry()=>entries.FirstOrDefault(e=>e.node==selectedNode);
        void Link(SerializedProperty property,string label,SerializedObject so)
        {
            using(new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(property,new GUIContent(label));var node=property.objectReferenceValue as DialogueNodeSO;
                using(new EditorGUI.DisabledScope(node==null))if(GUILayout.Button("เปิด",GUILayout.Width(45))){so.ApplyModifiedProperties();Select(node,selectedEvent);GUIUtility.ExitGUI();}
            }
        }
        void DrawEvent()
        {
            using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var so=new SerializedObject(selectedEvent);so.Update();EditorGUILayout.PropertyField(so.FindProperty("displayName"),new GUIContent("ชื่ออีเว้น"));EditorGUILayout.PropertyField(so.FindProperty("character"),new GUIContent("ตัวละครเจ้าของอีเว้น"));
                EditorGUILayout.PropertyField(so.FindProperty("startDay"),new GUIContent("เริ่มพบได้วันที่"));EditorGUILayout.PropertyField(so.FindProperty("endDay"),new GUIContent("พบได้ถึงวันที่"));EditorGUILayout.PropertyField(so.FindProperty("preferredDay"),new GUIContent("วันแนะนำ"));
                EditorGUILayout.PropertyField(so.FindProperty("anyPhase"),new GUIContent("พบได้ทุกช่วงเวลา"));if(!so.FindProperty("anyPhase").boolValue)EditorGUILayout.PropertyField(so.FindProperty("triggerPhase"),new GUIContent("ช่วงเวลา"));EditorGUILayout.PropertyField(so.FindProperty("elapsedPhases"),new GUIContent("ใช้กี่ช่วงเวลาเมื่อจบ"));
                var delivery=so.FindProperty("delivery");delivery.enumValueIndex=EditorGUILayout.Popup("วิธีเริ่ม",delivery.enumValueIndex,new[]{"เรียกจากบริบท","เปิดอัตโนมัติเมื่อว่าง","บับเบิลเหนือหัว"});EditorGUILayout.PropertyField(so.FindProperty("room"),new GUIContent("ฉากที่พบ (ว่าง = ทุกฉาก)"));
                if(advanced){EditorGUILayout.PropertyField(so.FindProperty("priority"),new GUIContent("ลำดับความสำคัญ"));EditorGUILayout.PropertyField(so.FindProperty("conditions"),new GUIContent("เงื่อนไขอีเว้น"),true);}
                so.ApplyModifiedProperties();
                using(new EditorGUILayout.HorizontalScope())
                {
                    if(GUILayout.Button("ทดสอบอีเว้นนี้"))ArtistPreviewWindow.Open(campaign,null,selectedEvent);
                    if(GUILayout.Button("เปิดบทเริ่มต้น"))Select(selectedEvent.entryNode,selectedEvent);
                    if(!campaign.events.Contains(selectedEvent)&&GUILayout.Button("เพิ่มอีเว้นนี้เข้าเกม")){var errors=AuthoringCatalog.Publish(campaign,selectedEvent);message=errors.Length==0?"เพิ่มอีเว้นเข้าเกมแล้ว":string.Join("\n",errors);Refresh();}
                }
                if(!campaign.events.Contains(selectedEvent))EditorGUILayout.HelpBox("ฉบับร่าง: ยังไม่เข้าเกม เขียนบทให้พร้อมแล้วกดเพิ่มเข้าเกม",MessageType.Info);
            }
        }
        void Speaker(SerializedProperty p)
        {var people=new CharacterDataSO[]{null}.Concat(campaign.characters).ToArray();int i=Array.IndexOf(people,p.objectReferenceValue);p.objectReferenceValue=people[EditorGUILayout.Popup("ผู้พูด",Mathf.Max(0,i),new[]{"คำบรรยาย / ผู้เล่น"}.Concat(campaign.characters.Select(c=>c.displayName)).ToArray())];}
        static void Expression(SerializedProperty p)=>p.enumValueIndex=EditorGUILayout.Popup("สีหน้า",p.enumValueIndex,new[]{"ปกติ","ยิ้ม","โกรธ","เศร้า","เขิน","ตกใจ"});
        void DrawCharacter()
        {
            person=EditorGUILayout.Popup("ตัวละคร",Mathf.Clamp(person,0,campaign.characters.Length-1),campaign.characters.Select(c=>c.displayName).ToArray());var c=campaign.characters[person];var so=new SerializedObject(c);so.Update();
            LocalizationAuthoring.DrawText(campaign,c,Core.LocalizationService.Key(c,"displayName"),so.FindProperty("displayName"),"ชื่อที่แสดง",false);LocalizationAuthoring.DrawText(campaign,c,Core.LocalizationService.Key(c,"roleTitle"),so.FindProperty("roleTitle"),"หน้าที่",false);EditorGUILayout.PropertyField(so.FindProperty("startingAffection"),new GUIContent("ความสนิทเริ่มต้น"));EditorGUILayout.PropertyField(so.FindProperty("dailyTalkAffection"),new GUIContent("หัวใจจากคุยครั้งแรกต่อวัน"));EditorGUILayout.PropertyField(so.FindProperty("taskAvatar"),new GUIContent("ภาพใบหน้าบนการ์ด"));
            so.ApplyModifiedProperties();ConversationPoolEditor.Draw(campaign,c,node=>{tab=0;Select(node);},Refresh);so.Update();
            var sprites=so.FindProperty("sprites");for(int i=0;i<sprites.arraySize;i++){var sp=sprites.GetArrayElementAtIndex(i);Expression(sp.FindPropertyRelative("expression"));EditorGUILayout.PropertyField(sp.FindPropertyRelative("sprite"),new GUIContent("ภาพตัวละคร"));var im=sp.FindPropertyRelative("sprite").objectReferenceValue as Sprite;if(im!=null){var rect=GUILayoutUtility.GetRect(130,150);DrawSprite(rect,im);}}
            EditorGUILayout.PropertyField(so.FindProperty("giftPreferences"),new GUIContent("ของขวัญที่ชอบ / ไม่ชอบ และบทตอบ"),true);so.ApplyModifiedProperties();
            EditorGUILayout.Space();EditorGUILayout.LabelField("บทของตัวละครนี้",EditorStyles.boldLabel);foreach(var e in entries.Where(e=>e.character==c))if(GUILayout.Button(e.category+" · "+e.title)){tab=0;Select(e.node,e.story);}
        }
        void DrawScenes()
        {
            EditorGUILayout.HelpBox("จัด UI: เปิด Prefab แล้วใช้ Rect Tool / Inspector ตามปกติ บันทึกแล้วกดรีเฟรชตัวอย่าง ตำแหน่งจะไม่ถูกโค้ดจัดทับ",MessageType.Info);
            if(GUILayout.Button("เปิด UI หลักเพื่อจัดหน้า"))AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(ArtistWorkspaceBuilder.PrefabPath));
            using(new EditorGUILayout.HorizontalScope())
            {if(GUILayout.Button("แก้รูปแบบปุ่ม / ตัวเลือก"))AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(ArtistWorkspaceBuilder.ActionPath));if(GUILayout.Button("แก้รูปแบบการ์ดตัวละคร"))AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(ArtistWorkspaceBuilder.CastPath));}
            using(new EditorGUILayout.HorizontalScope())
            {if(GUILayout.Button("หน้าพูดคุย"))OpenScreenPrefab("RomanceDialogue");if(GUILayout.Button("HUD"))OpenScreenPrefab("RomanceHud");if(GUILayout.Button("รายสัปดาห์"))OpenScreenPrefab("RomanceWeeklySupport");if(GUILayout.Button("สรุปวันใหม่"))OpenScreenPrefab("RomanceMorningRecap");}
            if(GUILayout.Button("ดูตัวอย่างหน้าจอ"))ArtistPreviewWindow.Open(campaign);
            EditorGUILayout.Space();roomIndex=EditorGUILayout.Popup("ห้อง",Mathf.Clamp(roomIndex,0,campaign.rooms.Length-1),campaign.rooms.Select(r=>r.displayName).ToArray());roomPhase=(DayPhase)EditorGUILayout.Popup("ช่วงเวลา",(int)roomPhase,new[]{"เช้า","บ่าย","เย็น","กลางคืน"});RoomPlacementEditor.Draw(campaign.rooms[roomIndex],roomPhase);
        }
        void DrawSettings()
        {
            EditorGUILayout.LabelField("รางวัลและจังหวะเกม",EditorStyles.boldLabel);var so=new SerializedObject(campaign.romance);so.Update();
            string[] props={"teamDailyProgress","sharedProgress","sharedAffection","personalAffection","producerProgress","producerMoney","focusAffection","daysPerFocus"};string[] labels={"ทีมเพิ่มความคืบหน้าต่อวัน","ความคืบหน้าจากทำงานด้วยกัน","หัวใจจากทำงานด้วยกัน","หัวใจจากใช้เวลาส่วนตัว","ความคืบหน้าจากงานโปรดิวเซอร์","เงินจากงานโปรดิวเซอร์","หัวใจเพิ่มจากจุดสนับสนุน","จำนวนวันต่อรอบสนับสนุน"};
            for(int i=0;i<props.Length;i++)EditorGUILayout.PropertyField(so.FindProperty(props[i]),new GUIContent(labels[i]));
            LocalizationAuthoring.DrawRomanceTextSettings(campaign,so);EditorGUILayout.PropertyField(so.FindProperty("moodSchedule"),new GUIContent("อารมณ์ตามวัน"),true);EditorGUILayout.PropertyField(so.FindProperty("moodOutcomes"),new GUIContent("อารมณ์หลังอีเว้น"),true);so.ApplyModifiedProperties();
            EditorGUILayout.Space();EditorGUILayout.LabelField("ข้อความและภาพ UI",EditorStyles.boldLabel);EditorGUILayout.HelpBox("ฟอนต์ ขนาด และสีบนหน้าจอจริง: แก้ Text / Image ใน Prefab ผ่านแท็บฉากและ UI",MessageType.None);var p=new SerializedObject(campaign.romance.presentation);p.Update();EditorGUILayout.PropertyField(p.FindProperty("charactersPerSecond"),new GUIContent("ความเร็วข้อความ"));uiQuery=EditorGUILayout.TextField("ค้นข้อความ UI",uiQuery);
            var uiText=p.FindProperty("text");string[] mainKeys={"WorkTogether","PersonalTime","Producer","talk","gift","focus","focusHint","focusBenefit","projectTitle","teamDaily","today","hint","recap","recapContinue","recapTeam","recapQuiet","inventoryHint","shopHint","travelHint","finalRecap","endDay","nextDay","options","save","load","giftHint","noGifts"};
            for(int i=0;i<uiText.arraySize;i++){var entry=uiText.GetArrayElementAtIndex(i);string key=entry.FindPropertyRelative("key").stringValue;var val=entry.FindPropertyRelative("value");if(string.IsNullOrWhiteSpace(uiQuery)?!mainKeys.Contains(key):(key+" "+val.stringValue).IndexOf(uiQuery,StringComparison.OrdinalIgnoreCase)<0)continue;EditorGUILayout.LabelField(key,EditorStyles.miniLabel);LocalizationAuthoring.DrawText(campaign,campaign.romance.presentation,Core.LocalizationService.Key(campaign.romance.presentation,"ui/"+key),val,"ข้อความ UI");}EditorGUILayout.PropertyField(p.FindProperty("icons"),new GUIContent("ไอคอน"),true);p.ApplyModifiedProperties();
            if(GUILayout.Button("ตรวจความพร้อมข้อมูลปัจจุบัน"))message=string.Join("\n",ProjectLove.Simulation.EventScheduler.ValidateCatalog(campaign));
        }
        static void DrawSprite(Rect rect,Sprite s)
        {GUI.DrawTextureWithTexCoords(rect,s.texture,new Rect(s.textureRect.x/s.texture.width,s.textureRect.y/s.texture.height,s.textureRect.width/s.texture.width,s.textureRect.height/s.texture.height),true);}
        static void OpenScreenPrefab(string name)=>AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/"+name+".prefab"));
        void OpenPlayingLine()
        {
            var v=UnityEngine.Object.FindFirstObjectByType<IsometricRoomView>();if(v==null||v.Session==null){message="ยังไม่มีบทที่กำลังเล่น";return;}
            var n=v.Session.Dialogue.Node??v.Session.Interactions.Greeting;if(n==null){message="หน้าปัจจุบันเป็นเมนู ไม่ใช่บทสนทนา";return;}selectedNode=n;focusLine=v.Session.Dialogue.CurrentLine?.lineId;selectedEvent=v.Session.Dialogue.ActiveEvent;tab=0;
        }
    }
}
