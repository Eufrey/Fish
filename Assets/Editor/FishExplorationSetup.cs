using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Fish.Editor
{
    public static class FishExplorationSetup
    {
        [MenuItem("FISH!/Setup Milestone 1A")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Exit Play Mode before setting up exploration.");
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            // Require a single scene so a camera or player in another scene cannot be duplicated.
            if (SceneManager.sceneCount != 1 || scene.path != "Assets/Scenes/CoastalPrototype.unity")
            {
                Debug.LogWarning("Open only Assets/Scenes/CoastalPrototype.unity, then run setup.");
                return;
            }

            GameObject[] objects = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Select(item => item.gameObject).ToArray();

            if (objects.Any(item => item.GetComponent<FishExplorationArea>() != null))
            {
                Debug.Log("Milestone 1A already exists. Existing objects and Inspector values were preserved.");
                return;
            }

            Camera[] cameras = objects.Select(item => item.GetComponent<Camera>()).Where(item => item != null).ToArray();
            if (cameras.Length != 1 || !cameras[0].isActiveAndEnabled ||
                cameras[0].GetComponent<FishOrbitCamera>() != null ||
                objects.Any(item => item.GetComponent<CharacterController>() != null ||
                    item.GetComponent<PlayerInput>() != null || item.CompareTag("Player") ||
                    item.GetComponent<FishExplorationPlayer>() != null))
            {
                Debug.LogWarning("Setup needs one active camera and no existing player/controller. Nothing was changed.");
                return;
            }

            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (actions == null || actions.FindAction("Player/Move") == null || shader == null)
            {
                Debug.LogError("The existing Player/Move input action and URP Lit shader are required. Nothing was changed.");
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup FISH! Milestone 1A");

            GameObject area = new GameObject("FISH Exploration Test Area");
            Undo.RegisterCreatedObjectUndo(area, "Create exploration area");
            Undo.AddComponent<FishExplorationArea>(area);

            Material sand = GetMaterial("Sand", new Color32(243, 214, 154, 255), shader);
            Material water = GetMaterial("Water", new Color32(119, 205, 187, 255), shader);
            Material playerMaterial = GetMaterial("Player", new Color32(233, 154, 121, 255), shader);

            // Ground top is y=0. Water is scenery outside the enclosed walkable rectangle.
            CreateBox("Beach Ground", area.transform, new Vector3(0f, -0.5f, 0f), new Vector3(24f, 1f, 20f), sand);
            CreateBox("Water Placeholder", area.transform, new Vector3(0f, -0.25f, 24f), new Vector3(60f, 0.1f, 28f), water, false);

            // Tall, invisible colliders keep the capsule on land. There is no swimming.
            CreateBox("North Boundary", area.transform, new Vector3(0f, 2f, 10f), new Vector3(25f, 6f, 1f), null);
            CreateBox("South Boundary", area.transform, new Vector3(0f, 2f, -10f), new Vector3(25f, 6f, 1f), null);
            CreateBox("East Boundary", area.transform, new Vector3(12f, 2f, 0f), new Vector3(1f, 6f, 21f), null);
            CreateBox("West Boundary", area.transform, new Vector3(-12f, 2f, 0f), new Vector3(1f, 6f, 21f), null);

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Undo.RegisterCreatedObjectUndo(player, "Create placeholder player");
            player.name = "Exploration Player";
            player.tag = "Player";
            player.transform.SetParent(area.transform, false);
            player.transform.position = new Vector3(0f, 1.1f, -3f);
            player.GetComponent<Renderer>().sharedMaterial = playerMaterial;
            Undo.DestroyObjectImmediate(player.GetComponent<CapsuleCollider>());
            CharacterController controller = Undo.AddComponent<CharacterController>(player);
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = Vector3.zero;
            controller.stepOffset = 0.25f;
            controller.slopeLimit = 45f;

            FishExplorationPlayer movement = Undo.AddComponent<FishExplorationPlayer>(player);
            movement.Configure(actions, cameras[0].transform);
            FishOrbitCamera orbit = Undo.AddComponent<FishOrbitCamera>(cameras[0].gameObject);
            orbit.Configure(player.transform);
            EditorUtility.SetDirty(movement);
            EditorUtility.SetDirty(orbit);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = player;
            Debug.Log("Milestone 1A added. Review and save the scene, then enter Play Mode. WASD: move; mouse: orbit; Escape: release cursor; click: resume.");
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collision = true)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(box, "Create " + name);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.position = position;
            box.transform.localScale = scale;
            Renderer renderer = box.GetComponent<Renderer>();
            if (material != null) renderer.sharedMaterial = material;
            else renderer.enabled = false;
            if (!collision) Undo.DestroyObjectImmediate(box.GetComponent<BoxCollider>());
            return box;
        }

        private static Material GetMaterial(string name, Color color, Shader shader)
        {
            const string folder = "Assets/FishPrototypeMaterials";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets", "FishPrototypeMaterials");
            string path = folder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0f);
            AssetDatabase.CreateAsset(material, AssetDatabase.GenerateUniqueAssetPath(path));
            return material;
        }
    }
}
