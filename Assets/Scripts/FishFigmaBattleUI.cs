using UnityEngine;
using UnityEngine.UI;
namespace Fish
{
    public static class FishFigmaBattleUI
    {
        public static void Apply(Transform root, Text result, params Button[] actions)
        {
            if (root.Find("Figma Turn badge") != null) return;
            var badge = FishFigmaUI.Section("6:70", root);
            FishFigmaUI.BindText(result, badge["6:71"]);
            result.alignment = TextAnchor.MiddleCenter;
            for (int i = 0; i < actions.Length; i++)
            {
                var button = actions[i]; if (button == null) continue;
                string id = i == 0 ? "6:76" : i == 1 ? "6:80" : "6:84";
                var icon = FishFigmaUI.Section(id, button.transform)[id];
                FishFigmaUI.Place(icon, 18, 18, i == 2 ? 40 : 30, 40);
                var label = button.GetComponentInChildren<Text>();
                label.rectTransform.offsetMin = new Vector2(64, 6);
                label.rectTransform.offsetMax = new Vector2(-12, -6);
                label.alignment = TextAnchor.MiddleLeft;
            }
        }
    }
}
