using UnityEngine;
using UnityEngine.UI;
namespace Fish
{
    public sealed class FishFigmaHUD : MonoBehaviour
    {
        private Text objective, count;
        private FishCollection collection;
        private RectTransform progress;
        private float progressWidth;
        private GameObject progressTrack;
        private GameObject card, navigation, guide;
        public static void Build(Text source)
        {
            if (source.GetComponentInParent<FishFigmaHUD>() != null) return;
            var view = source.transform.parent.gameObject.AddComponent<FishFigmaHUD>();
            view.objective = source;
            view.collection = Object.FindAnyObjectByType<FishCollection>();
            var layers = FishFigmaUI.Section("6:28", source.transform.parent);
            view.card = layers["6:28"].gameObject;
            FishFigmaUI.BindText(source, layers["6:30"]);
            view.count = layers["6:31"].GetComponent<Text>();
            view.progress = layers["6:33"]; view.progressWidth = layers["6:32"].sizeDelta.x;
            view.progressTrack = layers["6:32"].gameObject;
            FishFigmaUI.Place(layers["6:30"], 20, 14, 336, 68);
            var nav = FishFigmaUI.Section("6:34", view.transform);
            view.navigation = nav["6:34"].gameObject;
            var hint = FishFigmaUI.Section("6:46", view.transform);
            view.guide = hint["6:46"].gameObject;
            foreach (var text in source.transform.root.GetComponentsInChildren<Text>(true))
            {
                if (text.name != "Nearby Prompt") continue;
                var prompt = FishFigmaUI.Section("6:43", text.transform)["6:43"];
                prompt.anchorMin = prompt.anchorMax = prompt.pivot = Vector2.one * .5f;
                prompt.anchoredPosition = Vector2.zero;
                text.enabled = false;
            }
        }
        private void LateUpdate()
        {
            bool visible = objective != null && objective.gameObject.activeSelf;
            card.SetActive(visible); navigation.SetActive(visible); guide.SetActive(visible);
            if (collection == null) return;
            bool catching = collection.Story.Stage == 0;
            count.gameObject.SetActive(catching); progressTrack.SetActive(catching);
            progress.gameObject.SetActive(catching);
            count.text = Mathf.Min(3, collection.Count) + " / 3";
            progress.sizeDelta = new Vector2(progressWidth * Mathf.Clamp01(collection.Count / 3f), progress.sizeDelta.y);
            objective.fontSize = catching ? 26 : 24;
        }
    }
}
