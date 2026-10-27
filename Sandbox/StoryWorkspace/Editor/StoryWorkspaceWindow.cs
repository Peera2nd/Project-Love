using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectLove.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.StorySandbox
{
    public sealed partial class StoryWorkspaceWindow:EditorWindow
    {
        WorkspaceProject project;StoryIndex index;StoryEntry entry;DialogueNodeSO selected;
        string selectedLine,selectedChoice,query="",notice="",focusId,stateQuery="";
        int libraryTab,sideTab,locale,speakerFilter;Vector2 libraryScroll,scriptScroll,sideScroll;
        int indexedRevision=-1;List<ScriptRow> rows;
        readonly List<Tuple<StoryEntry,DialogueNodeSO>> history=new List<Tuple<StoryEntry,DialogueNodeSO>>();int historyAt=-1;
        WorkspacePlaytest test;WorkspaceGameView gameView;
        int day=1,phase,affection,roomIndex,stateGroup;string seedFlags="",seedEvents="";
        bool rawState,rawTrace,readHistory,advanced,showPaths,showVariables,showGameView=true;
        GUIStyle body,small,heading;LineDrag dragCandidate;
        sealed class LineDrag{public WorkspaceProject project;public DialogueNodeSO node;public string id;}
        public static bool IsSandbox=>File.Exists(Path.Combine(Application.dataPath,"../ProjectSettings/ProjectLoveStorySandbox.txt"));
        public static GUIContent C(string label,string tooltip)=>new GUIContent(label,tooltip);
        [MenuItem("Project Love/Story Workspace")]
        public static void Open()
        {
            if(!IsSandbox)throw new InvalidOperationException("Open .sandbox/StoryWorkspaceProject first.");
            var w=GetWindow<StoryWorkspaceWindow>("Story Workspace");w.minSize=new Vector2(1160,720);w.Show();
        }
        void OnEnable(){minSize=new Vector2(1160,720);titleContent=new GUIContent("Story Workspace");Undo.undoRedoPerformed+=OnUndo;if(IsSandbox)EditorApplication.delayCall+=DeferredLoad;}
        void DeferredLoad()
        {
            if(this==null||!IsSandbox)return;
            if(EditorApplication.isCompiling||EditorApplication.isUpdating){EditorApplication.delayCall+=DeferredLoad;return;}
            Reload();
        }
        public WorkspaceProject Document=>project;
        void OnDisable(){EditorApplication.delayCall-=DeferredLoad;Undo.undoRedoPerformed-=OnUndo;StopTest();}
        void OnUndo(){indexedRevision=-1;StopTest();Repaint();}
        public void Reload()
        {
            StopTest();project=WorkspaceProject.Open();Reindex();history.Clear();historyAt=-1;
            var first=index.entries.FirstOrDefault(e=>e.story!=null&&e.story.id=="romance_welcome")??index.entries.FirstOrDefault();
            if(first!=null)Navigate(first,first.node);
        }
        void Reindex()
        {
            index=new StoryIndex(project.game,project.assets);indexedRevision=project.revision;
            if(entry!=null)
            {
                var replacement=index.entries.FirstOrDefault(e=>e.owner==entry.owner&&e.sourceProperty==entry.sourceProperty&&e.node==entry.node);
                if(replacement!=null)entry=replacement;
                rows=index.Manuscript(entry.node);
            }
        }
        void Navigate(StoryEntry context,DialogueNodeSO node,bool record=true)
        {
            if(context==null||node==null)return;entry=context;selected=node;selectedLine=null;selectedChoice=null;focusId=node.id;
            rows=index.Manuscript(entry.node);
            if(record){if(historyAt<history.Count-1)history.RemoveRange(historyAt+1,history.Count-historyAt-1);history.Add(Tuple.Create(context,node));historyAt=history.Count-1;}
            Repaint();
        }
        void Go(StoryEntry context,DialogueNodeSO node){Navigate(context,node);GUIUtility.ExitGUI();}
        StoryEntry Context(DialogueNodeSO node)
        {
            if(entry!=null&&entry.nodes.Contains(node))return entry;
            var caller=index.Callers(node).FirstOrDefault();if(caller!=null)return caller;
            var orphan=new StoryEntry{node=node,owner=node,title=StoryIndex.Title(node),kind="Unlinked",gates="No registered caller"};
            foreach(var row in index.Manuscript(node))if(row.node!=null)orphan.nodes.Add(row.node);return orphan;
        }
        void Change(Action edit,bool exit=false)
        {
            edit();indexedRevision=-1;StopTest();Repaint();if(exit)GUIUtility.ExitGUI();
        }
        void StopTest(){gameView?.Dispose();gameView=null;test?.Dispose();test=null;}
        void Styles()
        {
            if(body!=null)return;body=new GUIStyle(EditorStyles.label){wordWrap=true,fontSize=12};
            small=new GUIStyle(EditorStyles.miniLabel){wordWrap=true};heading=new GUIStyle(EditorStyles.boldLabel){wordWrap=true,fontSize=14};
        }
        void OnGUI()
        {
            Styles();if(!IsSandbox){EditorGUILayout.HelpBox("Open StoryWorkspaceProject to edit the isolated copy.",MessageType.Info);return;}
            if(project==null){if(GUILayout.Button("Open Workspace"))Reload();return;}
            if(indexedRevision!=project.revision||indexedRevision<0)Reindex();
            ProcessDrag();
            using(new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(C("Story Workspace","Edit the isolated campaign copy. Script, flow, caller rules and playtest share this source."),EditorStyles.boldLabel);
                if(GUILayout.Button(C("New Scene","Create a disabled scene with an entry block. Write it, set its trigger and enable it here."),EditorStyles.toolbarButton,GUILayout.Width(90)))
                    Change(()=>{var scene=project.NewScene();Reindex();var root=index.entries.First(e=>e.story==scene);Navigate(root,root.node);sideTab=0;},true);
                if(GUILayout.Button(C("Save","Save all workspace edits. This never writes to the production campaign."),EditorStyles.toolbarButton,GUILayout.Width(50)))project.Save();
                if(GUILayout.Button(C("Validate","Check active scene links, conditions, localization and runtime rules."),EditorStyles.toolbarButton,GUILayout.Width(65)))Validate();
                GUILayout.FlexibleSpace();GUILayout.Label(C("Sandbox"+(EditorUtility.IsDirty(project)?" *":""),"Workspace.asset owns these edits. Original campaign assets and player saves are unchanged."),EditorStyles.miniLabel);
                locale=GUILayout.Toolbar(locale,new[]{C("TH","Edit Thai source text."),C("EN","Edit English. Empty translations use Thai at runtime.")},EditorStyles.toolbarButton,GUILayout.Width(80));
            }
            if(!string.IsNullOrEmpty(notice))using(new EditorGUILayout.HorizontalScope())
            {GUILayout.Label(notice,small);if(GUILayout.Button(C("×","Dismiss"),GUILayout.Width(24)))notice="";}
            using(new EditorGUILayout.HorizontalScope())
            {
                using(new EditorGUILayout.VerticalScope(GUILayout.Width(235)))DrawLibrary();
                using(new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))DrawScript();
                using(new EditorGUILayout.VerticalScope(GUILayout.Width(Mathf.Clamp(position.width*.29f,330,440))))
                {
                    sideTab=GUILayout.Toolbar(sideTab,new[]{C("Inspect","Edit trigger, conditions and selected choice results."),C("Flow","Trace incoming links, destinations and variable references."),C("Test","Run the actual caller or explicitly bypass entry gates."),C("State","Inspect the detached runtime values."),C("Trace","See what changed after each actual runner action.")});
                    sideScroll=EditorGUILayout.BeginScrollView(sideScroll);
                    if(sideTab==0)DrawInspect();else if(sideTab==1)DrawFlow();else if(sideTab==2)DrawTest();else if(sideTab==3)DrawState();else DrawTrace();
                    EditorGUILayout.EndScrollView();
                }
            }
        }
        void DrawLibrary()
        {
            query=EditorGUILayout.TextField(query,EditorStyles.toolbarSearchField);
            libraryTab=GUILayout.Toolbar(libraryTab,new[]{C("Scenes","Scheduled events, including disabled drafts."),C("Calls","Greetings, conversation pools and activity/gift callers."),C("Blocks","Every block, including unlinked writing.")});
            speakerFilter=EditorGUILayout.Popup(C("Speaker","Filter contexts containing this speaker on any branch."),speakerFilter,new[]{"All"}.Concat(project.game.characters.Select(c=>c.displayName)).ToArray());
            libraryScroll=EditorGUILayout.BeginScrollView(libraryScroll);
            if(libraryTab==2)
            {
                foreach(var n in index.nodes.Where(n=>StoryIndex.Matches(n,query)))
                    if(GUILayout.Button(C(StoryIndex.Title(n),n.id),new GUIStyle(EditorStyles.miniButton){wordWrap=true,alignment=TextAnchor.MiddleLeft}))Go(Context(n),n);
            }
            else foreach(var e in index.entries.Where(e=>(libraryTab==0?e.story!=null:e.story==null)&&
                (speakerFilter==0||e.Speakers.Contains(project.game.characters[speakerFilter-1]))&&
                (string.IsNullOrWhiteSpace(query)||StoryIndex.Contains(e.title+" "+e.gates,query)||e.nodes.Any(n=>StoryIndex.Matches(n,query)))))
            {
                using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    if(GUILayout.Button(C(e.title,e.kind+" · "+e.sourceProperty),new GUIStyle(EditorStyles.label){wordWrap=true,fontStyle=e.owner==entry?.owner?FontStyle.Bold:FontStyle.Normal}))
                        Go(e,string.IsNullOrWhiteSpace(query)?e.node:e.nodes.FirstOrDefault(n=>StoryIndex.Matches(n,query))??e.node);
                    GUILayout.Label(e.kind+(e.story!=null&&!project.game.events.Contains(e.story)?" · Draft":"")+" · "+e.nodes.Sum(n=>n.lines.Length)+" lines",small);
                    GUILayout.Label(string.Join(" · ",e.Speakers.Select(c=>c.displayName)),small);
                }
            }
            EditorGUILayout.EndScrollView();
        }
        void DrawScript()
        {
            if(entry==null||selected==null)return;
            using(new EditorGUILayout.HorizontalScope())
            {
                using(new EditorGUI.DisabledScope(historyAt<=0))if(GUILayout.Button(C("←","Previous location"),GUILayout.Width(26)))
                {var h=history[--historyAt];Navigate(h.Item1,h.Item2,false);GUIUtility.ExitGUI();}
                using(new EditorGUI.DisabledScope(historyAt>=history.Count-1))if(GUILayout.Button(C("→","Next location"),GUILayout.Width(26)))
                {var h=history[++historyAt];Navigate(h.Item1,h.Item2,false);GUIUtility.ExitGUI();}
                GUILayout.Label(entry.title,heading);
            }
            scriptScroll=EditorGUILayout.BeginScrollView(scriptScroll);
            foreach(var row in rows)
            {
                using(new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(Mathf.Min(row.depth,3)*12);
                    using(new EditorGUILayout.VerticalScope())
                    {
                        if(row.line!=null)DrawLine(row.node,row.line);
                        else if(row.choice!=null)DrawChoice(row.node,row.choice);
                        else if(row.reference)
                        {if(GUILayout.Button(C("↪ "+StoryIndex.Title(row.node),"Shared join. Jump to the existing block; no duplicated writing.")))Go(entry,row.node);}
                        else if(row.node!=null)
                        {
                            var n=row.node;
                            using(new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                            {
                                if(GUILayout.Button(C(n==selected?"●":"○","Select block and inspect its flow."),EditorStyles.toolbarButton,GUILayout.Width(24)))
                                {selected=n;selectedLine=null;selectedChoice=null;}
                                string title=EditorGUILayout.DelayedTextField(n.displayName);
                                GUI.Label(GUILayoutUtility.GetLastRect(),C("","Block title. Renaming does not change its stable identity."));
                                if(title!=n.displayName)Change(()=>{project.Edit("Rename block",n);n.displayName=title;});
                                int uses=index.Callers(n).Count();if(uses>1)GUILayout.Label(C("Shared",$"Used by {uses} caller contexts. Edits affect all of them."),EditorStyles.miniLabel,GUILayout.Width(45));
                                if(GUILayout.Button(C("+ Line","Append a line. Drag its handle to any insertion gap."),EditorStyles.toolbarButton,GUILayout.Width(48)))Change(()=>project.AddLine(n,n.lines.Length),true);
                                if(GUILayout.Button(C("+ Choice","Add a response with conditions, results and a destination."),EditorStyles.toolbarButton,GUILayout.Width(62)))Change(()=>{var c=project.AddChoice(n);selected=n;selectedChoice=c.choiceId;selectedLine=null;sideTab=0;},true);
                                if(GUILayout.Button(C("+ Block","Insert a block after these lines, before its previous continuation/choices."),EditorStyles.toolbarButton,GUILayout.Width(56)))Change(()=>{var next=project.InsertBlock(n,n.lines.Length);selected=next;focusId=next.id;},true);
                            }
                            var rect=GUILayoutUtility.GetLastRect();ScrollTo(n.id,rect);
                            DropGap(n,0);
                        }
                        else GUILayout.Label(C("End","This conversation ends here. The game may select another event later."),small);
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
        void DrawLine(DialogueNodeSO node,DialogueLine line)
        {
            using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using(new EditorGUILayout.HorizontalScope())
                {
                    var handle=GUILayoutUtility.GetRect(C("⋮⋮","Drag this line to a gap, including another block."),EditorStyles.miniButton,GUILayout.Width(22));
                    GUI.Label(handle,"⋮⋮",EditorStyles.miniButton);
                    if(Event.current.type==EventType.MouseDown&&handle.Contains(Event.current.mousePosition)){dragCandidate=new LineDrag{project=project,node=node,id=line.lineId};Event.current.Use();}
                    if(GUILayout.Button(C(selectedLine==line.lineId?"●":"○","Select line. See its caller, flow and state references."),GUILayout.Width(22))){selected=node;selectedLine=line.lineId;selectedChoice=null;}
                    int actor=line.speaker==null?0:Array.IndexOf(project.game.characters,line.speaker)+1;
                    int nextActor=EditorGUILayout.Popup(actor,new[]{"Narrator"}.Concat(project.game.characters.Select(c=>c.displayName)).ToArray(),GUILayout.MinWidth(85));
                    GUI.Label(GUILayoutUtility.GetLastRect(),C("","Speaker. Choose Narrator for stage directions or narration."));
                    var expression=(CharacterExpression)EditorGUILayout.EnumPopup(line.expression,GUILayout.Width(90));
                    GUI.Label(GUILayoutUtility.GetLastRect(),C("","Expression used by the character portrait on this line."));
                    if(actor!=nextActor||expression!=line.expression)Change(()=>{project.Edit("Edit performance",node);line.speaker=nextActor==0?null:project.game.characters[nextActor-1];line.expression=expression;});
                    if(GUILayout.Button(C("↑","Move line up"),GUILayout.Width(22)))Change(()=>project.MoveLine(node,line.lineId,node,Math.Max(0,Array.IndexOf(node.lines,line)-1)),true);
                    if(GUILayout.Button(C("↓","Move line down"),GUILayout.Width(22)))Change(()=>project.MoveLine(node,line.lineId,node,Array.IndexOf(node.lines,line)+2),true);
                    if(GUILayout.Button(C("×","Delete line. Undo restores it."),GUILayout.Width(22)))Change(()=>{project.Edit("Delete line",node);node.lines=node.lines.Where(l=>l!=line).ToArray();},true);
                }
                DrawText(line.localizedText,line.SourceText);
                ScrollTo(line.lineId,GUILayoutUtility.GetLastRect());
            }
            DropGap(node,Array.IndexOf(node.lines,line)+1);
        }
        void DrawChoice(DialogueNodeSO node,DialogueChoice choice)
        {
            using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using(new EditorGUILayout.HorizontalScope())
                {
                    if(GUILayout.Button(C(selectedChoice==choice.choiceId?"● Choice":"Choice","Edit this choice's conditions and results in Inspect."),GUILayout.Width(80)))
                    {selected=node;selectedChoice=choice.choiceId;selectedLine=null;sideTab=0;}
                    GUILayout.FlexibleSpace();
                    if(GUILayout.Button(C("↑","Move choice up without changing its ID or results."),GUILayout.Width(22)))Change(()=>MoveChoice(node,choice,-1),true);
                    if(GUILayout.Button(C("↓","Move choice down without changing its ID or results."),GUILayout.Width(22)))Change(()=>MoveChoice(node,choice,1),true);
                    if(GUILayout.Button(C("×","Delete choice. Undo restores it."),GUILayout.Width(22)))Change(()=>{project.Edit("Delete choice",node);node.choices=node.choices.Where(c=>c!=choice).ToArray();},true);
                }
                DrawText(choice.localizedText,choice.SourceText);
                using(new EditorGUILayout.HorizontalScope())
                {
                    if(GUILayout.Button(C("→ "+StoryIndex.Title(choice.nextNode),"Jump to this response or ending."),EditorStyles.miniButton)&&choice.nextNode!=null)Go(Context(choice.nextNode),choice.nextNode);
                    if(GUILayout.Button(C("Link","Choose a destination with search."),EditorStyles.miniButton,GUILayout.Width(40)))PickNode(choice.nextNode,n=>Change(()=>{project.Edit("Link choice",node);choice.nextNode=n;}));
                    if(GUILayout.Button(C("+ Reply","Create and connect a response block."),EditorStyles.miniButton,GUILayout.Width(55)))Change(()=>{var reply=ScriptableObject.CreateInstance<DialogueNodeSO>();project.Edit("Create reply",node);reply.id=Guid.NewGuid().ToString("N");reply.displayName="Untitled Reply";reply.name=reply.id;AssetDatabase.AddObjectToAsset(reply,project);Undo.RegisterCreatedObjectUndo(reply,"Create reply");project.assets.Add(reply);project.AddLine(reply,0);choice.nextNode=reply;focusId=reply.id;},true);
                }
            }
        }
        void DrawText(LocalizedTextRef text,string fallback)
        {
            var item=text.catalog?.Find(text.key);if(item==null){GUILayout.Label(fallback,body);return;}
            string original=locale==0?item.thai:item.english;
            string value=EditorGUILayout.TextArea(original??"",GUILayout.MinHeight(40));
            GUI.Label(GUILayoutUtility.GetLastRect(),C("",locale==0?"Thai source. Edits mark the existing English translation for review.":"English translation. Empty uses Thai at runtime."));
            if(value!=(original??""))try{Change(()=>project.SetText(text,locale==0?value:item.thai,locale==1?value:item.english));}catch(Exception e){notice=e.Message;}
        }
        void ScrollTo(string id,Rect rect)
        {if(Event.current.type==EventType.Repaint&&!string.IsNullOrEmpty(focusId)&&focusId==id){scriptScroll.y=Mathf.Max(0,rect.y-10);focusId=null;Repaint();}}
        void ProcessDrag()
        {
            if(dragCandidate!=null&&Event.current.type==EventType.MouseDrag)
            {DragAndDrop.PrepareStartDrag();DragAndDrop.SetGenericData("StoryWorkspace.Line",dragCandidate);DragAndDrop.StartDrag("Move line");dragCandidate=null;Event.current.Use();}
            if(Event.current.type==EventType.MouseUp)dragCandidate=null;
        }
        void DropGap(DialogueNodeSO node,int slot)
        {
            var rect=GUILayoutUtility.GetRect(0,8,GUILayout.ExpandWidth(true));var payload=DragAndDrop.GetGenericData("StoryWorkspace.Line") as LineDrag;
            GUI.Label(rect,C("","Click to insert a line. Drop a line here to move it between lines or blocks."));
            if(!rect.Contains(Event.current.mousePosition))return;
            if(payload!=null&&payload.project==project&&(Event.current.type==EventType.DragUpdated||Event.current.type==EventType.DragPerform))
            {
                DragAndDrop.visualMode=DragAndDropVisualMode.Move;
                if(Event.current.type==EventType.DragPerform){DragAndDrop.AcceptDrag();DragAndDrop.SetGenericData("StoryWorkspace.Line",null);Change(()=>project.MoveLine(payload.node,payload.id,node,slot),true);}
                Event.current.Use();
            }
            if(Event.current.type==EventType.Repaint)EditorGUI.DrawRect(new Rect(rect.x,rect.y+3,rect.width,2),new Color(.3f,.65f,1f));
            if(Event.current.type==EventType.MouseDown){Change(()=>project.AddLine(node,slot),true);}
        }
        void PickNode(DialogueNodeSO current,Action<DialogueNodeSO> pick)
        {PopupWindow.Show(GUILayoutUtility.GetLastRect(),new BlockPicker(index.nodes,current,pick));}
        void MoveChoice(DialogueNodeSO node,DialogueChoice choice,int direction)
        {
            int i=Array.IndexOf(node.choices,choice),j=Mathf.Clamp(i+direction,0,node.choices.Length-1);if(i==j)return;
            project.Edit("Reorder choice",node);var other=node.choices[j];node.choices[j]=choice;node.choices[i]=other;
        }
        sealed class BlockPicker:PopupWindowContent
        {
            readonly IEnumerable<DialogueNodeSO> nodes;readonly Action<DialogueNodeSO> pick;readonly DialogueNodeSO current;string search="";Vector2 scroll;
            public BlockPicker(IEnumerable<DialogueNodeSO> nodes,DialogueNodeSO current,Action<DialogueNodeSO> pick){this.nodes=nodes;this.current=current;this.pick=pick;}
            public override Vector2 GetWindowSize()=>new Vector2(350,330);
            public override void OnGUI(Rect rect)
            {
                search=EditorGUILayout.TextField(search,EditorStyles.toolbarSearchField);
                if(GUILayout.Button(C("End","End this conversation without a destination."))){pick(null);editorWindow.Close();}
                scroll=EditorGUILayout.BeginScrollView(scroll);
                foreach(var node in nodes.Where(n=>StoryIndex.Contains(StoryIndex.Title(n)+" "+n.id,search)))
                    if(GUILayout.Button(C((node==current?"● ":"")+StoryIndex.Title(node),node.id),new GUIStyle(EditorStyles.miniButton){wordWrap=true})){pick(node);editorWindow.Close();}
                EditorGUILayout.EndScrollView();
            }
        }
        void Validate()
        {
            var issues=ProjectLove.Simulation.EventScheduler.ValidateCatalog(project.game);
            try{_ =new ProjectLove.Core.GameSession(project.game);}catch(Exception e){issues.Add(e.Message);}
            foreach(var e in project.game.localization.catalog.entries){var error=ProjectLove.Core.LocalizationService.ValidateTranslation(e.thai,e.english);if(error!=null)issues.Add(e.key+": "+error);}
            notice=issues.Count==0?"No issues in active scenes.":string.Join("\n",issues.Distinct().Take(10));
        }
    }
}
