using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.EditorTools;
using ProjectLove.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProjectLove.Tests
{
    public sealed class LocalizationTests
    {
        GameConfigSO game;
        LocalizationSettingsSO settings;
        LocalizationCatalogSO catalog;
        [SetUp] public void Setup()
        {
            game=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath));
            Assert.That(game.localization,Is.Not.Null,"Install localization before these focused checks.");
            settings=UnityEngine.Object.Instantiate(game.localization);catalog=UnityEngine.Object.Instantiate(settings.catalog);settings.catalog=catalog;game.localization=settings;
        }
        [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(game);UnityEngine.Object.DestroyImmediate(settings);UnityEngine.Object.DestroyImmediate(catalog);}
        void Idle(GameSession s){foreach(var e in game.events.Where(e=>e.delivery==EventDelivery.Forced&&e.startDay<=s.State.day))s.State.completedEvents.Add(e.id);}
        [Test] public void LookupFallbackStalenessAndCsvProtectSourceAndRoundtripMultiline()
        {
            var e=catalog.entries.First();e.thai="สวัสดี {0}\n\"โลก\"";e.english="Hello {0}\n\"world\"";e.englishReviewedRevision=0;
            var service=new LocalizationService(settings);service.SetLocale("en");Assert.That(service.Resolve(e.key).stale,Is.True);Assert.That(service.Text(e.key),Is.EqualTo(e.english));
            string csv=LocalizationCsv.Export(catalog);var plan=LocalizationCsv.Preview(catalog,csv);Assert.That(plan.errors,Is.Empty);Assert.That(plan.changes,Is.Empty);
            var rows=LocalizationCsv.Parse(csv);Assert.That(rows[1][1],Is.EqualTo(e.thai));Assert.That(rows[1][2],Is.EqualTo(e.english));
            var changed=LocalizationCsv.Preview(catalog,csv.Replace("Hello {0}","Hi {0}"));Assert.That(changed.errors,Is.Empty);Assert.That(changed.changes.Count,Is.EqualTo(1));LocalizationCsv.Apply(catalog,changed);Assert.That(e.english,Does.StartWith("Hi"));
            Assert.That(LocalizationService.ValidateTranslation(e.thai,"Hello {1}"),Is.Not.Null);
            Assert.That(LocalizationService.Format("{oops}",1),Is.EqualTo("[invalid text format]"));
            var stale=LocalizationCsv.Preview(catalog,LocalizationCsv.Export(catalog));e.sourceRevision++;Assert.Throws<InvalidOperationException>(()=>LocalizationCsv.Apply(catalog,stale));
            Assert.That(LocalizationCsv.Preview(catalog,csv).errors,Is.Not.Empty);
            e.english="";Assert.That(service.Resolve(e.key).fallback,Is.True);Assert.That(service.Text(e.key),Is.EqualTo(e.thai));service.SetLocale("unknown");Assert.That(service.Locale,Is.EqualTo("th"));
            Assert.That(service.Resolve("unknown").missing,Is.True);
        }
        [Test] public void TranslationChangesPreserveSavesButMechanicalChangesAreRejectedAndV8IsRetained()
        {
            string dir=Path.Combine("Logs","LocalizationSaveTest_"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
            try
            {
                var store=new SaveStore(dir);var legacy=UnityEngine.Object.Instantiate(game);legacy.localization=null;
                try
                {
                    var old=new GameSession(legacy);Idle(old);old.State.readLines.Add("read_before_migration");Assert.That(store.Save(old,SaveSlot.Manual),Is.EqualTo(SaveResult.Success));
                    string oldPath=store.PathFor(old,SaveSlot.Manual);byte[] bytes=File.ReadAllBytes(oldPath);
                    var s=new GameSession(game);Assert.That(store.PathFor(s,SaveSlot.Manual),Is.Not.EqualTo(oldPath));Assert.That(store.Load(s,SaveSlot.Manual),Is.EqualTo(SaveResult.Success));Assert.That(s.State.readLines,Does.Contain("read_before_migration"));
                    Assert.That(File.ReadAllBytes(oldPath),Is.EqualTo(bytes));Assert.That(store.Save(s,SaveSlot.Manual),Is.EqualTo(SaveResult.Success));
                    catalog.entries[0].thai+=" edited";catalog.entries[0].english="Translation revised";var next=new GameSession(game);next.Localization.SetLocale("en");Assert.That(store.Load(next,SaveSlot.Manual),Is.EqualTo(SaveResult.Success));
                    game.startingMoney++;Assert.That(store.Load(new GameSession(game),SaveSlot.Manual),Is.EqualTo(SaveResult.Invalid));Assert.That(File.ReadAllBytes(oldPath),Is.EqualTo(bytes));
                }
                finally{UnityEngine.Object.DestroyImmediate(legacy);}
            }
            finally{foreach(var f in Directory.GetFiles(dir))File.Delete(f);Directory.Delete(dir);}
        }
        [Test] public void SourceEditsUndoAndPersistWithoutRewritingFrozenFields()
        {
            const string folder="Assets/_Project/Tests/Editor/LocalizationTemp";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/_Project/Tests/Editor","LocalizationTemp");
            var saved=UnityEngine.Object.Instantiate(catalog);AssetDatabase.CreateAsset(saved,folder+"/Catalog.asset");
            try
            {
                string legacy=saved.entries[0].legacy,thai=saved.entries[0].thai;int revision=saved.entries[0].sourceRevision;
                Undo.IncrementCurrentGroup();LocalizationAuthoring.SetText(saved,saved.entries[0],thai+" ทดสอบ","",false);Undo.FlushUndoRecordObjects();Undo.PerformUndo();
                Assert.That(saved.entries[0].thai,Is.EqualTo(thai));Assert.That(saved.entries[0].sourceRevision,Is.EqualTo(revision));
                LocalizationAuthoring.SetText(saved,saved.entries[0],thai+" ทดสอบ","",false);AssetDatabase.SaveAssetIfDirty(saved);AssetDatabase.ImportAsset(folder+"/Catalog.asset",ImportAssetOptions.ForceUpdate);
                var loaded=AssetDatabase.LoadAssetAtPath<LocalizationCatalogSO>(folder+"/Catalog.asset");Assert.That(loaded.entries[0].thai,Does.EndWith("ทดสอบ"));Assert.That(loaded.entries[0].legacy,Is.EqualTo(legacy));
            }
            finally{AssetDatabase.DeleteAsset(folder);}
            var node=ScriptableObject.CreateInstance<DialogueNodeSO>();node.id="new_localized_node";node.lines=new[]{new DialogueLine{lineId="new_localized_line",text=""}};
            try
            {
                LocalizationAuthoring.BindNode(game,node);var entry=catalog.Find(LocalizationService.LineKey(node,node.lines[0].lineId));LocalizationAuthoring.SetText(catalog,entry,"บทใหม่จากตารางภาษา","",false);
                Assert.That(ProjectLove.VN.DialogueRunner.IsPlayable(node),Is.True);Assert.That(node.lines[0].text,Is.Empty);Assert.That(node.lines[0].SourceText,Is.EqualTo("บทใหม่จากตารางภาษา"));
            }
            finally{UnityEngine.Object.DestroyImmediate(node);}
        }
        [Test] public void RealPrefabSwitchesActiveLineChoicesAndGreetingWithoutGameplayMutation()
        {
            var root=game.events.First(e=>e.delivery==EventDelivery.Forced).entryNode;
            using(var p=new AuthoringPreview(game,AuthoringScreen.Conversation,node:root))
            {
                var portrait=p.View.dialoguePortrait.rectTransform;var pos=portrait.anchoredPosition;string state=JsonUtility.ToJson(p.Session.State);string thai=p.View.dialogueText.text;
                p.Session.Localization.SetLocale("en");Assert.That(p.View.dialogueText.text,Is.Not.EqualTo(thai));Assert.That(JsonUtility.ToJson(p.Session.State),Is.EqualTo(state));Assert.That(p.View.IsRevealing,Is.False);Assert.That(portrait.anchoredPosition,Is.EqualTo(pos));
                Capture(p,"Session064_EnglishLine");
                while(!p.Session.Dialogue.ChoicesReady)p.Advance();
                state=JsonUtility.ToJson(p.Session.State);p.Session.Localization.SetLocale("th");Assert.That(p.View.dialogueText.text,Is.EqualTo(root.lines.Last().SourceText));Capture(p,"Session064_ThaiChoices");p.Session.Localization.SetLocale("en");
                Assert.That(JsonUtility.ToJson(p.Session.State),Is.EqualTo(state));
                foreach(var choice in root.choices)Assert.That(p.View.choiceButtons.GetComponentsInChildren<TMPro.TMP_Text>().Any(t=>t.text==p.Session.Localization.Choice(root,choice)),Is.True);
                Capture(p,"Session064_EnglishChoices");
            }
            using(var p=new AuthoringPreview(game,AuthoringScreen.Conversation,character:game.characters[0]))
            {string thai=p.View.dialogueText.text;p.Session.Localization.SetLocale("en");Assert.That(p.View.dialogueText.text,Is.Not.EqualTo(thai));Capture(p,"Session064_EnglishGreeting");}
        }
        static void Capture(AuthoringPreview p,string name)
        {
            var texture=p.Render();var rt=RenderTexture.GetTemporary(1280,720,0);Graphics.Blit(texture,rt);var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try{image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();Assert.That(image.GetPixels().Count(c=>c.maxColorComponent>.1f),Is.GreaterThan(30000));Directory.CreateDirectory("Logs");File.WriteAllBytes("Logs/"+name+".png",image.EncodeToPNG());}
            finally{RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(image);}
        }
        [UnityTest] public IEnumerator WorkspaceTranslationTabAndDetachedLanguagePreviewOpen()
        {
            var source=AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath);DesignerDashboard.OpenTranslation(source,source.localization.catalog.entries[0].key);
            yield return null;var window=EditorWindow.GetWindow<DesignerDashboard>();window.Repaint();yield return null;yield return null;window.Close();LogAssert.NoUnexpectedReceived();
        }
    }
}
