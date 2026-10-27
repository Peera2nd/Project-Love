using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.StorySandbox
{
    public sealed partial class StoryWorkspaceWindow
    {
        void Mutate(UnityEngine.Object owner,string action,Action edit)=>Change(()=>{project.Edit(action,owner);edit();});
        string Text(string label,string help,string value,UnityEngine.Object owner,Action<string> set)
        {
            string next=EditorGUILayout.DelayedTextField(C(label,help),value??"");
            if(next!=(value??""))Mutate(owner,"Edit "+label,()=>set(next));return next;
        }
        void Number(string label,string help,int value,UnityEngine.Object owner,Action<int> set)
        {int next=EditorGUILayout.IntField(C(label,help),value);if(next!=value)Mutate(owner,"Edit "+label,()=>set(next));}
        void Toggle(string label,string help,bool value,UnityEngine.Object owner,Action<bool> set)
        {bool next=EditorGUILayout.Toggle(C(label,help),value);if(next!=value)Mutate(owner,"Edit "+label,()=>set(next));}
        CharacterDataSO Actor(string label,string help,CharacterDataSO current,UnityEngine.Object owner,Action<CharacterDataSO> set)
        {
            int value=current==null?0:Array.IndexOf(project.game.characters,current)+1;
            int next=EditorGUILayout.Popup(C(label,help),Math.Max(0,value),new[]{"None"}.Concat(project.game.characters.Select(c=>c.displayName)).ToArray());
            if(next!=value)Mutate(owner,"Edit "+label,()=>set(next==0?null:project.game.characters[next-1]));return current;
        }
        void DrawInspect()
        {
            if(entry==null||selected==null)return;
            GUILayout.Label(C("Scene","Caller rules belong to the entry, not every spoken line."),heading);
            if(entry.story!=null)
            {
                var e=entry.story;Text("Name","Localized scene title in TH or EN. Thai owns the source; English can be empty. Stable event ID is unchanged.",project.SceneName(e,locale),e,v=>project.SetSceneName(e,v,locale));
                bool enabled=project.game.events.Contains(e);bool active=EditorGUILayout.Toggle(C("Enabled","Register this event in the workspace campaign. Invalid drafts cannot be enabled."),enabled);
                if(active!=enabled){notice=project.SetEnabled(e,active)??(active?"Scene enabled.":"Scene disabled.");indexedRevision=-1;StopTest();}
                var delivery=(EventDelivery)EditorGUILayout.Popup(C("Trigger","Auto: scheduler starts it when free. Character: player clicks the character event. Manual: an explicit system call."),(int)e.delivery,new[]{"Manual","Auto","Character"});
                if(delivery!=e.delivery)Mutate(e,"Edit trigger",()=>e.delivery=delivery);
                Actor("Character","Bubble owner and completion context; not necessarily every speaker.",e.character,e,v=>e.character=v);
                int r=e.room==null?0:Array.IndexOf(project.game.rooms,e.room)+1;
                int room=EditorGUILayout.Popup(C("Room","Restrict event availability to this room."),Math.Max(0,r),new[]{"Any"}.Concat(project.game.rooms.Select(x=>x.displayName)).ToArray());
                if(r!=room)Mutate(e,"Edit room",()=>e.room=room==0?null:project.game.rooms[room-1]);
                Number("Start Day","First eligible day, inclusive.",e.startDay,e,v=>e.startDay=v);
                Number("End Day","Last eligible day, inclusive.",e.endDay,e,v=>e.endDay=v);
                Number("Preferred","Scheduling tie-break after priority and expiry; not the only eligible day.",e.preferredDay,e,v=>e.preferredDay=v);
                Toggle("Any Phase","Allow every phase within the day range.",e.anyPhase,e,v=>e.anyPhase=v);
                if(!e.anyPhase){var next=(DayPhase)EditorGUILayout.EnumPopup(C("Phase","Required day phase."),e.triggerPhase);if(next!=e.triggerPhase)Mutate(e,"Edit phase",()=>e.triggerPhase=next);}
                Number("Priority","Higher values are selected first; ties use expiry, preferred day and stable ID.",e.priority,e,v=>e.priority=v);
                Number("Time Cost","Phases consumed after the event finishes, not at each line.",e.elapsedPhases,e,v=>e.elapsedPhases=v);
                Toggle("Repeat","Otherwise the completed event ID prevents another run.",e.repeatDaily,e,v=>{e.repeatDaily=v;if(!v){e.repeatGroup="";e.minimumRepeatDays=0;}});
                if(e.repeatDaily){Number("Cooldown","Minimum day gap for repeat/group reuse.",e.minimumRepeatDays,e,v=>e.minimumRepeatDays=v);Text("Repeat Group","Shares cooldown across alternate event IDs.",e.repeatGroup,e,v=>e.repeatGroup=v);}
                else Text("Exclusive","Completing one event excludes the other events in this group.",e.exclusiveGroup,e,v=>e.exclusiveGroup=v);
                DrawConditions(e.conditions,e,v=>e.conditions=v);
                DrawCompletion(e);
            }
            else if(entry.variant!=null)
            {
                var v=entry.variant;var owner=entry.character;
                GUILayout.Label(C(entry.kind+" · "+owner.displayName,"Interaction opens the character; matching pools compete by priority, then stable ID. Pool rotation uses day and phase."),body);
                Text("Pool Name","Author label for this caller group.",v.displayName,owner,x=>v.displayName=x);
                Number("Start Day","Inclusive",v.startDay,owner,x=>v.startDay=x);Number("End Day","Inclusive",v.endDay,owner,x=>v.endDay=x);
                Number("Priority","Higher matching pool wins; stable ID breaks ties.",v.priority,owner,x=>v.priority=x);
                Number("Min Love","Minimum affection.",v.minimumAffection,owner,x=>v.minimumAffection=x);Number("Max Love","Maximum affection.",v.maximumAffection,owner,x=>v.maximumAffection=x);
                Toggle("Any Phase","Ignore phase restriction.",v.anyPhase,owner,x=>v.anyPhase=x);
                if(!v.anyPhase){var p=(DayPhase)EditorGUILayout.EnumPopup(C("Phase","Required phase."),v.phase);if(p!=v.phase)Mutate(owner,"Edit pool phase",()=>v.phase=p);}
                DrawConditions(v.conditions,owner,x=>v.conditions=x);
                GUILayout.Label(C("Daily talk reward","Talking uses the interaction callback: its reward commits on completion once per day. Choice transaction effects are not the ambient talk reward."),small);
            }
            else GUILayout.Label(C(entry.kind,"This block is selected by an activity, mood/topic rule or gift preference. Flow lists the exact caller; Test shows which caller is supported."),body);

            EditorGUILayout.Space();GUILayout.Label(C("Block","A sequence of lines ending in choices, a continuation or End."),heading);
            Text("Name","Rename this shared block without changing its ID.",selected.displayName,selected,v=>selected.displayName=v);
            if(selected.choices.Length==0)
            {
                if(GUILayout.Button(C("Next: "+StoryIndex.Title(selected.nextNode),"No per-line condition is evaluated here; the runner follows this explicit continuation.")))PickNode(selected.nextNode,n=>Mutate(selected,"Link continuation",()=>selected.nextNode=n));
            }
            var choice=selected.choices.FirstOrDefault(c=>c.choiceId==selectedChoice);
            if(choice!=null)DrawChoiceInspect(choice);
            advanced=EditorGUILayout.Foldout(advanced,C("Source","Stable identity and exact caller property. Workspace edits never target the original campaign."),true);
            if(advanced){EditorGUILayout.SelectableLabel(selected.id,GUILayout.Height(20));EditorGUILayout.SelectableLabel(entry.sourceProperty??"",GUILayout.Height(35));EditorGUILayout.SelectableLabel(project.sourcePath,GUILayout.Height(35));}
        }
        void DrawConditions(Condition[] values,UnityEngine.Object owner,Action<Condition[]> set)
        {
            using(new EditorGUILayout.HorizontalScope())
            {GUILayout.Label(C("Conditions","All listed conditions must pass. Alternative incoming paths remain separate."),EditorStyles.boldLabel);if(GUILayout.Button(C("+","Add a typed condition."),GUILayout.Width(24)))Change(()=>{project.Edit("Add condition",owner);set(values.Concat(new[]{new Condition{type=ConditionType.FlagSet}}).ToArray());},true);}
            foreach(var c in values)
            {
                using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using(new EditorGUILayout.HorizontalScope())
                    {
                        var type=(ConditionType)EditorGUILayout.Popup((int)c.type,new[]{"Value ≥","Value ≤","Flag Set","Flag Clear","Event Done","Event Pending","Work Active","Work Inactive","Task Accepted","Task Pending","Overtime","Can Volunteer","Shared Night"});
                        if(type!=c.type)Mutate(owner,"Edit condition",()=>c.type=type);
                        if(GUILayout.Button(C("×","Remove condition."),GUILayout.Width(22)))Change(()=>{project.Edit("Delete condition",owner);set(values.Where(v=>v!=c).ToArray());},true);
                    }
                    if(c.type==ConditionType.FlagSet||c.type==ConditionType.FlagUnset)
                    {
                        using(new EditorGUILayout.HorizontalScope())
                        {
                            Text("Flag","Custom state key. Set or clear it in choice Results.",c.flagId,owner,v=>c.flagId=v);
                            if(GUILayout.Button(C("…","Search flags already read or written in this workspace."),GUILayout.Width(24)))PickKey(FlagNames().Select(k=>new KeyValuePair<string,string>(k,k)),v=>Mutate(owner,"Select flag",()=>c.flagId=v));
                        }
                    }
                    else if(c.type==ConditionType.EventCompleted||c.type==ConditionType.EventNotCompleted)
                    {
                        var target=project.assets.OfType<EventDataSO>().FirstOrDefault(e=>e.id==c.eventId);
                        if(GUILayout.Button(C("Event: "+(target==null?"Select…":project.Label(target)),"Choose an event by title. Its stable ID is stored; it counts as completed only at the end.")))
                            PickKey(project.assets.OfType<EventDataSO>().Select(e=>new KeyValuePair<string,string>(e.id,project.Label(e))),v=>Mutate(owner,"Select condition event",()=>c.eventId=v));
                    }
                    else if(c.type==ConditionType.StatAtLeast||c.type==ConditionType.StatAtMost)
                    {
                        var stat=(StatType)EditorGUILayout.EnumPopup(C("Value","Stored runtime value. Stress is inactive in the current Romance campaign."),c.stat);
                        if(stat!=c.stat)Mutate(owner,"Edit condition stat",()=>c.stat=stat);
                        if(stat==StatType.Affection||stat==StatType.Stress)Actor("Character","Target of the stat check.",c.character,owner,v=>c.character=v);
                        Number("Threshold","Inclusive comparison.",c.value,owner,v=>c.value=v);
                    }
                    else{Text("Target ID","Workload/task key, when applicable.",c.targetId,owner,v=>c.targetId=v);Actor("Character","Target worker, when applicable.",c.character,owner,v=>c.character=v);}
                }
            }
            if(values.Length==0)GUILayout.Label(C("Always","No additional typed conditions. Trigger windows and scheduler rules still apply."),small);
        }
        void DrawChoiceInspect(DialogueChoice c)
        {
            EditorGUILayout.Space();GUILayout.Label(C("Choice","The runner checks these conditions and costs before committing. Test/Trace shows actual changes."),heading);
            if(GUILayout.Button(C("Target: "+StoryIndex.Title(c.nextNode),"Choose a response or End.")))PickNode(c.nextNode,n=>Mutate(selected,"Link choice",()=>c.nextNode=n));
            DrawConditions(c.conditions,selected,v=>c.conditions=v);
            GUILayout.Label(C("Results","Event choices apply these on selection; the choice ID is also recorded. Ambient interaction callbacks own their completion rewards."),EditorStyles.boldLabel);
            foreach(var f in c.flags)
            {
                using(new EditorGUILayout.HorizontalScope())
                {
                    string id=EditorGUILayout.DelayedTextField(f.flagId??"");bool value=EditorGUILayout.Toggle(f.value,GUILayout.Width(20));
                    if(id!=f.flagId||value!=f.value)Mutate(selected,"Edit flag result",()=>{f.flagId=id;f.value=value;});
                    if(GUILayout.Button(C("×","Remove flag write."),GUILayout.Width(22)))Change(()=>{project.Edit("Remove flag result",selected);c.flags=c.flags.Where(v=>v!=f).ToArray();},true);
                }
            }
            if(GUILayout.Button(C("+ Flag","Set or clear a custom flag when this event choice is selected.")))Change(()=>{project.Edit("Add flag result",selected);c.flags=c.flags.Concat(new[]{new FlagChange()}).ToArray();},true);
            foreach(var e in c.effects)
            {
                using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    var stat=(StatType)EditorGUILayout.EnumPopup(e.stat);if(stat!=e.stat)Mutate(selected,"Edit result",()=>e.stat=stat);
                    if(e.stat==StatType.Affection||e.stat==StatType.Stress)Actor("Character","Target value.",e.character,selected,v=>e.character=v);
                    Number("Change","Signed amount, clamped by the runtime.",e.amount,selected,v=>e.amount=v);
                    if(GUILayout.Button(C("Remove","Delete this numeric effect.")))Change(()=>{project.Edit("Remove result",selected);c.effects=c.effects.Where(x=>x!=e).ToArray();},true);
                }
            }
            if(GUILayout.Button(C("+ Value","Add a numeric result. Runtime rules still determine whether it is valid.")))Change(()=>{project.Edit("Add value result",selected);c.effects=c.effects.Concat(new[]{new StatModifier{stat=StatType.Affection,character=project.game.characters.FirstOrDefault()}}).ToArray();},true);
            foreach(var f in c.flags.Where(f=>!string.IsNullOrWhiteSpace(f.flagId)))DrawReferences(f.flagId);
            if(c.actionPointCost!=0||c.staminaCost!=0||c.workAction!=DialogueWorkAction.None||c.activateWorkloads.Length>0)
                GUILayout.Label(C("Work transaction","This choice also has legacy AP/stamina/work commands. Test validates the actual transaction; this Romance workspace does not invent new work mechanics."),small);
        }
        void DrawCompletion(EventDataSO e)
        {
            GUILayout.Label(C("On Complete","Non-repeat events add completedEvents; repeat events stamp dailyInteractions. Time and Romance outcomes apply at the end. Defer rolls back the snapshot."),EditorStyles.boldLabel);
            if(project.game.romance==null)return;
            foreach(var o in project.game.romance.moodOutcomes.Where(x=>x.eventId==e.id))
            {
                var owner=project.game.romance;
                using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    Actor("Character","Mood recipient after this event finishes.",o.character,owner,v=>o.character=v);
                    var mood=(CharacterMood)EditorGUILayout.EnumPopup(C("Mood","Completion outcome."),o.mood);if(mood!=o.mood)Mutate(owner,"Edit mood outcome",()=>o.mood=mood);
                    Text("If Flag","Empty applies unconditionally; otherwise the flag must be set at completion.",o.requiredFlag,owner,v=>o.requiredFlag=v);
                    if(GUILayout.Button(C("Remove","Remove this completion outcome. Undo restores it.")))Change(()=>{project.Edit("Remove completion outcome",owner);owner.moodOutcomes=owner.moodOutcomes.Where(x=>x!=o).ToArray();},true);
                }
            }
            if(GUILayout.Button(C("+ Mood","Apply a conditional character mood after this event finishes.")))Change(()=>
            {var owner=project.game.romance;project.Edit("Add mood outcome",owner);owner.moodOutcomes=owner.moodOutcomes.Concat(new[]{new MoodOutcome{eventId=e.id,character=project.game.characters.FirstOrDefault()}}).ToArray();},true);
        }
        IEnumerable<string> FlagNames()
        {
            var names=new HashSet<string>();
            foreach(var a in project.assets.Where(a=>a!=null))
            {
                var p=new SerializedObject(a).GetIterator();
                while(p.NextVisible(true))if(p.propertyType==SerializedPropertyType.String&&new[]{"flagId","requiredFlag","topicFlag"}.Contains(p.name)&&!string.IsNullOrWhiteSpace(p.stringValue))names.Add(p.stringValue);
            }
            return names.OrderBy(n=>n);
        }
        void PickKey(IEnumerable<KeyValuePair<string,string>> items,Action<string> picked)
            =>PopupWindow.Show(GUILayoutUtility.GetLastRect(),new KeyPicker(items,picked));
        sealed class KeyPicker:PopupWindowContent
        {
            readonly KeyValuePair<string,string>[] items;readonly Action<string> picked;string query="";Vector2 scroll;
            public KeyPicker(IEnumerable<KeyValuePair<string,string>> items,Action<string> picked){this.items=items.ToArray();this.picked=picked;}
            public override Vector2 GetWindowSize()=>new Vector2(350,320);
            public override void OnGUI(Rect rect)
            {
                query=EditorGUILayout.TextField(query,EditorStyles.toolbarSearchField);
                if(GUILayout.Button("None")){picked("");editorWindow.Close();}
                scroll=EditorGUILayout.BeginScrollView(scroll);
                foreach(var item in items.Where(i=>StoryIndex.Contains(i.Key+" "+i.Value,query)))
                    if(GUILayout.Button(C(item.Value,item.Key),new GUIStyle(EditorStyles.miniButton){wordWrap=true})){picked(item.Key);editorWindow.Close();}
                EditorGUILayout.EndScrollView();
            }
        }
        void DrawFlow()
        {
            if(selected==null)return;
            GUILayout.Label(StoryIndex.Title(selected),heading);
            GUILayout.Label(C("Caller","How the game reaches this entry."),EditorStyles.boldLabel);
            GUILayout.Label(entry.kind+" → "+entry.title,body);GUILayout.Label(entry.gates,small);
            GUILayout.Label(C("Incoming","Every direct executable link and root caller. These are alternatives, not one AND condition."),EditorStyles.boldLabel);
            foreach(var root in index.entries.Where(e=>e.node==selected))if(GUILayout.Button(C("Start · "+root.title,root.sourceProperty),body))Go(root,selected);
            foreach(var edge in index.Incoming(selected))
            {
                if(GUILayout.Button(C("← "+StoryIndex.Title(edge.from)+" / "+edge.Label,"Jump to the source block."),body))Go(Context(edge.from),edge.from);
                if(edge.choice!=null)GUILayout.Label(StoryIndex.Conditions(edge.choice.conditions),small);
            }
            GUILayout.Label(C("Outgoing","Each choice target, continuation or End. Conditions are checked on choices, not every line."),EditorStyles.boldLabel);
            foreach(var edge in index.Outgoing(selected))
            {
                if(GUILayout.Button(C(edge.Label+" → "+StoryIndex.Title(edge.to),"Jump to destination."),body)&&edge.to!=null)Go(Context(edge.to),edge.to);
                if(edge.to!=null&&edge.to.lines.Length>0)GUILayout.Label(edge.to.lines[0].SourceText,small);
            }
            if(!index.Outgoing(selected).Any())GUILayout.Label("End",small);
            showPaths=EditorGUILayout.Foldout(showPaths,C("Paths","Every cycle-free route from this caller to the selected block, with local choice gates."),true);
            if(showPaths)DrawPaths(entry.node,selected,new HashSet<DialogueNodeSO>(),new List<string>());
            GUILayout.Label(C("Used By","All registered root contexts, including shared blocks."),EditorStyles.boldLabel);
            foreach(var root in index.Callers(selected))if(GUILayout.Button(C(root.title+" · "+root.kind,root.sourceProperty),body))Go(root,selected);
            showVariables=EditorGUILayout.Foldout(showVariables,C("Variables","Flags read or written at this entry/block, and the other places that use them."),true);
            if(showVariables)
            {
                var keys=selected.choices.SelectMany(c=>c.flags.Select(f=>f.flagId).Concat(c.conditions.Select(x=>x.flagId)))
                    .Concat((entry.story?.conditions??entry.variant?.conditions??Array.Empty<Condition>()).Select(c=>c.flagId));
                foreach(var key in keys.Where(k=>!string.IsNullOrWhiteSpace(k)).Distinct())DrawReferences(key);
            }
        }
        void DrawPaths(DialogueNodeSO current,DialogueNodeSO target,HashSet<DialogueNodeSO> seen,List<string> path)
        {
            if(current==null||!seen.Add(current))return;
            if(current==target)GUILayout.Label(path.Count==0?"Entry":string.Join("\n↓ ",path),small);
            else foreach(var edge in index.Outgoing(current)){path.Add(StoryIndex.Title(current)+" → "+edge.Label+(edge.choice==null?"":" · "+StoryIndex.Conditions(edge.choice.conditions)));DrawPaths(edge.to,target,seen,path);path.RemoveAt(path.Count-1);}
            seen.Remove(current);
        }
        IEnumerable<Tuple<IdentifiedDataSO,string>> References(string key)
        {
            foreach(var a in project.assets.Where(a=>a!=null))
            {
                var p=new SerializedObject(a).GetIterator();
                while(p.NextVisible(true))if(p.propertyType==SerializedPropertyType.String&&p.stringValue==key&&
                    new[]{"flagId","requiredFlag","topicFlag","eventId"}.Contains(p.name))yield return Tuple.Create(a,p.propertyPath);
            }
        }
        void DrawReferences(string key)
        {
            GUILayout.Label(C(key,"Custom runtime key. Choice results write it; conditions and completion rules may read it."),EditorStyles.boldLabel);
            foreach(var item in References(key))
            {
                string role=item.Item2.Contains(".flags.")?"Writes":"Reads";
                if(GUILayout.Button(C(role+" · "+project.Label(item.Item1),item.Item2),small))
                {
                    if(item.Item1 is DialogueNodeSO n)Go(Context(n),n);
                    else{var root=index.entries.FirstOrDefault(e=>e.owner==item.Item1);if(root!=null)Go(root,root.node);}
                }
            }
        }
        static string[] Split(string value)=>value.Split(',').Select(v=>v.Trim()).Where(v=>v.Length>0).Distinct().ToArray();
        void StartTest(bool bypass)
        {
            StopTest();try
            {
                test=new WorkspacePlaytest(project,day,(DayPhase)phase,affection,Split(seedFlags),Split(seedEvents),project.game.rooms.Length==0?null:project.game.rooms[Mathf.Clamp(roomIndex,0,project.game.rooms.Length-1)].id);
                if(bypass)test.PreviewBlock(selected);else test.Run(entry);
                notice=test.Notice??"";gameView=new WorkspaceGameView(test.Session);test.Session.Localization.SetLocale(locale==0?"th":"en");
            }
            catch(Exception e){notice=e.Message;StopTest();}
        }
        void DrawTest()
        {
            EditorGUI.BeginChangeCheck();
            day=EditorGUILayout.IntSlider(C("Day","New isolated test session."),day,1,project.game.totalDays);
            phase=EditorGUILayout.Popup(C("Phase","Initial time of day."),phase,new[]{"Morning","Afternoon","Evening","Night"});
            affection=EditorGUILayout.IntSlider(C("Affection","Seed affection for all characters in this test only."),affection,0,100);
            roomIndex=EditorGUILayout.Popup(C("Room","Initial room. Visibility and bubble ownership still apply."),roomIndex,project.game.rooms.Select(r=>r.displayName).ToArray());
            seedFlags=EditorGUILayout.TextField(C("Flags","Comma-separated custom keys set in the initial test state. See choice Results and Flow for exact keys."),seedFlags);
            seedEvents=EditorGUILayout.TextField(C("Completed","Comma-separated event IDs treated as completed in this custom test state."),seedEvents);
            if(EditorGUI.EndChangeCheck())StopTest();
            using(new EditorGUILayout.HorizontalScope())
            {
                if(GUILayout.Button(C("Run Caller","Use the actual event scheduler/interaction selector. A disabled/blocked scene will not start.")))StartTest(false);
                if(GUILayout.Button(C("Preview Block","Bypass entry gates in an isolated session. This does not prove the real caller is eligible.")))StartTest(true);
            }
            if(test==null)return;
            GUILayout.Label(C(test.Mode,test.Mode=="Caller"?"Real caller eligibility and selection.":"Entry gates bypassed. Pending forced events are cleared only in this isolated preview."),EditorStyles.boldLabel);
            foreach(var check in test.checks)GUILayout.Label(C((check.passed?"✓ ":"× ")+check.label,check.detail),small);
            var pending=test.Session.Events.NextForced;if(pending!=null)GUILayout.Label(C("Pending: "+StoryIndex.Title(pending),"This auto event takes priority before other callers."),small);
            showGameView=EditorGUILayout.Foldout(showGameView,C("Game View","Same saved game prefab and the exact detached session inspected by State/Trace."),true);
            if(showGameView&&gameView!=null){var rect=GUILayoutUtility.GetAspectRect(16f/9f);if(Event.current.type==EventType.Repaint)GUI.DrawTexture(rect,gameView.Render(),ScaleMode.ScaleToFit,false);}
            var runner=test.Session.Dialogue;
            if(runner.IsActive)
            {
                var line=runner.CurrentLine;if(line!=null){GUILayout.Label(line.speaker?.displayName??"Narrator",EditorStyles.boldLabel);GUILayout.Label(test.Session.Localization.Line(runner.Node,line),body);}
                if(runner.ChoicesReady)foreach(var c in runner.Node.choices)
                {
                    bool allowed=runner.CanChoose(c);
                    using(new EditorGUI.DisabledScope(!allowed))if(GUILayout.Button(C(test.Session.Localization.Choice(runner.Node,c),StoryIndex.Conditions(c.conditions)),new GUIStyle(EditorStyles.miniButton){wordWrap=true}))test.Choose(c);
                    if(!allowed)GUILayout.Label(StoryIndex.Conditions(c.conditions),small);
                }
                else if(GUILayout.Button(C("Next Line","Advance the actual runner. Read-line history and completion callbacks update State/Trace.")))test.Advance();
                if(GUILayout.Button(C("Defer","Roll back the event/interaction snapshot. Trace shows which values are restored.")))test.Back();
                if(GUILayout.Button(C("Locate","Select the current runtime block in this same editor."))){var n=test.Source(runner.Node);if(n!=null)Go(Context(n),n);}
            }
            else
            {
                if(test.Session.Interactions.IsOpen&&GUILayout.Button(C("Close","Close the character menu before another caller can run.")))test.CloseInteraction();
                if(GUILayout.Button(C("Next Event","Ask the real scheduler to start its next forced event.")))test.Next();
            }
        }
        void DrawState()
        {
            if(test==null){GUILayout.Label(C("No test","Run a Caller or Block Preview in Test."),small);return;}
            stateGroup=GUILayout.Toolbar(stateGroup,new[]{C("Values","Day, room, numeric and character state."),C("Flags","Custom booleans written by story/game systems."),C("History","Read lines, chosen options, completed events and repeat stamps.")});
            stateQuery=EditorGUILayout.TextField(stateQuery,EditorStyles.toolbarSearchField);
            var s=test.Session.State;
            if(stateGroup==0)
            {
                StateValue("Day",s.day.ToString(),"Current test day.");StateValue("Phase",StoryIndex.Phase(s.phase),"Current time of day.");
                StateValue("Room",test.Session.CurrentRoom?.displayName??s.roomId,"Room used by entry/visibility checks.");
                StateValue("Money",s.money.ToString(),"Current balance.");StateValue("Progress",s.mainProgress.ToString(),"Current main project progress.");
                StateValue("Skill Points",s.skillPoints.ToString(),"Current spendable skill points.");
                if(!test.Session.Romance.Enabled){StateValue("AP",s.actionPoints.ToString(),"Action points, when enabled.");StateValue("Stamina",s.stamina.ToString(),"Energy, when enabled.");}
                foreach(var c in s.characters)
                {
                    string name=project.game.characters.FirstOrDefault(a=>a.id==c.characterId)?.displayName??c.characterId;
                    StateValue(name+" · Affection",c.affection.ToString(),"Stored relationship value for this character.");
                    if(test.Session.Romance.Enabled)StateValue(name+" · Mood",test.Session.Romance.Mood(c.characterId).ToString(),"Current authored categorical mood.");
                    else StateValue(name+" · Stress",c.stress.ToString(),"Current stress; inactive in Romance.");
                }
            }
            else if(stateGroup==1)
            {
                if(s.flags.Count==0)GUILayout.Label(C("No flags","No custom flag is set in this test state."),small);
                foreach(var flag in s.flags)StateValue(flag,"On",StateHelp("flags"));
            }
            else
            {
                GUILayout.Label(C("Choices","IDs are stored on selection. Use explicit flag results for later Flag Set conditions."),EditorStyles.boldLabel);
                foreach(var id in s.chosenOptions)
                {
                    var owner=index.nodes.FirstOrDefault(n=>n.choices.Any(c=>c.choiceId==id));
                    var choice=owner?.choices.FirstOrDefault(c=>c.choiceId==id);
                    if(StoryIndex.Contains(id+" "+choice?.SourceText,stateQuery)&&GUILayout.Button(C(choice?.SourceText??id,id),body)&&owner!=null)Go(Context(owner),owner);
                }
                GUILayout.Label(C("Completed Events","Added only when non-repeat scenes finish."),EditorStyles.boldLabel);
                foreach(var id in s.completedEvents)
                {
                    var root=index.entries.FirstOrDefault(e=>e.story?.id==id);
                    if(StoryIndex.Contains(id+" "+root?.title,stateQuery)&&GUILayout.Button(C(root?.title??id,id),body)&&root!=null)Go(root,root.node);
                }
                GUILayout.Label(C("Repeat Events","Last completion day used by cooldown rules."),EditorStyles.boldLabel);
                foreach(var e in s.dailyInteractions)StateValue(index.entries.FirstOrDefault(x=>x.story?.id==e.eventId)?.title??e.eventId,"Day "+e.day,"Repeat-event completion stamp.");
                readHistory=EditorGUILayout.Foldout(readHistory,C("Read Lines ("+s.readLines.Count+")","Lines recorded by advancement. This history is separate from flags and event completion."),true);
                if(readHistory)foreach(var id in s.readLines)
                {
                    var owner=index.nodes.FirstOrDefault(n=>n.lines.Any(l=>l.lineId==id));var line=owner?.lines.FirstOrDefault(l=>l.lineId==id);
                    if(StoryIndex.Contains(id+" "+line?.SourceText,stateQuery)&&GUILayout.Button(C(line?.SourceText??id,id),body)&&owner!=null){Navigate(Context(owner),owner);focusId=id;GUIUtility.ExitGUI();}
                }
            }
            rawState=EditorGUILayout.Foldout(rawState,C("JSON","Detached RuntimeGameState only. The player save file is not loaded or written."),true);
            if(rawState)EditorGUILayout.TextArea(JsonUtility.ToJson(test.Session.State,true),GUILayout.MinHeight(180));
        }
        void StateValue(string label,string value,string help)
        {
            if(!StoryIndex.Contains(label+" "+value,stateQuery))return;
            using(new EditorGUILayout.HorizontalScope()){GUILayout.Label(C(label,help),body,GUILayout.MinWidth(110));GUILayout.Label(value,body,GUILayout.Width(110));}
        }
        static string StateHelp(string key)
        {
            if(key.StartsWith("flags"))return "Custom flags set/cleared by story results and other systems. Conditions can read them.";
            if(key.StartsWith("chosenOptions"))return "Recorded choice IDs. History alone is not a FlagSet condition; author an explicit flag result for later gates.";
            if(key.StartsWith("completedEvents"))return "Recorded when a non-repeat event finishes, not when its first line opens.";
            if(key.StartsWith("readLines"))return "Stable line IDs recorded as the runner advances.";
            if(key.StartsWith("dailyInteractions"))return "Last completion day of repeat events; used by repeat/cooldown checks.";
            return "Current detached runtime value. Trace lists the action that changed it.";
        }
        void DrawTrace()
        {
            if(test==null){GUILayout.Label(C("No test","Run a Caller or Block Preview in Test."),small);return;}
            rawTrace=EditorGUILayout.ToggleLeft(C("Raw","Show exact runtime paths and count changes for debugging."),rawTrace);
            foreach(var message in test.trace)
            {
                if(!rawTrace&&(message.Contains(".count:")||message.Contains("romance.catalog:")))continue;
                GUILayout.Label(C(rawTrace?message:TraceLabel(message),message),body);
            }
        }
        string TraceLabel(string raw)
        {
            var match=System.Text.RegularExpressions.Regex.Match(raw,@"(flags|chosenOptions|completedEvents|readLines)\[([^\]]+)\]");
            if(!match.Success)return raw;
            string kind=match.Groups[1].Value,id=match.Groups[2].Value,change=raw.Substring(raw.IndexOf(']')+1);
            if(kind=="flags")return "Flag · "+id+change;
            if(kind=="completedEvents")return "Completed · "+(index.entries.FirstOrDefault(e=>e.story?.id==id)?.title??id)+change;
            if(kind=="chosenOptions")return "Choice · "+(index.nodes.SelectMany(n=>n.choices).FirstOrDefault(c=>c.choiceId==id)?.SourceText??id)+change;
            return "Read · "+(index.nodes.SelectMany(n=>n.lines).FirstOrDefault(l=>l.lineId==id)?.SourceText??id)+change;
        }
    }
}
