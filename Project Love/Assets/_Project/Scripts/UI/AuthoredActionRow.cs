using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectLove.Data;
namespace ProjectLove.UI
{
    [Serializable] public sealed class AuthoredEffect {public GameObject root;public Image icon;public TMP_Text value;}
    public sealed class AuthoredActionRow : MonoBehaviour
    {
        public TMP_Text title;
        public Image icon;
        public AuthoredEffect[] effects=Array.Empty<AuthoredEffect>();
        public void BindIcon(Sprite sprite){if(icon!=null){icon.sprite=sprite;icon.enabled=sprite!=null;}}
        public void Bind(RoomPresentationSO style,string text,params ActionMetric[] metrics)
        {
            title.text=text;
            for(int i=0;i<effects.Length;i++)
            {
                var e=effects[i];e.root.SetActive(i<metrics.Length);if(i>=metrics.Length)continue;
                var m=metrics[i];e.icon.name="Effect "+m.icon;e.value.name="Delta "+m.icon;e.icon.sprite=style.Icon(m.icon);e.value.text=m.value>0?"+"+m.value:m.value<0?"−"+Mathf.Abs(m.value):"0";
            }
        }
    }
}
