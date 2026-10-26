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
    public sealed class PlanningPlayTests : InputTestFixture
    {
        private IsometricRoomView view; private PlanningBoardView board; private string directory;
        [TearDown] public override void TearDown()
        {if(directory!=null&&Directory.Exists(directory))Directory.Delete(directory,true);base.TearDown();}
        private IEnumerator Open()
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/Gameplay/StudioPlanningPrototype.unity",
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
#endif
            yield return null;yield return null;yield return null;yield return null;
            view=Object.FindFirstObjectByType<IsometricRoomView>();board=view.GetComponent<PlanningBoardView>();view.TextSpeed=0;
            directory=Path.GetFullPath(Path.Combine("Logs","PlanningUI",System.Guid.NewGuid().ToString("N")));view.SetSaveDirectory(directory);
        }
        private static Button Named(string name)=>Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name==name&&b.gameObject.activeInHierarchy);
        private IEnumerator Click(string name){yield return SandboxPlayTests.ClickWithMouse(Named(name));}
        private void Capture(string suffix)
        {view.RevealAll();IsometricPlayTests.Capture(view,"Logs/Session037_"+suffix+".png",false);IsometricPlayTests.AssertTextFits();}
        private void TeamVisible()
        {
            var roster=Object.FindObjectsByType<RectTransform>(FindObjectsSortMode.None).Single(r=>r.name=="Permanent Team");
            Assert.That(roster.GetComponentsInChildren<TMP_Text>().Count(t=>t.name=="Affection Value"),Is.EqualTo(3));
            Assert.That(roster.GetComponentsInChildren<TMP_Text>().Count(t=>t.name=="Stress Value"),Is.EqualTo(3));
            Assert.That(roster.GetComponentsInChildren<TMP_Text>().Count(t=>t.name=="Work Value"),Is.EqualTo(3));
            Assert.That(Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t=>t.name=="Worker Status"),Is.False);
        }
        [UnityTest] public IEnumerator RoomBoardInventoryAndConversationKeepTeamVisibleAndRenderWithoutOverflow()
        {
            yield return Open();TeamVisible();Assert.That(view.inventoryButton.transform.Find("Compact Icon").GetComponent<Image>().sprite,Is.Not.Null);Assert.That(view.optionsButton.transform.Find("Compact Icon").GetComponent<Image>().sprite,Is.Not.Null);Capture("Room");
            yield return Click("Planning Board");Assert.That(board.IsOpen,Is.True);Assert.That(view.inventoryButton.gameObject.activeInHierarchy,Is.False);TeamVisible();Capture("Board");
            IsometricPlayTests.Capture(view,"Logs/Session037_Board_1280.png",false,1280,720);
            yield return Click("Plan Card studio_w1_plaifah_2");Capture("Dependency");
            yield return Click("Planning Back");yield return SandboxPlayTests.ClickWithMouse(view.inventoryButton);TeamVisible();Capture("Inventory");yield return Click("Planning Panel Back");
            yield return SandboxPlayTests.ClickWithMouse(view.HotspotButtons["studio_grace"]);TeamVisible();Capture("Conversation");
            Assert.That(view.choiceButtons.GetComponentsInChildren<Button>().Count(b=>b.gameObject.activeInHierarchy),Is.GreaterThanOrEqualTo(3));
            LogAssert.NoUnexpectedReceived();
        }
        private IEnumerator Drag(string source,string target,bool cancel=false,bool autoScroll=false,bool outside=false)
        {
            Canvas.ForceUpdateCanvases();var card=board.Cards[source];var dest=board.Cards[target];
            if(autoScroll){var port=card.GetComponentInParent<ScrollRect>().viewport;var anchor=port.anchorMax;anchor.y=.60f;port.anchorMax=anchor;Canvas.ForceUpdateCanvases();}
            var a=(RectTransform)card.transform;var b=(RectTransform)dest.transform;
            Vector2 from=RectTransformUtility.WorldToScreenPoint(null,a.TransformPoint(a.rect.center));
            Vector2 to=RectTransformUtility.WorldToScreenPoint(null,b.TransformPoint(new Vector2(b.rect.center.x,b.rect.yMax-20)));
            if(autoScroll){var viewport=card.GetComponentInParent<ScrollRect>().viewport;to=RectTransformUtility.WorldToScreenPoint(null,viewport.TransformPoint(new Vector2(viewport.rect.center.x,viewport.rect.yMin+2)));}
            if(outside)to=new Vector2(Screen.width*.8f,Screen.height*.04f);
            var module=EventSystem.current.GetComponent<InputSystemUIInputModule>();var devices=module.actionsAsset.devices;
            var behavior=InputSystem.settings.backgroundBehavior;
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
                InputSystem.QueueStateEvent(mouse,new MouseState{position=from}.WithButton(MouseButton.Left));InputSystem.Update();yield return null;yield return null;
                for(int i=1;i<=12;i++)
                {
                    InputSystem.QueueStateEvent(mouse,new MouseState{position=Vector2.Lerp(from,to,i/12f)}.WithButton(MouseButton.Left));InputSystem.Update();yield return null;
                    if(i==5){Assert.That(board.Dragging,Is.SameAs(card));Assert.That(board.Cards[source],Is.SameAs(card));}
                }
                if(autoScroll){for(int i=0;i<18;i++)yield return null;Assert.That(card.transform.parent.Find("Planning Board Modal").GetComponentsInChildren<ScrollRect>().Any(s=>s.content.anchoredPosition.y>0),Is.True,"Edge drag must scroll.");}
                if(cancel){var keyboard=InputSystem.AddDevice<Keyboard>();try{module.actionsAsset.devices=new InputDevice[]{mouse,keyboard};Press(keyboard.escapeKey);yield return null;Release(keyboard.escapeKey);yield return null;}finally{InputSystem.RemoveDevice(keyboard);}}
                InputSystem.QueueStateEvent(mouse,new MouseState{position=to});InputSystem.Update();yield return null;yield return null;yield return null;
            }
            finally
            {
                InputSystem.settings.backgroundBehavior=behavior;
#if UNITY_EDITOR
                InputSystem.settings.editorInputBehaviorInPlayMode=editorBehavior;
#endif
                module.actionsAsset.devices=devices;InputSystem.RemoveDevice(mouse);
            }
        }
        [UnityTest] public IEnumerator MouseDragUsesPureDraftAndApplyCommitsWithoutTimeWhileCrossOwnerAndEscapeCancel()
        {
            yield return Open();yield return Click("Planning Board");var work=view.Session.Production;string before=JsonUtility.ToJson(view.Session.State);var root=board.Board;
            yield return Drag("studio_w1_grace_2","studio_w1_grace_1");
            Assert.That(JsonUtility.ToJson(view.Session.State),Is.EqualTo(before));Assert.That(work.DraftChanged(board.Draft),Is.True);Assert.That(board.Board,Is.SameAs(root));Capture("Dragged");
            yield return Click("Apply Plan");Assert.That(board.IsOpen,Is.False);Assert.That(view.Session.State.production.planning.revision,Is.EqualTo(1));
            Assert.That(view.Session.State.production.resolvedPhases,Is.Zero);
            yield return Click("Planning Board");before=JsonUtility.ToJson(view.Session.State);
            yield return Drag("studio_w1_grace_1","studio_w1_ploy_1");Assert.That(work.DraftChanged(board.Draft),Is.False);Assert.That(JsonUtility.ToJson(view.Session.State),Is.EqualTo(before));
            yield return Drag("studio_w1_grace_1","studio_w1_grace_2",true);Assert.That(board.Dragging,Is.Null);Assert.That(work.DraftChanged(board.Draft),Is.False);Assert.That(board.IsOpen,Is.True);
            yield return Click("Plan Card studio_w1_grace_1");yield return Click("Earlier Due");Assert.That(work.DraftChanged(board.Draft),Is.True);
            yield return Click("Planning Back");Assert.That(JsonUtility.ToJson(view.Session.State),Is.EqualTo(before));LogAssert.NoUnexpectedReceived();
        }

        [UnityTest] public IEnumerator EdgeScrollAndPointerReleaseOutsideRestoreWithoutClickThrough()
        {
            yield return Open();yield return Click("Planning Board");string before=JsonUtility.ToJson(view.Session.State);
            yield return Drag("studio_w1_grace_1","studio_w1_grace_2",true,true);
            Assert.That(board.Dragging,Is.Null);Assert.That(board.Session.Production.DraftChanged(board.Draft),Is.False);
            yield return Drag("studio_w1_ploy_1","studio_w1_ploy_2",false,false,true);
            Assert.That(board.Dragging,Is.Null);Assert.That(board.IsOpen,Is.True);Assert.That(JsonUtility.ToJson(view.Session.State),Is.EqualTo(before));
            Assert.That(board.Session.Production.DraftChanged(board.Draft),Is.False);LogAssert.NoUnexpectedReceived();
        }
    }
}
