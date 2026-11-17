using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Fish.Editor
{
    public static class FishUIProbe
    {
        [MenuItem("FISH!/Diagnostics/Capture Fishing UI")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene("Assets/Scenes/CoastalPrototype.unity");
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            var transforms = roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            var minigame = transforms.Select(t => t.GetComponent<FishTimingMinigame>()).First(m => m != null);
            var fields = new SerializedObject(minigame);
            var feedback = (Text)fields.FindProperty("feedback").objectReferenceValue;
            var retry = (Button)fields.FindProperty("retryButton").objectReferenceValue;
            FishPanelLayout.Fishing(minigame.transform, feedback, retry);
            FishPanelLayout.Fishing(minigame.transform, feedback, retry);
            var layout = minigame.transform.Find("Coastal Fishing Layout");
            if (minigame.GetComponentsInChildren<FishFishingDial>(true).Length != 1) throw new Exception("Duplicate dial");
            foreach (var canvas in transforms.Select(t => t.GetComponent<Canvas>()).Where(c => c != null)) canvas.gameObject.SetActive(false);
            var activeCanvas = minigame.GetComponentInParent<Canvas>(true);
            for (Transform t = minigame.transform; t != null; t = t.parent) t.gameObject.SetActive(true);
            var camera = new GameObject("UI probe camera", typeof(Camera)).GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = FishUITheme.Background;
            camera.cullingMask = 1 << 5;
            activeCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            activeCanvas.worldCamera = camera;
            activeCanvas.planeDistance = 1;
            foreach (var text in activeCanvas.GetComponentsInChildren<Text>(true)) text.font.RequestCharactersInTexture(text.text, text.fontSize, text.fontStyle);
            minigame.GetComponentInChildren<FishFishingDial>().ShowPosition(.7f);
            feedback.text = "Hit the green zone.\nSpace to catch / Esc to leave";
            Directory.CreateDirectory("Logs/UIProbe");
            foreach (int width in new[] { 1280, 1920, 800 })
            {
                int height = width * 9 / 16;
                var target = new RenderTexture(width, height, 24);
                camera.targetTexture = target;
                activeCanvas.GetComponent<CanvasScaler>().enabled = false;
                activeCanvas.scaleFactor = width / 1280f;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout);
                Canvas.ForceUpdateCanvases();
                ShaderUtil.allowAsyncCompilation = false;
                camera.Render(); camera.Render();
                RenderTexture.active = target;
                var image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
                File.WriteAllBytes("Logs/UIProbe/fishing-" + width + ".png", image.EncodeToPNG());
                foreach (var text in layout.GetComponentsInChildren<Text>())
                    Debug.Log("UI bounds " + width + " " + text.name + " rect=" + text.rectTransform.rect + " preferred=" + text.preferredWidth + "," + text.preferredHeight);
                UnityEngine.Object.DestroyImmediate(image);
                camera.targetTexture = null; RenderTexture.active = null;
                target.Release(); UnityEngine.Object.DestroyImmediate(target);
            }
            Debug.Log("UI probe completed; one dial after repeated adaptation. No scenes or saves written.");
        }
    }
}
