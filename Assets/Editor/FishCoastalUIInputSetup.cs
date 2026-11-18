using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace Fish.Editor
{
    public static class FishCoastalUIInputSetup
    {
        public static void Repair()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run outside Play Mode.");
            var scene = SceneManager.GetActiveScene();
            var modules = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<InputSystemUIInputModule>(true)).ToArray();
            if (modules.Length != 1)
                throw new InvalidOperationException("Expected one existing UI input module.");
            var module = modules[0];
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (actions == null) throw new InvalidOperationException("Project input actions are missing.");
            Undo.RecordObject(module, "Repair UI input actions");
            module.actionsAsset = actions;
            module.point = Reference(actions, "Point");
            module.leftClick = Reference(actions, "Click");
            module.rightClick = Reference(actions, "RightClick");
            module.middleClick = Reference(actions, "MiddleClick");
            module.scrollWheel = Reference(actions, "ScrollWheel");
            module.move = Reference(actions, "Navigate");
            module.submit = Reference(actions, "Submit");
            module.cancel = Reference(actions, "Cancel");
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("UI input actions restored on the existing EventSystem. Save the scene.");
        }

        private static InputActionReference Reference(InputActionAsset asset, string name)
        {
            var action = asset.FindAction("UI/" + name, true);
            const string path = "Assets/Settings/FishUIInputReferences.asset";
            var references = AssetDatabase.LoadAllAssetsAtPath(path).OfType<InputActionReference>();
            var existing = references.FirstOrDefault(reference => reference.action == action);
            if (existing != null) return existing;
            var created = InputActionReference.Create(action);
            created.name = name;
            if (AssetDatabase.LoadMainAssetAtPath(path) == null) AssetDatabase.CreateAsset(created, path);
            else AssetDatabase.AddObjectToAsset(created, path);
            return created;
        }
    }
}
