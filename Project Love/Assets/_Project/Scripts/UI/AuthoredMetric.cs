using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ProjectLove.UI
{
    // Geometry, fonts and colors belong to the prefab. Binding updates content only.
    public sealed class AuthoredMetric : MonoBehaviour
    {
        public Image icon, fill;
        public TMP_Text label;
        public void Bind(Sprite sprite,string text,float fraction=-1)
        {if(icon!=null){icon.sprite=sprite;icon.enabled=sprite!=null;}if(label!=null)label.text=text;if(fill!=null)fill.fillAmount=Mathf.Clamp01(fraction);}
    }
}
