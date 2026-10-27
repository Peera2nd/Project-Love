using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectLove.Data;
using ProjectLove.EditorTools;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.StorySandbox.Tests
{
    public sealed class UnifiedWorkspaceTests
    {
        WorkspaceProject project;GameConfigSO source;string path;byte[] sourceCatalog;
        [SetUp] public void Setup()
        {
            Assert.IsTrue(StoryWorkspaceWindow.IsSandbox);
            source=AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath);
            sourceCatalog=File.ReadAllBytes(AssetDatabase.GetAssetPath(source.localization.catalog));
            if(!AssetDatabase.IsValidFolder("Assets/_Project/StorySandboxDrafts"))AssetDatabase.CreateFolder("Assets/_Project","StorySandboxDrafts");
            path="Assets/_Project/StorySandboxDrafts/verify_"+Guid.NewGuid().ToString("N")+".asset";
            project=WorkspaceProject.Create(source,path);
        }
        [TearDown] public void Cleanup()
        {
            CollectionAssert.AreEqual(sourceCatalog,File.ReadAllBytes(AssetDatabase.GetAssetPath(source.localization.catalog)));
            foreach(var a in project.assets.Where(a=>a!=null))Undo.ClearUndo(a);Undo.ClearUndo(project);AssetDatabase.DeleteAsset(path);
        }
        [Test] public void CrossBlockMovePreservesIdentityAndEnglishThroughRuntimeAdapter()
        {
            var opening=project.game.events.Single(e=>e.id=="romance_welcome");var from=opening.entryNode;var to=from.choices[0].nextNode;
            var line=project.AddLine(from,0);string id=line.lineId,key=line.localizedText.key;
            project.SetText(line.localizedText,"บรรทัดทดลองที่ย้ายได้","Moved sandbox line");
            project.Edit("Set performance",from);line.speaker=project.game.characters[0];line.expression=CharacterExpression.Smile;
            project.MoveLine(from,id,to,0);Assert.IsFalse(from.lines.Any(l=>l.lineId==id));
            Assert.AreEqual(id,to.lines[0].lineId);Assert.AreEqual(key,to.lines[0].localizedText.key);Assert.AreEqual(CharacterExpression.Smile,to.lines[0].expression);
            using(var run=new WorkspacePlaytest(project,1,DayPhase.Morning,0,Array.Empty<string>(),Array.Empty<string>()))
            {
                run.Session.Localization.SetLocale("en");Assert.IsTrue(run.Run(new StoryIndex(project.game,project.assets).entries.Single(e=>e.story==opening)));
                for(int i=0;i<from.lines.Length;i++)run.Advance();run.Choose(run.Session.Dialogue.Node.choices[0]);
                var active=run.Session.Dialogue;Assert.AreEqual(id,active.CurrentLine.lineId);
                Assert.AreEqual("Moved sandbox line",run.Session.Localization.Line(active.Node,active.CurrentLine));
                using(var view=new WorkspaceGameView(run.Session))Assert.NotNull(view.Render());
            }
            Assert.AreEqual(8,source.events.Single(e=>e.id=="romance_welcome").entryNode.lines.Length);
        }
        [Test] public void RealChoiceTraceShowsFlagsHistoryCompletionAndRollback()
        {
            var root=new StoryIndex(project.game,project.assets).entries.Single(e=>e.story!=null&&e.story.id=="romance_welcome");
            using(var run=new WorkspacePlaytest(project,1,DayPhase.Morning,0,Array.Empty<string>(),Array.Empty<string>()))
            {
                Assert.IsTrue(run.Run(root));for(int i=0;i<8;i++)run.Advance();var choice=run.Session.Dialogue.Node.choices[0];
                run.Choose(choice);Assert.Contains(choice.choiceId,run.Session.State.chosenOptions);
                foreach(var f in choice.flags.Where(f=>f.value))Assert.Contains(f.flagId,run.Session.State.flags);
                Assert.IsFalse(run.Session.State.completedEvents.Contains(root.story.id));
                Assert.IsTrue(run.trace.Any(s=>s.Contains("chosenOptions[")));
                run.Back();Assert.IsEmpty(run.Session.State.chosenOptions);Assert.IsEmpty(run.Session.State.readLines);
                Assert.IsTrue(run.trace.Any(s=>s.Contains("rollback")));
            }
            using(var run=new WorkspacePlaytest(project,1,DayPhase.Morning,0,Array.Empty<string>(),Array.Empty<string>()))
            {
                Assert.IsTrue(run.Run(root));int guard=0;
                while(run.Session.Dialogue.IsActive&&guard++<100)
                {if(run.Session.Dialogue.ChoicesReady)run.Choose(run.Session.Dialogue.Node.choices.First(run.Session.Dialogue.CanChoose));else run.Advance();}
                Assert.Less(guard,100);Assert.Contains(root.story.id,run.Session.State.completedEvents);
                Assert.IsTrue(run.trace.Any(s=>s.Contains("completedEvents["+root.story.id+"]")));
            }
        }
        [Test] public void NewSceneHasExplicitCallerGateAndSavesStructuralUndo()
        {
            var scene=project.NewScene();Assert.IsFalse(project.game.events.Contains(scene));
            project.SetSceneName(scene,"ฉากเงื่อนไข",0);project.SetSceneName(scene,"Condition Scene",1);
            Assert.AreEqual("ฉากเงื่อนไข",project.Label(scene));
            project.SetText(scene.entryNode.lines[0].localizedText,"ฉากทดสอบเงื่อนไข","Gate test");
            project.Edit("Add test gate",scene);scene.conditions=new[]{new Condition{type=ConditionType.FlagSet,flagId="verify_gate"}};
            Assert.IsNull(project.SetEnabled(scene,true));
            var root=new StoryIndex(project.game,project.assets).entries.Single(e=>e.story==scene);
            using(var blocked=new WorkspacePlaytest(project,1,DayPhase.Morning,0,Array.Empty<string>(),new[]{"romance_welcome"}))
            {Assert.IsFalse(blocked.Run(root));Assert.IsTrue(blocked.checks.Any(c=>c.label=="Condition"&&!c.passed));}
            using(var ready=new WorkspacePlaytest(project,1,DayPhase.Morning,0,new[]{"verify_gate"},new[]{"romance_welcome"}))
            {
                Assert.IsTrue(ready.Run(root));Assert.IsTrue(ready.checks.Any(c=>c.label=="Caller"&&c.passed));
                ready.Session.Localization.SetLocale("en");Assert.AreEqual("Condition Scene",ready.Session.Localization.Content(ready.Copy(scene),"displayName",scene.displayName));
            }
            var node=scene.entryNode;string id=node.lines[0].lineId;
            project.AddLine(node,1);Undo.FlushUndoRecordObjects();Undo.IncrementCurrentGroup();
            project.MoveLine(node,id,node,2);Undo.FlushUndoRecordObjects();Assert.AreEqual(id,node.lines[1].lineId);
            Undo.PerformUndo();Assert.AreEqual(id,node.lines[0].lineId);Undo.PerformRedo();Assert.AreEqual(id,node.lines[1].lineId);
            project.Save();AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
            var loaded=AssetDatabase.LoadAssetAtPath<WorkspaceProject>(path);
            Assert.AreEqual(id,loaded.assets.OfType<EventDataSO>().Single(e=>e.id==scene.id).entryNode.lines[1].lineId);
        }
        [Test] public void ActivityCallerUsesActualCompletionCallback()
        {
            var root=new StoryIndex(project.game,project.assets).entries.First(e=>e.owner is RomanceConfigSO&&e.sourceProperty.EndsWith("sharedScene"));
            Assert.NotNull(root.character);
            using(var run=new WorkspacePlaytest(project,1,DayPhase.Morning,0,Array.Empty<string>(),new[]{"romance_welcome"}))
            {
                int before=run.Session.State.FindCharacter(root.character.id).affection;
                Assert.IsTrue(run.Run(root));Assert.AreEqual(root.node.id,run.Session.Dialogue.Node.id);
                int guard=0;while(run.Session.Dialogue.IsActive&&guard++<100)
                {if(run.Session.Dialogue.ChoicesReady)run.Choose(run.Session.Dialogue.Node.choices.First(run.Session.Dialogue.CanChoose));else run.Advance();}
                Assert.Less(guard,100);
                Assert.AreEqual(before+project.game.romance.sharedAffection,run.Session.State.FindCharacter(root.character.id).affection);
                Assert.IsTrue(run.trace.Any(s=>s.Contains("affection")));
            }
        }
    }
}
