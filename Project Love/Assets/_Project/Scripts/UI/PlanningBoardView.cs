using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace ProjectLove.UI
{
    [DefaultExecutionOrder(20)]
    public sealed class PlanningBoardView : MonoBehaviour
    {
        private IsometricRoomView view;
        private RoomPresentationSO style;
        private RectTransform root,hud,roster,board,detail,producerMetric;
        private Button openButton,phaseButton,applyButton,producerButton,panelBack,restButton;
        private TMP_Text draftLabel,notice;
        private PlanningDraft draft;
        private GameSession lastSession;
        private string selected;
        private readonly Dictionary<string,PlanningCardDrag> cards=new Dictionary<string,PlanningCardDrag>();
        private readonly Dictionary<string,RectTransform> lanes=new Dictionary<string,RectTransform>();
        private Dictionary<string,TaskProjection> projections;
        public PlanningTimelineView Timeline {get;private set;}
        public bool TimelineMode {get;private set;}
        public bool HasDrag=>Dragging!=null||(Timeline!=null&&Timeline.IsDragging);
        public RoomPresentationSO Style=>style;
        public string SelectedTaskId=>selected;
        public IReadOnlyDictionary<string,TaskProjection> Projections=>projections;
        public TaskState[] VisibleTasks=>Session.State.production.tasks.Where(t=>(Work.Definition(t).sprint==Work.CurrentSprint||t.acceptedDay==0&&Work.IsReleased(t))&& !Work.Config.events.Any(e=>e.tasks.Contains(Work.Definition(t))&&!Session.State.production.events.Any(r=>r.eventId==e.id))).ToArray();
        public bool IsOpen=>board!=null&&board.gameObject.activeInHierarchy;
        public RectTransform Board=>board;
        public PlanningDraft Draft=>draft;
        public IReadOnlyDictionary<string,PlanningCardDrag> Cards=>cards;
        public PlanningCardDrag Dragging {get;private set;}
        public GameSession Session=>view.Session;
        private ProductionSystem Work=>Session.Production;
        private string T(string key)=>style.Get(key);
        private void Start()
        {
            view=GetComponent<IsometricRoomView>();style=view.presentation;
            root=StudioUiWidgets.Rect("Planning Shell",transform,0,0,1,1);
            hud=StudioUiWidgets.Rect("Planning HUD",root,.25f,.90f,.985f,.985f);view.hudText.gameObject.SetActive(false);
            roster=StudioUiWidgets.Rect("Permanent Team",root,.775f,.16f,.98f,.88f);
            openButton=Icon("Planning Board",transform,.03f,.81f,.085f,.88f,"sprint_board",T("planningTitle"),ShowBoard);
            phaseButton=Icon("Planning Pass",transform,.095f,.81f,.15f,.88f,"time",T("passPhase"),()=>Session.TryAdvancePhase());
            openButton.transform.SetSiblingIndex(view.dialogueModal.transform.GetSiblingIndex());phaseButton.transform.SetSiblingIndex(view.dialogueModal.transform.GetSiblingIndex());
            var panel=(RectTransform)view.panelTitle.transform.parent;Place(panel,.035f,.19f,.735f,.79f);
            var speech=(RectTransform)view.speakerText.transform.parent;Place(speech,.03f,.65f,.42f,.93f);
            view.dialogueText.fontSizeMax=28;Place((RectTransform)view.choiceButtons.parent,.03f,.16f,.42f,.60f);
            Place(view.dialoguePortrait.rectTransform,.43f,.02f,.765f,.88f);
            var vector=view.deferButton.GetComponentInChildren<BackArrowGraphic>(true);if(vector!=null)vector.gameObject.SetActive(false);
            StudioUiWidgets.Image("Planning Back Icon",view.deferButton.transform,.16f,.16f,.84f,.84f,style.Icon("back"),Color.white).preserveAspect=true;
            StyleNav(view.travelButton,.03f,"travel");StyleNav(view.inventoryButton,.095f,"bag");StyleNav(view.optionsButton,.16f,"settings");
            restButton=Icon("Planning Rest",transform,.225f,.035f,.28f,.105f,"rest",T("planningPlayerRest"),view.OpenRestMenu);
            restButton.transform.SetSiblingIndex(view.dialogueModal.transform.GetSiblingIndex());
            StyleNav(view.waitButton,.29f,"night");StyleNav(view.nextDayButton,.355f,"day");
            view.hintText.gameObject.SetActive(false);
            BuildBoard();hud.SetAsLastSibling();roster.SetAsLastSibling();
            panelBack=Icon("Planning Panel Back",root,.92f,.03f,.98f,.11f,"back",T("close"),view.ClosePanel);Refresh();
        }
        private void Update()
        {
            if(!IsOpen)return;
            if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)
            {if(HasDrag)CancelActiveDrag();else CloseBoard();}
        }
        private void OnApplicationFocus(bool focused){if(!focused&&HasDrag)CancelActiveDrag();}
        private void OnDisable(){if(HasDrag)CancelActiveDrag();}
        private void StyleNav(Button button,float x,string icon)
        {
            Place((RectTransform)button.transform,x,.035f,x+.055f,.105f);
            StudioUiWidgets.Image("Compact Icon",button.transform,.18f,.14f,.82f,.86f,style.Icon(icon),Color.white).preserveAspect=true;
            button.GetComponentInChildren<TMP_Text>(true).gameObject.SetActive(false);
            var old=button.transform.Find("Action Icon");if(old!=null)old.gameObject.SetActive(false);
            StudioUiWidgets.Hint(button.gameObject,style,button.GetComponentInChildren<TMP_Text>(true).text);
        }
        private void BuildBoard()
        {
            board=Panel("Planning Board Modal",root,0,0,1,1,new Color(.01f,.02f,.035f,.94f));
            var area=Panel("Planning Workspace",board,.02f,.14f,.755f,.88f,new Color(.06f,.09f,.13f));
            Text("Board Title",area,.025f,.92f,.57f,.99f,T("planningTitle"),30);
            draftLabel=Text("Draft State",area,.025f,.855f,.61f,.92f,T("planningQueueHint"),19);
            Icon("Queue Mode",area,.68f,.87f,.75f,.98f,"sprint_board",T("timelineQueue"),()=>ShowTimeline(false));
            Icon("Timeline Mode",area,.76f,.87f,.83f,.98f,"time",T("timelineTitle"),()=>ShowTimeline(true));
            applyButton=Icon("Apply Plan",area,.85f,.87f,.92f,.98f,"review",T("planningApply"),Apply);
            Icon("Revert Plan",area,.93f,.87f,.995f,.98f,"back",T("planningRevert"),Revert);
            for(int i=0;i<Work.Config.workers.Length;i++)
            {
                var worker=Work.Config.workers[i];float x=.02f+i*.325f;
                var lane=Panel("Lane "+worker.character.id,area,x,.27f,x+.31f,.85f,new Color(.035f,.055f,.08f));
                Text("Lane Owner",lane,.04f,.88f,.96f,.995f,worker.character.displayName+" · "+worker.stream.displayName,21);
                var viewport=StudioUiWidgets.Rect("Lane Viewport",lane,.025f,.02f,.975f,.875f);
                viewport.gameObject.AddComponent<RectMask2D>();
                var content=StudioUiWidgets.Rect("Queue",viewport,0,1,1,1);content.pivot=new Vector2(.5f,1);
                var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=8;layout.childControlHeight=layout.childControlWidth=true;
                layout.childForceExpandHeight=false;content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
                var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
                var drop=viewport.gameObject.AddComponent<PlanningLaneDrop>();drop.ownerId=worker.character.id;drop.board=this;
                var hit=viewport.gameObject.AddComponent<Image>();hit.color=Color.clear;
                lanes[worker.character.id]=content;
            }
            Timeline=gameObject.AddComponent<PlanningTimelineView>();Timeline.Initialize(this,area);
            detail=Panel("Selected Task",area,.02f,.035f,.98f,.25f,new Color(.025f,.045f,.065f));
            notice=Text("Plan Notice",board,.03f,.105f,.75f,.14f,"",18);
            Icon("Planning Back",board,.92f,.03f,.98f,.11f,"back",T("close"),CloseBoard);
            producerButton=Icon("Planning Producer",board,.025f,.025f,.085f,.095f,"producer",string.Format(T("planningProducer"),Work.Config.producerCost,Work.Config.producerMoney,Work.Config.producerEffort),()=>Act(ProductionAction.Producer));
            producerMetric=StudioUiWidgets.Rect("Producer Progress",board,.10f,.025f,.38f,.095f);
            board.gameObject.SetActive(false);
        }
        public void Refresh()
        {
            if(root==null)return;
            if(lastSession!=Session){lastSession=Session;draft=null;board.gameObject.SetActive(false);}
            openButton.interactable=!Session.IsBusy;phaseButton.interactable=!Session.IsBusy&&!Session.State.dayEnded;
            var panel=view.panelTitle.transform.parent.gameObject;panel.SetActive(!IsOpen&&!view.IsOverviewPanel);
            panelBack.gameObject.SetActive(!IsOpen&&!view.IsOverviewPanel&&!Session.IsBusy);
            foreach(var nav in new[]{view.travelButton,view.inventoryButton,view.optionsButton,view.waitButton})nav.gameObject.SetActive(!IsOpen);
            view.nextDayButton.gameObject.SetActive(!IsOpen&&Session.State.dayEnded&&!Session.Days.CampaignEnded);
            restButton.gameObject.SetActive(!IsOpen);restButton.interactable=!Session.IsBusy&&!Session.State.dayEnded&&view.RestHotspot!=null;
            RenderHud();RenderRoster();
            if(IsOpen&&!HasDrag)RenderCards();
            root.SetAsLastSibling();
        }
        private void RenderHud()
        {
            Clear(hud);var s=Session.State;
            StudioUiWidgets.Metric(style,"Planning Day",hud,0,0,.37f,1,style.Icon(s.phase==DayPhase.Night?"night":"day"),
                string.Format(style.Get("hudDay"),s.day,Session.Config.totalDays,s.dayEnded?T("dayEnded"):style.phaseNames[(int)s.phase]),-1,Color.white);
            StudioUiWidgets.Metric(style,"Planning Money",hud,.38f,0,.55f,1,style.Icon("money"),s.money.ToString(),-1,Color.white);
            StudioUiWidgets.Metric(style,"Planning Energy",hud,.56f,0,.79f,1,style.Icon("stamina"),s.stamina+"/"+Session.Config.maxStamina,
                s.stamina/(float)Session.Config.maxStamina,style.staminaTint);
            StudioUiWidgets.Metric(style,"Planning Scope",hud,.80f,0,1,1,style.Icon("progress"),s.mainProgress+"%",s.mainProgress/100f,style.progressTint);
        }
        private void RenderRoster()
        {
            Clear(roster);
            for(int i=0;i<Work.Config.workers.Length;i++)
            {
                var worker=Work.Config.workers[i];var person=Session.State.FindCharacter(worker.character.id);var task=Work.Active(worker.character);
                float top=1-i/(float)Work.Config.workers.Length;var card=Panel("Status "+worker.character.id,roster,0,top-.315f,1,top,Color.white);StudioUiWidgets.Panel(card.GetComponent<Image>(),style,true);
                StudioUiWidgets.Image("Avatar",card,.035f,.70f,.23f,.96f,worker.character.taskAvatar,Color.white).preserveAspect=true;
                Text("Name",card,.27f,.73f,.96f,.97f,worker.character.displayName,24);
                MetricRow(card,"Affection",.49f,"heart",person.affection,100,style.affectionTint);
                MetricRow(card,"Stress",.27f,"stress",person.stress,100,style.stressTint);
                int effort=Session.State.production.tasks.Where(t=>Work.Definition(t).sprint==Work.CurrentSprint && Work.Definition(t).stream==worker.stream && Work.IsReleased(t)).Sum(t=>t.effort),goal=Math.Max(1,Work.Required(worker.stream));
                MetricRow(card,"Work",.05f,"work",effort,goal,worker.stream.tint);
                string reason=task==null?T("planningBlocked"):Work.Definition(task).displayName;
                StudioUiWidgets.Hint(card.gameObject,style,reason);
                string icon=Work.HasOrder(worker.character.id,StaffOrderKind.Rest)?"rest":Work.HasOrder(worker.character.id,StaffOrderKind.Overtime)?"overtime":null;
                if(icon!=null)StudioUiWidgets.Image("Order",card,.82f,.74f,.96f,.96f,style.Icon(icon),Color.white).preserveAspect=true;
            }
        }
        private void MetricRow(Transform parent,string name,float y,string icon,int value,int maximum,Color tint)
        {
            StudioUiWidgets.Image(name+" Icon",parent,.035f,y,.14f,y+.17f,style.Icon(icon),Color.white).preserveAspect=true;
            Text(name+" Value",parent,.18f,y+.055f,.96f,y+.18f,value+"/"+maximum,19);
            StudioUiWidgets.Bar(style,name+" Bar",parent,.18f,y,.94f,y+.035f,value/(float)maximum,tint);
        }
        public void ShowBoard()
        {
            if(root==null||Session.IsBusy)return;draft=Work.CreateDraft();selected=null;notice.text=TimelineMode?T("timelineLegend"):"";Session.BoardOpen=true;board.gameObject.SetActive(true);
            UpdateProjection();RenderCards();Refresh();
        }
        public void CloseBoard()
        {
            if(HasDrag){CancelActiveDrag();return;}
            draft=null;Session.BoardOpen=false;board.gameObject.SetActive(false);view.Refresh();
        }
        private void Apply()
        {
            if(HasDrag)return;
            if(!Work.TryApplyDraft(draft)){notice.text=T("planningStale");return;}
            CloseBoard();
        }
        private void Revert(){if(HasDrag)CancelActiveDrag();draft=Work.CreateDraft();UpdateProjection();RenderCards();}
        private void Act(ProductionAction action,string id=null)
        {if(Work.DraftChanged(draft))return;CloseBoard();Work.TryAct(action,id);}
        private void UpdateProjection(){projections=Work.ProjectPlan(draft).ToDictionary(p=>p.taskId);}
        private void RenderCards()
        {
            if(draft==null)return;
            var visible=VisibleTasks;
            foreach(var card in cards.Values)card.gameObject.SetActive(false);
            foreach(var task in visible.OrderBy(t=>draft.tasks.Single(e=>e.taskId==t.taskId).priority))
            {
                if(!cards.TryGetValue(task.taskId,out var card))
                {
                    var go=Panel("Plan Card "+task.taskId,lanes[task.ownerId],0,0,1,1,Color.white);go.gameObject.AddComponent<LayoutElement>().preferredHeight=132;
                    StudioUiWidgets.Panel(go.GetComponent<Image>(),style,true);
                    var button=go.gameObject.AddComponent<Button>();string id=task.taskId;button.onClick.AddListener(()=>Select(id));
                    card=go.gameObject.AddComponent<PlanningCardDrag>();card.board=this;card.taskId=task.taskId;cards.Add(task.taskId,card);
                    var data=Work.Definition(task);
                    StudioUiWidgets.Image("Discipline",go,0,.03f,.018f,.97f,null,data.stream.tint);
                    StudioUiWidgets.Image("Owner",go,.05f,.69f,.19f,.98f,data.owner.taskAvatar,Color.white).preserveAspect=true;
                    Text("Title",go,.23f,.59f,.95f,.99f,data.displayName,21);
                    Text("State",go,.05f,.37f,.95f,.62f,"",17);
                    Text("Due",go,.05f,.07f,.95f,.28f,"",16);
                }
                card.gameObject.SetActive(true);card.transform.SetParent(lanes[task.ownerId],false);card.transform.SetAsLastSibling();
                var labels=card.GetComponentsInChildren<TMP_Text>();var p=draft.tasks.Single(e=>e.taskId==task.taskId);
                labels.Single(t=>t.name=="State").text=task.acceptedDay>0?T("planningDone"):task.effort==Work.Definition(task).effort?T("planningReview"):Work.PlanFor(task.taskId).start>Work.Cursor?T("planningFuture"):Work.IsReady(task)?T("planningReady"):T("planningBlocked");
                labels.Single(t=>t.name=="Due").text=string.Format(T("planningDue"),Date(p.due));
                var old=card.transform.Find("Progress");if(old!=null){old.gameObject.SetActive(false);Destroy(old.gameObject);}
                StudioUiWidgets.Bar(style,"Progress",card.transform,.05f,.29f,.95f,.37f,task.effort/(float)Work.Definition(task).effort,Work.Definition(task).stream.tint);
                bool risk=projections!=null&&projections[task.taskId].atRisk;
                card.GetComponent<Image>().color=task.acceptedDay>0?new Color(.55f,.65f,.68f):selected==task.taskId?new Color(.64f,.86f,.95f):Color.white;
                if(risk&&task.acceptedDay==0)labels.Single(t=>t.name=="State").text+=" · !";
            }
            applyButton.interactable=!Session.State.dayEnded&&Work.DraftChanged(draft);
            draftLabel.text=Work.DraftChanged(draft)?T("planningDirty"):T("planningQueueHint");
            producerButton.interactable=!Work.DraftChanged(draft)&&Work.Preview(ProductionAction.Producer).allowed;
            Clear(producerMetric);var stage=Work.ProducerStage;int releasedGoal=Work.Config.sprints.Where(s=>s.startDay<=Session.State.day).Sum(s=>s.producerGoal);
            StudioUiWidgets.Metric(style,"Producer Duty",producerMetric,0,0,1,1,style.Icon("producer"),
                string.Format(T("planningProducerProgress"),stage==null?releasedGoal:Work.ProducerCompleted,stage==null?releasedGoal:stage.producerGoal),
                stage==null?1:Work.ProducerCompleted/(float)stage.producerGoal,style.progressTint);
            RenderDetail();
            foreach(var lane in lanes.Values)lane.parent.gameObject.SetActive(!TimelineMode);
            Timeline.SetVisible(TimelineMode);if(TimelineMode&&!Timeline.IsDragging)Timeline.Render();
        }
        public string Date(int cursor)=>string.Format(T("planningDay"),cursor/4+1,style.phaseNames[cursor%4]);
        public void Select(string id){if(HasDrag)return;selected=id;RenderCards();}
        private void RenderDetail()
        {
            Clear(detail);
            if(selected==null){Text("Selection Hint",detail,.025f,.15f,.97f,.85f,T("planningSelect"),22);return;}
            var task=Work.Find(selected);var data=Work.Definition(task);var edit=draft.tasks.Single(e=>e.taskId==selected);var f=projections[selected];
            Text("Detail Title",detail,.025f,.64f,.72f,.96f,data.displayName,23);
            string forecast=f.unknown?T("planningUnknown"):string.Format(T("planningEstimate"),Date(f.earliest),Date(f.latest));
            Text("Forecast",detail,.025f,.32f,.65f,.65f,forecast,18);
            if(!f.unknown)Text("Buffer",detail,.67f,.32f,.97f,.65f,string.Format(T(f.bufferSlots<0?"timelineLate":"timelineBuffer"),Math.Abs(f.bufferSlots)),17);
            string reason=Work.BlockReason(task);
            if(TimelineMode)reason=string.Format(T("planningDates"),Date(edit.start),Date(edit.due))+(string.IsNullOrEmpty(reason)?"":"\n"+reason);
            Text("Dependency",detail,.025f,.02f,.70f,.34f,reason,17);
            var minus=Icon("Earlier Due",detail,.74f,.66f,.81f,.98f,"time",T("planningEarlier"),()=>ChangeDue(TimelineMode?-1:-4));
            var plus=Icon("Later Due",detail,.83f,.66f,.90f,.98f,"rest",T("planningLater"),()=>ChangeDue(TimelineMode?1:4));
            minus.interactable=plus.interactable=task.acceptedDay==0&&!Session.State.dayEnded;
            Icon("First In Queue",detail,.92f,.66f,.99f,.98f,"priority",T("planningFirst"),()=>
            {
                var first=draft.tasks.Where(e=>Work.Find(e.taskId).ownerId==task.ownerId&&Work.Find(e.taskId).acceptedDay==0).OrderBy(e=>e.priority).FirstOrDefault();
                if(first!=null&&Work.MoveDraft(draft,selected,first.taskId)){UpdateProjection();RenderCards();}
            }).interactable=task.acceptedDay==0;
            if(TimelineMode&&task.effort<data.effort&&Work.PlanFor(selected).started<0)
            {
                Icon("Timeline Earlier",detail,.74f,.03f,.81f,.35f,"back",T("timelineEarlier"),()=>ShiftDates(-1));
                Icon("Timeline Later",detail,.83f,.03f,.90f,.35f,"time",T("timelineLater"),()=>ShiftDates(1));
            }
            if(task.acceptedDay==0&&task.effort==data.effort)
                Icon("Accept Checkpoint",detail,.83f,.03f,.90f,.35f,"review",T("planningReview"),()=>{if(Work.DraftChanged(draft))return;CloseBoard();Work.TryReview();}).interactable=!Work.DraftChanged(draft);
            if(data.decisionBlocked&&!task.decisionCleared)
                Icon("Clear Decision",detail,.92f,.03f,.99f,.35f,"unlock",string.Format(T("planningClarify"),Work.Config.clarifyCost),()=>Act(ProductionAction.Clarify,selected)).interactable=!Work.DraftChanged(draft)&&Work.Preview(ProductionAction.Clarify,selected).allowed;
        }
        public void ShowTimeline(bool visible)
        {if(HasDrag)CancelActiveDrag();TimelineMode=visible;notice.text=visible?T("timelineLegend"):"";RenderCards();}
        public void TimelineNotice(int start,int due,bool valid)
        {notice.text=valid?string.Format(T("timelineDates"),Date(start),Date(due)):T("timelineInvalid");}
        public void CancelActiveDrag(){if(Dragging!=null)Dragging.Cancel();if(Timeline!=null&&Timeline.IsDragging)Timeline.CancelDrag();}
        public bool SetTimelineDates(string id,int start,int due)
        {
            bool valid=Work.SetDraftDates(draft,id,start,due);notice.text=valid?"":T("timelineInvalid");
            if(valid)UpdateProjection();RenderCards();return valid;
        }
        private void ShiftDates(int phases)
        {if(Work.ShiftDraftDates(draft,selected,phases)){UpdateProjection();RenderCards();}}
        private void ChangeDue(int count)
        {var e=draft.tasks.Single(t=>t.taskId==selected);if(Work.SetDraftDates(draft,selected,e.start,e.due+count)){UpdateProjection();RenderCards();}}
        public bool CanDrag(string id)=>IsOpen&&!Session.State.dayEnded&&Work.Find(id).acceptedDay==0&&Work.Find(id).effort<Work.Definition(Work.Find(id)).effort;
        public void BeginDrag(PlanningCardDrag card){Dragging=card;notice.text="";}
        public void EndDrag(PlanningCardDrag card,string targetId,string laneOwner,bool after)
        {
            bool valid=false;
            if(targetId!=null&&targetId!=card.taskId&&Work.Find(targetId).ownerId==Work.Find(card.taskId).ownerId)
            {
                if(after)
                {
                    var order=draft.tasks.Where(e=>Work.Find(e.taskId).ownerId==laneOwner&&e.taskId!=card.taskId&&Work.Find(e.taskId).acceptedDay==0).OrderBy(e=>e.priority).ToList();
                    int i=order.FindIndex(e=>e.taskId==targetId);
                    valid=i>=0&&(i==order.Count-1?Work.MoveDraftToEnd(draft,card.taskId):Work.MoveDraft(draft,card.taskId,order[i+1].taskId));
                }
                else valid=Work.MoveDraft(draft,card.taskId,targetId);
            }
            else if(targetId==null&&laneOwner==Work.Find(card.taskId).ownerId)valid=Work.MoveDraftToEnd(draft,card.taskId);
            Dragging=null;if(valid)UpdateProjection();else notice.text=T("planningInvalidDrop");RenderCards();
        }
        public void CancelDrag(){Dragging=null;if(IsOpen)RenderCards();}
        public RectTransform DragRoot=>root;
        private RectTransform Panel(string name,Transform parent,float x0,float y0,float x1,float y1,Color color)
        {var r=StudioUiWidgets.Rect(name,parent,x0,y0,x1,y1);var image=r.gameObject.AddComponent<Image>();image.color=color;return r;}
        private TMP_Text Text(string name,Transform parent,float x0,float y0,float x1,float y1,string value,float size)
            =>StudioUiWidgets.Text(style,name,parent,x0,y0,x1,y1,value,size);
        private Button Icon(string name,Transform parent,float x0,float y0,float x1,float y1,string icon,string hint,Action action)
        {
            var r=Panel(name,parent,x0,y0,x1,y1,new Color(.12f,.24f,.29f));var button=r.gameObject.AddComponent<Button>();button.onClick.AddListener(()=>action());
            StudioUiWidgets.Image("Icon",r,.18f,.15f,.82f,.85f,style.Icon(icon),Color.white).preserveAspect=true;StudioUiWidgets.Hint(r.gameObject,style,hint);return button;
        }
        private static void Place(RectTransform r,float x0,float y0,float x1,float y1)
        {r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;}
        private static void Clear(Transform parent){foreach(Transform t in parent){t.gameObject.SetActive(false);Destroy(t.gameObject);}}
    }
}
