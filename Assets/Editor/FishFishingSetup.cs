using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fish.Editor
{
    public static class FishFishingSetup
    {
        private const string InputPath = "Assets/InputSystem_Actions.inputactions";

        [MenuItem("FISH!/Setup Milestone 1B")]
        public static void Setup()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.sceneCount != 1 ||
                scene.path != "Assets/Scenes/CoastalPrototype.unity")
            {
                Debug.LogWarning("Outside Play Mode, open only CoastalPrototype.unity, then run setup.");
                return;
            }

            Transform[] objects = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            if (objects.Any(item => item.GetComponent<FishFishingSpot>() != null))
            {
                Debug.Log("Milestone 1B already exists. Existing objects and settings were preserved.");
                return;
            }

            FishExplorationPlayer[] players = objects.Select(item => item.GetComponent<FishExplorationPlayer>()).Where(item => item != null).ToArray();
            FishOrbitCamera[] cameras = objects.Select(item => item.GetComponent<FishOrbitCamera>()).Where(item => item != null).ToArray();
            EventSystem[] systems = objects.Select(item => item.GetComponent<EventSystem>()).Where(item => item != null).ToArray();
            BaseInputModule[] modules = objects.SelectMany(item => item.GetComponents<BaseInputModule>()).ToArray();
            if (players.Length != 1 || cameras.Length != 1 || !players[0].isActiveAndEnabled || !cameras[0].isActiveAndEnabled)
            {
                Debug.LogWarning("Run Milestone 1A setup first. Exactly one active exploration player and orbit camera are required.");
                return;
            }

            // Preserve an existing compatible EventSystem. Never add a second handler or replace one silently.
            if (systems.Length > 1 || modules.Length > 1 ||
                (systems.Length == 0 && modules.Length != 0) ||
                (systems.Length == 1 && !systems[0].isActiveAndEnabled) ||
                (modules.Length == 1 && (!(modules[0] is InputSystemUIInputModule) ||
                    !modules[0].isActiveAndEnabled || modules[0].gameObject != systems[0].gameObject)))
            {
                Debug.LogWarning("Resolve duplicate, inactive, or legacy EventSystem/input modules first. Nothing was changed.");
                return;
            }

            if (modules.Length == 1)
            {
                InputSystemUIInputModule existing = (InputSystemUIInputModule)modules[0];
                if (existing.point == null || existing.leftClick == null || existing.submit == null)
                {
                    Debug.LogWarning("Configure Point, Left Click and Submit on the existing InputSystemUIInputModule first. Nothing was changed.");
                    return;
                }
            }

            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            InputActionReference[] references = AssetDatabase.LoadAllAssetsAtPath(InputPath).OfType<InputActionReference>().ToArray();
            string[] uiActions = { "Point", "Navigate", "Click", "RightClick", "MiddleClick", "ScrollWheel", "Submit", "Cancel", "TrackedDevicePosition", "TrackedDeviceOrientation" };
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (actions == null || actions.FindAction("Player/Interact") == null || shader == null || font == null ||
                uiActions.Any(name => !references.Any(reference => reference.action != null && reference.action.actionMap.name == "UI" && reference.action.name == name)))
            {
                Debug.LogError("Existing Interact/UI actions, URP Lit shader, and built-in font are required. Nothing was changed.");
                return;
            }

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup FISH! Milestone 1B");
            GameObject root = new GameObject("FISH Fishing Interaction");
            Undo.RegisterCreatedObjectUndo(root, "Create fishing interaction");

            // Fits inside Milestone 1A's north boundary. Leave that boundary intact.
            GameObject dock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(dock, "Create placeholder dock");
            dock.name = "Placeholder Dock";
            dock.transform.SetParent(root.transform, false);
            dock.transform.position = new Vector3(0f, 0.1f, 7f);
            dock.transform.localScale = new Vector3(3f, 0.2f, 4f);
            dock.GetComponent<Renderer>().sharedMaterial = GetDockMaterial(shader);

            GameObject point = new GameObject("Fishing Interaction Point");
            Undo.RegisterCreatedObjectUndo(point, "Create interaction point");
            point.transform.SetParent(root.transform, false);
            point.transform.position = new Vector3(0f, 1.2f, 8f);

            GameObject canvasObject = CreateUI("Fishing UI", root.transform);
            Canvas canvas = Undo.AddComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = Undo.AddComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            Undo.AddComponent<GraphicRaycaster>(canvasObject);

            Text prompt = CreateText("Nearby Prompt", canvasObject.transform, "Press E to Fish", font, 28);
            RectTransform promptRect = prompt.rectTransform;
            promptRect.anchorMin = promptRect.anchorMax = new Vector2(0.5f, 0f);
            promptRect.anchoredPosition = new Vector2(0f, 80f);
            promptRect.sizeDelta = new Vector2(500f, 60f);
            prompt.gameObject.SetActive(false);

            GameObject panel = CreateUI("Fishing Panel", canvasObject.transform);
            Stretch(panel.GetComponent<RectTransform>());
            Image backdrop = Undo.AddComponent<Image>(panel);
            backdrop.color = new Color(0.03f, 0.08f, 0.1f, 0.9f);
            Text title = CreateText("Placeholder Message", panel.transform,
                "Fishing\n\nThe fishing minigame will go here.\n\nPress Escape to return to exploration.", font, 28);
            title.rectTransform.anchoredPosition = new Vector2(0f, 55f);
            title.rectTransform.sizeDelta = new Vector2(700f, 260f);

            GameObject buttonObject = CreateUI("Close Button", panel.transform);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(0f, -140f);
            buttonRect.sizeDelta = new Vector2(240f, 60f);
            Image buttonImage = Undo.AddComponent<Image>(buttonObject);
            buttonImage.color = new Color32(71, 123, 86, 255);
            Button button = Undo.AddComponent<Button>(buttonObject);
            button.targetGraphic = buttonImage;
            // Escape is owned by FishFishingSpot, not a second UI Cancel handler.
            Text label = CreateText("Label", buttonObject.transform, "Return to beach", font, 24);
            Stretch(label.rectTransform);
            panel.SetActive(false);

            if (modules.Length == 0)
            {
                GameObject eventObject;
                if (systems.Length == 1) eventObject = systems[0].gameObject;
                else
                {
                    eventObject = new GameObject("EventSystem");
                    Undo.RegisterCreatedObjectUndo(eventObject, "Create EventSystem");
                    eventObject.transform.SetParent(root.transform, false);
                    Undo.AddComponent<EventSystem>(eventObject);
                }
                InputSystemUIInputModule module = Undo.AddComponent<InputSystemUIInputModule>(eventObject);
                module.actionsAsset = actions;
                module.point = Reference(references, "Point");
                module.move = Reference(references, "Navigate");
                module.leftClick = Reference(references, "Click");
                module.rightClick = Reference(references, "RightClick");
                module.middleClick = Reference(references, "MiddleClick");
                module.scrollWheel = Reference(references, "ScrollWheel");
                module.submit = Reference(references, "Submit");
                module.cancel = Reference(references, "Cancel");
                module.trackedDevicePosition = Reference(references, "TrackedDevicePosition");
                module.trackedDeviceOrientation = Reference(references, "TrackedDeviceOrientation");
                EditorUtility.SetDirty(module);
            }

            FishFishingSpot spot = Undo.AddComponent<FishFishingSpot>(point);
            spot.Configure(players[0], cameras[0], actions, prompt.gameObject, panel, button);
            EditorUtility.SetDirty(spot);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = point;
            Debug.Log("Milestone 1B added. Save the scene. Walk north to the dock, press E, then Escape or Return to beach.");
        }

        private static InputActionReference Reference(InputActionReference[] references, string name)
        {
            return references.First(reference => reference.action != null &&
                reference.action.actionMap.name == "UI" && reference.action.name == name);
        }

        private static GameObject CreateUI(string name, Transform parent)
        {
            GameObject item = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(item, "Create " + name);
            item.transform.SetParent(parent, false);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            return item;
        }

        private static Text CreateText(string name, Transform parent, string content, Font font, int size)
        {
            Text text = Undo.AddComponent<Text>(CreateUI(name, parent));
            text.text = content;
            text.font = font;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static Material GetDockMaterial(Shader shader)
        {
            const string folder = "Assets/FishPrototypeMaterials";
            const string path = folder + "/Dock.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "FishPrototypeMaterials");
            material = new Material(shader) { name = "Dock" };
            material.SetColor("_BaseColor", new Color32(217, 166, 111, 255));
            material.SetFloat("_Smoothness", 0f);
            AssetDatabase.CreateAsset(material, AssetDatabase.GenerateUniqueAssetPath(path));
            return material;
        }
    }
}
