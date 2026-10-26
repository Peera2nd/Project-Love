using System;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
namespace ProjectLove.EditorTools
{
    // Detached scene/state only: never calls live Start, LateUpdate or SaveStore.
    public sealed class AuthoringPreview : IDisposable
    {
        readonly PreviewRenderUtility renderer;
        readonly EventDataSO transient;
        public IsometricRoomView View {get;}
        public GameSession Session {get;}
        public string Notice {get;private set;}
        public AuthoringPreview(GameConfigSO game,AuthoringScreen screen,int day=1,DayPhase phase=DayPhase.Morning,int affection=0,CharacterDataSO character=null,DialogueNodeSO node=null,EventDataSO story=null,CharacterMood? mood=null)
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("หยุด Play ก่อนเปิดตัวอย่างแยก");
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(ArtistWorkspaceBuilder.PrefabPath);
            if(prefab==null)throw new InvalidOperationException("Prepare the artist workspace first.");
            renderer=new PreviewRenderUtility();
            try
            {
            var root=UnityEngine.Object.Instantiate(prefab);root.hideFlags=HideFlags.HideAndDontSave;renderer.AddSingleGO(root);View=root.GetComponent<IsometricRoomView>();View.config=game;View.presentation=game.romance.presentation;
            Session=new GameSession(game);Session.State.day=Mathf.Clamp(day,1,game.totalDays);Session.State.phase=phase;Session.Romance.StartDay();
            foreach(var c in Session.State.characters)c.affection=Mathf.Clamp(affection,0,100);
            if(character!=null&&mood.HasValue)Session.State.romance.moods.First(m=>m.characterId==character.id).mood=mood.Value;
            // Screen/node previews bypass unrelated forced scenes; event tests retain real arbitration.
            if(story==null)foreach(var e in game.events.Where(e=>e.delivery==EventDelivery.Forced))Session.State.completedEvents.Add(e.id);
            View.InitializeEditorPreview(Session);
            if(story!=null){if(!Session.Dialogue.TryBegin(story))Notice=AuthoringCatalog.ExplainBlockedEvent(Session,story);}
            else if(node!=null)
            {
                transient=ScriptableObject.CreateInstance<EventDataSO>();transient.hideFlags=HideFlags.HideAndDontSave;transient.id="editor_preview";transient.startDay=1;transient.endDay=game.totalDays;transient.preferredDay=Session.State.day;transient.anyPhase=true;transient.entryNode=node;
                if(!Session.Dialogue.TryBegin(transient))Notice="บทนี้ยังไม่พร้อมดูตัวอย่าง ตรวจข้อความและลิงก์ก่อน";
            }
            else if(screen==AuthoringScreen.Conversation||screen==AuthoringScreen.Gifts)
            {
                var c=character??game.characters[0];var h=Session.CurrentRoom.hotspots.FirstOrDefault(h=>h.character==c);
                if(h!=null)Session.Interactions.TryOpen(h);if(screen==AuthoringScreen.Gifts){foreach(var item in game.items.Where(i=>i.giftable))Session.State.inventory.Add(new InventoryEntry{itemId=item.id,quantity=1});View.OpenGifts();}
            }
            if(screen==AuthoringScreen.MorningRecap){Session.TryEndDay();Session.TryStartNextDay();}
            View.Refresh();View.Authored.PreviewScreen(screen,Session.Romance.PendingReport);
            var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=renderer.camera;
            var rect=root.GetComponent<RectTransform>();rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,1920);rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,1080);rect.localScale=Vector3.one*.01f;rect.position=new Vector3(0,0,5);
            // URP's preview camera path skips Canvas UI; render the isolated scene as a game camera.
            renderer.camera.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);renderer.camera.cullingMask=~0;renderer.camera.cameraType=CameraType.Game;
            renderer.camera.orthographic=true;renderer.camera.orthographicSize=5.4f;renderer.camera.nearClipPlane=.01f;renderer.camera.farClipPlane=100;renderer.camera.clearFlags=CameraClearFlags.SolidColor;renderer.camera.backgroundColor=Color.black;
            }
            catch{renderer.Cleanup();if(transient!=null)UnityEngine.Object.DestroyImmediate(transient);throw;}
        }
        public Texture Render(int width=1280,int height=720)
        {
            renderer.BeginPreview(new Rect(0,0,width,height),GUIStyle.none);renderer.camera.aspect=(float)width/height;
            Canvas.ForceUpdateCanvases();foreach(var t in View.GetComponentsInChildren<TMP_Text>())t.ForceMeshUpdate();renderer.Render(true);return renderer.EndPreview();
        }
        public void Advance(){if(Session.Dialogue.IsActive)Session.Dialogue.TryAdvance();View.Refresh();View.RevealAll();}
        public void Choose(DialogueChoice choice){Session.Dialogue.TryChoose(choice);View.Refresh();View.RevealAll();}
        public void Dispose(){renderer.Cleanup();if(transient!=null)UnityEngine.Object.DestroyImmediate(transient);}
    }
    public sealed class ArtistPreviewWindow : EditorWindow
    {
        [SerializeField]GameConfigSO game;
        [SerializeField]DialogueNodeSO node;
        [SerializeField]EventDataSO story;
        AuthoringScreen screen;
        int day=1,person,love,moodIndex,locale;DayPhase phase;
        AuthoringPreview preview;string notice;
        public static void Open(GameConfigSO game,DialogueNodeSO node=null,EventDataSO story=null)
        {
            if(EditorApplication.isPlaying)return;
            var w=GetWindow<ArtistPreviewWindow>("ตัวอย่างเกม");w.game=game;w.node=node;w.story=story;w.screen=node!=null||story!=null?AuthoringScreen.Conversation:AuthoringScreen.Room;
            w.day=story?.preferredDay??1;w.phase=story!=null&&!story.anyPhase?story.triggerPhase:DayPhase.Morning;w.Rebuild();w.Show();
        }
        void OnEnable(){minSize=new Vector2(760,650);}
        void OnDisable(){preview?.Dispose();preview=null;}
        void Rebuild()
        {
            preview?.Dispose();preview=null;notice=null;if(game==null)return;
            try{preview=new AuthoringPreview(game,screen,day,phase,love,game.characters[Mathf.Clamp(person,0,game.characters.Length-1)],node,story,moodIndex==0?(CharacterMood?)null:(CharacterMood)(moodIndex-1));preview.Session.Localization.SetLocale(locale==1?"en":"th");notice=preview.Notice;}
            catch(Exception e){notice=e.Message;}
            Repaint();
        }
        void OnGUI()
        {
            if(EditorApplication.isPlaying){EditorGUILayout.HelpBox("หยุด Play ก่อนดูตัวอย่างแยก",MessageType.Info);return;}
            EditorGUILayout.HelpBox("ตัวอย่างแยกจากเกมจริง · ไม่ใช้เซฟ ไม่เดินเวลาในเกมจริง · บันทึก Prefab แล้วกดรีเฟรชเพื่อดูการจัดหน้าใหม่",MessageType.Info);
            EditorGUI.BeginChangeCheck();locale=EditorGUILayout.Popup("ภาษาตัวอย่าง",locale,new[]{"ไทย","English"});if(EditorGUI.EndChangeCheck()){preview?.Session.Localization.SetLocale(locale==1?"en":"th");Repaint();}
            EditorGUI.BeginChangeCheck();screen=(AuthoringScreen)EditorGUILayout.Popup("หน้าจอ",(int)screen,new[]{"ห้อง","พูดคุย","ของขวัญ","สนับสนุนรายสัปดาห์","สรุปวันใหม่"});
            if(EditorGUI.EndChangeCheck()){node=null;story=null;Rebuild();}
            EditorGUI.BeginChangeCheck();
            day=EditorGUILayout.IntSlider("วันที่ทดสอบ",day,1,game!=null?game.totalDays:30);phase=(DayPhase)EditorGUILayout.Popup("ช่วงเวลา",(int)phase,new[]{"เช้า","บ่าย","เย็น","กลางคืน"});
            if(game!=null){person=EditorGUILayout.Popup("ตัวละคร",person,game.characters.Select(c=>c.displayName).ToArray());love=EditorGUILayout.IntSlider("ความสนิทในตัวอย่าง",love,0,100);moodIndex=EditorGUILayout.Popup("อารมณ์คนที่เลือก",moodIndex,new[]{"ตามข้อมูลเกม"}.Concat(game.romance.moods.OrderBy(m=>m.mood).Select(m=>m.label)).ToArray());}
            if(EditorGUI.EndChangeCheck())Rebuild();
            using(new EditorGUILayout.HorizontalScope())
            {
                if(GUILayout.Button("รีเฟรชตัวอย่าง"))Rebuild();
                if(GUILayout.Button("เปิด Prefab จัด UI"))AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(ArtistWorkspaceBuilder.PrefabPath));
            }
            if(!string.IsNullOrEmpty(notice))EditorGUILayout.HelpBox(notice,MessageType.Warning);
            if(preview==null)return;
            var rect=GUILayoutUtility.GetAspectRect(16f/9f);if(Event.current.type==EventType.Repaint)GUI.DrawTexture(rect,preview.Render(1280,720),ScaleMode.ScaleToFit,false);
            if(preview.Session.Dialogue.IsActive)
            {
                if(preview.Session.Dialogue.ChoicesReady)
                {foreach(var c in preview.Session.Dialogue.Node.choices)using(new EditorGUI.DisabledScope(!preview.Session.Dialogue.CanChoose(c)))if(GUILayout.Button(preview.Session.Localization.Choice(preview.Session.Dialogue.Node,c))){preview.Choose(c);Repaint();}}
                else if(GUILayout.Button("อ่านต่อ")){preview.Advance();Repaint();}
                if(GUILayout.Button("เปิดต้นฉบับบทนี้"))DesignerDashboard.OpenNode(preview.Session.Dialogue.Node);
            }
            else EditorGUILayout.LabelField("เลือกหน้าหรือบทที่ต้องการทดสอบได้จากหน้าทำงานหลัก");
        }
    }
}
