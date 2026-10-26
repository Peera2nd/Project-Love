using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ProjectLove.UI
{
    public enum TimelineGesture { Move,Start,Due }
    public sealed class PlanningTimelineView : MonoBehaviour
    {
        private PlanningBoardView board;
        private RectTransform root,header,names,content,viewport,nameContent;
        private ScrollRect scroll;
        private readonly Dictionary<string,RectTransform> bars=new Dictionary<string,RectTransform>();
        private readonly Dictionary<string,RectTransform> rows=new Dictionary<string,RectTransform>();
        private readonly Dictionary<string,int> indices=new Dictionary<string,int>();
        private TimelineLinksGraphic links;
        private string moving;private TimelineGesture gesture;
        private int originalStart,originalDue,candidateStart,candidateDue;
        private float pointerStart,horizontalIntent;private Vector2 pointer;private Camera eventCamera;
        public bool IsDragging=>moving!=null;
        public RectTransform Viewport=>viewport;
        public RectTransform Content=>content;
        public IReadOnlyDictionary<string,RectTransform> Bars=>bars;
        private ProductionSystem Work=>board.Session.Production;
        private RoomPresentationSO Style=>board.Style;
        private float Cell=>Work.Config.planning.timelineCellWidth;
        private float Row=>Work.Config.planning.timelineRowHeight;
        private int Phases=>board.Session.Config.totalDays*4;
        public void Initialize(PlanningBoardView owner,Transform parent)
        {
            board=owner;root=StudioUiWidgets.Rect("Timeline Workspace",parent,.02f,.27f,.98f,.85f);
            var bg=root.gameObject.AddComponent<Image>();bg.color=new Color(.025f,.045f,.065f);
            header=StudioUiWidgets.Rect("Timeline Header",root,.24f,.86f,.985f,1);header.gameObject.AddComponent<RectMask2D>();
            var headerContent=TopLeft("Dates",header);
            names=StudioUiWidgets.Rect("Timeline Names",root,0,.035f,.235f,.86f);names.gameObject.AddComponent<RectMask2D>();
            nameContent=TopLeft("Task Names",names);
            viewport=StudioUiWidgets.Rect("Timeline Viewport",root,.24f,.035f,.985f,.86f);viewport.gameObject.AddComponent<RectMask2D>();
            viewport.gameObject.AddComponent<Image>().color=new Color(.06f,.08f,.11f);
            content=TopLeft("Timeline Grid",viewport);
            scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;
            scroll.onValueChanged.AddListener(_=>{headerContent.anchoredPosition=new Vector2(content.anchoredPosition.x,0);nameContent.anchoredPosition=new Vector2(0,content.anchoredPosition.y);});
            var slider=StudioUiWidgets.Rect("Timeline Horizontal Scroll",root,.24f,0,.985f,.025f);slider.gameObject.AddComponent<Image>().color=new Color(.12f,.19f,.23f);
            var handle=StudioUiWidgets.Rect("Handle",slider,0,0,1,1);var image=handle.gameObject.AddComponent<Image>();image.color=new Color(.44f,.66f,.72f);
            var scrollbar=slider.gameObject.AddComponent<Scrollbar>();scrollbar.handleRect=handle;scrollbar.targetGraphic=image;scroll.horizontalScrollbar=scrollbar;
            for(int day=1;day<=board.Session.Config.totalDays;day++)
            {
                var label=StudioUiWidgets.Text(Style,"Timeline Day "+day,headerContent,0,0,0,0,string.Format(Style.Get("timelineDay"),day),20);
                PlacePixels(label.rectTransform,(day-1)*4*Cell,0,Cell*4,30);label.alignment=TextAlignmentOptions.Center;
                for(int phase=0;phase<4;phase++)
                {
                    var slot=TopLeft("Phase "+day+" "+phase,headerContent);PlacePixels(slot,((day-1)*4+phase)*Cell+5,32,Cell-10,24);
                    var icon=slot.gameObject.AddComponent<Image>();icon.sprite=Style.Icon(phase==3?"night":"day");icon.preserveAspect=true;
                    icon.color=phase==0?new Color(1,.88f,.50f):phase==1?Color.white:phase==2?new Color(1,.62f,.38f):new Color(.65f,.76f,1);
                    StudioUiWidgets.Hint(slot.gameObject,Style,board.Date((day-1)*4+phase));
                }
            }
            headerContent.sizeDelta=new Vector2(Phases*Cell,36);
            StudioUiWidgets.Text(Style,"Timeline Legend",root,0,.90f,.23f,1,Style.Get("timelineTitle"),24);
            links=TopLeft("Dependency Links",content).gameObject.AddComponent<TimelineLinksGraphic>();links.raycastTarget=false;
            root.gameObject.SetActive(false);
        }
        public void SetVisible(bool visible){root.gameObject.SetActive(visible);}
        private static RectTransform TopLeft(string name,Transform parent)
        {var r=StudioUiWidgets.Rect(name,parent,0,1,0,1);r.pivot=new Vector2(0,1);return r;}
        private static void PlacePixels(RectTransform r,float x,float y,float width,float height)
        {r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(width,height);}
        private void HideDecorations(RectTransform row)
        {foreach(Transform child in row)if(child.name!="Planned Bar"){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
        public void Render()
        {
            if(!root.gameObject.activeSelf||IsDragging)return;
            var tasks=board.VisibleTasks.OrderBy(t=>Array.FindIndex(Work.Config.workers,w=>w.character.id==t.ownerId)).ThenBy(t=>board.Draft.tasks.Single(e=>e.taskId==t.taskId).priority).ToArray();
            content.sizeDelta=new Vector2(Phases*Cell,tasks.Length*Row);nameContent.sizeDelta=new Vector2(names.rect.width,tasks.Length*Row);
            foreach(var r in rows.Values)r.gameObject.SetActive(false);
            foreach(Transform old in nameContent){old.gameObject.SetActive(false);Destroy(old.gameObject);}
            indices.Clear();
            for(int i=0;i<tasks.Length;i++)
            {
                var task=tasks[i];var data=Work.Definition(task);var plan=Work.PlanFor(task.taskId);var edit=board.Draft.tasks.Single(e=>e.taskId==task.taskId);var f=board.Projections[task.taskId];indices[task.taskId]=i;
                if(!rows.TryGetValue(task.taskId,out var row))
                {
                    row=TopLeft("Timeline Row "+task.taskId,content);row.gameObject.AddComponent<Image>();rows[task.taskId]=row;
                    var bar=TopLeft("Planned Bar",row);bar.gameObject.AddComponent<Image>();var select=bar.gameObject.AddComponent<Button>();string id=task.taskId;select.onClick.AddListener(()=>board.Select(id));
                    var drag=bar.gameObject.AddComponent<PlanningTimelineDrag>();drag.timeline=this;drag.taskId=id;drag.gesture=TimelineGesture.Move;
                    Handle(bar,"Start Handle",id,TimelineGesture.Start,true);Handle(bar,"Due Handle",id,TimelineGesture.Due,false);bars[id]=bar;
                }
                row.gameObject.SetActive(true);PlacePixels(row,0,i*Row,Phases*Cell,Row-2);row.GetComponent<Image>().color=i%2==0?new Color(.045f,.065f,.09f):new Color(.065f,.085f,.11f);HideDecorations(row);
                var name=StudioUiWidgets.Rect("Timeline Name "+task.taskId,nameContent,0,1,1,1);name.pivot=new Vector2(.5f,1);name.anchoredPosition=new Vector2(0,-i*Row);name.sizeDelta=new Vector2(0,Row-2);
                var nameImage=name.gameObject.AddComponent<Image>();var button=name.gameObject.AddComponent<Button>();button.targetGraphic=nameImage;nameImage.color=board.SelectedTaskId==task.taskId?new Color(.10f,.24f,.28f):new Color(.04f,.06f,.08f);string selected=task.taskId;button.onClick.AddListener(()=>board.Select(selected));
                StudioUiWidgets.Image("Discipline",name,0,.06f,.018f,.94f,null,data.stream.tint);
                StudioUiWidgets.Image("Owner",name,.04f,.72f,.16f,.98f,data.owner.taskAvatar,Color.white).preserveAspect=true;
                var title=StudioUiWidgets.Text(Style,"Timeline Task Title",name,.04f,.03f,.98f,.74f,data.displayName,18);title.fontSizeMin=16;
                // Night bands and immutable sprint deadlines are reference geometry, never draggable controls.
                for(int day=0;day<board.Session.Config.totalDays;day++)
                {
                    PixelImage("Night",row,day*4*Cell+3*Cell,0,Cell,Row,new Color(0,0,0,.20f));
                    PixelImage("Day Grid",row,day*4*Cell,0,1,Row,new Color(.32f,.44f,.48f,.25f));
                }
                foreach(var sprint in Work.Config.sprints)PixelImage("Sprint Deadline "+sprint.id,row,((sprint.endDay-1)*4+3)*Cell,0,2,Row,new Color(1,.72f,.25f,.6f));
                PixelImage("Current Cursor",row,Work.Cursor*Cell,0,2,Row,new Color(.45f,.95f,1,.8f));
                if(plan.started>=0)
                {
                    int through=plan.finished>=0?plan.finished+1:Work.Cursor;
                    PixelImage("Actual History",row,plan.started*Cell,Row*.25f,Mathf.Max(2,(through-plan.started)*Cell),Row*.30f,new Color(data.stream.tint.r,data.stream.tint.g,data.stream.tint.b,.45f));
                }
                if(!f.unknown&&task.acceptedDay==0)
                {
                    int start=Math.Max(Work.Cursor,f.projectedStart>=0?f.projectedStart:edit.start);
                    var forecast=PixelImage("Estimated Work",row,start*Cell,Row*.74f,Mathf.Max(2,(f.latest+1-start)*Cell),Row*.17f,new Color(data.stream.tint.r,data.stream.tint.g,data.stream.tint.b,.6f));
                    forecast.sprite=Style.barForecast;forecast.type=Image.Type.Sliced;
                    if(f.bufferSlots>0)PixelImage("Usable Buffer",row,(f.expected+1)*Cell,Row*.74f,Mathf.Max(0,(edit.due-f.expected)*Cell),Row*.17f,new Color(.40f,.90f,.62f,.35f));
                }
                var planned=bars[task.taskId];bool finished=task.effort==data.effort;
                planned.gameObject.SetActive(!finished);PositionBar(task.taskId,edit.start,edit.due);planned.SetAsLastSibling();
                planned.GetComponent<Image>().color=f.atRisk?Color.Lerp(data.stream.tint,new Color(1,.45f,.28f),.35f):data.stream.tint;
                planned.Find("Start Handle").gameObject.SetActive(plan.started<0&&!finished);planned.Find("Due Handle").gameObject.SetActive(!finished);
                if(task.acceptedDay==0&&(f.atRisk||data.prerequisites.Length>0||data.decisionBlocked))
                {
                    var status=PixelImage("Timeline State",row,Mathf.Max(0,Math.Max(Work.Cursor,edit.start)*Cell-22),Row*.18f,20,24,Color.white);
                    status.sprite=Style.Icon(f.atRisk?"warning":"dependency");status.preserveAspect=true;
                    if(status.sprite==null){status.sprite=Style.Icon("blocked");}
                }
            }
            links.rectTransform.sizeDelta=content.sizeDelta;links.transform.SetAsLastSibling();RefreshLinks();Canvas.ForceUpdateCanvases();
        }
        private void Handle(RectTransform bar,string name,string id,TimelineGesture kind,bool left)
        {
            var r=StudioUiWidgets.Rect(name,bar,left?0:1,0,left?0:1,1);r.pivot=new Vector2(left?0:1,.5f);r.sizeDelta=new Vector2(12,0);
            r.gameObject.AddComponent<Image>().color=new Color(1,1,1,.7f);r.gameObject.AddComponent<Button>();
            var gesture=r.gameObject.AddComponent<PlanningTimelineDrag>();gesture.timeline=this;gesture.taskId=id;gesture.gesture=kind;
        }
        private Image PixelImage(string name,Transform parent,float x,float y,float width,float height,Color color)
        {var r=TopLeft(name,parent);PlacePixels(r,x,y,width,height);var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;}
        private void PositionBar(string id,int start,int due)
        {int from=Math.Max(Work.Cursor,start);PlacePixels(bars[id],from*Cell,Row*.18f,Mathf.Max(12,(due+1-from)*Cell),Row*.43f);}
        private void RefreshLinks()
        {
            links.segments.Clear();string selected=board.SelectedTaskId;
            if(selected!=null)
            foreach(var task in board.VisibleTasks)
            foreach(var predecessor in Work.Definition(task).prerequisites)
            {
                if(task.taskId!=selected&&predecessor.id!=selected||!indices.ContainsKey(predecessor.id))continue;
                var before=board.Projections[predecessor.id];var after=board.Projections[task.taskId];
                int x0=before.unknown?board.Draft.tasks.Single(e=>e.taskId==predecessor.id).due+1:before.expected+1;
                int x1=after.projectedStart<0?board.Draft.tasks.Single(e=>e.taskId==task.taskId).start:after.projectedStart;
                links.segments.Add(new Vector4(x0*Cell,-indices[predecessor.id]*Row-Row*.4f,x1*Cell,-indices[task.taskId]*Row-Row*.4f));
            }
            links.SetVerticesDirty();
        }
        public bool CanDrag(string id,TimelineGesture kind)
        {
            var task=Work.Find(id);return board.IsOpen&&board.TimelineMode&&!board.Session.State.dayEnded&&task.acceptedDay==0&&task.effort<Work.Definition(task).effort&&
                (kind==TimelineGesture.Due||Work.PlanFor(id).started<0);
        }
        public void BeginDrag(string id,TimelineGesture kind,PointerEventData e)
        {
            if(!CanDrag(id,kind))return;board.Select(id);moving=id;gesture=kind;
            var edit=board.Draft.tasks.Single(t=>t.taskId==id);originalStart=candidateStart=edit.start;originalDue=candidateDue=edit.due;eventCamera=e.pressEventCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(content,e.pressPosition,eventCamera,out var local);pointerStart=local.x;pointer=e.position;horizontalIntent=Mathf.Sign(e.position.x-e.pressPosition.x);scroll.StopMovement();
        }
        public void MoveDrag(PointerEventData e){if(!IsDragging)return;if(Mathf.Abs(e.delta.x)>.01f)horizontalIntent=Mathf.Sign(e.delta.x);pointer=e.position;UpdateCandidate();}
        private void UpdateCandidate()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(content,pointer,eventCamera,out var local);int delta=Mathf.RoundToInt((local.x-pointerStart)/Cell);
            candidateStart=originalStart+(gesture==TimelineGesture.Due?0:delta);candidateDue=originalDue+(gesture==TimelineGesture.Start?0:delta);
            bool valid=CandidateValid();board.TimelineNotice(candidateStart,candidateDue,valid);PositionBar(moving,candidateStart,candidateDue);bars[moving].GetComponent<Image>().color=valid?new Color(.65f,.95f,.85f):new Color(1,.35f,.3f);
        }
        private bool CandidateValid()
        {
            var plan=Work.PlanFor(moving);var def=Work.Definition(Work.Find(moving));
            return candidateDue>=candidateStart&&candidateDue<Phases&&candidateStart>=Math.Max((Math.Max(def.releaseDay,def.sprint.startDay)-1)*4,plan.started<0?Work.Cursor:plan.start)&&
                (candidateDue==originalDue||candidateDue>=Work.Cursor)&&
                RectTransformUtility.RectangleContainsScreenPoint(viewport,pointer,eventCamera);
        }
        private void LateUpdate()
        {
            if(!IsDragging)return;
            var bounds=new Vector3[4];viewport.GetWorldCorners(bounds);float margin=(bounds[2].x-bounds[0].x)*.08f;
            float direction=pointer.x<bounds[0].x+margin?1:pointer.x>bounds[2].x-margin?-1:0;
            if(direction==0||direction<0&&horizontalIntent<=0||direction>0&&horizontalIntent>=0)return;float max=Mathf.Max(0,content.rect.width-viewport.rect.width);var p=content.anchoredPosition;
            p.x=Mathf.Clamp(p.x+direction*Work.Config.planning.autoScrollSpeed*Time.unscaledDeltaTime,-max,0);content.anchoredPosition=p;UpdateCandidate();
        }
        public void EndDrag(PointerEventData e)
        {
            if(!IsDragging)return;pointer=e.position;UpdateCandidate();string id=moving;bool valid=CandidateValid();moving=null;
            if(valid)board.SetTimelineDates(id,candidateStart,candidateDue);else Render();
        }
        public void CancelDrag(){if(!IsDragging)return;moving=null;Render();}
        private void OnDisable(){if(IsDragging)CancelDrag();}
    }
    public sealed class PlanningTimelineDrag : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IInitializePotentialDragHandler
    {
        public PlanningTimelineView timeline;public string taskId;public TimelineGesture gesture;
        public void OnInitializePotentialDrag(PointerEventData e){e.useDragThreshold=true;EventSystem.current.pixelDragThreshold=Mathf.CeilToInt(timeline.GetComponent<PlanningBoardView>().Session.Production.Config.planning.dragThreshold*GetComponentInParent<Canvas>().scaleFactor);}
        public void OnBeginDrag(PointerEventData e)=>timeline.BeginDrag(taskId,gesture,e);
        public void OnDrag(PointerEventData e)=>timeline.MoveDrag(e);
        public void OnEndDrag(PointerEventData e)=>timeline.EndDrag(e);
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TimelineLinksGraphic : MaskableGraphic
    {
        public readonly List<Vector4> segments=new List<Vector4>();
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();foreach(var s in segments)
            {
                float elbow=Mathf.Max(s.x,s.z)+8;
                Line(vh,new Vector2(s.x,s.y),new Vector2(elbow,s.y));Line(vh,new Vector2(elbow,s.y),new Vector2(elbow,s.w));Line(vh,new Vector2(elbow,s.w),new Vector2(s.z,s.w));
            }
        }
        private void Line(VertexHelper vh,Vector2 a,Vector2 b)
        {
            Vector2 n=(b-a).normalized;n=new Vector2(-n.y,n.x)*1.5f;int i=vh.currentVertCount;var tint=new Color(.80f,.88f,.95f,.8f);
            vh.AddVert(a-n,tint,Vector2.zero);vh.AddVert(a+n,tint,Vector2.zero);vh.AddVert(b+n,tint,Vector2.zero);vh.AddVert(b-n,tint,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
        }
    }
}
