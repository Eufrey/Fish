using UnityEngine;
using UnityEngine.UI;
namespace Fish
{
    public static class FishHUDLayout
    {
        public static void Hint(Text text, int row)
        {
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-28, -28 - row * 48);
            rect.sizeDelta = new Vector2(280, 34);
            text.alignment = TextAnchor.MiddleRight;
        }
        public static void Story(Text objective, Text notice)
        {
            var root = objective.transform.parent as RectTransform;
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            Place(objective, 28, 48);
            Place(notice, 90, 130);
        }
        private static void Place(Text text, float top, float height)
        {
            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0,1); rect.anchorMax = new Vector2(.65f,1);
            rect.pivot = new Vector2(0,1);
            rect.offsetMin = new Vector2(28,-top-height);
            rect.offsetMax = new Vector2(-24,-top);
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
        }
    }
}
