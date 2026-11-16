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
    public static class FishCollectionSetup
    {
        [MenuItem("FISH!/Setup Milestone 2A")]
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
            if (objects.Any(item => item.GetComponent<FishCollectionPanel>() != null))
            {
                Debug.Log("Milestone 2A already exists. Collection, species, and UI settings were preserved.");
                return;
            }

            FishExplorationPlayer[] players = objects.Select(item => item.GetComponent<FishExplorationPlayer>()).Where(item => item != null).ToArray();
            FishOrbitCamera[] cameras = objects.Select(item => item.GetComponent<FishOrbitCamera>()).Where(item => item != null).ToArray();
            FishFishingSpot[] spots = objects.Select(item => item.GetComponent<FishFishingSpot>()).Where(item => item != null).ToArray();
            FishTimingMinigame[] games = objects.Select(item => item.GetComponent<FishTimingMinigame>()).Where(item => item != null).ToArray();
            FishCollection[] collections = objects.Select(item => item.GetComponent<FishCollection>()).Where(item => item != null).ToArray();
            EventSystem[] systems = objects.Select(item => item.GetComponent<EventSystem>()).Where(item => item != null).ToArray();
            BaseInputModule[] modules = objects.SelectMany(item => item.GetComponents<BaseInputModule>()).ToArray();
            if (players.Length != 1 || cameras.Length != 1 || spots.Length != 1 || games.Length != 1 || collections.Length > 1 ||
                !players[0].isActiveAndEnabled || !cameras[0].isActiveAndEnabled || !spots[0].isActiveAndEnabled ||
                spots[0].FishingPanel == null || systems.Length != 1 || !systems[0].isActiveAndEnabled ||
                modules.Length != 1 || !(modules[0] is InputSystemUIInputModule) ||
                !modules[0].isActiveAndEnabled || modules[0].gameObject != systems[0].gameObject)
            {
                Debug.LogWarning("Run the 1A-1C setups first. One player, camera, fishing spot, timing game and compatible EventSystem are required; duplicate collections are not supported. Nothing was changed.");
                return;
            }

            Canvas canvas = spots[0].FishingPanel.GetComponentInParent<Canvas>(true);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (canvas == null || font == null || games[0].HasCollectionSetup || canvas.transform.Find("Collection UI") != null)
            {
                Debug.LogWarning("The existing fishing canvas/font is required. Resolve a partial or manually configured collection setup before proceeding. Nothing was changed.");
                return;
            }

            const string speciesPath = "Assets/Data/Fish/PlaceholderFish.asset";
            Object existingAsset = AssetDatabase.LoadMainAssetAtPath(speciesPath);
            FishSpecies species = existingAsset as FishSpecies;
            if (existingAsset != null && (species == null || !species.isPlaceholder || species.speciesId != "placeholder-fish"))
            {
                Debug.LogWarning("The placeholder asset path is already occupied by other data. Nothing was changed.");
                return;
            }

            if (species == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Data")) AssetDatabase.CreateFolder("Assets", "Data");
                if (!AssetDatabase.IsValidFolder("Assets/Data/Fish")) AssetDatabase.CreateFolder("Assets/Data", "Fish");
                species = ScriptableObject.CreateInstance<FishSpecies>();
                species.speciesId = "placeholder-fish";
                species.displayName = "Placeholder Fish (unresearched)";
                species.isPlaceholder = true;
                // No unverified species names, biological content, stats, or references are invented.
                AssetDatabase.CreateAsset(species, speciesPath);
            }

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup FISH! Milestone 2A");
            FishCollection collection = collections.Length == 1 ? collections[0] : Undo.AddComponent<FishCollection>(players[0].gameObject);

            GameObject root = FishTimingSetup.CreateUI("Collection UI", canvas.transform);
            Stretch(root.GetComponent<RectTransform>());
            Text hint = FishTimingSetup.CreateText("Collection Hint", root.transform, "C: Collection", font, 24, Vector2.zero, new Vector2(500f, 50f));
            hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            FishHUDLayout.Hint(hint, 0);

            GameObject panel = FishTimingSetup.CreateUI("Collection Panel", root.transform);
            Stretch(panel.GetComponent<RectTransform>());
            Image backdrop = FishTimingSetup.AddImage(panel, new Color(0.03f, 0.08f, 0.1f, 0.96f));
            backdrop.raycastTarget = true;
            FishTimingSetup.CreateText("Title", panel.transform, "Fish Collection", font, 36,
                new Vector2(0f, 190f), new Vector2(800f, 60f));
            Text contents = FishTimingSetup.CreateText("Names and Quantities", panel.transform, "No fish collected yet.", font, 28,
                new Vector2(0f, 40f), new Vector2(950f, 200f));
            FishTimingSetup.CreateText("Session Note", panel.transform, "Current Play session only.  C / Escape: close", font, 22,
                new Vector2(0f, -95f), new Vector2(900f, 45f));
            GameObject closeObject = FishTimingSetup.CreateUI("Close Collection", panel.transform);
            RectTransform closeRect = closeObject.GetComponent<RectTransform>();
            closeRect.anchoredPosition = new Vector2(0f, -165f);
            closeRect.sizeDelta = new Vector2(260f, 60f);
            Image closeImage = FishTimingSetup.AddImage(closeObject, new Color32(71, 123, 86, 255));
            closeImage.raycastTarget = true;
            Button closeButton = Undo.AddComponent<Button>(closeObject);
            closeButton.targetGraphic = closeImage;
            FishTimingSetup.CreateText("Label", closeObject.transform, "Return to beach", font, 24, Vector2.zero, new Vector2(260f, 60f));
            panel.SetActive(false);

            FishCollectionPanel controller = Undo.AddComponent<FishCollectionPanel>(root);
            controller.Configure(collection, players[0], cameras[0], spots[0], panel, contents, hint, closeButton);
            Undo.RecordObject(games[0], "Connect collection reward");
            games[0].ConfigureCollection(collection, species);
            Undo.RecordObject(spots[0], "Connect panel exclusion");
            spots[0].ConfigureCollectionPanel(controller);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(games[0]);
            EditorUtility.SetDirty(spots[0]);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("Milestone 2A added. Save, then Play. C opens collection; a successful fishing attempt adds one owned Placeholder Fish.");
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}

