using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.EditorTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProjectLove.StorySandbox.Tests
{
    public sealed class StoryWorkspaceTests
    {
        GameConfigSO game;
        StoryIndex index;
        StoryDraft draft;
        [SetUp] public void Setup()
        {
            Assert.IsTrue(StoryWorkspaceWindow.IsSandbox,"Never run the prototype in the production project.");
            game=AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath);
            Assert.NotNull(game);index=new StoryIndex(game);draft=ScriptableObject.CreateInstance<StoryDraft>();
        }
        [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(draft);}

        [Test] public void OpeningManuscriptIncludesAllResponsesAndSharedJoin()
        {
            var start=game.events.Single(e=>e.id=="romance_welcome").entryNode;
            var rows=index.Manuscript(start);
            Assert.AreEqual(8,rows.Count(r=>r.node==start&&r.line!=null));
            Assert.AreEqual(3,rows.Count(r=>r.node==start&&r.choice!=null));
            foreach(var c in start.choices)Assert.IsTrue(rows.Any(r=>r.node==c.nextNode&&r.line!=null));
            var join=start.choices[0].nextNode.nextNode;
            Assert.NotNull(join);Assert.AreEqual(join.lines.Length,rows.Count(r=>r.node==join&&r.line!=null));
            Assert.AreEqual(2,rows.Count(r=>r.node==join&&r.reference));
            Assert.AreEqual(3,index.Incoming(join).Count());
            var entry=index.entries.Single(e=>e.story!=null&&e.story.id=="romance_welcome");
            Assert.AreEqual(3,entry.Speakers.Count());
        }
        [Test] public void AllRegisteredCallersRemainSeparateAndSearchFindsExactLocalizedLine()
        {
            foreach(var n in index.nodes)
            {
                foreach(var edge in index.Incoming(n))Assert.AreSame(n,edge.to);
                foreach(var caller in index.Callers(n))Assert.Contains(n,caller.nodes.ToArray());
            }
            var start=game.events.Single(e=>e.id=="romance_welcome").entryNode;
            var line=start.lines.First(l=>l.speaker!=null);
            draft.Set(line.localizedText,"ข้อความทดสอบเฉพาะฉบับทดลอง","Unique isolated English search");
            Assert.IsTrue(StoryIndex.Matches(start,"เฉพาะฉบับทดลอง",draft));
            Assert.IsTrue(StoryIndex.Matches(start,"isolated English",draft));
            Assert.IsTrue(StoryIndex.Matches(start,line.speaker.displayName,draft));
            Assert.IsFalse(start.lines.Any(l=>l.SourceText=="ข้อความทดสอบเฉพาะฉบับทดลอง"));
            Assert.IsTrue(index.entries.Any(e=>e.variant!=null&&e.gates.Contains("ความเครียด")));
        }
        [Test] public void DraftUndoSaveReloadNeverWritesSourceCatalog()
        {
            string folder="Assets/_Project/StorySandboxDrafts";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/_Project","StorySandboxDrafts");
            string path=folder+"/test_"+Guid.NewGuid().ToString("N")+".asset";
            var line=game.events.Single(e=>e.id=="romance_welcome").entryNode.lines[0];
            string catalog=AssetDatabase.GetAssetPath(line.localizedText.catalog);
            byte[] before=File.ReadAllBytes(catalog);
            AssetDatabase.CreateAsset(draft,path);
            try
            {
                Undo.IncrementCurrentGroup();draft.Set(line.localizedText,"ฉบับทดลองที่บันทึกได้","Saved sandbox draft");
                Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.AreEqual(0,draft.texts.Count);
                Undo.PerformRedo();Assert.AreEqual(1,draft.texts.Count);
                AssetDatabase.SaveAssetIfDirty(draft);
                var loaded=AssetDatabase.LoadAssetAtPath<StoryDraft>(path);
                Assert.AreEqual("ฉบับทดลองที่บันทึกได้",loaded.Thai(line.localizedText,line.SourceText));
                CollectionAssert.AreEqual(before,File.ReadAllBytes(catalog));
                Assert.IsFalse(EditorUtility.IsDirty(line.localizedText.catalog));
            }
            finally{Undo.ClearUndo(draft);AssetDatabase.DeleteAsset(path);}
        }
        [Test] public void SharedPassageRetainsAlternativeRootGatesAndCyclesTerminate()
        {
            var copy=UnityEngine.Object.Instantiate(game);
            var a=ScriptableObject.CreateInstance<EventDataSO>();var b=ScriptableObject.CreateInstance<EventDataSO>();
            var start=ScriptableObject.CreateInstance<DialogueNodeSO>();var join=ScriptableObject.CreateInstance<DialogueNodeSO>();
            try
            {
                a.id="test_root_a";b.id="test_root_b";start.id="test_start";join.id="test_join";
                a.entryNode=start;b.entryNode=join;
                a.conditions=new[]{new Condition{type=ConditionType.FlagSet,flagId="root_a_only"}};
                b.conditions=new[]{new Condition{type=ConditionType.FlagSet,flagId="root_b_only"}};
                start.choices=new[]{new DialogueChoice{choiceId="a",nextNode=join},new DialogueChoice{choiceId="b",nextNode=join}};
                join.nextNode=start;copy.events=new[]{a,b};
                var graph=new StoryIndex(copy);var roots=graph.Callers(join).Where(e=>e.story==a||e.story==b).ToArray();
                Assert.AreEqual(2,roots.Length);
                Assert.IsTrue(roots.Single(e=>e.story==a).gates.Contains("root_a_only"));
                Assert.IsFalse(roots.Single(e=>e.story==a).gates.Contains("root_b_only"));
                Assert.IsTrue(roots.Single(e=>e.story==b).gates.Contains("root_b_only"));
                Assert.AreEqual(2,graph.Incoming(join).Count());
                Assert.AreEqual(2,graph.Manuscript(start).Count(r=>r.reference));
            }
            finally{foreach(var o in new UnityEngine.Object[]{copy,a,b,start,join})UnityEngine.Object.DestroyImmediate(o);}
        }
        [Test] public void DetachedDraftPreviewUsesSameGameViewAndPreservesSourceAndPreferences()
        {
            var start=game.events.Single(e=>e.id=="romance_welcome").entryNode;
            var paths=AuthoringCatalog.Reachable(game).Select(AssetDatabase.GetAssetPath).Where(File.Exists).Distinct().ToArray();
            var before=paths.ToDictionary(p=>p,File.ReadAllBytes);
            string pref=EditorPrefs.GetString("ProjectLove.StorySandbox.TestUnrelated","unchanged");
            string playerPref=PlayerPrefs.GetString(LocalizationService.PreferenceKey,"<missing>");
            draft.Set(start.lines[0].localizedText,"คำบรรยายทดลองในหน้าจอเกมเดิม","");
            using(var preview=new DraftPreview(game,start,draft,1,DayPhase.Morning,"en"))
            {
                Assert.AreEqual("คำบรรยายทดลองในหน้าจอเกมเดิม",preview.view.Session.Dialogue.CurrentLine.SourceText);
                Assert.AreEqual("คำบรรยายทดลองในหน้าจอเกมเดิม",preview.view.Session.Localization.Line(preview.view.Session.Dialogue.Node,preview.view.Session.Dialogue.CurrentLine));
                Assert.AreNotSame(start,preview.view.Session.Dialogue.Node);
                Assert.NotNull(preview.view.Render(960,540));
            }
            foreach(var path in paths)CollectionAssert.AreEqual(before[path],File.ReadAllBytes(path),path);
            Assert.AreEqual(playerPref,PlayerPrefs.GetString(LocalizationService.PreferenceKey,"<missing>"));
            Assert.AreEqual(pref,EditorPrefs.GetString("ProjectLove.StorySandbox.TestUnrelated","unchanged"));
        }
        [UnityTest] public IEnumerator WindowOpensWithOpeningAndRepaints()
        {
            StoryWorkspaceWindow.Open();yield return null;yield return null;
            var window=Resources.FindObjectsOfTypeAll<StoryWorkspaceWindow>().Single();
            window.position=new Rect(30,50,1440,900);window.Repaint();yield return null;
            Assert.NotNull(window);window.Close();LogAssert.NoUnexpectedReceived();
        }
    }
}
