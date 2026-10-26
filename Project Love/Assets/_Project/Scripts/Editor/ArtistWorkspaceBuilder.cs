using System;
using System.IO;
using System.Linq;
using ProjectLove.Data;
using ProjectLove.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace ProjectLove.EditorTools
{
    // One-time migration only. Existing authored prefabs/scenes are never regenerated on ordinary open.
    public static class ArtistWorkspaceBuilder
    {
        public const string PrefabPath="Assets/_Project/Prefabs/UI/RomanceWorkspace.prefab";
        public const string ActionPath="Assets/_Project/Prefabs/UI/RomanceActionRow.prefab";
        public const string CastPath="Assets/_Project/Prefabs/UI/RomanceCharacterCard.prefab";
        static RoomPresentationSO style;
        [MenuItem("Tools/Project Love/Prepare Artist Workspace")]
        public static void BuildOrOpen()
        {
            if(EditorApplication.isPlaying||!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            BuildFromCommandLine();EditorSceneManager.OpenScene(RomancePrototypeBuilder.ScenePath);DesignerDashboard.Open();
        }
        public static void BuildFromCommandLine()
        {
            RomancePrototypeBuilder.BuildFromCommandLine();var game=AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath);style=game.romance.presentation;
            if(!File.Exists(PrefabPath))CreatePrefab(game);
            var scene=EditorSceneManager.OpenScene(RomancePrototypeBuilder.ScenePath);
            var old=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<IsometricRoomView>(true)).FirstOrDefault();
            if(old!=null&&old.Authored!=null){Debug.Log("Artist workspace already authored; preserved existing layout.");return;}
            if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath),scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("Artist workspace ready: "+PrefabPath);
        }
        static void CreatePrefab(GameConfigSO game)
        {
            var root=new GameObject("Project Love · Artist UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            try
            {
                root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;var scale=root.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1920,1080);scale.matchWidthOrHeight=.5f;
                var v=root.AddComponent<IsometricRoomView>();v.config=game;v.presentation=style;var a=root.AddComponent<AuthoredRomanceLayout>();a.view=v;root.AddComponent<RomanceRoomView>().authored=a;
                Panel("Black Backdrop",root.transform,0,0,1,1,Color.black);
                var room=Rect("ห้อง · Room Frame",root.transform,.36f,.47f,.36f,.47f);room.sizeDelta=new Vector2(900,900);v.roomImage=room.gameObject.AddComponent<Image>();v.roomImage.preserveAspect=true;v.roomImage.raycastTarget=false;
                v.hotspotRoot=Rect("ตัวละครและจุดกด · Hotspots",room,0,0,1,1);
                var hp=new GameObject("Room Hotspot",typeof(RectTransform),typeof(Image),typeof(Button));hp.GetComponent<Image>().preserveAspect=true;hp.GetComponent<Button>().targetGraphic=hp.GetComponent<Image>();
                a.hotspotTemplate=SavePrefab(hp,"Assets/_Project/Prefabs/UI/RomanceHotspot.prefab").GetComponent<Button>();UnityEngine.Object.DestroyImmediate(hp);
                var action=MakeButton("Action Row",root.transform,0,0,.4f,.1f,"",null,true);v.buttonTemplate=SavePrefab(action.gameObject,ActionPath).GetComponent<Button>();UnityEngine.Object.DestroyImmediate(action.gameObject);
                var bubble=MakeButton("Event Bubble",root.transform,0,0,0,0,style.Get("eventBubble"),style.Icon("talk"));((RectTransform)bubble.transform).sizeDelta=new Vector2(100,65);
                a.bubbleTemplate=SavePrefab(bubble.gameObject,"Assets/_Project/Prefabs/UI/RomanceEventBubble.prefab").GetComponent<Button>();UnityEngine.Object.DestroyImmediate(bubble.gameObject);
                var hud=Panel("ข้อมูลด้านบน · HUD",root.transform,.02f,.90f,.98f,.98f,new Color(.07f,.1f,.12f,.96f));v.titleText=Text("ชื่อฉาก",hud,.015f,.05f,.24f,.95f,game.rooms[0].displayName,32);v.hudText=Text("Legacy Resource Binding",hud,0,0,0,0,"",20);v.hudText.gameObject.SetActive(false);
                var metrics=Rect("HUD Metrics",hud,.25f,.05f,.985f,.95f);a.day=Metric("Day",metrics,0,0,.45f,1,"day",false);a.money=Metric("Money",metrics,.47f,0,.63f,1,"money",false);a.project=Metric("Project",metrics,.66f,0,1,1,"progress",true);
                var panel=Panel("เมนูย่อย · Action Panel",root.transform,.03f,.14f,.42f,.85f,new Color(.06f,.09f,.11f,.98f));v.panelTitle=Text("ชื่อเมนู",panel,.05f,.87f,.95f,.97f,"",30);v.panelText=Text("รายละเอียด",panel,.05f,.61f,.95f,.85f,"",25);v.panelButtons=List("รายการเมนู",panel,.05f,.05f,.95f,.58f,88);
                v.travelButton=MakeButton("Travel",root.transform,.03f,.03f,.13f,.10f,style.Get("travel"),style.Icon("travel"));v.inventoryButton=MakeButton("Inventory",root.transform,.14f,.03f,.24f,.10f,style.Get("inventory"),style.Icon("bag"));v.optionsButton=MakeButton("Options",root.transform,.25f,.03f,.35f,.10f,style.Get("options"),style.Icon("settings"));
                v.waitButton=MakeButton("End Day",root.transform,.40f,.03f,.52f,.10f,style.Get("endDay"),style.Icon("night"));v.nextDayButton=MakeButton("Next Day",root.transform,.54f,.03f,.69f,.10f,style.Get("nextDay"),style.Icon("day"));v.hintText=Text("ข้อความเมื่อจบวัน",root.transform,.03f,.36f,.41f,.45f,"",23);
                a.producer=MakeButton("Producer Activity",root.transform,.03f,.13f,.31f,.24f,style.Get("Producer"),style.Icon("work"),true);a.focus=MakeButton("Weekly Focus",root.transform,.03f,.79f,.40f,.87f,style.Get("focus"),style.Icon("sprint_board"));
                a.opportunity=Panel("โอกาสวันนี้",root.transform,.03f,.65f,.41f,.76f,new Color(.045f,.065f,.09f,.95f)).gameObject;a.todayHint=Text("คำแนะนำวันนี้",a.opportunity.transform,.025f,.28f,.975f,.97f,"",24);a.baseline=Text("งานอัตโนมัติ",a.opportunity.transform,.025f,.025f,.975f,.28f,"",20);
                a.feedback=Rect("ผลกิจกรรม",root.transform,.03f,.26f,.41f,.35f).gameObject;a.receiptTitle=Text("ชื่อกิจกรรม",a.feedback.transform,0,.62f,1,1,"",22);a.receiptMetrics=new[]{Metric("Progress",a.feedback.transform,0,0,.31f,.59f,"progress",false),Metric("Heart",a.feedback.transform,.34f,0,.64f,.59f,"heart",false),Metric("Money",a.feedback.transform,.67f,0,.99f,.59f,"money",false)};
                var modal=Panel("บทสนทนา · Dialogue Modal",root.transform,0,0,1,1,new Color(0,0,0,style.dialogueDimAlpha));modal.GetComponent<Image>().raycastTarget=true;modal.gameObject.AddComponent<Button>().transition=Selectable.Transition.None;v.dialogueModal=modal.gameObject;
                v.dialoguePortrait=Image("ภาพตัวละคร · Portrait",modal,.43f,0,.765f,.86f,null);v.dialoguePortrait.preserveAspect=true;
                var speech=Panel("กล่องบทพูด · Speech",modal,.03f,.60f,.42f,.86f,new Color(.06f,.09f,.12f,.98f));v.speakerText=Text("ชื่อตัวละคร",speech,.04f,.80f,.96f,.96f,"",32);v.dialogueText=Text("บทพูด",speech,.04f,.08f,.96f,.77f,"",28);
                v.choiceButtons=List("ตัวเลือก · Choices",modal,.03f,.12f,.42f,.56f,96);v.continueButton=MakeButton("Continue (unused)",modal,0,0,0,0,"",null);v.continueButton.gameObject.SetActive(false);v.deferButton=Back("Defer",modal,.92f,.03f,.98f,.11f);
                a.panelBack=Back("Auxiliary Back",root.transform,.92f,.03f,.98f,.11f);
                BuildFocus(a,root.transform);BuildRecap(a,root.transform);
                a.roster=Rect("ความสัมพันธ์และอารมณ์ · Roster",root.transform,.78f,.30f,.98f,.86f).gameObject;a.cast=new AuthoredCastCard[game.characters.Length];
                var card=Panel("Character Card",root.transform,0,0,1,1,Color.white);StudioUiWidgets.Panel(card.GetComponent<Image>(),style,true);
                Image("Portrait",card,.025f,.40f,.21f,.96f,null).preserveAspect=true;Text("Name",card,.25f,.66f,.97f,.97f,"",26);Image("Mood Icon",card,.25f,.42f,.32f,.65f,null).preserveAspect=true;Text("Mood",card,.36f,.38f,.97f,.67f,"",23);Metric("Affection",card,.05f,.03f,.96f,.37f,"heart",true);
                var cardPrefab=SavePrefab(card.gameObject,CastPath);UnityEngine.Object.DestroyImmediate(card.gameObject);
                for(int i=0;i<a.cast.Length;i++)
                {
                    var go=(GameObject)PrefabUtility.InstantiatePrefab(cardPrefab,a.roster.transform);go.name="Romance Character "+game.characters[i].id;float high=1-i/(float)a.cast.Length;Place((RectTransform)go.transform,0,high-1f/a.cast.Length+.03f,1,high);
                    a.cast[i]=new AuthoredCastCard{portrait=go.transform.Find("Portrait").GetComponent<Image>(),name=go.transform.Find("Name").GetComponent<TMP_Text>(),moodIcon=go.transform.Find("Mood Icon").GetComponent<Image>(),mood=go.transform.Find("Mood").GetComponent<TMP_Text>(),affection=go.transform.Find("Affection").GetComponent<AuthoredMetric>()};
                    var c=game.characters[i];a.cast[i].name.text=c.displayName;a.cast[i].portrait.sprite=c.taskAvatar;a.cast[i].mood.text=game.romance.moods.First(m=>m.mood==game.romance.cast[i].initialMood).label;a.cast[i].moodIcon.sprite=style.Icon("talk");a.cast[i].affection.Bind(style.Icon("heart"),c.startingAffection+"/100",c.startingAffection/100f);
                }
                a.day.Bind(style.Icon("day"),string.Format(style.Get("romanceDay"),1,game.totalDays,style.phaseNames[0]));a.money.Bind(style.Icon("money"),game.startingMoney.ToString());a.project.Bind(style.Icon("progress"),style.Get("projectTitle")+"  "+game.startingProgress+"%",game.startingProgress/100f);a.todayHint.text=game.romance.dailyHints[0];a.baseline.text=string.Format(style.Get("teamDaily"),game.romance.teamDailyProgress);
                a.BindRoom(game.rooms.First(r=>r.id==game.startingRoomId),DayPhase.Morning);
                panel.gameObject.SetActive(false);a.feedback.SetActive(false);a.focusRoot.gameObject.SetActive(false);a.recapRoot.gameObject.SetActive(false);a.panelBack.gameObject.SetActive(false);modal.gameObject.SetActive(false);v.nextDayButton.gameObject.SetActive(false);v.hintText.gameObject.SetActive(false);
                PrefabUtility.SaveAsPrefabAssetAndConnect(hud.gameObject,"Assets/_Project/Prefabs/UI/RomanceHud.prefab",InteractionMode.AutomatedAction);
                // Screen prefabs open active in Prefab Mode; the main screen stores only its initial visibility overrides.
                modal.gameObject.SetActive(true);PrefabUtility.SaveAsPrefabAssetAndConnect(modal.gameObject,"Assets/_Project/Prefabs/UI/RomanceDialogue.prefab",InteractionMode.AutomatedAction);modal.gameObject.SetActive(false);
                a.focusRoot.gameObject.SetActive(true);PrefabUtility.SaveAsPrefabAssetAndConnect(a.focusRoot.gameObject,"Assets/_Project/Prefabs/UI/RomanceWeeklySupport.prefab",InteractionMode.AutomatedAction);a.focusRoot.gameObject.SetActive(false);
                a.recapRoot.gameObject.SetActive(true);PrefabUtility.SaveAsPrefabAssetAndConnect(a.recapRoot.gameObject,"Assets/_Project/Prefabs/UI/RomanceMorningRecap.prefab",InteractionMode.AutomatedAction);a.recapRoot.gameObject.SetActive(false);
                SavePrefab(root,PrefabPath);
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        static void BuildFocus(AuthoredRomanceLayout a,Transform parent)
        {
            a.focusRoot=Panel("Weekly Support",parent,.015f,.12f,.765f,.885f,new Color(.045f,.065f,.09f,.99f));a.focusRoot.GetComponent<Image>().raycastTarget=true;var r=a.focusRoot;
            a.focusTitle=Text("หัวข้อ",r,.05f,.87f,.93f,.98f,"",32);a.milestone=Text("เป้าหมาย",r,.05f,.73f,.94f,.87f,"",25);a.focusGoal=Metric("Weekly Goal",r,.05f,.69f,.94f,.72f,null,true,true);a.focusHint=Text("คำแนะนำ",r,.05f,.57f,.94f,.68f,"",24);a.focusCards=new AuthoredFocusCard[3];
            for(int i=0;i<3;i++){float x=.05f+i*.30f;var c=Panel("Support Card "+i,r,x,.23f,x+.275f,.56f,new Color(.13f,.18f,.24f));a.focusCards[i]=new AuthoredFocusCard{portrait=Image("Owner",c,.04f,.59f,.32f,.96f,null),name=Text("Name",c,.36f,.62f,.96f,.94f,"",26),work=Text("Work",c,.05f,.37f,.96f,.62f,"",23),topic=Text("Topic",c,.05f,.20f,.96f,.40f,"",21),choose=MakeButton("Choose Focus "+i,c,.04f,.025f,.96f,.20f,"",style.Icon("heart"))};a.focusCards[i].portrait.preserveAspect=true;}
            a.focusBenefit=Text("ผลที่ได้รับ",r,.05f,.09f,.94f,.22f,"",24);a.focusBack=Back("Focus Back",r,.90f,.025f,.98f,.12f);
        }
        static void BuildRecap(AuthoredRomanceLayout a,Transform parent)
        {
            a.recapRoot=Panel("Morning Recap",parent,.015f,.12f,.765f,.885f,new Color(.045f,.065f,.09f,.99f));a.recapRoot.GetComponent<Image>().raycastTarget=true;var r=a.recapRoot;
            a.recapTitle=Text("หัวข้อ",r,.06f,.86f,.94f,.98f,"",34);a.recapProject=Text("ความคืบหน้า",r,.06f,.72f,.94f,.85f,"",32);a.recapGoal=Metric("Project Growth Bar",r,.06f,.68f,.94f,.71f,null,true,true);a.recapSources=Text("ที่มาความคืบหน้า",r,.06f,.59f,.94f,.68f,"",24);a.recapMoney=Text("เงิน",r,.06f,.51f,.94f,.59f,"",23);a.recapAffection=new TMP_Text[3];
            for(int i=0;i<3;i++){float x=.06f+i*.295f;Image("Heart "+i,r,x,.405f,x+.04f,.47f,style.Icon("heart")).color=style.affectionTint;a.recapAffection[i]=Text("ความสัมพันธ์ "+i,r,x+.045f,.39f,x+.28f,.49f,"",23);}
            a.recapTopics=Text("หัวข้อใหม่",r,.06f,.29f,.94f,.39f,"",23);a.recapToday=Text("วันนี้",r,.06f,.16f,.94f,.28f,"",24);a.recapContinue=MakeButton("Begin Today",r,.55f,.04f,.94f,.14f,style.Get("recapContinue"),style.Icon("day"));
        }
        static AuthoredMetric Metric(string name,Transform parent,float x0,float y0,float x1,float y1,string icon,bool bar,bool barOnly=false)
        {
            var r=Rect(name,parent,x0,y0,x1,y1);var m=r.gameObject.AddComponent<AuthoredMetric>();
            if(!barOnly){m.icon=Image("Icon",r,.01f,.20f,.16f,.95f,style.Icon(icon));m.icon.preserveAspect=true;m.label=Text("Value",r,.19f,.28f,.99f,1,"",23);}
            if(bar){var track=Image("Track",r,barOnly?0:.19f,barOnly?0:.03f,barOnly?1:.99f,barOnly?1:.22f,style.barTrack);track.type=UnityEngine.UI.Image.Type.Sliced;m.fill=Image("Fill",track.transform,0,0,1,1,style.barFill);m.fill.type=UnityEngine.UI.Image.Type.Filled;m.fill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;m.fill.color=icon=="heart"?style.affectionTint:style.progressTint;Image("Frame",track.transform,0,0,1,1,style.barFrame).type=UnityEngine.UI.Image.Type.Sliced;}
            return m;
        }
        static Transform List(string name,Transform parent,float x0,float y0,float x1,float y1,float height)
        {
            var viewport=Rect(name+" Viewport",parent,x0,y0,x1,y1);viewport.gameObject.AddComponent<RectMask2D>();var scroll=viewport.gameObject.AddComponent<ScrollRect>();var content=Rect(name,viewport,0,1,1,1);content.pivot=new Vector2(.5f,1);
            var group=content.gameObject.AddComponent<VerticalLayoutGroup>();group.spacing=10;group.childControlHeight=true;group.childControlWidth=true;group.childForceExpandHeight=false;group.childForceExpandWidth=true;content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;content.gameObject.AddComponent<AuthoredButtonList>();scroll.content=content;scroll.viewport=viewport;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;return content;
        }
        static Button MakeButton(string name,Transform parent,float x0,float y0,float x1,float y1,string label,Sprite icon,bool effects=false)
        {
            var r=Panel(name,parent,x0,y0,x1,y1,new Color(.16f,.27f,.30f));var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();r.GetComponent<Image>().raycastTarget=true;var le=r.gameObject.AddComponent<LayoutElement>();le.preferredHeight=effects?96:80;var a=r.gameObject.AddComponent<AuthoredActionRow>();
            a.title=Text("Label",r,effects?.12f:.25f,effects?.46f:.08f,.97f,.95f,label,23);a.icon=Image("Action Icon",r,.025f,.22f,effects?.10f:.20f,.82f,icon);a.icon.preserveAspect=true;
            if(effects){a.effects=new AuthoredEffect[3];for(int i=0;i<3;i++){float x=.12f+i*.285f;var slot=Rect("Effect Slot "+i,r,x,.06f,x+.27f,.43f);a.effects[i]=new AuthoredEffect{root=slot.gameObject,icon=Image("Effect",slot,0,0,.14f,1,null),value=Text("Delta",slot,.17f,0,1,1,"",21)};a.effects[i].icon.preserveAspect=true;slot.gameObject.SetActive(false);}}
            return b;
        }
        static Button Back(string name,Transform p,float x0,float y0,float x1,float y1){var b=MakeButton(name,p,x0,y0,x1,y1,"",style.Icon("back"));var row=b.GetComponent<AuthoredActionRow>();row.title.gameObject.SetActive(false);Place(row.icon.rectTransform,.16f,.16f,.84f,.84f);return b;}
        static GameObject SavePrefab(GameObject root,string path){if(File.Exists(path))return AssetDatabase.LoadAssetAtPath<GameObject>(path);return PrefabUtility.SaveAsPrefabAsset(root,path);}
        static RectTransform Rect(string n,Transform p,float a,float b,float c,float d)=>StudioUiWidgets.Rect(n,p,a,b,c,d);
        static RectTransform Panel(string n,Transform p,float a,float b,float c,float d,Color color){var r=Rect(n,p,a,b,c,d);var im=r.gameObject.AddComponent<Image>();im.color=color;im.raycastTarget=false;return r;}
        static Image Image(string n,Transform p,float a,float b,float c,float d,Sprite s)=>StudioUiWidgets.Image(n,p,a,b,c,d,s,Color.white);
        static TMP_Text Text(string n,Transform p,float a,float b,float c,float d,string t,float size)=>StudioUiWidgets.Text(style,n,p,a,b,c,d,t,size);
        static void Place(RectTransform r,float a,float b,float c,float d){r.anchorMin=new Vector2(a,b);r.anchorMax=new Vector2(c,d);r.offsetMin=r.offsetMax=Vector2.zero;}
    }
}
