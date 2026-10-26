using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Data;
using ProjectLove.Simulation;
using ProjectLove.UI;
using UnityEditor;
using UnityEngine;
namespace ProjectLove.EditorTools
{
    public sealed class AuthoringEntry
    {
        public DialogueNodeSO node;public CharacterDataSO character;public EventDataSO story;
        public string category,title;
        public string SearchText=>string.Join(" ",new[]{title,category,character?.displayName,node.displayName,node.id}.Concat(node.lines.Select(l=>l.SourceText)).Concat(node.choices.Select(c=>c.SourceText)));
    }
    public static class AuthoringCatalog
    {
        public static string ExplainBlockedEvent(Core.GameSession s,EventDataSO e)
        {
            var reasons=new List<string>();
            if(s.State.day<e.startDay||s.State.day>e.endDay)reasons.Add("วันไม่อยู่ในช่วง "+e.startDay+"–"+e.endDay);
            if(!e.anyPhase&&s.State.phase!=e.triggerPhase)reasons.Add("ต้องเป็นช่วง "+s.Config.romance.presentation.phaseNames[(int)e.triggerPhase]);
            if(e.room!=null&&e.room.id!=s.State.roomId)reasons.Add("ต้องอยู่ใน "+e.room.displayName);
            if(!VN.DialogueRunner.IsPlayable(e.entryNode))reasons.Add("บทเริ่มต้นยังไม่พร้อมเล่น");
            if(!s.Days.CanConsumePhases(e.elapsedPhases))reasons.Add("เหลือเวลาในวันไม่พอ");
            foreach(var c in e.conditions.Where(c=>!s.Stats.Meets(new[]{c})))
                reasons.Add(c==null?"มีเงื่อนไขว่าง": "เงื่อนไขยังไม่ผ่าน: "+c.type+" · "+(c.character!=null?c.character.displayName+" ":"")+c.stat+" "+c.value+" "+c.flagId+c.eventId+c.targetId);
            var forced=s.Events.NextForced;if(forced!=null&&forced!=e)reasons.Add("อีเว้นอัตโนมัติรอก่อน: "+forced.displayName);
            if(reasons.Count==0)reasons.Add("ยังไม่พร้อมในบริบทนี้: ตรวจเจ้าของบับเบิล ลำดับอีเว้น กลุ่มทางเลือก หรือการเล่นซ้ำ");
            return string.Join("\n",reasons);
        }
        public static List<IdentifiedDataSO> Reachable(GameConfigSO game)
        {
            var seen=new HashSet<IdentifiedDataSO>();
            void Visit(IdentifiedDataSO asset)
            {
                if(asset==null||!seen.Add(asset))return;var so=new SerializedObject(asset);var p=so.GetIterator();
                while(p.NextVisible(true))if(p.propertyType==SerializedPropertyType.ObjectReference&&p.objectReferenceValue is IdentifiedDataSO next)Visit(next);
            }
            Visit(game);return seen.ToList();
        }
        public static List<AuthoringEntry> Entries(GameConfigSO game)
        {
            var result=new List<AuthoringEntry>();var keys=new HashSet<string>();
            void Scene(DialogueNodeSO node,CharacterDataSO c,string kind,string title,EventDataSO story=null)
            {
                if(node==null)return;string key=AssetDatabase.GetAssetPath(node)+":"+kind+":"+c?.id;if(!keys.Add(key))return;
                result.Add(new AuthoringEntry{node=node,character=c,category=kind,title=string.IsNullOrEmpty(node.displayName)?title:node.displayName,story=story});
                Scene(node.nextNode,c,kind,title+" / ต่อ",story);
                for(int i=0;i<node.choices.Length;i++)Scene(node.choices[i].nextNode,c,kind,title+" / ตัวเลือก "+(i+1),story);
            }
            foreach(var c in game.characters)
            {
                foreach(var v in c.greetings)foreach(var n in v.Nodes)Scene(n,c,"ทักทาย",c.displayName+" · ทักทาย");
                foreach(var v in c.conversations)foreach(var n in v.Nodes)Scene(n,c,"คุยเล่น",c.displayName+" · คุยเล่น");
                Scene(c.neutralGiftResponse,c,"ของขวัญ",c.displayName+" · ของขวัญทั่วไป");
                foreach(var g in c.giftPreferences)Scene(g.response,c,"ของขวัญ",c.displayName+" · "+g.item.displayName);
            }
            if(game.romance!=null)
            {
                foreach(var c in game.romance.cast)
                {Scene(c.sharedScene,c.character,"ทำงานด้วยกัน",c.character.displayName+" · ทำงานด้วยกัน");Scene(c.personalScene,c.character,"เวลาส่วนตัว",c.character.displayName+" · เวลาส่วนตัว");Scene(c.comfortScene,c.character,"ปลอบใจ",c.character.displayName+" · อยู่เป็นเพื่อน");Scene(c.warmScene,c.character,"สนิทสนม",c.character.displayName+" · เมื่อสนิท");Scene(c.focusTalk,c.character,"หัวข้อใหม่",c.character.displayName+" · "+c.topicTitle);}
                Scene(game.romance.producerScene,null,"โปรดิวเซอร์",game.romance.presentation.Get("Producer"));
            }
            foreach(var e in game.events)Scene(e.entryNode,e.character,"อีเว้น",e.displayName,e);
            return result.OrderBy(e=>e.character?.displayName).ThenBy(e=>e.category).ThenBy(e=>e.title).ToList();
        }
        public static IEnumerable<AuthoringEntry> Search(IEnumerable<AuthoringEntry> entries,string text,CharacterDataSO c=null,string category=null)
        {return entries.Where(e=>(c==null||e.character==c)&&(string.IsNullOrEmpty(category)||e.category==category)&&(string.IsNullOrWhiteSpace(text)||e.SearchText.IndexOf(text,StringComparison.OrdinalIgnoreCase)>=0));}
        public static void AddLine(DialogueNodeSO node,CharacterDataSO speaker)
        {
            Undo.RecordObject(node,"Add dialogue line");node.lines=node.lines.Concat(new[]{new DialogueLine{lineId=Guid.NewGuid().ToString("N"),speaker=speaker,text=""}}).ToArray();EditorUtility.SetDirty(node);
        }
        public static EventDataSO CreateDraft(GameConfigSO game,string displayName,CharacterDataSO character)
        {
            string folder=System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(game)).Replace('\\','/')+"/Drafts";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(folder).Replace('\\','/'),"Drafts");
            string key=Guid.NewGuid().ToString("N");var n=ScriptableObject.CreateInstance<DialogueNodeSO>();n.id="node_"+key;n.displayName=displayName;
            var e=ScriptableObject.CreateInstance<EventDataSO>();e.id="event_"+key;e.displayName=displayName;e.character=character;e.startDay=e.preferredDay=1;e.endDay=game.totalDays;e.anyPhase=true;e.delivery=character!=null?EventDelivery.CharacterBubble:EventDelivery.Forced;e.entryNode=n;
            AssetDatabase.CreateAsset(n,folder+"/"+n.id+".asset");AssetDatabase.CreateAsset(e,folder+"/"+e.id+".asset");AddLine(n,character);AssetDatabase.SaveAssets();return e;
        }
        public static string[] Publish(GameConfigSO game,EventDataSO story)
        {
            if(game.events.Contains(story))return Array.Empty<string>();var probe=UnityEngine.Object.Instantiate(game);
            try
            {
                probe.events=game.events.Concat(new[]{story}).ToArray();var errors=EventScheduler.ValidateCatalog(probe);if(errors.Count>0)return errors.ToArray();
                try{_ = new Core.GameSession(probe);}catch(ArgumentException e){return new[]{e.Message};}
                Undo.RecordObject(game,"Publish story event");game.events=probe.events;EditorUtility.SetDirty(game);AssetDatabase.SaveAssets();return Array.Empty<string>();
            }
            finally{UnityEngine.Object.DestroyImmediate(probe);}
        }
        public static IEnumerable<EventDataSO> Drafts(GameConfigSO game)
        {
            string folder=System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(game)).Replace('\\','/')+"/Drafts";
            if(!AssetDatabase.IsValidFolder(folder))return Array.Empty<EventDataSO>();return AssetDatabase.FindAssets("t:EventDataSO",new[]{folder}).Select(g=>AssetDatabase.LoadAssetAtPath<EventDataSO>(AssetDatabase.GUIDToAssetPath(g))).Where(e=>!game.events.Contains(e));
        }
    }
}
