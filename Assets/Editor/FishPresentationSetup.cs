using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fish.Editor
{
    public static class FishPresentationSetup
    {
        [MenuItem("FISH!/Setup Step 6C UI and Audio")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Run Step 6C setup outside Play Mode.");
                return;
            }

            const string path = "Assets/Resources/FishAudioSettings.asset";
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            var settings = AssetDatabase.LoadAssetAtPath<FishAudioSettings>(path);
            if (settings == null)
            {
                if (AssetDatabase.LoadMainAssetAtPath(path) != null)
                {
                    Debug.LogError("An unrelated asset occupies " + path + ". Nothing was replaced.");
                    return;
                }
                settings = ScriptableObject.CreateInstance<FishAudioSettings>();
                AssetDatabase.CreateAsset(settings, path);
            }

            // Only style existing UI in the active scene. Runtime builders use the same theme.
            Scene scene = SceneManager.GetActiveScene();
            if (scene.IsValid() && scene.isLoaded)
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (Text text in root.GetComponentsInChildren<Text>(true))
                        Undo.RecordObject(text, "Style FISH UI text");
                    foreach (Button button in root.GetComponentsInChildren<Button>(true))
                    {
                        Undo.RecordObject(button, "Style FISH UI button");
                        if (button.targetGraphic != null) Undo.RecordObject(button.targetGraphic, "Style FISH UI graphic");
                    }
                    foreach (CanvasScaler scaler in root.GetComponentsInChildren<CanvasScaler>(true))
                        Undo.RecordObject(scaler, "Scale FISH UI");
                    foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
                        Undo.RecordObject(rect, "Pad FISH UI");
                    foreach (Image image in root.GetComponentsInChildren<Image>(true))
                        Undo.RecordObject(image, "Color FISH panels");
                    FishUITheme.Apply(root);
                }
                EditorSceneManager.MarkSceneDirty(scene);
            }
            AssetDatabase.SaveAssets();
            Selection.activeObject = settings;
            int listenerCount = 0;
            foreach (var listener in Object.FindObjectsByType<AudioListener>())
                if (listener.isActiveAndEnabled) listenerCount++;
            if (listenerCount > 1)
                Debug.LogWarning("More than one active AudioListener exists. Keep the intended active camera's listener; " +
                    "review extras before testing. Setup has preserved your cameras.");
            Debug.Log("Step 6C ready. Assign available clips in the selected audio settings; empty slots are silent. " +
                "Review/save scene styling. Runtime creates one presentation service, no cameras, listeners or EventSystems. P opens shore pause/volume.");
        }
    }
}
