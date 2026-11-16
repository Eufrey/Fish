using UnityEngine;
using UnityEngine.UI;
namespace Fish
{
    // Rebuilds presentation once, preserving the existing buttons and listeners.
    public static class FishTeamLayout
    {
        public static void Apply(GameObject panel)
        {
            if (panel.transform.Find("Layout") != null) return;
            Transform[] original = new Transform[panel.transform.childCount];
            for (int i = 0; i < original.Length; i++) original[i] = panel.transform.GetChild(i);
            RectTransform layout = Rect("Layout", panel.transform);
            layout.anchorMin = Vector2.zero; layout.anchorMax = Vector2.one;
            layout.offsetMin = new Vector2(28,28); layout.offsetMax = new Vector2(-28,-28);
            var vertical = layout.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.spacing = 20; vertical.childControlHeight = true; vertical.childForceExpandHeight = false;
            RectTransform header = Group("Header", layout, false, 118);
            RectTransform viewport = Rect("Content", layout);
            viewport.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.viewport = viewport;
            viewport.gameObject.AddComponent<Image>().color = new Color(0,0,0,0.01f);
            RectTransform content = Group("Columns", viewport, true, 0);
            content.anchorMin = new Vector2(0,1); content.anchorMax = Vector2.one; content.pivot = new Vector2(.5f,1);
            content.sizeDelta = Vector2.zero;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;
            RectTransform left = Group("Owned", content, false, 0), right = Group("Ordered", content, false, 0);
            // Long names must increase row height, not steal width from the other column.
            left.GetComponent<LayoutElement>().minWidth = right.GetComponent<LayoutElement>().minWidth = 0;
            left.GetComponent<LayoutElement>().preferredWidth = right.GetComponent<LayoutElement>().preferredWidth = 0;
            RectTransform pages = Group("Pages", left, true, 52), editing = Group("Editing", right, true, 52);
            RectTransform footer = Group("Footer", layout, true, 56);
            foreach (Transform item in original)
            {
                string n = item.name;
                Transform parent = n == "Title" || n == "Instructions" ? header : n == "Status" ? layout :
                    n == "Confirm Team" || n == "Close Team" ? footer : n == "Previous Page" || n == "Next Page" ? pages :
                    n == "Remove" || n == "Move Up" || n == "Move Down" ? editing : n.StartsWith("Owned") ? left : right;
                item.SetParent(parent, false);
                var size = item.gameObject.AddComponent<LayoutElement>();
                size.preferredHeight = n.Contains("Catch") || n.StartsWith("Team Slot") ? 120 : n == "Status" ? 64 : n == "Instructions" ? 60 : n.EndsWith("Header") ? 60 : 44;
                // Text preferred widths otherwise force the two columns outside the panel.
                size.minWidth = size.preferredWidth = 0;
                size.flexibleWidth = 1;
                foreach (Text text in item.GetComponentsInChildren<Text>(true))
                {
                    text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
                    if (text.transform != item)
                    {
                        text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
                        text.rectTransform.offsetMin = new Vector2(12,8); text.rectTransform.offsetMax = new Vector2(-12,-8);
                    }
                }
            }
            pages.SetAsLastSibling(); editing.SetAsLastSibling(); footer.SetAsLastSibling();
            FishUITheme.Apply(panel);
        }
        public static void FitRows(Button[] buttons)
        {
            Canvas.ForceUpdateCanvases();
            foreach (var button in buttons)
            {
                Text label = button.GetComponentInChildren<Text>(true);
                LayoutElement element = button.GetComponent<LayoutElement>();
                if (label != null && element != null && button.gameObject.activeInHierarchy)
                    element.preferredHeight = Mathf.Max(120, label.preferredHeight + 24);
            }
        }
        private static RectTransform Rect(string name, Transform parent)
        { var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false); return r; }
        private static RectTransform Group(string name, Transform parent, bool horizontal, float height)
        {
            var r = Rect(name,parent);
            HorizontalOrVerticalLayoutGroup g = horizontal ? (HorizontalOrVerticalLayoutGroup)r.gameObject.AddComponent<HorizontalLayoutGroup>() : r.gameObject.AddComponent<VerticalLayoutGroup>();
            g.spacing = 14; g.childControlWidth = g.childControlHeight = true; g.childForceExpandHeight = false;
            var e = r.gameObject.AddComponent<LayoutElement>(); e.flexibleWidth = 1; if(height > 0) e.preferredHeight = height;
            return r;
        }
    }
}
