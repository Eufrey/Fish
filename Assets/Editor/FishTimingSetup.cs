using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fish.Editor
{
    public static class FishTimingSetup
    {
        [MenuItem("FISH!/Setup Milestone 1C")]
        public static void Setup()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.sceneCount != 1 ||
                scene.path != "Assets/Scenes/CoastalPrototype.unity")
            {
                Debug.LogWarning("Outside Play Mode, open only CoastalPrototype.unity, then run setup.");
                return;
            }

            Transform[] objects = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            if (objects.Any(item => item.GetComponent<FishTimingMinigame>() != null))
            {
                Debug.Log("Milestone 1C already exists. Existing UI and settings were preserved.");
                return;
            }
            FishFishingSpot[] spots = objects.Select(item => item.GetComponent<FishFishingSpot>()).Where(item => item != null).ToArray();
            EventSystem[] systems = objects.Select(item => item.GetComponent<EventSystem>()).Where(item => item != null).ToArray();
            BaseInputModule[] modules = objects.SelectMany(item => item.GetComponents<BaseInputModule>()).ToArray();
            if (spots.Length != 1 || spots[0].FishingPanel == null || spots[0].CloseButton == null ||
                systems.Length != 1 || !systems[0].isActiveAndEnabled || modules.Length != 1 ||
                !(modules[0] is InputSystemUIInputModule) || !modules[0].isActiveAndEnabled ||
                modules[0].gameObject != systems[0].gameObject)
            {
                Debug.LogWarning("Run Milestone 1B setup first. One fishing spot and one active Input System EventSystem are required. Nothing was changed.");
                return;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) { Debug.LogError("Built-in UI font not found. Nothing was changed."); return; }
            GameObject panel = spots[0].FishingPanel;
            if (panel.transform.Find("Timing Minigame") != null)
            {
                Debug.LogWarning("An unrecognized Timing Minigame object already exists. Resolve it before setup. Nothing was changed.");
                return;
            }

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup FISH! Milestone 1C");

            // Retain the old text for Undo; replace only the known 1B placeholder.
            Transform oldMessage = panel.transform.Find("Placeholder Message");
            if (oldMessage != null)
            {
                Undo.RecordObject(oldMessage.gameObject, "Hide placeholder message");
                oldMessage.gameObject.SetActive(false);
            }

            GameObject root = CreateUI("Timing Minigame", panel.transform);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;

            CreateText("Title", root.transform, "Fishing", font, 36, new Vector2(0f, 190f), new Vector2(700f, 60f));
            CreateText("Instructions", root.transform, "Space: attempt catch    |    Escape: cancel / close", font, 24,
                new Vector2(0f, 135f), new Vector2(800f, 45f));

            GameObject bar = CreateUI("Timing Bar", root.transform);
            RectTransform barRect = bar.GetComponent<RectTransform>();
            barRect.anchoredPosition = new Vector2(0f, 60f);
            barRect.sizeDelta = new Vector2(600f, 42f);
            AddImage(bar, new Color32(83, 106, 112, 255));

            GameObject zone = CreateUI("Target Zone", bar.transform);
            RectTransform zoneRect = zone.GetComponent<RectTransform>();
            zoneRect.anchorMin = new Vector2(FishTimingAttempt.TargetStart, 0f);
            zoneRect.anchorMax = new Vector2(FishTimingAttempt.TargetEnd, 1f);
            zoneRect.offsetMin = zoneRect.offsetMax = Vector2.zero;
            AddImage(zone, new Color32(71, 183, 110, 255));

            GameObject indicator = CreateUI("Moving Indicator", bar.transform);
            RectTransform indicatorRect = indicator.GetComponent<RectTransform>();
            indicatorRect.anchorMin = indicatorRect.anchorMax = new Vector2(0f, 0.5f);
            indicatorRect.sizeDelta = new Vector2(8f, 64f);
            AddImage(indicator, Color.white);

            Text feedback = CreateText("Result Feedback", root.transform,
                "Press Space when the white indicator is inside the green zone.", font, 26,
                new Vector2(0f, -40f), new Vector2(1100f, 110f));

            GameObject retryObject = CreateUI("Retry Button", root.transform);
            RectTransform retryRect = retryObject.GetComponent<RectTransform>();
            retryRect.anchoredPosition = new Vector2(-140f, -140f);
            retryRect.sizeDelta = new Vector2(240f, 60f);
            Image retryImage = AddImage(retryObject, new Color32(71, 123, 86, 255));
            retryImage.raycastTarget = true;
            Button retry = Undo.AddComponent<Button>(retryObject);
            retry.targetGraphic = retryImage;
            Text retryLabel = CreateText("Label", retryObject.transform, "Retry", font, 24, Vector2.zero, new Vector2(240f, 60f));
            retryLabel.raycastTarget = false;

            // Keep 1B's Close button and its listener, moving it beside Retry.
            RectTransform closeRect = spots[0].CloseButton.GetComponent<RectTransform>();
            Undo.RecordObject(closeRect, "Position Close button");
            closeRect.anchoredPosition = new Vector2(140f, -140f);

            FishTimingMinigame minigame = Undo.AddComponent<FishTimingMinigame>(root);
            minigame.Configure(indicatorRect, feedback, retry);
            EditorUtility.SetDirty(minigame);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("Milestone 1C added to the existing panel. Save, approach the dock, press E, then Space to attempt a catch.");
        }

        internal static GameObject CreateUI(string name, Transform parent)
        {
            GameObject item = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(item, "Create " + name);
            item.transform.SetParent(parent, false);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            return item;
        }

        internal static Image AddImage(GameObject item, Color color)
        {
            Image image = Undo.AddComponent<Image>(item);
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        internal static Text CreateText(string name, Transform parent, string content, Font font, int size, Vector2 position, Vector2 dimensions)
        {
            Text text = Undo.AddComponent<Text>(CreateUI(name, parent));
            text.text = content;
            text.font = font;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.rectTransform.anchoredPosition = position;
            text.rectTransform.sizeDelta = dimensions;
            return text;
        }
    }
}
