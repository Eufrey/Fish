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
    public static class FishTeamSetup
    {
        [MenuItem("FISH!/Setup Milestone 2B")]
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
            if (objects.Any(item => item.GetComponent<FishTeamPanel>() != null))
            {
                Debug.Log("Milestone 2B already exists. Team selection and UI settings were preserved.");
                return;
            }
            FishExplorationPlayer[] players = Find<FishExplorationPlayer>(objects);
            FishOrbitCamera[] cameras = Find<FishOrbitCamera>(objects);
            FishFishingSpot[] spots = Find<FishFishingSpot>(objects);
            FishCollection[] collections = Find<FishCollection>(objects);
            FishCollectionPanel[] collectionPanels = Find<FishCollectionPanel>(objects);
            FishBattleTeam[] teams = Find<FishBattleTeam>(objects);
            EventSystem[] systems = Find<EventSystem>(objects);
            BaseInputModule[] modules = objects.SelectMany(item => item.GetComponents<BaseInputModule>()).ToArray();
            if (players.Length != 1 || cameras.Length != 1 || spots.Length != 1 || collections.Length != 1 ||
                collectionPanels.Length != 1 || teams.Length > 1 || systems.Length != 1 || modules.Length != 1 ||
                !(modules[0] is InputSystemUIInputModule) || modules[0].gameObject != systems[0].gameObject ||
                !modules[0].isActiveAndEnabled || !systems[0].isActiveAndEnabled ||
                !players[0].isActiveAndEnabled || !cameras[0].isActiveAndEnabled || !spots[0].isActiveAndEnabled ||
                !collectionPanels[0].isActiveAndEnabled || spots[0].FishingPanel == null)
            {
                Debug.LogWarning("Set up 1A-2A first, with one active player, camera, fishing/collection controller and Input System EventSystem. Nothing was changed.");
                return;
            }
            Canvas canvas = spots[0].FishingPanel.GetComponentInParent<Canvas>(true);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (canvas == null || font == null || canvas.transform.Find("Team UI") != null ||
                (teams.Length == 1 && teams[0].Collection != collections[0]))
            {
                Debug.LogWarning("Resolve missing canvas/font or conflicting partial team setup first. Nothing was changed.");
                return;
            }

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup FISH! Milestone 2B");
            FishBattleTeam team = teams.Length == 1 ? teams[0] : Undo.AddComponent<FishBattleTeam>(players[0].gameObject);
            if (teams.Length == 0) team.Configure(collections[0]);
            EditorUtility.SetDirty(team);

            GameObject root = FishTimingSetup.CreateUI("Team UI", canvas.transform);
            Stretch(root.GetComponent<RectTransform>());
            Text hint = FishTimingSetup.CreateText("Team Hint", root.transform, "T: Team", font, 24, Vector2.zero, new Vector2(500f, 40f));
            hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            FishHUDLayout.Hint(hint, 1);
            GameObject panel = FishTimingSetup.CreateUI("Team Panel", root.transform);
            Stretch(panel.GetComponent<RectTransform>());
            FishTimingSetup.AddImage(panel, new Color(0.03f, 0.08f, 0.1f, 0.98f)).raycastTarget = true;
            FishTimingSetup.CreateText("Title", panel.transform, "Battle Team Selection", font, 34, new Vector2(0f, 285f), new Vector2(1100f, 50f));
            FishTimingSetup.CreateText("Instructions", panel.transform, "Add owned catches. Click a team slot, then Remove / Up / Down. T or Escape: close.",
                font, 21, new Vector2(0f, 235f), new Vector2(1170f, 45f));
            Text pageLabel = FishTimingSetup.CreateText("Owned Header", panel.transform, "Owned catches", font, 24, new Vector2(-310f, 185f), new Vector2(540f, 45f));
            FishTimingSetup.CreateText("Team Header", panel.transform, "Ordered team - fictional gameplay stats", font, 23, new Vector2(310f, 185f), new Vector2(540f, 45f));

            var catchButtons = new Button[4];
            var catchLabels = new Text[4];
            for (int row = 0; row < catchButtons.Length; row++)
                catchButtons[row] = CreateButton("Owned Catch " + (row + 1), panel.transform, "", font,
                    new Vector2(-310f, 120f - row * 80f), new Vector2(540f, 64f), out catchLabels[row]);
            var slotButtons = new Button[FishTeamSelection.MaximumMembers];
            var slotLabels = new Text[slotButtons.Length];
            for (int slot = 0; slot < slotButtons.Length; slot++)
                slotButtons[slot] = CreateButton("Team Slot " + (slot + 1), panel.transform, (slot + 1) + ". Empty", font,
                    new Vector2(310f, 120f - slot * 100f), new Vector2(540f, 84f), out slotLabels[slot]);

            Button previous = CreateButton("Previous Page", panel.transform, "Previous", font, new Vector2(-440f, -190f), new Vector2(220f, 50f), out _);
            Button next = CreateButton("Next Page", panel.transform, "Next", font, new Vector2(-180f, -190f), new Vector2(220f, 50f), out _);
            Button remove = CreateButton("Remove", panel.transform, "Remove", font, new Vector2(140f, -155f), new Vector2(150f, 50f), out _);
            Button up = CreateButton("Move Up", panel.transform, "Up", font, new Vector2(310f, -155f), new Vector2(150f, 50f), out _);
            Button down = CreateButton("Move Down", panel.transform, "Down", font, new Vector2(480f, -155f), new Vector2(150f, 50f), out _);
            Text status = FishTimingSetup.CreateText("Status", panel.transform, "Select 1-3 owned fish.", font, 21, new Vector2(0f, -235f), new Vector2(1170f, 45f));
            Button confirm = CreateButton("Confirm Team", panel.transform, "Confirm team", font, new Vector2(190f, -295f), new Vector2(240f, 55f), out _);
            Button close = CreateButton("Close Team", panel.transform, "Return to beach", font, new Vector2(460f, -295f), new Vector2(240f, 55f), out _);
            panel.SetActive(false);

            FishTeamPanel controller = Undo.AddComponent<FishTeamPanel>(root);
            var serialized = new SerializedObject(controller);
            SetReference(serialized, "team", team);
            SetReference(serialized, "collection", collections[0]);
            SetReference(serialized, "player", players[0]);
            SetReference(serialized, "orbitCamera", cameras[0]);
            SetReference(serialized, "fishingSpot", spots[0]);
            SetReference(serialized, "collectionPanel", collectionPanels[0]);
            SetReference(serialized, "panel", panel);
            SetReference(serialized, "explorationHint", hint);
            SetReference(serialized, "pageLabel", pageLabel);
            SetReference(serialized, "status", status);
            SetArray(serialized, "catchButtons", catchButtons);
            SetArray(serialized, "catchLabels", catchLabels);
            SetArray(serialized, "slotButtons", slotButtons);
            SetArray(serialized, "slotLabels", slotLabels);
            SetReference(serialized, "previousPage", previous);
            SetReference(serialized, "nextPage", next);
            SetReference(serialized, "remove", remove);
            SetReference(serialized, "moveUp", up);
            SetReference(serialized, "moveDown", down);
            SetReference(serialized, "confirm", confirm);
            SetReference(serialized, "close", close);
            serialized.ApplyModifiedProperties();
            Undo.RecordObject(spots[0], "Connect team panel exclusion");
            spots[0].ConfigureTeamPanel(controller);
            Undo.RecordObject(collectionPanels[0], "Connect team panel exclusion");
            collectionPanels[0].ConfigureTeamPanel(controller);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(spots[0]);
            EditorUtility.SetDirty(collectionPanels[0]);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("Milestone 2B added. Review and save. During Play, press T to select and order 1-3 owned catches; confirmation does not start a battle.");
        }

        private static T[] Find<T>(Transform[] objects) where T : Component =>
            objects.Select(item => item.GetComponent<T>()).Where(item => item != null).ToArray();

        private static void SetReference(SerializedObject serialized, string field, Object value) =>
            serialized.FindProperty(field).objectReferenceValue = value;

        private static void SetArray(SerializedObject serialized, string field, Object[] values)
        {
            SerializedProperty property = serialized.FindProperty(field);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static Button CreateButton(string name, Transform parent, string label, Font font,
            Vector2 position, Vector2 dimensions, out Text text)
        {
            GameObject item = FishTimingSetup.CreateUI(name, parent);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            Image image = FishTimingSetup.AddImage(item, new Color32(71, 123, 86, 255));
            image.raycastTarget = true;
            Button button = Undo.AddComponent<Button>(item);
            button.targetGraphic = image;
            text = FishTimingSetup.CreateText("Label", item.transform, label, font, 20, Vector2.zero, dimensions - new Vector2(16f, 4f));
            return button;
        }
    }
}

