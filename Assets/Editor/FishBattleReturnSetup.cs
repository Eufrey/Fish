using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fish.Editor
{
    public static class FishBattleReturnSetup
    {
        [MenuItem("FISH!/Setup Milestone 3B")]
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
            FishBattleEncounter[] encounters = objects.SelectMany(item => item.GetComponents<FishBattleEncounter>()).ToArray();
            if (encounters.Length != 1)
            {
                Debug.LogWarning("Run Milestone 3A setup first. Exactly one encounter is required for this test-area upgrade. Nothing was changed.");
                return;
            }
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup FISH! Milestone 3B shore return");
            if (!ConfigureReturnPoint(encounters[0], objects)) return;
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = encounters[0].ShoreReturnPoint.gameObject;
            Debug.Log("Milestone 3B shore return configured. Review the point on the beach and save. No battle scene, manager or input handler was added.");
        }

        internal static bool ConfigureReturnPoint(FishBattleEncounter encounter, Transform[] objects)
        {
            Collider ground = encounter.ShoreGround;
            if (ground == null)
                ground = objects.Where(item => item.name == "Beach Ground").Select(item => item.GetComponent<Collider>()).FirstOrDefault(item => item != null);
            if (ground == null)
            {
                Debug.LogWarning("Assign the beach ground collider before setting up a shore return. Nothing was changed.");
                return false;
            }
            Transform point = encounter.ShoreReturnPoint;
            if (point == null) point = encounter.transform.Find("Shore Return Point");
            if (point == null)
            {
                GameObject pointObject = new GameObject("Shore Return Point");
                Undo.RegisterCreatedObjectUndo(pointObject, "Create safe shore return point");
                point = pointObject.transform;
                point.SetParent(encounter.transform, false);
                point.localPosition = new Vector3(0f, 0f, -3f);
            }
            if (encounter.ShoreReturnPoint == point && encounter.ShoreGround == ground) return true;
            Undo.RecordObject(encounter, "Connect shore return point");
            encounter.ConfigureShoreReturn(point, ground);
            EditorUtility.SetDirty(encounter);
            return true;
        }
    }
}
