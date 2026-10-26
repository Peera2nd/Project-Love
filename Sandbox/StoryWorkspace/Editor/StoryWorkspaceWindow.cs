using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.EditorTools;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.StorySandbox
{
    public sealed class StoryWorkspaceWindow : EditorWindow
    {
        GameConfigSO game;
        StoryIndex index;
        StoryDraft draft;
        StoryEntry entry;
        DialogueNodeSO selected;
        DialogueLine selectedLine;
        DialogueChoice selectedChoice;
        List<ScriptRow> rows;
        Vector2 libraryScroll,scriptScroll,detailScroll;
        string query="",notice="",flags="",completed="",probeResult="";
        int speaker,kind,day=1,affection,phase,probeKind;
        bool english,contextFold,sourceFold,filterTime;
        readonly List<Tuple<StoryEntry,DialogueNodeSO>> history=new List<Tuple<StoryEntry,DialogueNodeSO>>();
        int historyAt=-1;
        DraftPreview preview;
        GUIStyle body,small,heading,rowStyle;
        string focusNode;

        public static bool IsSandbox => File.Exists(Path.Combine(Application.dataPath,"../ProjectSettings/ProjectLoveStorySandbox.txt"));
        [MenuItem("Project Love/ทดลองหน้ารวมบท (Sandbox)")]
        public static void Open()
        {
            if(!IsSandbox)throw new InvalidOperationException("เปิดผ่าน Sandbox/StoryWorkspace/Open-StorySandbox.ps1 เท่านั้น");
            var w=GetWindow<StoryWorkspaceWindow>();w.titleContent=new GUIContent("ทดลองหน้ารวมบท");
            w.minSize=new Vector2(1120,700);w.Show();
        }
        void OnEnable()
        {
            minSize=new Vector2(1120,700);Undo.undoRedoPerformed+=OnUndo;
            if(IsSandbox&&!SessionState.GetBool("StorySandbox.InitialPlacement",false))
            {
                position=new Rect(40,60,1440,900);
                SessionState.SetBool("StorySandbox.InitialPlacement",true);
            }
            if(IsSandbox)Reload();
        }
        void OnDisable(){Undo.undoRedoPerformed-=OnUndo;preview?.Dispose();preview=null;}
        void OnUndo(){preview?.Dispose();preview=null;Repaint();}
        public void Reload()
        {
            game=AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath);
            if(game==null)return;
            index=new StoryIndex(game);draft=StoryDraft.Load();history.Clear();historyAt=-1;
            var opening=game.events.FirstOrDefault(e=>e.id=="romance_welcome");
            var first=index.entries.FirstOrDefault(e=>e.story==opening)??index.entries.FirstOrDefault();
            if(first!=null)Navigate(first,first.node);
        }
        void Navigate(StoryEntry root,DialogueNodeSO node,bool record=true)
        {
            if(root==null||node==null)return;
            entry=root;selected=node;selectedLine=null;selectedChoice=null;focusNode=node.id;
            probeKind=root.kind=="คุยเล่น"?1:0;probeResult="";
            rows=index.Manuscript(root.node);
            if(record)
            {
                if(historyAt<history.Count-1)history.RemoveRange(historyAt+1,history.Count-historyAt-1);
                history.Add(Tuple.Create(root,node));historyAt=history.Count-1;
            }
            preview?.Dispose();preview=null;Repaint();
        }
        void Go(StoryEntry root,DialogueNodeSO node){Navigate(root,node);GUIUtility.ExitGUI();}
        void Jump(DialogueNodeSO n)
        {
            if(n==null)return;
            Go(entry.nodes.Contains(n)?entry:index.Callers(n).FirstOrDefault(),n);
        }
        void Styles()
        {
            if(body!=null)return;
            body=new GUIStyle(EditorStyles.label){wordWrap=true,fontSize=13};
            small=new GUIStyle(body){fontSize=11};small.normal.textColor=new Color(.67f,.71f,.76f);
            heading=new GUIStyle(EditorStyles.boldLabel){wordWrap=true,fontSize=15};
            rowStyle=new GUIStyle(EditorStyles.helpBox){padding=new RectOffset(12,12,9,9)};
        }
        void OnGUI()
        {
            Styles();
            if(!IsSandbox){EditorGUILayout.HelpBox("เครื่องมือนี้ใช้ได้เฉพาะโปรเจกต์ทดลอง",MessageType.Error);return;}
            if(game==null||index==null){if(GUILayout.Button("โหลดข้อมูลทดลอง"))Reload();return;}
            using(new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("STORY WORKSPACE  /  พื้นที่ทดลอง",EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();GUILayout.Label("ต้นฉบับเกม: อ่านอย่างเดียว",EditorStyles.miniLabel);
                if(GUILayout.Button("วิธีลองใช้งาน",EditorStyles.toolbarButton,GUILayout.Width(95)))
                    EditorUtility.DisplayDialog("ลองหน้ารวมบท","1. อ่านบทเปิดเรื่องตรงกลางและเลื่อนดูทั้งสามทางเลือก\n2. ค้นประโยคหรือกรองผู้พูดด้านซ้าย\n3. กดประโยคเพื่อแก้ไทย/อังกฤษด้านขวา แล้วลอง Ctrl+Z และบันทึกฉบับทดลอง\n4. ตรวจต้นทาง ปลายทาง และเงื่อนไขด้านขวา\n5. เลื่อนลงเพื่อเปิดตัวอย่างเกมจากข้อมูลสำเนา\n\nทุกการแก้ไขอยู่ในโปรเจกต์ทดลอง ยังไม่นำไปใช้กับเกมจริง","เริ่มลอง");
                if(GUILayout.Button("บันทึกฉบับทดลอง"+(EditorUtility.IsDirty(draft)?" *":""),EditorStyles.toolbarButton,GUILayout.Width(155)))
                {AssetDatabase.SaveAssetIfDirty(draft);notice="บันทึกข้อความฉบับทดลองแล้ว · ยังไม่ปรับใช้กับเกมจริง";}
            }
            EditorGUILayout.LabelField("แคมเปญ: "+game.name+"  ·  "+index.nodes.Count+" ช่วงบท  ·  "+draft.texts.Count+" ข้อความทดลอง",small);
            if(!string.IsNullOrEmpty(notice))EditorGUILayout.HelpBox(notice,MessageType.Info);
            using(new EditorGUILayout.HorizontalScope())
            {
                using(new EditorGUILayout.VerticalScope(GUILayout.Width(255))){DrawLibrary();}
                using(new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true))){DrawScript();}
                using(new EditorGUILayout.VerticalScope(GUILayout.Width(Mathf.Clamp(position.width*.27f,315,420)))){DrawDetails();}
            }
        }
        void DrawLibrary()
        {
            GUILayout.Label("คลังบท",heading);
            query=EditorGUILayout.TextField(query,EditorStyles.toolbarSearchField);
            GUILayout.Label("ค้นไทย / อังกฤษ / ชื่อบท / ผู้พูด",small);
            speaker=EditorGUILayout.Popup("มีผู้พูด",speaker,new[]{"ทุกคน"}.Concat(game.characters.Select(c=>c.displayName)).ToArray());
            var kinds=new[]{"ทุกประเภท","อีเว้น","ทักทาย","คุยเล่น","กิจกรรม / ของขวัญ"};
            kind=EditorGUILayout.Popup("ประเภท",kind,kinds);
            filterTime=EditorGUILayout.ToggleLeft("กรองช่วงวัน / เวลา",filterTime);
            if(filterTime)
            {
                day=EditorGUILayout.IntSlider("วันที่",day,1,game.totalDays);
                phase=EditorGUILayout.Popup("ช่วงเวลา",phase,new[]{"เช้า","บ่าย","เย็น","กลางคืน"});
                GUILayout.Label("กรองช่วงที่ระบุในข้อมูลเท่านั้น\nเงื่อนไขอื่นตรวจด้วยตัวลองบริบท",small);
            }
            var matches=index.entries.Where(e=>(kind==0||e.kind==kinds[kind])&&
                (speaker==0||e.Speakers.Contains(game.characters[speaker-1]))&&
                (!filterTime||InWindow(e))&&
                (string.IsNullOrWhiteSpace(query)||StoryIndex.Contains(e.title+" "+e.gates,query)||e.nodes.Any(n=>StoryIndex.Matches(n,query,draft)))).ToList();
            GUILayout.Label(matches.Count+" บริบท · นับจุดเรียกแยกกัน",small);
            libraryScroll=EditorGUILayout.BeginScrollView(libraryScroll);
            foreach(var e in matches)
            {
                using(new EditorGUILayout.VerticalScope(rowStyle))
                {
                    var old=GUI.backgroundColor;if(e==entry)GUI.backgroundColor=new Color(.35f,.65f,.9f);
                    if(GUILayout.Button(e.title,new GUIStyle(EditorStyles.label){wordWrap=true,fontStyle=FontStyle.Bold}))
                        Go(e,string.IsNullOrWhiteSpace(query)?e.node:e.nodes.FirstOrDefault(n=>StoryIndex.Matches(n,query,draft))??e.node);
                    GUI.backgroundColor=old;
                    GUILayout.Label(e.kind+" · "+e.nodes.Sum(n=>n.lines.Length)+" บรรทัด",small);
                    GUILayout.Label(string.Join(" · ",e.Speakers.Select(c=>c.displayName)),small);
                    if(!string.IsNullOrWhiteSpace(query))foreach(var n in e.nodes)
                        foreach(var l in n.lines.Where(l=>StoryIndex.Contains(draft.Thai(l.localizedText,l.SourceText)+" "+draft.English(l.localizedText),query)).Take(2))
                            if(GUILayout.Button("↳ "+(l.speaker?.displayName??"คำบรรยาย")+": "+draft.Thai(l.localizedText,l.SourceText),small))
                            {Navigate(e,n);selectedLine=l;GUIUtility.ExitGUI();}
                }
            }
            EditorGUILayout.EndScrollView();
        }
        void DrawScript()
        {
            if(entry==null)return;
            using(new EditorGUILayout.HorizontalScope())
            {
                using(new EditorGUI.DisabledScope(historyAt<=0))if(GUILayout.Button("←",GUILayout.Width(32)))
                {var h=history[--historyAt];Navigate(h.Item1,h.Item2,false);GUIUtility.ExitGUI();}
                using(new EditorGUI.DisabledScope(historyAt>=history.Count-1))if(GUILayout.Button("→",GUILayout.Width(32)))
                {var h=history[++historyAt];Navigate(h.Item1,h.Item2,false);GUIUtility.ExitGUI();}
                GUILayout.Label("บทเรียงต่อเนื่อง",heading);GUILayout.FlexibleSpace();english=GUILayout.Toggle(english,"แสดงอังกฤษ");
            }
            GUILayout.Label(entry.title,heading);
            GUILayout.Label("ผู้พูดทั้งหมด: "+string.Join(" · ",entry.Speakers.Select(c=>c.displayName))+(entry.nodes.Any(n=>n.lines.Any(l=>l.speaker==null))?" · คำบรรยาย":""),small);
            GUILayout.Label("แสดงทุกแขนงที่เป็นไปได้ · จุดรวมอ่านครั้งเดียว กด ↪ เพื่อย้อนดู · กดประโยคเพื่อแก้ฉบับทดลอง",small);
            scriptScroll=EditorGUILayout.BeginScrollView(scriptScroll);
            foreach(var row in rows)
            {
                GUILayout.Space(row.line==null?6:1);
                using(new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(Mathf.Min(row.depth,4)*16);
                    using(new EditorGUILayout.VerticalScope(rowStyle))
                    {
                        var rect=GUILayoutUtility.GetRect(0,0);
                        if(Event.current.type==EventType.Repaint&&!string.IsNullOrEmpty(focusNode)&&focusNode==row.node?.id&&row.line==null&&row.choice==null)
                        {scriptScroll.y=Mathf.Max(0,rect.y-8);focusNode=null;Repaint();}
                        if(row.line!=null)
                        {
                            bool active=selectedLine==row.line;
                            GUILayout.Label((active?"● ":"")+(row.line.speaker?.displayName??"คำบรรยาย")+"  ·  "+Expression(row.line.expression),small);
                            if(GUILayout.Button(draft.Thai(row.line.localizedText,row.line.SourceText),body))
                            {selected=row.node;selectedLine=row.line;selectedChoice=null;}
                            if(english)GUILayout.Label(English(row.line.localizedText),small);
                        }
                        else if(row.choice!=null)
                        {
                            GUILayout.Label("ทางเลือก → "+StoryIndex.Title(row.choice.nextNode),small);
                            if(GUILayout.Button(draft.Thai(row.choice.localizedText,row.choice.SourceText),body))
                            {selected=row.node;selectedLine=null;selectedChoice=row.choice;}
                            if(english)GUILayout.Label(English(row.choice.localizedText),small);
                            GUILayout.Label(StoryIndex.Conditions(row.choice.conditions),small);
                        }
                        else if(row.reference)
                        {if(GUILayout.Button(row.label,body))Jump(row.node);}
                        else if(row.node!=null)
                        {
                            if(GUILayout.Button((row.node==selected?"● ":"")+row.label,heading))
                            {selected=row.node;selectedLine=null;selectedChoice=null;}
                        }
                        else GUILayout.Label("— จบบท —",small);
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
        string English(LocalizedTextRef text)
        {
            string value=draft.English(text);return string.IsNullOrWhiteSpace(value)?"EN: ยังไม่มีคำแปล · ตัวอย่างใช้ไทยแทน":"EN: "+value;
        }
        static string Expression(CharacterExpression e)=>new[]{"ปกติ","ยิ้ม","โกรธ","เศร้า","เขิน","ตกใจ"}[(int)e];
        void DrawDetails()
        {
            detailScroll=EditorGUILayout.BeginScrollView(detailScroll);
            if(selected==null){EditorGUILayout.EndScrollView();return;}
            GUILayout.Label("บริบทและปลายทาง",heading);
            GUILayout.Label(StoryIndex.Title(selected),body);
            if(selectedLine!=null||selectedChoice!=null)DrawEdit();
            EditorGUILayout.Space();GUILayout.Label("บริบทที่กำลังอ่าน",EditorStyles.boldLabel);
            GUILayout.Label(entry.title,body);GUILayout.Label(entry.gates,small);
            contextFold=EditorGUILayout.Foldout(contextFold,"เส้นทางจากต้นเรื่องถึงช่วงนี้ (ทุกทาง)",true);
            if(contextFold)DrawPaths(entry.node,selected,new HashSet<DialogueNodeSO>(),new List<string>());
            GUILayout.Label("ต้นทางโดยตรง",EditorStyles.boldLabel);
            foreach(var caller in index.entries.Where(e=>e.node==selected))
                if(GUILayout.Button("เริ่มจาก: "+caller.title+" · "+caller.kind,body))Go(caller,selected);
            foreach(var edge in index.Incoming(selected))
            {
                if(GUILayout.Button("← "+StoryIndex.Title(edge.from)+"\n"+EdgeLabel(edge),body))Jump(edge.from);
                if(edge.choice!=null)GUILayout.Label(StoryIndex.Conditions(edge.choice.conditions),small);
            }
            GUILayout.Label("ไปต่อ / แต่ละทางเลือก",EditorStyles.boldLabel);
            var outgoing=index.Outgoing(selected).ToArray();
            if(outgoing.Length==0)GUILayout.Label("จบบท",small);
            foreach(var edge in outgoing)
            {
                using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    if(GUILayout.Button(EdgeLabel(edge)+" → "+StoryIndex.Title(edge.to),body)&&edge.to!=null)Jump(edge.to);
                    if(edge.to!=null&&edge.to.lines.Length>0)GUILayout.Label("ตอบ: "+(edge.to.lines[0].speaker?.displayName??"คำบรรยาย")+" · "+draft.Thai(edge.to.lines[0].localizedText,edge.to.lines[0].SourceText),small);
                    if(edge.choice!=null)
                    {
                        GUILayout.Label(StoryIndex.Conditions(edge.choice.conditions),small);
                        GUILayout.Label($"ค่าใช้จ่าย AP {edge.choice.actionPointCost} · พลังงาน {edge.choice.staminaCost}",small);
                        foreach(var effect in edge.choice.effects)GUILayout.Label((effect.character?.displayName??"")+" "+StoryIndex.Stat(effect.stat)+" "+effect.amount.ToString("+0;-0;0"),small);
                        foreach(var flag in edge.choice.flags)GUILayout.Label("ธง "+flag.flagId+" = "+(flag.value?"มี":"ไม่มี"),small);
                        if(edge.choice.workAction!=DialogueWorkAction.None)GUILayout.Label("คำสั่งงาน: "+edge.choice.workAction,small);
                        foreach(var workload in edge.choice.activateWorkloads)GUILayout.Label("เปิดงาน: "+workload,small);
                    }
                }
            }
            GUILayout.Label("ถูกใช้ในบริบทใดบ้าง",EditorStyles.boldLabel);
            foreach(var root in index.Callers(selected))
                if(GUILayout.Button(root.title+" · "+root.kind+"\nเริ่มที่: "+StoryIndex.Title(root.node),small))Go(root,selected);
            DrawProbe();
            EditorGUILayout.Space();GUILayout.Label("ตัวอย่างเกมจากฉบับทดลอง",EditorStyles.boldLabel);
            GUILayout.Label("เริ่มตรงช่วงที่เลือก ข้ามเงื่อนไขเข้าถึง · ใช้หน้าจอเกมเดิมกับสถานะแยก",small);
            if(GUILayout.Button("เปิด / เริ่มตัวอย่างช่วงนี้ใหม่"))
            {
                preview?.Dispose();preview=null;
                try{preview=new DraftPreview(game,selected,draft,day,(DayPhase)phase,english?"en":"th");notice=preview.view.Notice??"ตัวอย่างแยกพร้อมอ่าน";}
                catch(Exception e){notice=e.Message;}
            }
            if(preview!=null)
            {
                var rect=GUILayoutUtility.GetAspectRect(16f/9f);
                if(Event.current.type==EventType.Repaint)GUI.DrawTexture(rect,preview.view.Render(960,540),ScaleMode.ScaleToFit,false);
                var runner=preview.view.Session.Dialogue;
                if(runner.ChoicesReady)
                {foreach(var c in runner.Node.choices)using(new EditorGUI.DisabledScope(!runner.CanChoose(c)))if(GUILayout.Button(preview.view.Session.Localization.Choice(runner.Node,c),body))preview.view.Choose(c);}
                else if(runner.IsActive&&GUILayout.Button("อ่านต่อ"))preview.view.Advance();
            }
            sourceFold=EditorGUILayout.Foldout(sourceFold,"รหัส / ไฟล์ต้นทาง (อ่านอย่างเดียว)",true);
            if(sourceFold)
            {
                EditorGUILayout.SelectableLabel(AssetDatabase.GetAssetPath(selected),small,GUILayout.Height(42));
                if(GUILayout.Button("แสดงไฟล์ต้นทางใน Project"))EditorGUIUtility.PingObject(selected);
                EditorGUILayout.SelectableLabel(selectedLine?.localizedText.key??selectedChoice?.localizedText.key??selected.id,small,GUILayout.Height(42));
            }
            EditorGUILayout.EndScrollView();
        }
        void DrawEdit()
        {
            var text=selectedLine!=null?selectedLine.localizedText:selectedChoice.localizedText;
            var fallback=selectedLine!=null?selectedLine.SourceText:selectedChoice.SourceText;
            EditorGUILayout.Space();GUILayout.Label("แก้เฉพาะฉบับทดลอง",EditorStyles.boldLabel);
            if(text.catalog==null){EditorGUILayout.HelpBox("ยังไม่มีคีย์ภาษา · ต้นแบบนี้ไม่สร้างคีย์หรือแก้ต้นฉบับ",MessageType.Warning);return;}
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("ไทย",small);string th;
            using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))th=EditorGUILayout.TextArea(draft.Thai(text,fallback),GUILayout.MinHeight(80));
            GUILayout.Label("English (เว้นว่างได้)",small);string en;
            using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))en=EditorGUILayout.TextArea(draft.English(text),GUILayout.MinHeight(60));
            if(EditorGUI.EndChangeCheck())
            {
                draft.Set(text,th,en);preview?.Dispose();preview=null;
                notice=string.IsNullOrWhiteSpace(th)?"ข้อความไทยว่าง · เติมก่อนลองเล่น":"มีการแก้ไขฉบับทดลอง · Ctrl+Z ย้อนกลับ · กดบันทึกด้านบน";
            }
        }
        void DrawPaths(DialogueNodeSO current,DialogueNodeSO target,HashSet<DialogueNodeSO> visited,List<string> path)
        {
            if(current==null||!visited.Add(current))return;
            if(current==target)GUILayout.Label(path.Count==0?"เริ่มที่บทนี้":string.Join("\n↓ ",path),small);
            else foreach(var edge in index.Outgoing(current))
            {
                path.Add(StoryIndex.Title(current)+" → "+EdgeLabel(edge)+(edge.choice==null?"":"\n"+StoryIndex.Conditions(edge.choice.conditions)));
                DrawPaths(edge.to,target,visited,path);path.RemoveAt(path.Count-1);
            }
            visited.Remove(current);
        }
        void DrawProbe()
        {
            EditorGUILayout.Space();GUILayout.Label("ลองบริบทด้วยระบบเลือกบทจริง",EditorStyles.boldLabel);
            day=EditorGUILayout.IntSlider("วันที่",day,1,game.totalDays);
            phase=EditorGUILayout.Popup("ช่วงเวลา",phase,new[]{"เช้า","บ่าย","เย็น","กลางคืน"});
            affection=EditorGUILayout.IntSlider("ความสนิท",affection,0,100);
            flags=EditorGUILayout.TextField("ธงที่มี (คั่น ,)",flags);
            completed=EditorGUILayout.TextField("อีเว้นที่จบ (คั่น ,)",completed);
            GUILayout.Label("ห้อง/ความเครียด/งาน ใช้สถานะเริ่มต้นของเกม · ไม่ใช่เซฟผู้เล่น",small);
            if(entry.story==null&&entry.character!=null)probeKind=GUILayout.Toolbar(probeKind,new[]{"ทักทาย","คุยเล่น"});
            bool supported=entry.story!=null||entry.character!=null;
            using(new EditorGUI.DisabledScope(!supported))if(GUILayout.Button("ตรวจการเข้าถึงในบริบทนี้"))
            {
                try
                {
                    if(entry.story!=null)
                    {
                        var session=new GameSession(game);session.State.day=day;session.State.phase=(DayPhase)phase;
                        foreach(var c in session.State.characters)c.affection=affection;
                        foreach(var f in Split(flags))if(!session.State.flags.Contains(f))session.State.flags.Add(f);
                        foreach(var e in Split(completed))if(!session.State.completedEvents.Contains(e))session.State.completedEvents.Add(e);
                        bool ready=session.CanStartEvent(entry.story);
                        probeResult=(ready?"ผ่านการเข้าถึงอีเว้น":"ยังไม่พร้อม: "+AuthoringCatalog.ExplainBlockedEvent(session,entry.story));
                    }
                    else
                    {
                        probeResult=ConversationPoolEditor.Probe(game,entry.character,probeKind==0,affection,day,(DayPhase)phase,flags,completed,out var chosen);
                        if(chosen!=null)probeResult+="\nรหัสบท: "+chosen.id;
                    }
                }
                catch(Exception e){probeResult="ตรวจไม่ได้: "+e.Message;}
            }
            if(!supported)GUILayout.Label("ตัวเรียกประเภทนี้ยังไม่มีตัวลองบริบทในต้นแบบ",small);
            if(!string.IsNullOrEmpty(probeResult))EditorGUILayout.HelpBox(probeResult,MessageType.Info);
        }
        static IEnumerable<string> Split(string value)=>value.Split(',').Select(v=>v.Trim()).Where(v=>v.Length>0);
        string EdgeLabel(StoryEdge edge)=>edge.choice==null?"อ่านต่อ":draft.Thai(edge.choice.localizedText,edge.choice.SourceText);
        bool InWindow(StoryEntry e)
        {
            if(e.story!=null)return day>=e.story.startDay&&day<=e.story.endDay&&(e.story.anyPhase||(int)e.story.triggerPhase==phase);
            if(e.variant!=null)return day>=e.variant.startDay&&day<=e.variant.endDay&&(e.variant.anyPhase||(int)e.variant.phase==phase);
            return true;
        }
    }
}
