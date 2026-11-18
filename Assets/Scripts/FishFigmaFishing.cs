using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Fish
{
    public sealed class FishFigmaFishing : MonoBehaviour
    {
        private RectTransform pointer;
        private GameObject heading, command, success, failure;
        private Text resultFeedback;
        private Dictionary<string, RectTransform> dial;
        public static FishFigmaFishing Build(Transform root, Text feedback, Button retry)
        {
            var existing = root.GetComponent<FishFigmaFishing>();
            if (existing != null) return existing;
            var close = root.parent.GetComponentsInChildren<Button>(true);
            Button closeButton = null;
            foreach (var button in close) if (button.name == "Close Button") closeButton = button;
            if (closeButton == null) return null;
            var view = root.gameObject.AddComponent<FishFigmaFishing>();
            // Reuse the original controls before hiding their previous layout containers.
            feedback.transform.SetParent(root, false); retry.transform.SetParent(root, false); closeButton.transform.SetParent(root, false);
            foreach (Transform child in root)
                if (child != feedback.transform && child != retry.transform && child != closeButton.transform) child.gameObject.SetActive(false);
            view.dial = FishFigmaUI.Section("2:108", root);
            var heading = FishFigmaUI.Section("6:48", root);
            view.heading = heading["6:48"].gameObject;
            var command = heading;
            view.command = command["6:52"].gameObject;
            var commandButton = command["6:52"].gameObject.AddComponent<Button>();
            var hit = command["6:52"].gameObject.AddComponent<Image>(); hit.color = Color.clear;
            commandButton.targetGraphic = hit;
            commandButton.onClick.AddListener(() => root.GetComponent<FishTimingMinigame>().RequestCatch());
            FishFigmaUI.Place(feedback.rectTransform, 614, 560, 586, 84);
            var fitter = feedback.GetComponent<ContentSizeFitter>(); if (fitter != null) fitter.enabled = false;
            feedback.font = FishFigmaUI.Font(); feedback.fontSize = 20;
            feedback.alignment = TextAnchor.UpperLeft;
            var retryLayout = retry.GetComponent<LayoutElement>(); if (retryLayout != null) retryLayout.ignoreLayout = true;
            FishFigmaUI.Place(retry.GetComponent<RectTransform>(), 624, 384, 250, 64);
            FishFigmaUI.Place(closeButton.GetComponent<RectTransform>(), 24, 24, 212, 56);
            closeButton.GetComponentInChildren<Text>().text = "Esc   Leave";
            FishFigmaUI.ButtonArt(closeButton, "2:29"); FishFigmaUI.ButtonArt(retry, "2:26");
            FishUITheme.Apply(root.gameObject);
            view.resultFeedback = feedback;
            view.success = ResultCard("2:198", root); view.failure = ResultCard("2:204", root);
            feedback.transform.SetAsLastSibling(); retry.transform.SetAsLastSibling();
            view.pointer = view.dial["3:16"];
            // Rotate the exported pointer about the dial hub, retaining its original shape.
            Vector2 size = view.pointer.sizeDelta;
            view.pointer.pivot = new Vector2((212.5f - 206.25f) / size.x, 1 - (212.5f - 55.90908f) / size.y);
            view.pointer.anchoredPosition = new Vector2(212.5f, -212.5f);
            return view;
        }
        private static GameObject ResultCard(string id, Transform root)
        {
            var layers = FishFigmaUI.Section(id, root);
            var card = layers[id]; FishFigmaUI.Place(card, 600, 252, 604, 220);
            foreach (var layer in layers) if (layer.Key != id) layer.Value.gameObject.SetActive(false);
            card.gameObject.SetActive(false); return card.gameObject;
        }
        public void Result(bool resolved, bool caught)
        {
            heading.SetActive(!resolved); command.SetActive(!resolved);
            success.SetActive(resolved && caught); failure.SetActive(resolved && !caught);
            resultFeedback.gameObject.SetActive(resolved);
            FishFigmaUI.Place(resultFeedback.rectTransform, 624, 276, 556, 112);
            if (resolved) resultFeedback.color = FishUITheme.Ink;
        }
        public void Show(float position)
        {
            if (pointer != null) pointer.localRotation = Quaternion.Euler(0, 0, 30 - 360 * position);
        }
    }
}
