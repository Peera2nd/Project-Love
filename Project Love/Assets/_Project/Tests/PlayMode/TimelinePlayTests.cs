using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectLove.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace ProjectLove.Tests
{
    public sealed class TimelinePlayTests : InputTestFixture
    {
        private IsometricRoomView view;private PlanningBoardView board;private string directory;
        [TearDown]public override void TearDown(){if(directory!=null&&Directory.Exists(directory))Directory.Delete(directory,true);base.TearDown();}
        private IEnumerator Open()
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/Gameplay/StudioPlanningPrototype.unity",new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
#endif
            for(int i=0;i<4;i++)yield return null;
            view=Object.FindFirstObjectByType<IsometricRoomView>();board=view.GetComponent<PlanningBoardView>();view.TextSpeed=0;
            directory=Path.GetFullPath(Path.Combine("Logs","TimelineUI",System.Guid.NewGuid().ToString("N")));view.SetSaveDirectory(directory);
        }
        private static Button Named(string name)=>Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name==name&&b.gameObject.activeInHierarchy);
        private IEnumerator Click(string name){yield return SandboxPlayTests.ClickWithMouse(Named(name));}
        private Button Choice(int n)=>view.choiceButtons.GetComponentsInChildren<Button>().Where(b=>b.gameObject.activeInHierarchy).ElementAt(n);
        private void Capture(string suffix)
        {view.RevealAll();IsometricPlayTests.Capture(view,"Logs/Session038_"+suffix+".png",false);IsometricPlayTests.AssertTextFits();}
        [UnityTest]public IEnumerator NoDuplicateHotspotsSpeechAvoidsPortraitAndActionsUseIconsWithoutHoverDuplicates()
        {
            yield return Open();
            Assert.That(view.Session.CurrentRoom.hotspots.Where(h=>h.kind==ProjectLove.Data.RoomInteractionKind.Producer||h.activity!=null&&h.activity.staminaRecovery>0).All(h=>!view.HotspotButtons.ContainsKey(h.id)),Is.True);
            yield return SandboxPlayTests.ClickWithMouse(view.HotspotButtons["studio_plaifah"]);view.RevealAll();Capture("Conversation");
            var speech=(RectTransform)view.speakerText.transform.parent;var portrait=view.dialoguePortrait.rectTransform;
            var a=new Vector3[4];var b=new Vector3[4];speech.GetWorldCorners(a);portrait.GetWorldCorners(b);Assert.That(a[2].x,Is.LessThan(b[0].x));
            Assert.That(view.choiceButtons.GetComponentsInChildren<StudioUiHint>(),Is.Empty);
            Assert.That(Choice(0).GetComponentsInChildren<Image>().Any(i=>i.name=="Effect stamina"&&i.sprite==view.presentation.Icon("stamina")),Is.True);
            Assert.That(Choice(0).GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Stamina")),Is.False);
            Assert.That(Choice(3).GetComponentsInChildren<TMP_Text>().Any(t=>t.name=="Delta stress"),Is.True);
            IsometricPlayTests.Capture(view,"Logs/Session038_Conversation_1280.png",false,1280,720);LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]public IEnumerator PlayerRestClosesMenuAndConsumesOnePhaseRatherThanEndingDay()
        {
            yield return Open();view.Session.State.stamina=40;view.Refresh();yield return null;yield return null;
            yield return Click("Planning Rest");Assert.That(view.IsOverviewPanel,Is.False);Capture("RestMenu");
            int cursor=view.Session.State.production.resolvedPhases;var rest=view.panelButtons.GetComponentsInChildren<Button>().Single();yield return SandboxPlayTests.ClickWithMouse(rest);
            Assert.That(view.IsOverviewPanel,Is.True);Assert.That(view.panelTitle.transform.parent.gameObject.activeInHierarchy,Is.False);
            Assert.That(view.Session.State.production.resolvedPhases,Is.EqualTo(cursor+1));Assert.That(view.Session.State.dayEnded,Is.False);Assert.That(view.Session.State.stamina,Is.GreaterThan(40));Capture("AfterRest");
            LogAssert.NoUnexpectedReceived();
        }
        private IEnumerator Drag(RectTransform target,float phaseDelta,bool cancel=false,bool outside=false)
        {
            Canvas.ForceUpdateCanvases();
            var port=board.Timeline.Viewport;var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(port,target);var position=board.Timeline.Content.anchoredPosition;
            if(bounds.max.x>port.rect.xMax-20)position.x-=bounds.max.x-port.rect.xMax+20;
            if(bounds.min.x<port.rect.xMin+20)position.x+=port.rect.xMin+20-bounds.min.x;
            var scroll=port.GetComponent<ScrollRect>();scroll.StopMovement();position.x=Mathf.Clamp(position.x,Mathf.Min(0,port.rect.width-board.Timeline.Content.rect.width),0);board.Timeline.Content.anchoredPosition=position;
            Canvas.ForceUpdateCanvases();yield return null;yield return null;
            Vector2 from=RectTransformUtility.WorldToScreenPoint(null,target.TransformPoint(target.rect.center));
            Vector2 to=from+new Vector2(phaseDelta*board.Session.Production.Config.planning.timelineCellWidth*view.GetComponent<Canvas>().scaleFactor,0);
            if(outside)to=new Vector2(Screen.width*.95f,Screen.height*.04f);
            var module=EventSystem.current.GetComponent<InputSystemUIInputModule>();var previous=module.actionsAsset.devices;var behavior=InputSystem.settings.backgroundBehavior;
#if UNITY_EDITOR
            var editorBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;
#endif
            var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                module.actionsAsset.devices=new InputDevice[]{mouse};InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
                InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
                InputSystem.QueueStateEvent(mouse,new MouseState{position=from});InputSystem.Update();yield return null;yield return null;
                var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=from},hits);
                Assert.That(hits.Count>0&&hits[0].gameObject.GetComponentInParent<PlanningTimelineDrag>()==target.GetComponent<PlanningTimelineDrag>(),Is.True,"Timeline press "+target.name+" at "+from+" | hits "+string.Join(",",hits.Select(h=>h.gameObject.name))+" | screen "+Screen.width+"x"+Screen.height+" | viewport center "+board.Timeline.Viewport.position+" rect "+board.Timeline.Viewport.rect+" | content pos "+board.Timeline.Content.anchoredPosition+" | target row "+((RectTransform)target.parent.parent).anchoredPosition+" | bars "+string.Join(",",board.Timeline.Bars.Select(t=>t.Key+":"+t.Value.gameObject.activeInHierarchy)));
                InputSystem.QueueStateEvent(mouse,new MouseState{position=from}.WithButton(MouseButton.Left));InputSystem.Update();yield return null;yield return null;
                var root=board.Timeline.Content;
                for(int i=1;i<=8;i++)
                {InputSystem.QueueStateEvent(mouse,new MouseState{position=Vector2.Lerp(from,to,i/8f)}.WithButton(MouseButton.Left));InputSystem.Update();yield return null;}
                Assert.That(board.Timeline.IsDragging,Is.True,"Drag target "+target.name+" from "+from+" to "+to+" | rect "+target.rect+" | active "+target.gameObject.activeInHierarchy);Assert.That(board.Timeline.Content,Is.SameAs(root));
                if(cancel)
                {
                    var keyboard=InputSystem.AddDevice<Keyboard>();try{module.actionsAsset.devices=new InputDevice[]{mouse,keyboard};Press(keyboard.escapeKey);yield return null;Release(keyboard.escapeKey);yield return null;}finally{InputSystem.RemoveDevice(keyboard);}
                }
                InputSystem.QueueStateEvent(mouse,new MouseState{position=to});InputSystem.Update();yield return null;yield return null;yield return null;
            }
            finally
            {
                module.actionsAsset.devices=previous;InputSystem.settings.backgroundBehavior=behavior;
#if UNITY_EDITOR
                InputSystem.settings.editorInputBehaviorInPlayMode=editorBehavior;
#endif
                InputSystem.RemoveDevice(mouse);
            }
        }
        [UnityTest]public IEnumerator TimelineMouseMoveResizeAndQueueSwitchUseOneDraftWithoutChangingWorkUntilApply()
        {
            yield return Open();yield return Click("Planning Board");yield return Click("Timeline Mode");Assert.That(board.TimelineMode,Is.True);Capture("Timeline");
            string before=JsonUtility.ToJson(view.Session.State);string id="studio_w1_grace_1";var edit=board.Draft.tasks.Single(e=>e.taskId==id);int start=edit.start,due=edit.due;
            yield return Drag(board.Timeline.Bars[id],2);Assert.That(edit.start,Is.EqualTo(start+2));Assert.That(edit.due,Is.EqualTo(due+2));Assert.That(JsonUtility.ToJson(view.Session.State),Is.EqualTo(before));
            var handle=(RectTransform)board.Timeline.Bars[id].Find("Due Handle");yield return Drag(handle,-2);Assert.That(edit.due,Is.EqualTo(due));
            yield return Drag((RectTransform)board.Timeline.Bars[id].Find("Start Handle"),1);Assert.That(edit.start,Is.EqualTo(start+3));Assert.That(edit.due,Is.EqualTo(due));
            yield return Click("Queue Mode");Assert.That(board.Draft.tasks.Single(e=>e.taskId==id).start,Is.EqualTo(start+3));
            yield return Click("Timeline Mode");
            yield return Click("Timeline Name studio_w1_grace_2");
            Assert.That(board.Timeline.Content.GetComponentInChildren<TimelineLinksGraphic>().segments.Count,Is.GreaterThan(0));
            Capture("TimelineDraft");IsometricPlayTests.Capture(view,"Logs/Session038_Timeline_1280.png",false,1280,720);
            yield return Click("Apply Plan");Assert.That(view.Session.Production.PlanFor(id).start,Is.EqualTo(start+3));Assert.That(view.Session.State.production.resolvedPhases,Is.Zero);
            Assert.That(view.Session.TryAdvancePhase(),Is.True);yield return null;yield return null;yield return Click("Planning Board");
            Assert.That(board.Timeline.CanDrag("studio_w1_ploy_1",TimelineGesture.Move),Is.False);
            Assert.That(board.Timeline.Bars["studio_w1_ploy_1"].Find("Start Handle").gameObject.activeSelf,Is.False);
            Capture("TimelineHistory");LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]public IEnumerator TimelineEscapeOutsideAndRevertLeaveNoPartialPlanOrTimeChanges()
        {
            yield return Open();yield return Click("Planning Board");yield return Click("Timeline Mode");string before=JsonUtility.ToJson(view.Session.State);string id="studio_w1_grace_1";
            yield return Drag(board.Timeline.Bars[id],2,true);Assert.That(view.Session.Production.DraftChanged(board.Draft),Is.False);
            yield return Drag(board.Timeline.Bars[id],2,false,true);Assert.That(view.Session.Production.DraftChanged(board.Draft),Is.False);
            yield return Drag(board.Timeline.Bars[id],2);Assert.That(view.Session.Production.DraftChanged(board.Draft),Is.True);
            yield return Click("Revert Plan");Assert.That(view.Session.Production.DraftChanged(board.Draft),Is.False);Assert.That(JsonUtility.ToJson(view.Session.State),Is.EqualTo(before));
            yield return Click("Planning Back");Assert.That(view.Session.Interactions.IsOpen,Is.False);Assert.That(board.IsOpen,Is.False);LogAssert.NoUnexpectedReceived();
        }
    }
}
