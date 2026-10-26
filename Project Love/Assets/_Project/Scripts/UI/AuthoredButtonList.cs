using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ProjectLove.UI
{
    public sealed class AuthoredButtonList : MonoBehaviour
    {
        readonly List<Button> pool=new List<Button>();int cursor;
        public void Begin(){cursor=0;foreach(var b in pool)if(b!=null)b.gameObject.SetActive(false);}
        public Button Take(Button template)
        {
            if(cursor==pool.Count)pool.Add(Instantiate(template,transform));
            var b=pool[cursor++];b.onClick.RemoveAllListeners();b.gameObject.SetActive(true);return b;
        }
    }
}
