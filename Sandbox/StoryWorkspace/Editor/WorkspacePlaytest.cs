using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProjectLove.Core;
using ProjectLove.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.StorySandbox
{
    public sealed class LogicCheck
    {
        public string label,detail;public bool passed;
        public LogicCheck(string label,bool passed,string detail){this.label=label;this.passed=passed;this.detail=detail;}
    }
    public sealed class WorkspacePlaytest:IDisposable
    {
        readonly Dictionary<IdentifiedDataSO,IdentifiedDataSO> copies;
        EventDataSO transient;
        public GameSession Session{get;}
        public string Mode{get;private set;}="Caller";
        public readonly List<string> trace=new List<string>();
        public readonly List<LogicCheck> checks=new List<LogicCheck>();
        public string Notice{get;private set;}
        public WorkspacePlaytest(WorkspaceProject project,int day,DayPhase phase,int affection,string[] flags,string[] completed,string room=null)
        {
            copies=WorkspaceProject.CloneGraph(project.game,project.assets);
            try
            {
                foreach(var a in copies.Values)a.hideFlags=HideFlags.HideAndDontSave;
                WorkspaceProject.AddRuntimeAliases(copies.Values);
                Session=new GameSession((GameConfigSO)copies[project.game]);Session.State.day=Mathf.Clamp(day,1,Session.Config.totalDays);
                Session.State.phase=phase;Session.Romance.StartDay();
                if(!string.IsNullOrWhiteSpace(room))Session.State.roomId=room;
                foreach(var c in Session.State.characters)c.affection=Mathf.Clamp(affection,0,100);
                foreach(var f in flags.Where(f=>!string.IsNullOrWhiteSpace(f)).Distinct())if(!Session.State.flags.Contains(f))Session.State.flags.Add(f);
                foreach(var e in completed.Where(e=>Session.Config.events.Any(s=>s.id==e)).Distinct())if(!Session.State.completedEvents.Contains(e))Session.State.completedEvents.Add(e);
            }
            catch{Dispose();throw;}
        }
        public T Copy<T>(T source) where T:IdentifiedDataSO=>source!=null&&copies.TryGetValue(source,out var copy)?(T)copy:null;
        public DialogueNodeSO Source(DialogueNodeSO node)=>copies.FirstOrDefault(p=>p.Value==node).Key as DialogueNodeSO;
        public bool Run(StoryEntry entry)
        {
            bool result=false;checks.Clear();Notice=null;
            Perform("Caller: "+entry.kind+" / "+entry.title,()=>
            {
                if(entry.story!=null)
                {
                    var story=Copy(entry.story);
                    if(!Session.Config.events.Contains(story)){Notice="Scene is disabled.";return;}
                    checks.AddRange(EventChecks(Session,story));result=Session.Dialogue.TryBegin(story);
                }
                else if(entry.character!=null)
                {
                    var actor=Copy(entry.character);
                    var hotspot=Session.CurrentRoom?.hotspots.FirstOrDefault(h=>h.character==actor);
                    if(hotspot==null||!Session.Interactions.TryOpen(hotspot)){Notice="Character is unavailable. Check room, visibility and pending auto events.";return;}
                    if(entry.variant!=null)
                    {
                        var groups=entry.kind=="Greeting"?actor.greetings:actor.conversations;
                        foreach(var group in groups)checks.Add(new LogicCheck("Pool · "+(string.IsNullOrWhiteSpace(group.displayName)?group.id:group.displayName),Session.Interactions.MatchesVariant(actor,group),
                            $"Days {group.startDay}–{group.endDay}; affection {group.minimumAffection}–{group.maximumAffection}; priority {group.priority}; "+StoryIndex.Conditions(group.conditions)));
                    }
                    if(entry.kind=="Talk"||entry.sourceProperty.EndsWith("focusTalk"))result=Session.Interactions.TryTalk();
                    else if(entry.kind=="Greeting")
                    {
                        var greeting=Session.Interactions.Greeting;result=greeting!=null;
                        Notice=greeting==null?"No greeting is eligible.":"Greeting selected: "+StoryIndex.Title(greeting);
                        if(greeting!=null)trace.Add("Greeting text is displayed by the interaction menu; it does not commit choice effects.");
                    }
                    else if(entry.owner is RomanceConfigSO)result=Session.Romance.TryStart(entry.sourceProperty.EndsWith("sharedScene")?RomanceActivity.WorkTogether:RomanceActivity.PersonalTime,actor.id);
                    else Notice="This caller requires its gift action and inventory. Use Block Preview to inspect its writing.";
                    if(result&&Session.Dialogue.IsActive&&Session.Dialogue.Node!=Copy(entry.node))
                        trace.Add("Selector chose a different pool/topic block: "+StoryIndex.Title(Session.Dialogue.Node));
                }
                else if(entry.owner is RomanceConfigSO && entry.sourceProperty=="producerScene")
                    result=Session.Romance.TryStart(RomanceActivity.Producer);
                else Notice="This caller is not simulated. Use Block Preview; check the caller reference in Flow.";
            });
            if(!result&&string.IsNullOrEmpty(Notice))Notice="Blocked. Inspect Checks and the pending event.";
            return result;
        }
        public bool PreviewBlock(DialogueNodeSO source)
        {
            Mode="Block Preview";
            // Deliberate isolated bypass; never presented as real caller eligibility.
            for(int i=0;i<Session.Config.events.Length;i++)
            {
                var pending=Session.Events.NextForced;if(pending==null)break;
                if(pending.repeatDaily)Session.State.dailyInteractions.Add(new DailyInteractionState{eventId=pending.id,day=Session.State.day});
                else Session.State.completedEvents.Add(pending.id);
            }
            transient=ScriptableObject.CreateInstance<EventDataSO>();transient.hideFlags=HideFlags.HideAndDontSave;
            transient.id="workspace_preview";transient.startDay=1;transient.endDay=Session.Config.totalDays;transient.anyPhase=true;transient.entryNode=Copy(source);
            bool result=false;Perform("Block Preview: entry gates bypassed in isolated state",()=>result=Session.Dialogue.TryBegin(transient));
            if(!result)Notice="Block is not playable. Fill its lines and check links.";return result;
        }
        public void Advance()=>Perform("Advance",()=>Session.Dialogue.TryAdvance());
        public void Choose(DialogueChoice choice)=>Perform("Choose: "+choice.SourceText,()=>Session.Dialogue.TryChoose(choice));
        public void Back()=>Perform("Defer / rollback",()=>Session.Dialogue.TryDefer());
        public void Next()=>Perform("Check next auto event",()=>Session.Events.TryBeginForced());
        public void CloseInteraction()=>Perform("Close character interaction",()=>Session.Interactions.TryClose());
        void Perform(string action,Action run)
        {
            var before=Snapshot(Session.State);trace.Add(action);run();
            var after=Snapshot(Session.State);
            foreach(var key in before.Keys.Concat(after.Keys).Distinct().OrderBy(k=>k))
            {before.TryGetValue(key,out var a);after.TryGetValue(key,out var b);if(a!=b)trace.Add("  "+key+": "+(a??"—")+" → "+(b??"—"));}
            if(Session.Dialogue.IsActive)trace.Add("  At: "+StoryIndex.Title(Session.Dialogue.Node)+" / line "+(Session.Dialogue.LineIndex+1));
            else trace.Add("  No active dialogue");
        }
        public static List<LogicCheck> EventChecks(GameSession s,EventDataSO e)
        {
            var checks=new List<LogicCheck>{
                new LogicCheck("Day",s.State.day>=e.startDay&&s.State.day<=e.endDay,$"{s.State.day} in {e.startDay}–{e.endDay}"),
                new LogicCheck("Phase",e.anyPhase||s.State.phase==e.triggerPhase,e.anyPhase?"Any phase":StoryIndex.Phase(e.triggerPhase)),
                new LogicCheck("Room",e.room==null||e.room.id==s.State.roomId,e.room==null?"Any room":e.room.displayName),
                new LogicCheck("History",e.repeatDaily||!s.State.completedEvents.Contains(e.id),e.repeatDaily?"Repeat / cooldown policy":"Not completed"),
                new LogicCheck("Time",s.Days.CanConsumePhases(e.elapsedPhases),e.elapsedPhases+" phases"),
                new LogicCheck("Cost",s.CanPay(0,e.staminaCost),e.staminaCost+" stamina"),
                new LogicCheck("Playable",ProjectLove.VN.DialogueRunner.IsPlayable(e.entryNode),"Entry block has valid lines"),
                new LogicCheck("Scheduler",s.Events.CanBegin(e),"Priority, visibility, repeat/exclusive groups and pending auto event"),
                new LogicCheck("Caller",s.CanStartEvent(e),"Actual GameSession.CanStartEvent result")};
            foreach(var c in e.conditions)checks.Add(new LogicCheck("Condition",s.Stats.Meets(new[]{c}),StoryIndex.Conditions(new[]{c})));
            return checks;
        }
        public static Dictionary<string,string> Snapshot(RuntimeGameState state)
        {
            var values=new Dictionary<string,string>();
            void Walk(object value,string path)
            {
                if(value==null){values[path]="null";return;}
                var type=value.GetType();
                if(value is string||type.IsPrimitive||type.IsEnum){values[path]=Convert.ToString(value,System.Globalization.CultureInfo.InvariantCulture);return;}
                if(value is IEnumerable list)
                {
                    int i=0;foreach(var item in list)
                    {
                        string key=item is string text?text:null;
                        if(item!=null&&key==null)foreach(string name in new[]{"characterId","eventId","taskId","itemId"})
                        {key=item.GetType().GetField(name)?.GetValue(item) as string;if(!string.IsNullOrEmpty(key))break;}
                        if(item is string)values[path+"["+key+"]"]="true";else Walk(item,path+"["+(key??i.ToString())+"]");i++;
                    }
                    values[path+".count"]=i.ToString();return;
                }
                foreach(var f in type.GetFields(BindingFlags.Instance|BindingFlags.Public))Walk(f.GetValue(value),string.IsNullOrEmpty(path)?f.Name:path+"."+f.Name);
            }
            Walk(state,"");return values;
        }
        public void Dispose(){if(copies!=null)foreach(var a in copies.Values)UnityEngine.Object.DestroyImmediate(a);if(transient!=null)UnityEngine.Object.DestroyImmediate(transient);}
    }
}
