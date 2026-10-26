using ProjectLove.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectLove.UI
{
    public sealed class StudioUiHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RoomPresentationSO style;
        public string value;
        private GameObject tip;
        public void OnPointerEnter(PointerEventData data)
        {
            Clear(); if (style == null || string.IsNullOrEmpty(value)) return;
            var canvas = GetComponentInParent<Canvas>(); if (canvas == null) return;
            var root = (RectTransform)canvas.rootCanvas.transform;
            var rect = StudioUiWidgets.Rect("Studio Tooltip", root, .5f, .5f, .5f, .5f); tip = rect.gameObject;
            var bg = tip.AddComponent<Image>(); bg.raycastTarget = false; bg.color = new Color(.025f, .055f, .075f, .98f);
            var text = StudioUiWidgets.Text(style, "Tooltip Text", rect, 0, 0, 1, 1, value, 22);
            text.rectTransform.offsetMin = new Vector2(14, 10); text.rectTransform.offsetMax = new Vector2(-14, -10);
            float width = Mathf.Min(440, root.rect.width - 24);
            float height = Mathf.Min(root.rect.height - 24, Mathf.Max(70, text.GetPreferredValues(value, width - 28, 0).y + 24));
            rect.sizeDelta = new Vector2(width, height);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, data.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out var position);
            rect.anchoredPosition = new Vector2(Mathf.Clamp(position.x, root.rect.xMin + width / 2 + 12, root.rect.xMax - width / 2 - 12),
                Mathf.Clamp(position.y + height / 2 + 22, root.rect.yMin + height / 2 + 12, root.rect.yMax - height / 2 - 12));
        }
        public void OnPointerExit(PointerEventData data) => Clear();
        private void OnDisable() => Clear();
        private void OnDestroy() => Clear();
        private void Clear() { if (tip != null) { tip.SetActive(false); Destroy(tip); } tip = null; }
    }
}
