using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.StorySandbox
{
    [Serializable]
    public sealed class DraftText { public string catalogGuid,key,thai,english; }

    // Lives only in the generated project. Catalog GUID + stable key survives source reimport.
    public sealed class StoryDraft : ScriptableObject
    {
        public List<DraftText> texts = new List<DraftText>();
        public DraftText Find(LocalizedTextRef text) => texts.FirstOrDefault(t => t.key == text.key && t.catalogGuid == Guid(text));
        static string Guid(LocalizedTextRef text) => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(text.catalog));
        public string Thai(LocalizedTextRef text,string fallback) => Find(text)?.thai ?? text.Source(fallback);
        public string English(LocalizedTextRef text) => Find(text)?.english ?? text.catalog?.Find(text.key)?.english ?? "";
        public void Set(LocalizedTextRef text,string thai,string english)
        {
            if(text.catalog == null || string.IsNullOrEmpty(text.key)) throw new InvalidOperationException("บทนี้ยังไม่มีคีย์ภาษา");
            Undo.RecordObject(this,"แก้ข้อความฉบับทดลอง");
            var row=Find(text);
            if(row==null){row=new DraftText{catalogGuid=Guid(text),key=text.key};texts.Add(row);}
            row.thai=thai;row.english=english;EditorUtility.SetDirty(this);
        }
        public static StoryDraft Load()
        {
            const string folder="Assets/_Project/StorySandboxDrafts";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/_Project","StorySandboxDrafts");
            var draft=AssetDatabase.LoadAssetAtPath<StoryDraft>(folder+"/TextDraft.asset");
            if(draft!=null)return draft;
            draft=CreateInstance<StoryDraft>();AssetDatabase.CreateAsset(draft,folder+"/TextDraft.asset");return draft;
        }
    }

    // Deep detached clone of the reachable definitions. Remap catalog and graph references before applying drafts.
    public sealed class DraftPreview : IDisposable
    {
        readonly Dictionary<IdentifiedDataSO,IdentifiedDataSO> clones = new Dictionary<IdentifiedDataSO,IdentifiedDataSO>();
        public ProjectLove.EditorTools.AuthoringPreview view;
        public DraftPreview(GameConfigSO game,DialogueNodeSO node,StoryDraft draft,int day,DayPhase phase,string locale)
        {
            try
            {
                foreach(var a in ProjectLove.EditorTools.AuthoringCatalog.Reachable(game))
                { var clone=UnityEngine.Object.Instantiate(a);clone.hideFlags=HideFlags.HideAndDontSave;clones.Add(a,clone); }
                foreach(var a in clones.Values)
                {
                    var so=new SerializedObject(a);var p=so.GetIterator();
                    while(p.NextVisible(true)) if(p.propertyType==SerializedPropertyType.ObjectReference &&
                        p.objectReferenceValue is IdentifiedDataSO source && clones.TryGetValue(source,out var clone))p.objectReferenceValue=clone;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                foreach(var pair in clones.Where(p=>p.Key is LocalizationCatalogSO))
                {
                    string guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(pair.Key));
                    var catalog=(LocalizationCatalogSO)pair.Value;
                    foreach(var row in draft.texts.Where(t=>t.catalogGuid==guid))
                    {var e=catalog.Find(row.key);if(e!=null){e.thai=row.thai;e.english=row.english;}}
                }
                view=new ProjectLove.EditorTools.AuthoringPreview((GameConfigSO)clones[game],ProjectLove.UI.AuthoringScreen.Conversation,
                    day,phase,node:(DialogueNodeSO)clones[node]);
                view.Session.Localization.SetLocale(locale);view.View.RevealAll();
            }
            catch{Dispose();throw;}
        }
        public void Dispose()
        {
            view?.Dispose();view=null;
            foreach(var a in clones.Values)UnityEngine.Object.DestroyImmediate(a);clones.Clear();
        }
    }
}
