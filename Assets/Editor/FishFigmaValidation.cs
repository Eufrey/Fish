using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Fish.Editor
{
    // A disposable preview scene exercises imports without changing the authored scene.
    [InitializeOnLoad]
    public static class FishFigmaValidation
    {
        static FishFigmaValidation() { EditorApplication.update += CheckRequest; }
        private static void CheckRequest()
        {
            const string request = "Logs/FigmaImport/capture.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(request);
            try { Capture(); } catch (Exception error) { File.WriteAllText("Logs/FigmaImport/validation.txt", error.ToString()); Debug.LogException(error); }
        }
        [MenuItem("FISH!/Validate Imported Figma UI")]
        public static void Capture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var source = Resources.FindObjectsOfTypeAll<FishTimingMinigame>().FirstOrDefault(m => m.gameObject.scene.IsValid());
            if (source == null) throw new InvalidOperationException("Open the shore scene first.");
            var scene = EditorSceneManager.NewPreviewScene();
            GameObject clone = null;
            try
            {
                var canvas = source.GetComponentInParent<Canvas>(true);
                clone = UnityEngine.Object.Instantiate(canvas.gameObject);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(clone, scene);
                var minigame = clone.GetComponentInChildren<FishTimingMinigame>(true);
                FishUITheme.Apply(clone);
                var data = new SerializedObject(minigame);
                var feedback = (Text)data.FindProperty("feedback").objectReferenceValue;
                var retry = (Button)data.FindProperty("retryButton").objectReferenceValue;
                FishPanelLayout.Fishing(minigame.transform, feedback, retry);
                FishPanelLayout.Fishing(minigame.transform, feedback, retry);
                if (minigame.GetComponents<FishFigmaFishing>().Length != 1) throw new Exception("Imported fishing view duplicated.");
                foreach (Transform child in clone.transform) child.gameObject.SetActive(false);
                for (Transform t = minigame.transform; t != null && t != clone.transform; t = t.parent) t.gameObject.SetActive(true);
                minigame.GetComponent<FishFigmaFishing>().Show(.7f);
                minigame.GetComponent<FishFigmaFishing>().Result(false, false);
                retry.gameObject.SetActive(false);
                var camera = new GameObject("Preview camera", typeof(Camera)).GetComponent<Camera>();
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = FishUITheme.Background;
                camera.cullingMask = -1; camera.scene = scene;
                var previewCanvas = clone.GetComponent<Canvas>();
                previewCanvas.renderMode = RenderMode.ScreenSpaceCamera; previewCanvas.worldCamera = camera; previewCanvas.planeDistance = 1;
                clone.GetComponent<CanvasScaler>().enabled = false;
                foreach (var text in clone.GetComponentsInChildren<Text>(true)) if (text.font != null) text.font.RequestCharactersInTexture(text.text, text.fontSize, text.fontStyle);
                Directory.CreateDirectory("Logs/FigmaImport");
                foreach (int width in new[] {1280,1920,800})
                {
                    int height = width * 9 / 16;
                    var target = new RenderTexture(width,height,24);
                    var previous = RenderTexture.active;
                    try
                    {
                        camera.targetTexture = target; previewCanvas.scaleFactor = width / 1280f;
                        Canvas.ForceUpdateCanvases();
                        ShaderUtil.allowAsyncCompilation = false; camera.Render(); camera.Render();
                        RenderTexture.active = target;
                        var image = new Texture2D(width,height,TextureFormat.RGB24,false);
                        image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
                        File.WriteAllBytes("Logs/FigmaImport/fishing-"+width+".png",image.EncodeToPNG());
                        UnityEngine.Object.DestroyImmediate(image);
                    }
                    finally { camera.targetTexture=null; RenderTexture.active=previous; target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                }
                File.WriteAllText("Logs/FigmaImport/validation.txt", "PASS: repeated setup has one imported view. Rendered fishing at 1280x720, 1920x1080, 800x450. Original scene and saves untouched.");
            }
            finally { if (clone != null) UnityEngine.Object.DestroyImmediate(clone); EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}
