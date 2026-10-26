using System;
using System.Collections.Generic;
using ProjectLove.Data;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace ProjectLove.UI
{
    // Shared sprite geometry and vertex tint; no per-discipline textures or material instances.
    public static class StudioUiWidgets
    {
        public static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
            r.anchorMin = new Vector2(x0, y0); r.anchorMax = new Vector2(x1, y1); r.offsetMin = r.offsetMax = Vector2.zero; return r;
        }
        public static Image Image(string name, Transform parent, float x0, float y0, float x1, float y1, Sprite sprite, Color tint)
        {
            var image = Rect(name, parent, x0, y0, x1, y1).gameObject.AddComponent<Image>();
            image.sprite = sprite; image.color = tint; image.raycastTarget = false; return image;
        }
        public static TMP_Text Text(RoomPresentationSO style, string name, Transform parent, float x0, float y0, float x1, float y1, string value, float size = 22)
        {
            var text = Rect(name, parent, x0, y0, x1, y1).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = style.font; text.text = value; text.fontSize = size; text.enableAutoSizing = true;
            text.fontSizeMin = 16; text.fontSizeMax = size; text.color = new Color(.94f, .97f, 1);
            text.raycastTarget = false; text.textWrappingMode = TextWrappingModes.Normal; text.alignment = TextAlignmentOptions.MidlineLeft;
            text.fontFeatures = new List<OTL_FeatureTag> { OTL_FeatureTag.kern, OTL_FeatureTag.mark, OTL_FeatureTag.mkmk }; return text;
        }
        public static void Hint(GameObject target, RoomPresentationSO style, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            var graphic = target.GetComponent<Graphic>();
            if (graphic == null) { graphic = target.AddComponent<Image>(); graphic.color = Color.clear; }
            graphic.raycastTarget = true;
            var hint = target.GetComponent<StudioUiHint>() ?? target.AddComponent<StudioUiHint>(); hint.style = style; hint.value = value;
        }
        public static void DecorateButton(Button button, RoomPresentationSO style, Sprite icon, string hint = null)
        {
            var authored=button.GetComponent<AuthoredActionRow>();
            if(authored!=null){authored.BindIcon(icon);return;}
            if (!style.useStudioSkin) return;
            if (icon != null)
            {
                var old = button.transform.Find("Action Icon");
                var image = old != null ? old.GetComponent<Image>() : Image("Action Icon", button.transform, .035f, .17f, .15f, .83f, icon, Color.white);
                image.sprite = icon; image.preserveAspect = true; image.color = button.interactable ? Color.white : new Color(1, 1, 1, .4f);
                bool compact = ((RectTransform)button.transform).rect.width < 200;
                var rect = image.rectTransform; rect.anchorMin = new Vector2(0, .16f); rect.anchorMax = new Vector2(0, .84f);
                rect.offsetMin = new Vector2(compact ? 8 : 12, 0); rect.offsetMax = new Vector2(compact ? 40 : 54, 0);
                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.rectTransform.anchorMin = new Vector2(0, .05f); label.rectTransform.anchorMax = new Vector2(.975f, .95f);
                    label.rectTransform.offsetMin = new Vector2(compact ? 48 : 64, 0); label.rectTransform.offsetMax = Vector2.zero;
                    label.fontSizeMin = 16;
                }
            }
            Hint(button.gameObject, style, hint);
        }
        public static RectTransform Bar(RoomPresentationSO style, string name, Transform parent, float x0, float y0, float x1, float y1, float fraction, Color tint, float forecast = -1)
        {
            var root = Rect(name, parent, x0, y0, x1, y1); fraction = Mathf.Clamp01(fraction);
            var track = Image("Track", root, 0, 0, 1, 1, style.useStudioSkin ? style.barTrack : null,
                style.useStudioSkin ? Color.white : new Color(.025f, .045f, .055f)); track.type = UnityEngine.UI.Image.Type.Sliced;
            void Segment(string id, float from, float through, Sprite sprite, Color color)
            {
                float width = through - from;
                var clip = Rect(id + " Clip", root, from, 0, through, 1); clip.gameObject.AddComponent<RectMask2D>();
                var image = Image(id, clip, width > 0 ? -from / width : 0, 0, width > 0 ? (1 - from) / width : 1, 1, sprite, color);
                image.type = UnityEngine.UI.Image.Type.Sliced; clip.gameObject.SetActive(width > .00001f);
            }
            Segment("Fill", 0, fraction, style.useStudioSkin ? style.barFill : null, tint);
            if (forecast > fraction) Segment("Forecast", fraction, Mathf.Clamp01(forecast), style.barForecast, tint);
            if (style.useStudioSkin && style.barFrame != null)
            { var frame = Image("Frame", root, 0, 0, 1, 1, style.barFrame, new Color(1, 1, 1, .55f)); frame.type = UnityEngine.UI.Image.Type.Sliced; }
            return root;
        }
        public static void Panel(Image image, RoomPresentationSO style, bool card = false)
        {
            if (!style.useStudioSkin) return;
            image.sprite = card ? style.taskCardSprite : style.panelSprite;
            image.type = UnityEngine.UI.Image.Type.Sliced; image.color = Color.white;
        }
        public static RectTransform Metric(RoomPresentationSO style, string name, Transform parent, float x0, float y0, float x1, float y1,
            Sprite icon, string label, float fraction, Color tint, float forecast = -1)
        {
            var root = Rect(name, parent, x0, y0, x1, y1);
            if (icon != null) Image("Icon", root, .01f, .20f, .16f, .95f, icon, Color.white).preserveAspect = true;
            Text(style, "Value", root, .19f, .28f, .99f, 1, label, 23);
            if (fraction >= 0) Bar(style, "Value Bar", root, .19f, .03f, .99f, .22f, fraction, tint, forecast);
            return root;
        }
    }
}
