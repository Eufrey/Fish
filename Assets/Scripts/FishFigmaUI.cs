using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fish
{
    // The manifest and transparent sprites come from the supplied local Figma export.
    // Text and controls stay live uGUI objects, so gameplay owns their state.
    public static class FishFigmaUI
    {
        [Serializable] public sealed class Layer
        {
            public string id, parent, name, text, sprite, fontStyle;
            public float x, y, width, height, fontSize, opacity;
            public Color color;
        }
        [Serializable] private sealed class Document { public Layer[] nodes; }
        private static Document document;
        private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        public static Font Font(bool heading = false) => Resources.Load<Font>(heading ? "UI/Fonts/Nunito-Black" : "UI/Fonts/Nunito-ExtraBold");
        private static Document Data => document ?? (document = JsonUtility.FromJson<Document>(Resources.Load<TextAsset>("UI/Figma/layout").text));

        public static Sprite Sprite(string id)
        {
            if (sprites.TryGetValue(id, out var sprite) && sprite != null) return sprite;
            var texture = Resources.Load<Texture2D>("UI/Figma/" + id.Replace(':', '_'));
            if (texture == null) return null;
            sprite = UnityEngine.Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f, 200);
            sprites[id] = sprite;
            return sprite;
        }

        public static Dictionary<string, RectTransform> Section(string id, Transform parent)
        {
            var result = new Dictionary<string, RectTransform>();
            Add(id, parent, result);
            return result;
        }
        private static void Add(string id, Transform parent, Dictionary<string, RectTransform> result)
        {
            var layer = Array.Find(Data.nodes, n => n.id == id);
            if (layer == null) return;
            var rect = new GameObject("Figma " + layer.name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.gameObject.layer = parent.gameObject.layer;
            Place(rect, layer.x, layer.y, layer.width, layer.height);
            result[id] = rect;
            if (!string.IsNullOrEmpty(layer.sprite))
            {
                var art = new GameObject("Artwork", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                art.transform.SetParent(rect, false); art.layer = rect.gameObject.layer;
                var image = art.GetComponent<UnityEngine.UI.Image>();
                image.sprite = Sprite(id);
                image.raycastTarget = false;
                // Exported artwork includes 32 reference pixels of shadow padding.
                Place(image.rectTransform, -32, -32, layer.width + 64, layer.height + 64);
            }
            if (!string.IsNullOrEmpty(layer.text))
            {
                var text = rect.gameObject.AddComponent<UnityEngine.UI.Text>();
                text.font = Font(layer.fontSize >= 30);
                text.fontSize = Mathf.RoundToInt(layer.fontSize);
                text.fontStyle = FontStyle.Normal;
                text.text = layer.text;
                text.color = layer.color;
                text.raycastTarget = false;
                text.horizontalOverflow = layer.text.Length <= 22 && !layer.text.Contains("\n") ? HorizontalWrapMode.Overflow : HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
            }
            foreach (var child in Data.nodes)
                if (child.parent == id) Add(child.id, rect, result);
        }
        public static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
        public static void BindText(Text source, RectTransform destination)
        {
            var sample = destination.GetComponent<Text>();
            source.transform.SetParent(destination, false);
            source.rectTransform.anchorMin = Vector2.zero; source.rectTransform.anchorMax = Vector2.one;
            source.rectTransform.offsetMin = source.rectTransform.offsetMax = Vector2.zero;
            source.font = sample.font; source.fontSize = sample.fontSize; source.fontStyle = FontStyle.Normal;
            source.color = sample.color; source.alignment = sample.alignment;
            source.horizontalOverflow = HorizontalWrapMode.Wrap;
            source.verticalOverflow = VerticalWrapMode.Truncate;
            sample.enabled = false;
        }
        public static void ButtonArt(Button button, string id)
        {
            var existing = button.transform.Find("Figma Button Surface");
            if (existing != null) { SetButtonColors(button); return; }
            var rect = new GameObject("Figma Button Surface", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            rect.SetParent(button.transform, false); rect.SetAsFirstSibling();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(-32, -32); rect.offsetMax = new Vector2(32, 32);
            var image = rect.GetComponent<Image>();
            var source = Sprite(id);
            if (source == null || source.texture == null) return;
            image.sprite = UnityEngine.Sprite.Create(source.texture, source.rect, Vector2.one * .5f, 200, 0, SpriteMeshType.FullRect, new Vector4(112, 112, 112, 112));
            image.type = Image.Type.Sliced; image.raycastTarget = false;
            if (button.targetGraphic != null) button.targetGraphic.color = Color.clear;
            button.targetGraphic = image;
            SetButtonColors(button);
        }
        public static void SetButtonColors(Button button)
        {
            var colors = button.colors;
            colors.normalColor = Color.white; colors.highlightedColor = new Color(1, 1, .85f);
            colors.selectedColor = new Color(.85f, 1, .8f); colors.pressedColor = new Color(.65f, .75f, .65f);
            colors.disabledColor = new Color(.5f, .6f, .55f, .65f); button.colors = colors;
        }
    }
}
