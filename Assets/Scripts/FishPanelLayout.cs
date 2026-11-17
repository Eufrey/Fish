using UnityEngine;
using UnityEngine.UI;

namespace Fish
{
    // Adapt old saved panels once, keeping their buttons, references and listeners.
    public static class FishPanelLayout
    {
        public static void Fishing(Transform root, Text feedback, Button retry)
        {
            if (root.Find("Coastal Fishing Layout") != null) return;
            // Find old saved layouts too, retaining the same controls and listeners.
            Transform title = Find(root, "Title"), instructions = Find(root, "Instructions");
            Transform bar = Find(root, "Timing Bar"), close = Find(root.parent, "Close Button");
            if (title == null || instructions == null || bar == null || close == null) return;

            RectTransform layout = Rect("Coastal Fishing Layout", root);
            layout.anchorMin = new Vector2(0, .1f); layout.anchorMax = new Vector2(1, .9f);
            layout.offsetMin = new Vector2(28, 0); layout.offsetMax = new Vector2(-28, 0);
            var columns = layout.gameObject.AddComponent<HorizontalLayoutGroup>();
            columns.spacing = 28;
            columns.childControlWidth = columns.childControlHeight = true;
            columns.childForceExpandWidth = columns.childForceExpandHeight = true;

            RectTransform dialArea = Rect("Dial Area", layout);
            var dialSize = dialArea.gameObject.AddComponent<LayoutElement>();
            dialSize.flexibleWidth = 1;
            RectTransform dialRect = Rect("Coastal Timing Dial", dialArea);
            dialRect.anchorMin = Vector2.zero; dialRect.anchorMax = Vector2.one;
            dialRect.offsetMin = dialRect.offsetMax = Vector2.zero;
            var dial = dialRect.gameObject.AddComponent<FishFishingDial>();
            dial.raycastTarget = false;
            bar.gameObject.SetActive(false);
            bar.SetParent(dialArea, false);

            RectTransform content = Rect("Fishing Actions", layout);
            content.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            Vertical(content, 12);
            content.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
            Place(title, content, 112);
            var titleText = title.GetComponent<Text>();
            titleText.text = "TIME YOUR\nCATCH!";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = FishUITheme.Gold;
            titleText.alignment = TextAnchor.MiddleLeft;
            RectTransform catchCard = Rect("Catch Action", content);
            Size(catchCard, 80);
            var catchSurface = catchCard.gameObject.AddComponent<Image>();
            FishUITheme.Surface(catchSurface, FishUITheme.Gold);
            catchSurface.raycastTarget = false;
            instructions.SetParent(catchCard, false);
            var instructionRect = (RectTransform)instructions;
            instructionRect.anchorMin = Vector2.zero; instructionRect.anchorMax = Vector2.one;
            instructionRect.offsetMin = new Vector2(20, 8); instructionRect.offsetMax = new Vector2(-20, -8);
            var actionText = instructions.GetComponent<Text>();
            actionText.text = "CATCH";
            actionText.fontSize = 48;
            actionText.color = FishUITheme.Ink;
            actionText.alignment = TextAnchor.MiddleCenter;
            feedback.fontSize = 22;
            ScrollText("Feedback Scroll", content, feedback, 104);
            RectTransform footer = Row("Actions", content, 64);
            Place(retry.transform, footer, 64);
            Place(close, footer, 64);
            Transform old = root.Find("Fishing Layout");
            if (old != null) old.gameObject.SetActive(false);
            FishUITheme.Apply(root.parent.gameObject);
            titleText.color = FishUITheme.Gold;
        }

        private static Transform Find(Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child;
            return null;
        }
        public static void Battle(Transform root, Text feedback, Text specialInfo, params Button[] buttons)
        {
            if (specialInfo == null) return;
            if (root.Find("Battle Controls") != null)
            {
                CompactBattleControls(root.Find("Battle Controls"));
                return;
            }
            Transform title = root.Find("Title");
            if (title != null)
            {
                var titleRect = (RectTransform)title;
                titleRect.anchoredPosition = new Vector2(0, 304);
                titleRect.sizeDelta = new Vector2(1100, 48);
                var titleText = title.GetComponent<Text>();
                if (titleText != null) titleText.fontSize = 32;
            }
            RectTransform layout = Rect("Battle Controls", root);
            layout.anchorMin = Vector2.zero; layout.anchorMax = new Vector2(1, 0);
            layout.offsetMin = new Vector2(28, 28); layout.offsetMax = new Vector2(-28, 254);
            Vertical(layout, 12);
            ScrollText("Action Feedback Scroll", layout, feedback, 52);
            ScrollText("Special Description Scroll", layout, specialInfo, 42);
            RectTransform actions = Row("Turn Actions", layout, 52);
            RectTransform outcome = Row("Outcome Actions", layout, 44);
            for (int i = 0; i < buttons.Length; i++)
                if (buttons[i] != null) Place(buttons[i].transform, i < 3 ? actions : outcome, i < 3 ? 52 : 44);
            CompactBattleControls(layout);
        }

        private static void CompactBattleControls(Transform controls)
        {
            var rect = (RectTransform)controls;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0);
            rect.pivot = new Vector2(.5f, 0);
            rect.anchoredPosition = new Vector2(0, 24);
            rect.sizeDelta = new Vector2(1184, 186);
            var group = controls.GetComponent<VerticalLayoutGroup>();
            group.spacing = 8;
            group.childAlignment = TextAnchor.LowerCenter;
            Size(controls.Find("Action Feedback Scroll"), 48);
            Size(controls.Find("Special Description Scroll"), 40);
            Size(controls.Find("Turn Actions"), 72);
            foreach (var button in controls.Find("Turn Actions").GetComponentsInChildren<Button>(true))
            {
                Size(button.transform, 72);
                button.GetComponentInChildren<Text>(true).fontSize = 30;
            }
        }

        private static RectTransform ScrollText(string name, Transform parent, Text text, float height)
        {
            RectTransform viewport = Rect(name, parent);
            Size(viewport, height);
            viewport.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, .01f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.viewport = viewport;
            text.transform.SetParent(viewport, false);
            text.rectTransform.anchorMin = new Vector2(0, 1); text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.pivot = new Vector2(.5f, 1);
            text.rectTransform.sizeDelta = Vector2.zero; text.rectTransform.anchoredPosition = Vector2.zero;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow;
            text.alignment = TextAnchor.UpperCenter;
            var fit = text.GetComponent<ContentSizeFitter>() ?? text.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = text.rectTransform;
            return viewport;
        }

        private static RectTransform Row(string name, Transform parent, float height)
        {
            RectTransform row = Rect(name, parent);
            Size(row, height);
            var group = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            group.spacing = 16; group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandHeight = false;
            return row;
        }
        private static void Vertical(RectTransform rect, float spacing)
        {
            var group = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            group.spacing = spacing; group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandHeight = false;
        }
        private static void Place(Transform item, Transform parent, float height)
        {
            item.SetParent(parent, false);
            Size(item, height);
        }
        private static void Size(Transform item, float height)
        {
            var element = item.GetComponent<LayoutElement>() ?? item.gameObject.AddComponent<LayoutElement>();
            element.minWidth = 0; element.preferredWidth = 0; element.flexibleWidth = 1;
            element.minHeight = element.preferredHeight = height;
        }
        private static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }
    }
}
