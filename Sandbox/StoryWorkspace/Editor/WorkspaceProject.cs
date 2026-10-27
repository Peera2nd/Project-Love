using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.EditorTools;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.StorySandbox
{
    // One persisted, isolated authoring copy. Original campaign assets are never editing targets.
    public sealed class WorkspaceProject : ScriptableObject
    {
        public GameConfigSO game;
        public List<IdentifiedDataSO> assets=new List<IdentifiedDataSO>();
        public string sourcePath;
        public int revision;
        public const string DefaultPath="Assets/_Project/StorySandboxDrafts/Workspace.asset";

        public static WorkspaceProject Open()
        {
            if(!StoryWorkspaceWindow.IsSandbox)throw new InvalidOperationException("Open the sandbox project first.");
            var existing=AssetDatabase.LoadAssetAtPath<WorkspaceProject>(DefaultPath);
            if(existing!=null)return existing;
            var source=AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath);
            return Create(source,DefaultPath,StoryDraft.Load());
        }
        public static WorkspaceProject Create(GameConfigSO source,string path,StoryDraft legacy=null)
        {
            if(source==null)throw new ArgumentException("Campaign not found.");
            if(AssetDatabase.LoadMainAssetAtPath(path)!=null)throw new InvalidOperationException("Workspace already exists.");
            var project=CreateInstance<WorkspaceProject>();project.name="Story Workspace";
            project.sourcePath=AssetDatabase.GetAssetPath(source);
            AssetDatabase.CreateAsset(project,path);
            try
            {
                var copies=CloneGraph(source);
                foreach(var pair in copies)
                {
                    pair.Value.hideFlags=HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(pair.Value,project);project.assets.Add(pair.Value);
                }
                project.game=(GameConfigSO)copies[source];
                if(legacy!=null)foreach(var pair in copies.Where(p=>p.Key is LocalizationCatalogSO))
                {
                    string guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(pair.Key));
                    foreach(var row in legacy.texts.Where(t=>t.catalogGuid==guid))
                    {
                        var entry=((LocalizationCatalogSO)pair.Value).Find(row.key);
                        if(entry==null)continue;entry.thai=row.thai;entry.english=row.english;
                    }
                }
                EditorUtility.SetDirty(project);project.Save();return project;
            }
            catch{AssetDatabase.DeleteAsset(path);throw;}
        }
        public static Dictionary<IdentifiedDataSO,IdentifiedDataSO> CloneGraph(GameConfigSO source,IEnumerable<IdentifiedDataSO> extra=null)
        {
            var copies=new Dictionary<IdentifiedDataSO,IdentifiedDataSO>();
            foreach(var a in AuthoringCatalog.Reachable(source).Concat(extra??Array.Empty<IdentifiedDataSO>()).Where(a=>a!=null).Distinct())
            {var clone=Instantiate(a);clone.name=a.name;copies.Add(a,clone);}
            foreach(var clone in copies.Values)
            {
                var so=new SerializedObject(clone);var p=so.GetIterator();
                while(p.NextVisible(true))if(p.propertyType==SerializedPropertyType.ObjectReference&&
                    p.objectReferenceValue is IdentifiedDataSO original&&copies.TryGetValue(original,out var next))p.objectReferenceValue=next;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            return copies;
        }
        public void Edit(string action,params UnityEngine.Object[] targets)
        {
            Undo.RecordObjects(targets.Concat(new UnityEngine.Object[]{this}).Distinct().ToArray(),action);
            revision++;EditorUtility.SetDirty(this);
            foreach(var target in targets)EditorUtility.SetDirty(target);
        }
        T Add<T>() where T:IdentifiedDataSO
        {
            var value=CreateInstance<T>();value.id=Guid.NewGuid().ToString("N");value.name=typeof(T).Name+"_"+value.id;
            AssetDatabase.AddObjectToAsset(value,this);Undo.RegisterCreatedObjectUndo(value,"Create story data");
            assets.Add(value);return value;
        }
        public void Save(){foreach(var a in assets.Where(a=>a!=null))EditorUtility.SetDirty(a);EditorUtility.SetDirty(this);AssetDatabase.SaveAssetIfDirty(this);}
        public EventDataSO NewScene()
        {
            Edit("New scene");var story=Add<EventDataSO>();story.displayName="Untitled Scene";
            var title=CreateText(LocalizationService.Key(story,"displayName"));SetText(title,story.displayName,"");
            story.startDay=story.preferredDay=1;story.endDay=game.totalDays;story.anyPhase=true;story.delivery=EventDelivery.Manual;
            story.entryNode=Add<DialogueNodeSO>();story.entryNode.displayName="Untitled Block";
            AddLine(story.entryNode,0);return story;
        }
        public DialogueLine AddLine(DialogueNodeSO node,int index)
        {
            Edit("Insert line",node,game.localization.catalog);
            var line=new DialogueLine{lineId=Guid.NewGuid().ToString("N")};
            line.localizedText=CreateText(LocalizationService.LineKey(node,line.lineId));
            var list=node.lines.ToList();list.Insert(Mathf.Clamp(index,0,list.Count),line);node.lines=list.ToArray();return line;
        }
        public LocalizedTextRef CreateText(string key)
        {
            var catalog=game.localization.catalog;
            catalog.entries=catalog.entries.Concat(new[]{new LocalizationEntry{key=key,thai="",english=""}}).ToArray();
            EditorUtility.SetDirty(catalog);return new LocalizedTextRef{catalog=catalog,key=key};
        }
        public DialogueChoice AddChoice(DialogueNodeSO node)
        {
            Edit("Add choice",node,game.localization.catalog);
            var choice=new DialogueChoice{choiceId=Guid.NewGuid().ToString("N")};
            choice.localizedText=CreateText(LocalizationService.ChoiceKey(node,choice.choiceId));
            if(node.choices.Length==0&&node.nextNode!=null){choice.nextNode=node.nextNode;node.nextNode=null;}
            node.choices=node.choices.Concat(new[]{choice}).ToArray();return choice;
        }
        public DialogueNodeSO InsertBlock(DialogueNodeSO node,int afterLine)
        {
            Edit("Insert block",node,game.localization.catalog);
            var next=Add<DialogueNodeSO>();next.displayName="Untitled Block";
            int cut=Mathf.Clamp(afterLine,0,node.lines.Length);
            next.lines=node.lines.Skip(cut).ToArray();node.lines=node.lines.Take(cut).ToArray();
            next.choices=node.choices;next.nextNode=node.nextNode;node.choices=Array.Empty<DialogueChoice>();node.nextNode=next;
            if(next.lines.Length==0)AddLine(next,0);
            return next;
        }
        public void MoveLine(DialogueNodeSO from,string id,DialogueNodeSO to,int slot)
        {
            var line=from.lines.FirstOrDefault(l=>l.lineId==id);if(line==null)return;
            Edit("Move line",from,to);
            int old=Array.IndexOf(from.lines,line);
            var source=from.lines.ToList();source.Remove(line);from.lines=source.ToArray();
            var target=to.lines.ToList();if(from==to&&old<slot)slot--;
            target.Insert(Mathf.Clamp(slot,0,target.Count),line);to.lines=target.ToArray();
            // Identity and catalog key stay stable. Runtime aliases are generated only in detached copies.
        }
        public void SetText(LocalizedTextRef text,string thai,string english)
        {
            var entry=text.catalog?.Find(text.key);if(entry==null)throw new InvalidOperationException("Text key not found.");
            Edit("Edit text",text.catalog);
            if(entry.thai!=thai){entry.thai=thai;entry.sourceRevision++;entry.englishReviewedRevision=0;}
            entry.english=english;
        }
        public string Label(IdentifiedDataSO asset)=>asset is EventDataSO e?game.localization.catalog.Find(LocalizationService.Key(e,"displayName"))?.thai??StoryIndex.Title(e):StoryIndex.Title(asset);
        public string SceneName(EventDataSO scene,int locale)
        {
            var text=game.localization.catalog.Find(LocalizationService.Key(scene,"displayName"));
            return locale==1?text?.english??"":text?.thai??scene.displayName;
        }
        public void SetSceneName(EventDataSO scene,string value,int locale)
        {
            var catalog=game.localization.catalog;string key=LocalizationService.Key(scene,"displayName");
            var text=catalog.Find(key);
            if(text==null){Edit("Add scene title",catalog);CreateText(key);text=catalog.Find(key);text.thai=scene.displayName;}
            SetText(new LocalizedTextRef{catalog=catalog,key=key},locale==0?value:text.thai,locale==1?value:text.english);
        }
        public string SetEnabled(EventDataSO story,bool enabled)
        {
            if(!enabled&&game.romance!=null&&game.romance.moodOutcomes.Any(o=>o.eventId==story.id))
                return "Remove or reassign this scene's completion outcomes before disabling it.";
            if(enabled)
            {
                var candidate=Instantiate(game);candidate.events=game.events.Concat(new[]{story}).Distinct().ToArray();
                try{var errors=ProjectLove.Simulation.EventScheduler.ValidateCatalog(candidate);if(errors.Count>0)return string.Join("\n",errors);}
                finally{DestroyImmediate(candidate);}
            }
            Edit(enabled?"Enable scene":"Disable scene",game);
            game.events=enabled?game.events.Concat(new[]{story}).Distinct().ToArray():game.events.Where(e=>e!=story).ToArray();return null;
        }
        public static void AddRuntimeAliases(IEnumerable<IdentifiedDataSO> data)
        {
            foreach(var node in data.OfType<DialogueNodeSO>())
            {
                void Alias(LocalizedTextRef text,string key)
                {
                    if(text.catalog==null||text.key==key)return;
                    var original=text.catalog.Find(text.key);if(original==null)return;
                    if(text.catalog.Find(key)!=null)throw new InvalidOperationException("Runtime text-key collision: "+key);
                    var alias=JsonUtility.FromJson<LocalizationEntry>(JsonUtility.ToJson(original));alias.key=key;
                    text.catalog.entries=text.catalog.entries.Concat(new[]{alias}).ToArray();
                }
                foreach(var line in node.lines)Alias(line.localizedText,LocalizationService.LineKey(node,line.lineId));
                foreach(var choice in node.choices)Alias(choice.localizedText,LocalizationService.ChoiceKey(node,choice.choiceId));
            }
        }
    }
}
