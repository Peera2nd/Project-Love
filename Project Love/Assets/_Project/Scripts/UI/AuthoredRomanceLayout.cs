using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ProjectLove.UI
{
    [Serializable] public sealed class AuthoredCastCard { public Image portrait,moodIcon;public TMP_Text name,mood;public AuthoredMetric affection; }
    [Serializable] public sealed class AuthoredFocusCard { public Image portrait;public TMP_Text name,work,topic;public Button choose; }
    public enum AuthoringScreen { Room, Conversation, Gifts, WeeklyFocus, MorningRecap }
    public sealed class AuthoredRomanceLayout : MonoBehaviour
    {
        public IsometricRoomView view;
        public AuthoredMetric day,money,project;
        public GameObject roster,opportunity,feedback;
        public TMP_Text todayHint,baseline,receiptTitle;
        public AuthoredMetric[] receiptMetrics;
        public AuthoredCastCard[] cast;
        public Button producer,focus,panelBack;
        public RectTransform focusRoot,recapRoot;
        public TMP_Text focusTitle,milestone,focusHint,focusBenefit;
        public AuthoredMetric focusGoal;
        public AuthoredFocusCard[] focusCards;
        public Button focusBack,recapContinue;
        public TMP_Text recapTitle,recapProject,recapSources,recapMoney,recapTopics,recapToday;
        public AuthoredMetric recapGoal;
        public TMP_Text[] recapAffection;
        public Button hotspotTemplate,bubbleTemplate;
        public List<Button> roomObjects=new List<Button>(),bubbles=new List<Button>();
        [Tooltip("ตำแหน่งบับเบิลเทียบกับขนาดตัวละคร")]public Vector2 bubbleOffset=new Vector2(0,.40f);
        GameSession S=>view.Session;
        RoomPresentationSO P=>view.presentation;
        string T(string k)=>view.T(k);
        string CastText(RomanceCast c,string field,string value)=>S.Localization.Content(S.Romance.Config,"cast/"+c.character.id+"/"+field,value);
        string GoalText(RomanceMilestone m)=>S.Localization.Content(S.Romance.Config,"milestone/"+m.localizationId,m.title);
        public RectTransform ActiveOverlay=>focusRoot.gameObject.activeSelf?focusRoot:recapRoot.gameObject.activeSelf?recapRoot:null;
        public void InitializeRuntime(IsometricRoomView owner)
        {
            view=owner;focusRoot.gameObject.SetActive(false);recapRoot.gameObject.SetActive(false);
            producer.onClick.AddListener(()=>S.Romance.TryStart(RomanceActivity.Producer));focus.onClick.AddListener(ShowFocus);
            panelBack.onClick.AddListener(view.ClosePanel);focusBack.onClick.AddListener(CloseOverlay);
            recapContinue.onClick.AddListener(()=>{S.Romance.AcknowledgeReport();CloseOverlay();});Render(owner);
        }
        public void Render(IsometricRoomView owner,bool allowReport=true)
        {
            view=owner;if(S==null)return;
            var r=S.Romance;bool busy=S.IsBusy,main=view.IsOverviewPanel;
            view.hudText.gameObject.SetActive(false);view.panelTitle.transform.parent.gameObject.SetActive(!main);
            panelBack.gameObject.SetActive(!main&&!S.HasActiveModal);
            producer.gameObject.SetActive(main&&!busy&&!S.State.dayEnded);producer.interactable=r.Preview(RomanceActivity.Producer).allowed;
            focus.gameObject.SetActive(main&&!busy&&!S.State.dayEnded);focus.interactable=!busy;
            opportunity.SetActive(main&&!busy&&!S.State.dayEnded);feedback.SetActive(main&&!busy&&S.State.romance.receipt!=null&&!string.IsNullOrEmpty(S.State.romance.receipt.title));
            view.hintText.gameObject.SetActive(main&&!busy&&S.State.dayEnded);
            day.Bind(P.Icon(S.State.phase==DayPhase.Night?"night":"day"),LocalizationService.Format(T("romanceDay"),S.State.day,S.Config.totalDays,S.State.dayEnded?T("dayEnded"):view.Phase(S.State.phase)));
            money.Bind(P.Icon("money"),S.State.money.ToString());project.Bind(P.Icon("progress"),T("projectTitle")+"  "+S.State.mainProgress+"%",S.State.mainProgress/100f);
            for(int i=0;i<cast.Length;i++)
            {
                bool visible=i<r.Config.cast.Length;cast[i].name.transform.parent.gameObject.SetActive(visible);if(!visible)continue;
                var c=r.Config.cast[i];var card=cast[i];var m=r.MoodInfo(c.character.id);card.name.text=view.Name(c.character);card.portrait.sprite=c.character.taskAvatar;
                card.mood.text=S.Localization.Content(r.Config,"mood/"+m.mood,m.label);card.moodIcon.sprite=P.Icon(m.icon);int love=S.State.FindCharacter(c.character.id).affection;card.affection.Bind(P.Icon("heart"),love+"/100",love/100f);
            }
            todayHint.text=r.TodayHint;baseline.text=LocalizationService.Format(T("teamDaily"),r.Config.teamDailyProgress);
            var goal=r.Milestone;focus.GetComponent<AuthoredActionRow>().title.text=T("focus")+(goal==null?"":"\n"+GoalText(goal)+" · "+S.State.mainProgress+"/"+goal.target+"%");
            var f=r.Preview(RomanceActivity.Producer);producer.GetComponent<AuthoredActionRow>().Bind(P,T("Producer"),new ActionMetric("progress",f.project),new ActionMetric("money",f.money),new ActionMetric("time",-1));
            if(feedback.activeSelf)
            {
                var a=S.State.romance.receipt;receiptTitle.text=S.Localization.Text(a.titleKey,a.title)+(string.IsNullOrEmpty(a.topicFlag)?"":" · "+T("receiptTopic"));
                string Sign(int n)=>n>0?"+"+n:n.ToString();receiptMetrics[0].Bind(P.Icon("progress"),Sign(a.project));receiptMetrics[1].Bind(P.Icon("heart"),Sign(a.affection));receiptMetrics[2].Bind(P.Icon("money"),Sign(a.money));
            }
            if(allowReport&&r.PendingReport!=null&&ActiveOverlay==null&&!S.HasActiveModal)ShowReport();
        }
        public void ShowFocus()
        {
            if(S.IsBusy||S.State.dayEnded)return;S.BoardOpen=true;focusRoot.gameObject.SetActive(true);recapRoot.gameObject.SetActive(false);BindFocus();view.Refresh();
        }
        public void BindFocus()
        {
            var r=S.Romance;focusTitle.text=T("focus");var goal=r.Milestone;
            milestone.text=goal==null?"":GoalText(goal)+" · "+LocalizationService.Format(T("milestone"),goal.startDay,goal.endDay,goal.target);
            focusGoal.Bind(null,"",goal==null?0:Mathf.Min(1,(float)S.State.mainProgress/goal.target));
            focusHint.text=r.Focus==null?T("focusHint"):LocalizationService.Format(T("focusSelected"),view.Name(r.Cast(r.Focus.characterId).character));
            for(int i=0;i<focusCards.Length;i++)
            {
                var card=focusCards[i];bool visible=i<r.Config.cast.Length;card.name.transform.parent.gameObject.SetActive(visible);if(!visible)continue;
                var c=r.Config.cast[i];card.name.text=view.Name(c.character);card.portrait.sprite=c.character.taskAvatar;card.work.text=CastText(c,"workTitle",c.workTitle);card.topic.text=CastText(c,"topicTitle",c.topicTitle);
                card.choose.name="Choose Focus "+c.character.id;card.choose.GetComponent<AuthoredActionRow>().title.text=LocalizationService.Format(T("focusPick"),view.Name(c.character));card.choose.interactable=r.Focus==null;
                card.choose.onClick.RemoveAllListeners();card.choose.onClick.AddListener(()=>{CloseOverlay();r.TryFocus(c.character.id);ShowFocus();});
            }
            focusBenefit.text=r.Focus==null?LocalizationService.Format(T("focusBenefit"),r.Config.focusAffection):T("focusLater");
        }
        void ShowReport(){S.BoardOpen=true;recapRoot.gameObject.SetActive(true);focusRoot.gameObject.SetActive(false);BindReport(S.Romance.PendingReport);view.Refresh();}
        public void BindReport(RomanceDayReport a)
        {
            if(a==null)return;var r=S.Romance;recapTitle.text=S.Days.CampaignEnded?T("finalRecap"):T("recap");
            recapProject.text=LocalizationService.Format(T("recapProject"),a.projectBefore,a.projectAfter);recapGoal.Bind(null,"",a.projectAfter/100f);
            recapSources.text=LocalizationService.Format(T("recapTeam"),a.teamGain,a.projectAfter-a.projectBefore-a.teamGain);recapMoney.text=LocalizationService.Format(T("recapMoney"),a.moneyBefore,a.moneyAfter);
            for(int i=0;i<recapAffection.Length&&i<r.Config.cast.Length;i++){var c=r.Config.cast[i];int delta=a.affectionAfter.First(v=>v.characterId==c.character.id).value-a.affectionBefore.First(v=>v.characterId==c.character.id).value;recapAffection[i].text=LocalizationService.Format(T("recapAffection"),view.Name(c.character),delta);}
            string topics=string.Join(" · ",a.newTopics.Select(t=>CastText(r.Config.cast.First(c=>c.topicFlag==t),"topicTitle",r.Config.cast.First(c=>c.topicFlag==t).topicTitle)));recapTopics.text=string.IsNullOrEmpty(topics)?T("recapQuiet"):T("newTopic")+": "+topics;recapToday.text=r.TodayHint;
            recapContinue.GetComponent<AuthoredActionRow>().title.text=T("recapContinue");
        }
        public void CloseOverlay(){focusRoot.gameObject.SetActive(false);recapRoot.gameObject.SetActive(false);S.BoardOpen=false;view.Refresh();}
        public void RenderRoom(GameSession session)
        {
            BindRoom(session.CurrentRoom,session.State.phase,session.State.dayEnded,session);
        }
        public void BindRoom(RoomDataSO room,DayPhase phase,bool ended=false,GameSession session=null)
        {
            view.roomImage.sprite=room.Background(phase,ended);foreach(var b in roomObjects)b.gameObject.SetActive(false);foreach(var b in bubbles)b.gameObject.SetActive(false);
            view.HotspotButtons.Clear();view.EventBubbleButtons.Clear();if(ended)return;
            foreach(var h in room.hotspots)
            {
                var place=h.placements.FirstOrDefault(p=>p.phase==phase&&p.visible);if(place==null||session!=null&&h.kind==RoomInteractionKind.Pickup&&session.State.collectedPickups.Contains(h.id))continue;
                var b=roomObjects.FirstOrDefault(o=>o.name==h.id);if(b==null){b=Instantiate(hotspotTemplate,view.hotspotRoot);b.name=h.id;roomObjects.Add(b);}
                b.gameObject.SetActive(true);var rect=(RectTransform)b.transform;rect.anchorMin=place.position-place.size/2;rect.anchorMax=place.position+place.size/2;rect.offsetMin=rect.offsetMax=Vector2.zero;
                var im=b.GetComponent<Image>();im.sprite=place.sprite;im.color=place.sprite==null?new Color(.2f,.4f,.45f,.3f):Color.white;im.alphaHitTestMinimumThreshold=place.sprite==null?0:.1f;
                b.interactable=session!=null&&!session.IsBusy;b.onClick.RemoveAllListeners();var target=h;b.onClick.AddListener(()=>view.SelectHotspot(target));view.HotspotButtons[h.id]=b;
            }
            if(session==null)return;
            foreach(var e in session.Events.BubbleEvents())
            {
                var h=room.hotspots.First(o=>o.character==e.character);var p=h.placements.First(o=>o.phase==phase);
                var b=bubbles.FirstOrDefault(o=>o.name=="Event Bubble "+e.id);if(b==null){b=Instantiate(bubbleTemplate,view.hotspotRoot);b.name="Event Bubble "+e.id;bubbles.Add(b);}
                b.gameObject.SetActive(true);var rect=(RectTransform)b.transform;rect.anchorMin=rect.anchorMax=p.position+Vector2.Scale(p.size,bubbleOffset);rect.anchoredPosition=Vector2.zero;
                b.onClick.RemoveAllListeners();var target=e;b.onClick.AddListener(()=>session.Dialogue.TryBegin(target));view.EventBubbleButtons[e.id]=b;
            }
        }
        // Used only on an isolated prefab instance in the Editor preview scene.
        public void PreviewScreen(AuthoringScreen screen,RomanceDayReport report=null)
        {
            S.BoardOpen=screen==AuthoringScreen.WeeklyFocus||screen==AuthoringScreen.MorningRecap;
            Render(view,false);focusRoot.gameObject.SetActive(screen==AuthoringScreen.WeeklyFocus);recapRoot.gameObject.SetActive(screen==AuthoringScreen.MorningRecap);
            if(screen==AuthoringScreen.WeeklyFocus)BindFocus();if(screen==AuthoringScreen.MorningRecap)BindReport(report);
        }
    }
}
