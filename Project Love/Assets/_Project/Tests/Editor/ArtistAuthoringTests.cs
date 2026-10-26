using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.UI;
using ProjectLove.EditorTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace ProjectLove.Tests
{
    public sealed class ArtistAuthoringTests
    {
        const string Temp="Assets/_Project/Tests/Editor/AuthoringTestTemp";
        GameConfigSO source,game;RomanceConfigSO rules;
        [SetUp]public void Setup()
        {
            source=AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath);
            if(!AssetDatabase.IsValidFolder(Temp))AssetDatabase.CreateFolder("Assets/_Project/Tests/Editor","AuthoringTestTemp");
            game=UnityEngine.Object.Instantiate(source);game.id="authoring_test_campaign";AssetDatabase.CreateAsset(game,Temp+"/Game.asset");
            rules=UnityEngine.Object.Instantiate(source.romance);rules.id="authoring_test_rules";AssetDatabase.CreateAsset(rules,Temp+"/Rules.asset");game.romance=rules;
        }
        [TearDown]public void Teardown(){if(AssetDatabase.IsValidFolder(Temp))AssetDatabase.DeleteAsset(Temp);}
        [Test]public void SearchFindsThaiBodyAndFiltersActualCharacterAndActivityWithoutLegacyNodes()
        {
            var entries=AuthoringCatalog.Entries(source);var c=source.romance.cast[1];string phrase=c.personalScene.lines[0].text.Substring(0,12);
            Assert.That(AuthoringCatalog.Search(entries,phrase,c.character,"เวลาส่วนตัว").Any(e=>e.node==c.personalScene),Is.True);
            Assert.That(AuthoringCatalog.Search(entries,phrase,source.characters[0],"เวลาส่วนตัว"),Is.Empty);
            Assert.That(entries.All(e=>AssetDatabase.GetAssetPath(e.node).Contains("/Romance/")),Is.True);Assert.That(entries.Select(e=>e.node).Distinct().Count(),Is.EqualTo(38));
            Assert.That(AuthoringCatalog.Reachable(source).Count,Is.EqualTo(55));
        }
        [Test]public void TextEditSaveReloadAndRuntimeUseTheSameNodeWithoutChangingStableIds()
        {
            var node=UnityEngine.Object.Instantiate(rules.cast[0].personalScene);node.id="authoring_test_node";node.lines[0].lineId="authoring_test_line";AssetDatabase.CreateAsset(node,Temp+"/Node.asset");rules.cast[0].personalScene=node;
            string guid=AssetDatabase.AssetPathToGUID(Temp+"/Node.asset");var so=new SerializedObject(node);so.FindProperty("lines").GetArrayElementAtIndex(0).FindPropertyRelative("text").stringValue="บททดสอบที่แก้แล้ว";so.ApplyModifiedProperties();AssetDatabase.SaveAssets();AssetDatabase.ImportAsset(Temp+"/Node.asset",ImportAssetOptions.ForceUpdate);
            var loaded=AssetDatabase.LoadAssetAtPath<DialogueNodeSO>(Temp+"/Node.asset");Assert.That(loaded.lines[0].text,Is.EqualTo("บททดสอบที่แก้แล้ว"));Assert.That(loaded.lines[0].lineId,Is.EqualTo("authoring_test_line"));Assert.That(AssetDatabase.AssetPathToGUID(Temp+"/Node.asset"),Is.EqualTo(guid));
            var s=new GameSession(game);s.State.completedEvents.Add(game.events.First(e=>e.delivery==EventDelivery.Forced).id);s.Interactions.TryOpen(s.CurrentRoom.hotspots.First(h=>h.character==game.characters[0]));Assert.That(s.Romance.TryStart(RomanceActivity.PersonalTime,game.characters[0].id),Is.True);Assert.That(s.Dialogue.CurrentLine.text,Is.EqualTo("บททดสอบที่แก้แล้ว"));
        }
        [Test]public void AddingAndReorderingLinesKeepsExistingIdsAndSupportsUndo()
        {
            var node=UnityEngine.Object.Instantiate(rules.cast[0].personalScene);AssetDatabase.CreateAsset(node,Temp+"/Node.asset");string[] before=node.lines.Select(l=>l.lineId).ToArray();Undo.IncrementCurrentGroup();
            AuthoringCatalog.AddLine(node,source.characters[0]);Undo.FlushUndoRecordObjects();Assert.That(node.lines.Length,Is.EqualTo(before.Length+1));Assert.That(node.lines.Last().lineId,Is.Not.Empty);Assert.That(node.lines.Select(l=>l.lineId).Distinct().Count(),Is.EqualTo(node.lines.Length));
            Undo.PerformUndo();Assert.That(node.lines.Select(l=>l.lineId),Is.EqualTo(before));
            var so=new SerializedObject(node);so.FindProperty("lines").MoveArrayElement(0,1);so.ApplyModifiedProperties();Assert.That(node.lines[1].lineId,Is.EqualTo(before[0]));
        }
        [Test]public void DraftCannotEnterCampaignUntilPlayableThenRegistersWithUndo()
        {
            var draft=AuthoringCatalog.CreateDraft(game,"ทดสอบอีเว้น",game.characters[0]);Assert.That(game.events.Contains(draft),Is.False);Assert.That(AuthoringCatalog.Drafts(game),Does.Contain(draft));Assert.That(AuthoringCatalog.Publish(game,draft),Is.Not.Empty);
            draft.entryNode.lines[0].text="บททดสอบ";EditorUtility.SetDirty(draft.entryNode);Undo.IncrementCurrentGroup();Assert.That(AuthoringCatalog.Publish(game,draft),Is.Empty);Assert.That(game.events.Contains(draft),Is.True);Assert.That(AuthoringCatalog.Entries(game).Any(e=>e.node==draft.entryNode),Is.True);Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.That(game.events.Contains(draft),Is.False);
        }
        [Test]public void RoomPlacementEditsPersistPerPhaseAndUndoWithoutMutatingOtherRooms()
        {
            var room=UnityEngine.Object.Instantiate(source.rooms[0]);AssetDatabase.CreateAsset(room,Temp+"/Room.asset");var h=room.hotspots.First(h=>h.character!=null);var morning=h.placements.First(p=>p.phase==DayPhase.Morning);var afternoon=h.placements.First(p=>p.phase==DayPhase.Daytime);Vector2 old=morning.position,other=afternoon.position;
            Undo.IncrementCurrentGroup();RoomPlacementEditor.SetPlacement(room,h.id,DayPhase.Morning,new Vector2(.2f,.3f),new Vector2(.18f,.24f));Undo.FlushUndoRecordObjects();Assert.That(morning.position,Is.EqualTo(new Vector2(.2f,.3f)));Undo.PerformUndo();Assert.That(room.hotspots.First(o=>o.id==h.id).placements.First(p=>p.phase==DayPhase.Morning).position,Is.EqualTo(old));
            RoomPlacementEditor.SetPlacement(room,h.id,DayPhase.Morning,new Vector2(.23f,.34f),new Vector2(.19f,.25f));AssetDatabase.SaveAssetIfDirty(room);AssetDatabase.ImportAsset(Temp+"/Room.asset",ImportAssetOptions.ForceUpdate);var loaded=AssetDatabase.LoadAssetAtPath<RoomDataSO>(Temp+"/Room.asset");Assert.That(loaded.hotspots.First(o=>o.id==h.id).placements.First(p=>p.phase==DayPhase.Morning).position,Is.EqualTo(new Vector2(.23f,.34f)));Assert.That(loaded.hotspots.First(o=>o.id==h.id).placements.First(p=>p.phase==DayPhase.Daytime).position,Is.EqualTo(other));
        }
        [Test]public void SavedArtistLayoutSurvivesBuilderReopenAndUsesNestedScreenPrefabs()
        {
            string path=ArtistWorkspaceBuilder.PrefabPath;byte[] bytes=File.ReadAllBytes(path);var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var v=root.GetComponent<IsometricRoomView>();var expected=v.dialoguePortrait.rectTransform.anchoredPosition+new Vector2(23,11);v.dialoguePortrait.rectTransform.anchoredPosition=expected;PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);root=null;
                ArtistWorkspaceBuilder.BuildFromCommandLine();var loaded=PrefabUtility.LoadPrefabContents(path);try{Assert.That(loaded.GetComponent<IsometricRoomView>().dialoguePortrait.rectTransform.anchoredPosition,Is.EqualTo(expected));}finally{PrefabUtility.UnloadPrefabContents(loaded);}
                Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/RomanceDialogue.prefab"),Is.Not.Null);
            }
            finally{if(root!=null)PrefabUtility.UnloadPrefabContents(root);File.WriteAllBytes(path,bytes);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);}
        }
        [TestCase(AuthoringScreen.Room)][TestCase(AuthoringScreen.Conversation)][TestCase(AuthoringScreen.WeeklyFocus)][TestCase(AuthoringScreen.MorningRecap)]
        public void PreviewRendersSamePrefabWithNoSourceOrActualSessionMutation(AuthoringScreen screen)
        {
            var actual=new GameSession(source);string state=JsonUtility.ToJson(actual.State);var definitions=AuthoringCatalog.Reachable(source).ToDictionary(a=>a,a=>JsonUtility.ToJson(a));var active=SceneManager.GetActiveScene().handle;
            using(var p=new AuthoringPreview(source,screen))
            {
                Assert.That(p.View.IsEditorPreview,Is.True);Assert.That(p.View.Saves,Is.Null);Assert.That(p.View.Authored,Is.Not.Null);
                var tex=p.Render();var rt=RenderTexture.GetTemporary(1280,720,0);Graphics.Blit(tex,rt);var old=RenderTexture.active;RenderTexture.active=rt;var png=new Texture2D(1280,720,TextureFormat.RGB24,false);
                try{png.ReadPixels(new Rect(0,0,1280,720),0,0);png.Apply();Directory.CreateDirectory("Logs");File.WriteAllBytes("Logs/Session044_Editor_"+screen+".png",png.EncodeToPNG());Assert.That(png.GetPixels().Count(c=>c.maxColorComponent>.1f),Is.GreaterThan(30000));}finally{RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(png);}
            }
            Assert.That(SceneManager.GetActiveScene().handle,Is.EqualTo(active));Assert.That(JsonUtility.ToJson(actual.State),Is.EqualTo(state));foreach(var pair in definitions)Assert.That(JsonUtility.ToJson(pair.Key),Is.EqualTo(pair.Value),pair.Key.name);
        }
        [UnityTest]public IEnumerator WorkspaceWindowOpensAndRepaintsWithoutEditorErrors()
        {
            DesignerDashboard.OpenNode(source.events[0].entryNode);yield return null;var w=EditorWindow.GetWindow<DesignerDashboard>();Assert.That(new SerializedObject(w).FindProperty("selectedEvent").objectReferenceValue,Is.EqualTo(source.events[0]));
            DesignerDashboard.OpenNode(source.romance.cast[0].personalScene);yield return null;var so=new SerializedObject(w);Assert.That(so.FindProperty("selectedNode").objectReferenceValue,Is.EqualTo(source.romance.cast[0].personalScene));Assert.That(so.FindProperty("selectedEvent").objectReferenceValue,Is.Null);
            typeof(DesignerDashboard).GetField("tab",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(w,1);w.Repaint();yield return null;yield return null;w.Close();LogAssert.NoUnexpectedReceived();
        }
        [Test]public void ConversationGroupsCreatePersistSearchAndProbeWithoutChangingOriginalCharacter()
        {
            var original=source.characters[1];string before=JsonUtility.ToJson(original);var c=UnityEngine.Object.Instantiate(original);AssetDatabase.CreateAsset(c,Temp+"/Cast.asset");
            var group=ConversationPoolEditor.AddGroup(game,c,true);group.minimumAffection=30;group.maximumAffection=100;group.node.lines[0].text="บทสนิททดสอบ";EditorUtility.SetDirty(group.node);
            var extra=ConversationPoolEditor.AddAlternative(c,group,true);extra.lines[0].text="อีกประโยคทดสอบ";EditorUtility.SetDirty(extra);EditorUtility.SetDirty(c);AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(Temp+"/Cast.asset",ImportAssetOptions.ForceUpdate);c=AssetDatabase.LoadAssetAtPath<CharacterDataSO>(Temp+"/Cast.asset");Assert.That(c.greetings.Last().Nodes.Count(),Is.EqualTo(2));
            string low=ConversationPoolEditor.Probe(game,c,true,29,1,DayPhase.Morning,"","",out var lowNode);Assert.That(lowNode,Is.Not.EqualTo(extra));Assert.That(low,Does.Contain("ความสนิทต้องอยู่ที่ 30–100"));
            ConversationPoolEditor.Probe(game,c,true,30,1,DayPhase.Morning,"","",out var highNode);Assert.That(c.greetings.Last().Nodes,Does.Contain(highNode));Assert.That(JsonUtility.ToJson(original),Is.EqualTo(before));
            game.characters=game.characters.Select(x=>x==original?c:x).ToArray();Assert.That(AuthoringCatalog.Entries(game).Any(e=>e.node==extra),Is.True);
        }
        [Test]public void SelectedEventPreviewKeepsDateAndForcedPriorityChecksAndDoesNotMutateSource()
        {
            var e=source.events.First(e=>e.delivery==EventDelivery.CharacterBubble);string before=JsonUtility.ToJson(e);
            using(var p=new AuthoringPreview(source,AuthoringScreen.Conversation,1,story:e))
            {Assert.That(p.Session.Dialogue.IsActive,Is.False);Assert.That(p.Notice,Does.Contain("วันไม่อยู่ในช่วง"));Assert.That(p.Notice,Does.Contain("อีเว้นอัตโนมัติรอก่อน"));}
            using(var p=new AuthoringPreview(source,AuthoringScreen.Conversation,e.preferredDay,character:e.character,story:e,mood:CharacterMood.Tired))
            {
                Assert.That(p.Notice,Is.Null);Assert.That(p.Session.Dialogue.ActiveEvent,Is.EqualTo(e));Assert.That(p.Session.Romance.Mood(e.character.id),Is.EqualTo(CharacterMood.Tired));
                p.Advance();Assert.That(p.Session.Dialogue.ChoicesReady,Is.True);p.Choose(e.entryNode.choices[0]);p.Advance();Assert.That(p.Session.State.completedEvents,Does.Contain(e.id));
            }
            Assert.That(JsonUtility.ToJson(e),Is.EqualTo(before));
        }
    }
}
