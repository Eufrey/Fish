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
            rect.anchoredPosition = new Vector2(-28, -24 - row * 36);
            rect.sizeDelta = new Vector2(240, 28);
            text.alignment = TextAnchor.MiddleRight;
            text.fontSize = 17; text.fontStyle = FontStyle.Bold;
        }
        public static void Story(Text objective, Text notice)
        {
            var root = objective.transform.parent as RectTransform;
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            Place(objective, 24, 64);
            objective.fontSize = 30; objective.fontStyle = FontStyle.Bold;
            objective.color = FishUITheme.Gold;
            Place(notice, 144, 100);
            notice.fontSize = 20;
            FishFigmaHUD.Build(objective);
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
