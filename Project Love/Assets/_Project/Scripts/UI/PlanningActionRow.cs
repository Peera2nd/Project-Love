using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectLove.Data;
namespace ProjectLove.UI
{
    public readonly struct ActionMetric
    {public readonly string icon;public readonly int value;public ActionMetric(string icon,int value){this.icon=icon;this.value=value;}}
    public static class PlanningActionRow
    {
        public static void Render(Button button,RoomPresentationSO style,string title,params ActionMetric[] metrics)
        {
            var authored=button.GetComponent<AuthoredActionRow>();if(authored!=null){authored.Bind(style,title,metrics);return;}
            var hint=button.GetComponent<StudioUiHint>();if(hint!=null){hint.enabled=false;Object.Destroy(hint);}
            var label=button.GetComponentInChildren<TMP_Text>();label.text=title;
            label.rectTransform.anchorMin=new Vector2(.12f,metrics.Length==0?.1f:.46f);label.rectTransform.anchorMax=new Vector2(.97f,.95f);
            label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;label.alignment=TextAlignmentOptions.MidlineLeft;label.fontSizeMax=23;
            var icon=button.transform.Find("Action Icon") as RectTransform;
            if(icon!=null){icon.anchorMin=new Vector2(.02f,.32f);icon.anchorMax=new Vector2(.09f,.80f);icon.offsetMin=icon.offsetMax=Vector2.zero;}
            button.GetComponent<LayoutElement>().preferredHeight=96;
            float width=.86f/Mathf.Max(1,metrics.Length);
            for(int i=0;i<metrics.Length;i++)
            {
                var m=metrics[i];float x=.12f+i*width;
                StudioUiWidgets.Image("Effect "+m.icon,button.transform,x,.08f,x+.035f,.42f,style.Icon(m.icon),Color.white).preserveAspect=true;
                var text=StudioUiWidgets.Text(style,"Delta "+m.icon,button.transform,x+.043f,.06f,x+width-.015f,.45f,m.value>0?"+"+m.value:m.value<0?"−"+Mathf.Abs(m.value):"0",21);
                bool benefit=m.icon=="stress"?m.value<0:m.icon!="time"&&m.value>0;
                text.color=m.value==0?new Color(.74f,.80f,.84f):benefit?new Color(.68f,1,.80f):new Color(1,.75f,.63f);
            }
        }
    }
}
