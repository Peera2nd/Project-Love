using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace ProjectLove.UI
{
    public sealed class PlanningLaneDrop : MonoBehaviour
    {public PlanningBoardView board;public string ownerId;}
    public sealed class PlanningCardDrag : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IInitializePotentialDragHandler
    {
        public PlanningBoardView board;public string taskId;
        private RectTransform rect,placeholder,marker;
        private Transform originalParent;private int originalIndex;
        private CanvasGroup group;private ScrollRect scroll;
        private Vector2 pointer,grabOffset;private Camera camera;
        public bool IsDragging {get;private set;}
        public void OnInitializePotentialDrag(PointerEventData e)
        {
            e.useDragThreshold=true;
            var canvas=GetComponentInParent<Canvas>();EventSystem.current.pixelDragThreshold=Mathf.CeilToInt(board.Session.Production.Config.planning.dragThreshold*canvas.scaleFactor);
        }
        public void OnBeginDrag(PointerEventData e)
        {
            if(!board.CanDrag(taskId))return;
            rect=(RectTransform)transform;originalParent=transform.parent;originalIndex=transform.GetSiblingIndex();scroll=GetComponentInParent<ScrollRect>();
            placeholder=new GameObject("Drag Placeholder",typeof(RectTransform),typeof(LayoutElement)).GetComponent<RectTransform>();
            placeholder.SetParent(originalParent,false);placeholder.SetSiblingIndex(originalIndex);placeholder.GetComponent<LayoutElement>().preferredHeight=rect.rect.height;
            var image=placeholder.gameObject.AddComponent<Image>();image.color=new Color(.22f,.6f,.7f,.15f);image.raycastTarget=false;
            Vector2 size=rect.rect.size;Vector3 world=rect.position;
            rect.SetParent(board.DragRoot,true);rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=size;rect.position=world;
            group=GetComponent<CanvasGroup>();if(group==null)group=gameObject.AddComponent<CanvasGroup>();group.alpha=.85f;group.blocksRaycasts=false;
            camera=e.pressEventCamera;RectTransformUtility.ScreenPointToLocalPointInRectangle(board.DragRoot,e.position,camera,out var local);
            grabOffset=rect.anchoredPosition-local;pointer=e.position;
            marker=new GameObject("Insertion Marker",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();marker.SetParent(board.DragRoot,false);
            marker.GetComponent<Image>().color=new Color(.4f,.95f,.85f);marker.GetComponent<Image>().raycastTarget=false;marker.gameObject.SetActive(false);
            IsDragging=true;board.BeginDrag(this);OnDrag(e);
        }
        public void OnDrag(PointerEventData e)
        {
            if(!IsDragging)return;pointer=e.position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(board.DragRoot,pointer,camera,out var local);rect.anchoredPosition=local+grabOffset;
            FindTarget(out var target,out var lane,out bool after);
            marker.gameObject.SetActive(target!=null&&lane!=null&&lane.ownerId==board.Session.Production.Find(taskId).ownerId);
            if(marker.gameObject.activeSelf)
            {
                var corners=new Vector3[4];((RectTransform)target.transform).GetWorldCorners(corners);
                marker.anchorMin=marker.anchorMax=new Vector2(.5f,.5f);marker.sizeDelta=new Vector2(((RectTransform)target.transform).rect.width,4);
                marker.position=after?(corners[0]+corners[3])*.5f:(corners[1]+corners[2])*.5f;
            }
        }
        private void LateUpdate()
        {
            if(!IsDragging||scroll==null)return;
            var c=new Vector3[4];scroll.viewport.GetWorldCorners(c);float margin=(c[2].y-c[0].y)*.13f;
            float direction=pointer.y<c[0].y+margin?1:pointer.y>c[2].y-margin?-1:0;
            if(direction==0)return;
            float max=Mathf.Max(0,scroll.content.rect.height-scroll.viewport.rect.height);
            scroll.StopMovement();var p=scroll.content.anchoredPosition;
            p.y=Mathf.Clamp(p.y+direction*board.Session.Production.Config.planning.autoScrollSpeed*Time.unscaledDeltaTime,0,max);scroll.content.anchoredPosition=p;
        }
        public void OnEndDrag(PointerEventData e)
        {
            if(!IsDragging)return;pointer=e.position;FindTarget(out var target,out var lane,out bool after);
            Restore();board.EndDrag(this,target?.taskId,lane?.ownerId,after);
        }
        private void FindTarget(out PlanningCardDrag target,out PlanningLaneDrop lane,out bool after)
        {
            target=null;lane=null;after=false;
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=pointer},hits);
            foreach(var hit in hits)
            {
                var card=hit.gameObject.GetComponentInParent<PlanningCardDrag>();
                var area=hit.gameObject.GetComponentInParent<PlanningLaneDrop>();
                if(card!=null&&card!=this)
                {
                    target=card;lane=area;
                    after=pointer.y<RectTransformUtility.WorldToScreenPoint(camera,((RectTransform)card.transform).TransformPoint(((RectTransform)card.transform).rect.center)).y;return;
                }
                if(area!=null){lane=area;return;}
                // A foreground modal/control blocks dropping through it to a lane.
                if(hit.gameObject.GetComponent<Image>()!=null)break;
            }
        }
        public void Cancel(){if(!IsDragging)return;Restore();board.CancelDrag();}
        private void Restore()
        {
            IsDragging=false;if(originalParent!=null&&originalParent.gameObject.activeInHierarchy&&gameObject.activeInHierarchy){rect.SetParent(originalParent,false);rect.SetSiblingIndex(originalIndex);}
            group.alpha=1;group.blocksRaycasts=true;
            if(placeholder!=null){placeholder.gameObject.SetActive(false);Destroy(placeholder.gameObject);}
            if(marker!=null){marker.gameObject.SetActive(false);Destroy(marker.gameObject);}
        }
        private void OnDisable(){if(IsDragging)Cancel();}
    }
}
