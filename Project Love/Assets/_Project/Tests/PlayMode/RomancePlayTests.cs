using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace ProjectLove.Tests
{
    public sealed class RomancePlayTests:InputTestFixture
    {
        IsometricRoomView view;RomanceRoomView ui;string directory;
        [TearDown]public override void TearDown(){if(directory!=null&&Directory.Exists(directory))Directory.Delete(directory,true);base.TearDown();}
        IEnumerator Open()
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/Gameplay/StudioRomancePrototype.unity",new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
#endif
            for(int i=0;i<5;i++)yield return null;
            view=Object.FindFirstObjectByType<IsometricRoomView>();ui=view.GetComponent<RomanceRoomView>();view.TextSpeed=0;
            directory=Path.GetFullPath(Path.Combine("Logs","RomanceUI",System.Guid.NewGuid().ToString("N")));view.SetSaveDirectory(directory);
            yield return Finish();yield return null;yield return null;
        }
        IEnumerator Finish(){int count=0;while(view.Session.Dialogue.IsActive&&count++<20){view.RevealAll();yield return SandboxPlayTests.ClickWithMouse(view.DialogueTapButton);}Assert.That(view.Session.Dialogue.IsActive,Is.False);}
        Button Choice(int n)=>view.choiceButtons.GetComponentsInChildren<Button>().Where(b=>b.gameObject.activeInHierarchy).ElementAt(n);
        Button Named(string name)=>Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name==name&&b.gameObject.activeInHierarchy);
        IEnumerator Click(Button b)=>SandboxPlayTests.ClickWithMouse(b);
        void Capture(string name){view.RevealAll();IsometricPlayTests.Capture(view,"Logs/Session041_"+name+".png",false);IsometricPlayTests.AssertTextFits();}
        [UnityTest]public IEnumerator ClearActivitiesMoodRosterAndSceneRewardsUseRealPointer()
        {
            yield return Open();Capture("Room");
            Assert.That(ui.ProducerButton.GetComponentInChildren<TMP_Text>().text,Is.Not.Empty);
            Assert.That(ui.FocusButton.GetComponentInChildren<TMP_Text>().text,Is.Not.Empty);
            Assert.That(Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t=>t.text.Contains("วันที่ 1/30")),Is.True);Assert.That(Object.FindFirstObjectByType<PlanningBoardView>(),Is.Null);
            string labels=string.Join(" ",Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Select(t=>t.text));Assert.That(labels,Does.Not.Contain("Stamina"));Assert.That(labels,Does.Not.Contain("เครียด"));Assert.That(labels,Does.Contain("สบายใจ"));
            yield return Click(view.HotspotButtons["studio_plaifah"]);view.RevealAll();Capture("Character");
            Assert.That(ui.ProducerButton.gameObject.activeInHierarchy,Is.False);Assert.That(ui.FocusButton.gameObject.activeInHierarchy,Is.False);
            Assert.That(Choice(0).GetComponentInChildren<TMP_Text>().text,Is.EqualTo("ทำงานด้วยกัน"));Assert.That(Choice(1).GetComponentInChildren<TMP_Text>().text,Is.EqualTo("ใช้เวลาด้วยกัน"));
            Assert.That(view.choiceButtons.GetComponentsInChildren<StudioUiHint>(),Is.Empty);Assert.That(view.choiceButtons.GetComponentsInChildren<TMP_Text>().Any(t=>t.name=="Delta progress"),Is.True);
            var before=JsonUtility.ToJson(view.Session.State);yield return Click(Choice(0));yield return Click(view.deferButton);Assert.That(JsonUtility.ToJson(view.Session.State),Is.EqualTo(before));
            yield return Click(Choice(0));yield return Finish();Assert.That(view.Session.State.mainProgress,Is.EqualTo(2));Assert.That(view.Session.State.phase,Is.EqualTo(DayPhase.Daytime));Capture("Reward");
            yield return Click(view.HotspotButtons["studio_plaifah"]);yield return Click(Choice(1));yield return Finish();Assert.That(view.Session.State.mainProgress,Is.EqualTo(2));Assert.That(view.Session.Romance.Mood("romance_plaifah"),Is.EqualTo(CharacterMood.Cheerful));LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]public IEnumerator WeeklyFocusAndMorningRecapAreOptionalAndReadableAtTwoSizes()
        {
            yield return Open();yield return Click(ui.FocusButton);Capture("WeeklyFocus");IsometricPlayTests.Capture(view,"Logs/Session041_WeeklyFocus_1280.png",false,1280,720);
            yield return Click(Named("Choose Focus romance_grace"));Assert.That(view.Session.State.romance.focuses.Count,Is.EqualTo(1));yield return Click(Named("Focus Back"));
            yield return Click(ui.ProducerButton);yield return Finish();Assert.That(view.Session.State.money,Is.EqualTo(view.config.startingMoney+10));
            yield return Click(view.waitButton);yield return Click(view.nextDayButton);Assert.That(ui.Overlay.name,Is.EqualTo("Morning Recap"));Capture("MorningRecap");
            IsometricPlayTests.Capture(view,"Logs/Session041_MorningRecap_1280.png",false,1280,720);int progress=view.Session.State.mainProgress;yield return Click(Named("Begin Today"));Assert.That(view.Session.Romance.PendingReport,Is.Null);Assert.That(view.Session.State.mainProgress,Is.EqualTo(progress));Assert.That(view.Session.State.day,Is.EqualTo(2));Assert.That(view.EventBubbleButtons.Count,Is.GreaterThan(0));Capture("Day2");
            yield return Click(view.EventBubbleButtons["romance_coffee"]);view.RevealAll();yield return Click(view.DialogueTapButton);Capture("StoryChoice");yield return Click(Choice(1));yield return Finish();Assert.That(view.Session.Romance.Mood("romance_grace"),Is.EqualTo(CharacterMood.Cheerful));Assert.That(view.Session.State.phase,Is.EqualTo(DayPhase.Daytime));Capture("StoryResult");LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]public IEnumerator DailyTalkGiftInventoryAndSaveKeepCasualUiUsable()
        {
            yield return Open();var pickup=view.Session.CurrentRoom.hotspots.First(h=>h.kind==RoomInteractionKind.Pickup);yield return Click(view.HotspotButtons[pickup.id]);yield return Click(view.HotspotButtons["studio_grace"]);yield return Click(Choice(2));yield return Finish();Assert.That(view.Session.State.FindCharacter("romance_grace").affection,Is.EqualTo(1));yield return Click(Choice(2));yield return Finish();Assert.That(view.Session.State.FindCharacter("romance_grace").affection,Is.EqualTo(1));yield return Click(Choice(3));yield return Click(Choice(0));yield return Finish();Assert.That(view.Session.ItemCount(pickup.item),Is.Zero);Assert.That(view.Session.State.FindCharacter("romance_grace").affection,Is.EqualTo(5));yield return Click(view.deferButton);
            yield return Click(view.inventoryButton);Assert.That(ui.PanelBack.gameObject.activeInHierarchy,Is.True);yield return Click(ui.PanelBack);Assert.That(view.IsOverviewPanel,Is.True);Assert.That(view.Save(SaveSlot.Manual),Is.EqualTo(SaveResult.Success));yield return null;yield return null;
            yield return Click(ui.ProducerButton);yield return Finish();Assert.That(view.Load(SaveSlot.Manual),Is.EqualTo(SaveResult.Success));Assert.That(view.Session.State.mainProgress,Is.Zero);yield return null;yield return null;Capture("Loaded");LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]public IEnumerator AuthoredLayoutSurvivesRefreshDialogueAndDayChangesWithoutRecreatingFixedViews()
        {
            yield return Open();var layout=view.Authored;Assert.That(layout,Is.Not.Null);
            int[] cards=layout.cast.Select(c=>c.name.transform.parent.gameObject.GetInstanceID()).ToArray();int focus=layout.focusRoot.GetInstanceID(),recap=layout.recapRoot.GetInstanceID();
            var portrait=view.dialoguePortrait.rectTransform;Vector2 position=portrait.anchoredPosition+new Vector2(9,0);portrait.anchoredPosition=position;
            float font=view.dialogueText.fontSize-1;view.dialogueText.enableAutoSizing=false;view.dialogueText.fontSize=font;
            yield return Click(view.HotspotButtons["studio_grace"]);var row=Choice(0);int rowId=row.GetInstanceID();var title=row.GetComponent<AuthoredActionRow>().title;title.enableAutoSizing=false;title.fontSize=22;
            for(int i=0;i<6;i++)view.Refresh();
            Assert.That(Choice(0).GetInstanceID(),Is.EqualTo(rowId));Assert.That(title.fontSize,Is.EqualTo(22));Assert.That(portrait.anchoredPosition,Is.EqualTo(position));Assert.That(view.dialogueText.fontSize,Is.EqualTo(font));
            view.RevealAll();IsometricPlayTests.Capture(view,"Logs/Session044_AuthoredConversation.png",false);IsometricPlayTests.Capture(view,"Logs/Session044_AuthoredConversation_1280.png",false,1280,720);IsometricPlayTests.AssertTextFits();
            yield return Click(view.deferButton);yield return Click(ui.FocusButton);yield return Click(Named("Focus Back"));yield return Click(view.waitButton);yield return Click(view.nextDayButton);yield return Click(Named("Begin Today"));
            Assert.That(layout.cast.Select(c=>c.name.transform.parent.gameObject.GetInstanceID()).ToArray(),Is.EqualTo(cards));Assert.That(layout.focusRoot.GetInstanceID(),Is.EqualTo(focus));Assert.That(layout.recapRoot.GetInstanceID(),Is.EqualTo(recap));Assert.That(portrait.anchoredPosition,Is.EqualTo(position));
            IsometricPlayTests.Capture(view,"Logs/Session044_AuthoredRoom.png",false);IsometricPlayTests.AssertTextFits();LogAssert.NoUnexpectedReceived();
        }
    }
}
