using System;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ProjectLove.UI
{
    [DefaultExecutionOrder(10)]
    public sealed class RomanceRoomView : MonoBehaviour
    {
        public AuthoredRomanceLayout authored;
        IsometricRoomView view;
        RectTransform hud,roster,feedback,opportunity,overlay;
        Button producer,focus,closePanel;
        GameSession S=>view.Session;
        RomanceSystem R=>S.Romance;
        RoomPresentationSO P=>view.presentation;
        string T(string key)=>view.T(key);
        public Button ProducerButton=>authored!=null?authored.producer:producer;
        public Button FocusButton=>authored!=null?authored.focus:focus;
        public RectTransform Overlay=>authored!=null?authored.ActiveOverlay:overlay;
        public Button PanelBack=>authored!=null?authored.panelBack:closePanel;
        void Start()
        {
            view=GetComponent<IsometricRoomView>();if(!S.Romance.Enabled){enabled=false;return;}
            if(authored!=null){authored.InitializeRuntime(view);return;}
            view.hudText.gameObject.SetActive(false);
            hud=StudioUiWidgets.Rect("Romance HUD",view.hudText.transform.parent,.25f,.05f,.985f,.95f);
            roster=StudioUiWidgets.Rect("Romance Roster",transform,.78f,.30f,.98f,.86f);
            feedback=StudioUiWidgets.Rect("Activity Result",transform,.03f,.26f,.41f,.35f);
            opportunity=Panel("Today Opportunity",transform,.03f,.65f,.41f,.76f,new Color(.045f,.065f,.09f,.95f));
            producer=Button("Producer Activity",transform,.03f,.13f,.31f,.24f,T("Producer"),()=>R.TryStart(RomanceActivity.Producer),P.Icon("work"));
            focus=Button("Weekly Focus",transform,.03f,.79f,.40f,.87f,T("focus"),ShowFocus,P.Icon("sprint_board"));
            Place(view.travelButton,.03f,.03f,.13f,.10f);Place(view.inventoryButton,.14f,.03f,.24f,.10f);Place(view.optionsButton,.25f,.03f,.35f,.10f);
            Place(view.waitButton,.40f,.03f,.52f,.10f);Place(view.nextDayButton,.54f,.03f,.69f,.10f);
            Place(view.hintText.rectTransform,.03f,.36f,.41f,.45f);view.hintText.fontSizeMax=23;
            Place((RectTransform)view.panelTitle.transform.parent,.03f,.14f,.42f,.85f);
            Place((RectTransform)view.speakerText.transform.parent,.03f,.60f,.42f,.86f);
            Place((RectTransform)view.choiceButtons.parent,.03f,.12f,.42f,.56f);
            Place(view.dialoguePortrait.rectTransform,.43f,0,.765f,.86f);view.dialogueText.fontSizeMax=28;
            closePanel=Button("Auxiliary Back",transform,.92f,.03f,.98f,.11f,"",view.ClosePanel,P.Icon("back"));
            IconOnly(closePanel);var back=view.deferButton.GetComponentInChildren<BackArrowGraphic>(true);if(back!=null)back.gameObject.SetActive(false);
            StudioUiWidgets.Image("Back Icon",view.deferButton.transform,.16f,.16f,.84f,.84f,P.Icon("back"),Color.white).preserveAspect=true;
            producer.transform.SetSiblingIndex(view.dialogueModal.transform.GetSiblingIndex());focus.transform.SetSiblingIndex(view.dialogueModal.transform.GetSiblingIndex());
            roster.SetAsLastSibling();Refresh();
        }
        public void Refresh()
        {
            if(authored!=null){authored.Render(view!=null?view:GetComponent<IsometricRoomView>(),!GetComponent<IsometricRoomView>().IsEditorPreview);return;}
            if(view==null||hud==null)return;
            bool busy=S.IsBusy;bool main=view.IsOverviewPanel;
            view.panelTitle.transform.parent.gameObject.SetActive(!main);
            closePanel.gameObject.SetActive(!main&&!S.HasActiveModal);
            producer.gameObject.SetActive(main&&!busy&&!S.State.dayEnded);producer.interactable=R.Preview(RomanceActivity.Producer).allowed;
            focus.gameObject.SetActive(main&&!busy&&!S.State.dayEnded);focus.interactable=!busy;
            opportunity.gameObject.SetActive(main&&!busy&&!S.State.dayEnded);
            feedback.gameObject.SetActive(main&&!busy&&S.State.romance.receipt!=null&&!string.IsNullOrEmpty(S.State.romance.receipt.title));
            view.hintText.gameObject.SetActive(main&&!busy&&S.State.dayEnded);view.hintText.text=S.State.dayEnded?T(S.Days.CampaignEnded?"campaignEnded":"summary"):T("hint");
            Clear(hud);Clear(roster);Clear(feedback);Clear(opportunity);
            StudioUiWidgets.Metric(P,"Day",hud,0,0,.45f,1,P.Icon(S.State.phase==DayPhase.Night?"night":"day"),LocalizationService.Format(T("romanceDay"),S.State.day,S.Config.totalDays,S.State.dayEnded?T("dayEnded"):P.phaseNames[(int)S.State.phase]),-1,Color.white);
            StudioUiWidgets.Metric(P,"Money",hud,.47f,0,.63f,1,P.Icon("money"),S.State.money.ToString(),-1,Color.white);
            StudioUiWidgets.Metric(P,"Project",hud,.66f,0,1,1,P.Icon("progress"),T("projectTitle")+"  "+S.State.mainProgress+"%",S.State.mainProgress/100f,P.progressTint);
            for(int i=0;i<R.Config.cast.Length;i++)
            {
                var c=R.Config.cast[i];float top=1-i/3f;
                var card=Panel("Romance Character "+c.character.id,roster,0,top-.30f,1,top,new Color(.09f,.11f,.16f,.97f));
                StudioUiWidgets.Panel(card.GetComponent<Image>(),P,true);
                StudioUiWidgets.Image("Portrait",card,.025f,.40f,.21f,.96f,c.character.taskAvatar!=null?c.character.taskAvatar:c.character.GetSprite(CharacterExpression.Normal),Color.white).preserveAspect=true;
                StudioUiWidgets.Text(P,"Name",card,.25f,.66f,.97f,.97f,c.character.displayName,26);
                var mood=R.MoodInfo(c.character.id);
                StudioUiWidgets.Image("Mood Icon",card,.25f,.42f,.32f,.65f,P.Icon(mood.icon),Color.white).preserveAspect=true;
                StudioUiWidgets.Text(P,"Mood",card,.36f,.38f,.97f,.67f,mood.label,23);
                StudioUiWidgets.Metric(P,"Affection",card,.05f,.03f,.96f,.37f,P.Icon("heart"),S.State.FindCharacter(c.character.id).affection+"/100",S.State.FindCharacter(c.character.id).affection/100f,P.affectionTint);
            }
            roster.SetAsLastSibling();
            StudioUiWidgets.Text(P,"Today Hint",opportunity,.025f,.28f,.975f,.97f,R.TodayHint,24);
            StudioUiWidgets.Text(P,"Team Baseline",opportunity,.025f,.025f,.975f,.28f,LocalizationService.Format(T("teamDaily"),R.Config.teamDailyProgress),20);
            var goal=R.Milestone;
            focus.GetComponentInChildren<TMP_Text>(true).text=T("focus")+(goal==null?"":"\n"+goal.title+" · "+S.State.mainProgress+"/"+goal.target+"%");
            if(feedback.gameObject.activeSelf)
            {
                var f=S.State.romance.receipt;
                StudioUiWidgets.Text(P,"Result Title",feedback,0,.62f,1,1,f.title+(string.IsNullOrEmpty(f.topicFlag)?"":" · "+T("receiptTopic")),22);
                ResultMetric(feedback,.0f,.31f,"progress",f.project);ResultMetric(feedback,.34f,.64f,"heart",f.affection);ResultMetric(feedback,.67f,.99f,"money",f.money);
            }
            var preview=R.Preview(RomanceActivity.Producer);
            RebuildAction(producer,T("Producer"),new ActionMetric("progress",preview.project),new ActionMetric("money",preview.money),new ActionMetric("time",-1));
            if(R.PendingReport!=null&&overlay==null&&!S.HasActiveModal)ShowReport();
        }
        public void RenderCharacterMenu(CharacterDataSO character)
        {
            if(view==null)view=GetComponent<IsometricRoomView>();
            var f=R.Preview(RomanceActivity.WorkTogether,character.id);
            ActionButton(T("WorkTogether"),"work",()=>R.TryStart(RomanceActivity.WorkTogether,character.id),f.allowed,new ActionMetric("progress",f.project),new ActionMetric("heart",f.affection),new ActionMetric("time",-1));
            var personal=R.Preview(RomanceActivity.PersonalTime,character.id);
            ActionButton(T("PersonalTime"),"heart",()=>R.TryStart(RomanceActivity.PersonalTime,character.id),personal.allowed,new ActionMetric("heart",personal.affection),new ActionMetric("time",-1));
            int gain=S.Interactions.RewardAvailable?Math.Min(100-S.State.FindCharacter(character.id).affection,character.dailyTalkAffection):0;
            ActionButton(gain>0?T("talk"):T("freeTalk"),"talk",()=>S.Interactions.TryTalk(),S.Interactions.CanTalk,new ActionMetric("heart",gain),new ActionMetric("time",0));
            ActionButton(T("gift"),"gift",view.OpenGifts,true);
        }
        void ActionButton(string label,string icon,Action action,bool allowed,params ActionMetric[] metrics)
        {var b=view.AddButton(view.choiceButtons,label,action,allowed,P.Icon(icon));PlanningActionRow.Render(b,P,label,metrics);}
        void RebuildAction(Button b,string label,params ActionMetric[] metrics)
        {
            foreach(Transform t in b.transform)if(t.name.StartsWith("Effect ")||t.name.StartsWith("Delta ")){t.gameObject.SetActive(false);Destroy(t.gameObject);}
            PlanningActionRow.Render(b,P,label,metrics);
        }
        void ResultMetric(Transform parent,float x0,float x1,string icon,int amount)
        {
            StudioUiWidgets.Image(icon,parent,x0,.10f,x0+.055f,.53f,P.Icon(icon),Color.white).preserveAspect=true;
            StudioUiWidgets.Text(P,"Result "+icon,parent,x0+.07f,.05f,x1,.59f,amount>0?"+"+amount:amount.ToString(),24);
        }
        public void ShowFocus()
        {
            if(authored!=null){authored.ShowFocus();return;}
            if(S.IsBusy||S.State.dayEnded)return;
            BeginOverlay("Weekly Support");
            StudioUiWidgets.Text(P,"Focus Title",overlay,.05f,.87f,.93f,.98f,T("focus"),32);
            var m=R.Milestone;
            if(m!=null)
            {
                StudioUiWidgets.Text(P,"Milestone",overlay,.05f,.73f,.94f,.87f,m.title+" · "+LocalizationService.Format(T("milestone"),m.startDay,m.endDay,m.target),25);
                StudioUiWidgets.Bar(P,"Weekly Goal",overlay,.05f,.69f,.94f,.72f,Mathf.Min(1,(float)S.State.mainProgress/m.target),P.progressTint);
            }
            StudioUiWidgets.Text(P,"Focus Hint",overlay,.05f,.57f,.94f,.68f,R.Focus==null?T("focusHint"):LocalizationService.Format(T("focusSelected"),R.Cast(R.Focus.characterId).character.displayName),24);
            for(int i=0;i<R.Config.cast.Length;i++)
            {
                var c=R.Config.cast[i];float x=.05f+i*.30f;
                var card=Panel("Support "+c.character.id,overlay,x,.23f,x+.275f,.56f,new Color(.13f,.18f,.24f));
                StudioUiWidgets.Image("Owner",card,.04f,.59f,.32f,.96f,c.character.taskAvatar,Color.white).preserveAspect=true;
                StudioUiWidgets.Text(P,"Name",card,.36f,.62f,.96f,.94f,c.character.displayName,26);
                StudioUiWidgets.Text(P,"Work",card,.05f,.37f,.96f,.62f,c.workTitle,23);
                StudioUiWidgets.Text(P,"Topic",card,.05f,.20f,.96f,.40f,c.topicTitle,21);
                var pick=Button("Choose Focus "+c.character.id,card,.04f,.025f,.96f,.20f,LocalizationService.Format(T("focusPick"),c.character.displayName),()=>
                {
                    CloseOverlay();R.TryFocus(c.character.id);ShowFocus();
                },P.Icon("heart"));pick.interactable=R.Focus==null;
            }
            StudioUiWidgets.Text(P,"Focus Benefit",overlay,.05f,.09f,.94f,.22f,R.Focus==null?LocalizationService.Format(T("focusBenefit"),R.Config.focusAffection):T("focusLater"),24);
            BackOverlay();
        }
        void ShowReport()
        {
            var r=R.PendingReport;if(r==null)return;BeginOverlay("Morning Recap");
            StudioUiWidgets.Text(P,"Recap Title",overlay,.06f,.86f,.94f,.98f,S.Days.CampaignEnded?T("finalRecap"):T("recap"),34);
            StudioUiWidgets.Text(P,"Project Growth",overlay,.06f,.72f,.94f,.85f,LocalizationService.Format(T("recapProject"),r.projectBefore,r.projectAfter),32);
            StudioUiWidgets.Bar(P,"Project Growth Bar",overlay,.06f,.68f,.94f,.71f,r.projectAfter/100f,P.progressTint);
            StudioUiWidgets.Text(P,"Growth Sources",overlay,.06f,.59f,.94f,.68f,LocalizationService.Format(T("recapTeam"),r.teamGain,r.projectAfter-r.projectBefore-r.teamGain),24);
            StudioUiWidgets.Text(P,"Money Growth",overlay,.06f,.51f,.94f,.59f,LocalizationService.Format(T("recapMoney"),r.moneyBefore,r.moneyAfter),23);
            for(int i=0;i<R.Config.cast.Length;i++)
            {
                var c=R.Config.cast[i];int gain=r.affectionAfter.First(a=>a.characterId==c.character.id).value-r.affectionBefore.First(a=>a.characterId==c.character.id).value;
                float x=.06f+i*.295f;
                StudioUiWidgets.Image("Recap Heart",overlay,x,.405f,x+.04f,.47f,P.Icon("heart"),P.affectionTint).preserveAspect=true;
                StudioUiWidgets.Text(P,"Closer "+c.character.id,overlay,x+.045f,.39f,x+.28f,.49f,LocalizationService.Format(T("recapAffection"),c.character.displayName,gain),23);
            }
            string topics=string.Join(" · ",r.newTopics.Select(t=>R.Config.cast.First(c=>c.topicFlag==t).topicTitle));
            StudioUiWidgets.Text(P,"New Topics",overlay,.06f,.29f,.94f,.39f,string.IsNullOrEmpty(topics)?T("recapQuiet"):T("newTopic")+": "+topics,23);
            StudioUiWidgets.Text(P,"Today",overlay,.06f,.16f,.94f,.28f,R.TodayHint,24);
            Button("Begin Today",overlay,.55f,.04f,.94f,.14f,T("recapContinue"),()=>{R.AcknowledgeReport();CloseOverlay();},P.Icon("day"));
        }
        void BeginOverlay(string name)
        {
            S.BoardOpen=true;overlay=Panel(name,transform,.015f,.12f,.765f,.885f,new Color(.045f,.065f,.09f,.99f));overlay.GetComponent<Image>().raycastTarget=true;view.Refresh();overlay.SetAsLastSibling();roster.SetAsLastSibling();
        }
        void BackOverlay(){var b=Button("Focus Back",overlay,.90f,.025f,.98f,.12f,"",CloseOverlay,P.Icon("back"));IconOnly(b);}
        public void CloseOverlay()
        {if(authored!=null){authored.CloseOverlay();return;}if(overlay!=null){overlay.gameObject.SetActive(false);Destroy(overlay.gameObject);overlay=null;}S.BoardOpen=false;view.Refresh();}
        void OnDestroy(){if(view!=null&&view.Session!=null)view.Session.BoardOpen=false;}
        Button Button(string name,Transform parent,float x0,float y0,float x1,float y1,string title,Action onClick,Sprite icon)
        {
            var b=Instantiate(view.buttonTemplate,parent);b.gameObject.SetActive(true);b.name=name;Place(b,x0,y0,x1,y1);b.GetComponentInChildren<TMP_Text>(true).text=title;b.onClick.AddListener(()=>onClick());StudioUiWidgets.DecorateButton(b,P,icon);return b;
        }
        static void IconOnly(Button b)
        {b.GetComponentInChildren<TMP_Text>(true).gameObject.SetActive(false);var icon=b.transform.Find("Action Icon") as RectTransform;if(icon!=null)Place(icon,.16f,.16f,.84f,.84f);}
        static RectTransform Panel(string name,Transform parent,float x0,float y0,float x1,float y1,Color color)
        {var r=StudioUiWidgets.Rect(name,parent,x0,y0,x1,y1);var i=r.gameObject.AddComponent<Image>();i.color=color;i.raycastTarget=false;return r;}
        static void Place(Button b,float x0,float y0,float x1,float y1)=>Place((RectTransform)b.transform,x0,y0,x1,y1);
        static void Place(RectTransform r,float x0,float y0,float x1,float y1){r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;}
        static void Clear(Transform root){foreach(Transform c in root){c.gameObject.SetActive(false);Destroy(c.gameObject);}}
    }
}
