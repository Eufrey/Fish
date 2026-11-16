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
    public static class FishBattleSetup
    {
        [MenuItem("FISH!/Setup Milestone 3A")]
        public static void Setup()
        {
            Scene overworld = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.sceneCount != 1 ||
                overworld.path != "Assets/Scenes/CoastalPrototype.unity")
            {
                Debug.LogWarning("Outside Play Mode, open only CoastalPrototype.unity, then run setup.");
                return;
            }
            Transform[] objects = overworld.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            if (Find<FishBattleEncounter>(objects).Length != 0)
            {
                Debug.Log("Milestone 3A encounter already exists. Scene/data settings were preserved.");
                return;
            }
            var teams = Find<FishBattleTeam>(objects);
            var players = Find<FishExplorationPlayer>(objects);
            var cameras = Find<FishOrbitCamera>(objects);
            var spots = Find<FishFishingSpot>(objects);
            var collections = Find<FishCollectionPanel>(objects);
            var teamPanels = Find<FishTeamPanel>(objects);
            var systems = Find<EventSystem>(objects);
            var modules = objects.SelectMany(item => item.GetComponents<BaseInputModule>()).ToArray();
            if (teams.Length != 1 || players.Length != 1 || cameras.Length != 1 || spots.Length != 1 || collections.Length != 1 ||
                teamPanels.Length != 1 || systems.Length != 1 || modules.Length != 1 ||
                !(modules[0] is InputSystemUIInputModule) || !modules[0].isActiveAndEnabled || !systems[0].isActiveAndEnabled ||
                modules[0].gameObject != systems[0].gameObject || spots[0].FishingPanel == null || teams[0].Collection == null ||
                !players[0].isActiveAndEnabled || !cameras[0].isActiveAndEnabled || !spots[0].isActiveAndEnabled ||
                !collections[0].isActiveAndEnabled || !teamPanels[0].isActiveAndEnabled)
            {
                Debug.LogWarning("Complete the 1A-2B setup first with one player/team, camera, fishing/collection/team UI and Input System EventSystem. Nothing was changed.");
                return;
            }
            Canvas canvas = spots[0].FishingPanel.GetComponentInParent<Canvas>(true);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            const string inputPath = "Assets/InputSystem_Actions.inputactions";
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath);
            InputActionReference[] references = AssetDatabase.LoadAllAssetsAtPath(inputPath).OfType<InputActionReference>().ToArray();
            string[] needed = { "Point", "Navigate", "Click", "RightClick", "MiddleClick", "ScrollWheel", "Submit", "Cancel", "TrackedDevicePosition", "TrackedDeviceOrientation" };
            if (canvas == null || font == null || shader == null || actions == null ||
                needed.Any(name => !references.Any(reference => reference.action != null && reference.action.actionMap.name == "UI" && reference.action.name == name)))
            {
                Debug.LogWarning("Existing canvas, font, URP shader and UI actions are required. Nothing was changed.");
                return;
            }
            const string enemyPath = "Assets/Data/Fish/PlaceholderEnemy.asset";
            const string teamPath = "Assets/Data/Fish/TestEnemyTeam.asset";
            Object enemyAsset = AssetDatabase.LoadMainAssetAtPath(enemyPath);
            Object teamAsset = AssetDatabase.LoadMainAssetAtPath(teamPath);
            Object sceneAsset = AssetDatabase.LoadMainAssetAtPath(FishBattleEncounter.BattleScenePath);
            if ((enemyAsset != null && !(enemyAsset is FishSpecies)) || (teamAsset != null && !(teamAsset is FishEnemyTeam)) ||
                (sceneAsset != null && !(sceneAsset is SceneAsset)))
            {
                Debug.LogWarning("A battle asset path is occupied by other data. Nothing was changed.");
                return;
            }
            if (teamAsset is FishEnemyTeam existingTeam && !existingTeam.TryCreateFighters(out _))
            {
                Debug.LogWarning("Existing enemy team has missing species. Repair its references before setup. Nothing was changed.");
                return;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Data/Fish"))
            {
                Debug.LogWarning("The 2A data folder is missing. Run its setup first.");
                return;
            }
            FishSpecies enemy = enemyAsset as FishSpecies;
            if (enemy == null)
            {
                enemy = ScriptableObject.CreateInstance<FishSpecies>();
                enemy.speciesId = "placeholder-enemy";
                enemy.displayName = "Placeholder Enemy (fictional)";
                enemy.isPlaceholder = true;
                enemy.health = 16;
                enemy.attack = 6;
                enemy.defense = 1;
                AssetDatabase.CreateAsset(enemy, enemyPath);
            }
            FishEnemyTeam enemyTeam = teamAsset as FishEnemyTeam;
            if (enemyTeam == null)
            {
                enemyTeam = ScriptableObject.CreateInstance<FishEnemyTeam>();
                enemyTeam.members = new[] { enemy, enemy };
                AssetDatabase.CreateAsset(enemyTeam, teamPath);
            }
            FishSpecies placeholder = AssetDatabase.LoadAssetAtPath<FishSpecies>("Assets/Data/Fish/PlaceholderFish.asset");
            if (placeholder != null && placeholder.isPlaceholder && placeholder.health == 0 && placeholder.attack == 0 && placeholder.defense == 0)
            {
                Undo.RecordObject(placeholder, "Author placeholder combat stats");
                placeholder.health = 24;
                placeholder.attack = 8;
                placeholder.defense = 2;
                EditorUtility.SetDirty(placeholder);
                AssetDatabase.SaveAssetIfDirty(placeholder);
            }
            FishBattleSpecialSetup.ConfigurePlaceholderAbilities();
            // A new battle scene is saved separately; existing scenes are never overwritten.
            if (sceneAsset == null) CreateBattleScene(overworld, font, actions, references);
            var buildScenes = EditorBuildSettings.scenes.ToList();
            int battleIndex = buildScenes.FindIndex(item => item.path == FishBattleEncounter.BattleScenePath);
            if (battleIndex < 0) buildScenes.Add(new EditorBuildSettingsScene(FishBattleEncounter.BattleScenePath, true));
            else if (!buildScenes[battleIndex].enabled)
                buildScenes[battleIndex] = new EditorBuildSettingsScene(FishBattleEncounter.BattleScenePath, true);
            EditorBuildSettings.scenes = buildScenes.ToArray();

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup FISH! Milestone 3A encounter");
            GameObject root = new GameObject("FISH Test Battle Encounter");
            Undo.RegisterCreatedObjectUndo(root, "Create test battle encounter");
            root.transform.position = new Vector3(-7f, 1f, 4f);
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Undo.RegisterCreatedObjectUndo(visual, "Create encounter marker visual");
            visual.name = "Encounter Marker";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, -0.4f, 0f);
            visual.transform.localScale = new Vector3(1.2f, 0.6f, 1.2f);
            const string materialPath = "Assets/FishPrototypeMaterials/EncounterMarker.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "EncounterMarker" };
                material.SetColor("_BaseColor", new Color32(233, 154, 121, 255));
                AssetDatabase.CreateAsset(material, AssetDatabase.GenerateUniqueAssetPath(materialPath));
            }
            visual.GetComponent<Renderer>().sharedMaterial = material;
            Text prompt = FishTimingSetup.CreateText("Battle Encounter Prompt", canvas.transform, "Confirm a team with T before battle", font,
                24, Vector2.zero, new Vector2(900f, 50f));
            prompt.rectTransform.anchorMin = prompt.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            prompt.rectTransform.anchoredPosition = new Vector2(0f, 140f);
            prompt.gameObject.SetActive(false);
            FishBattleEncounter encounter = Undo.AddComponent<FishBattleEncounter>(root);
            var serialized = new SerializedObject(encounter);
            Set(serialized, "team", teams[0]);
            Set(serialized, "enemyTeam", enemyTeam);
            Set(serialized, "player", players[0]);
            Set(serialized, "orbitCamera", cameras[0]);
            Set(serialized, "fishingSpot", spots[0]);
            Set(serialized, "collectionPanel", collections[0]);
            Set(serialized, "teamPanel", teamPanels[0]);
            Set(serialized, "prompt", prompt);
            Set(serialized, "markerVisual", visual);
            serialized.ApplyModifiedProperties();
            FishBattleReturnSetup.ConfigureReturnPoint(encounter, objects);
            EditorUtility.SetDirty(encounter);
            EditorSceneManager.MarkSceneDirty(overworld);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
            Debug.Log("Milestone 3A created. Review/save the overworld. Confirm a team with T, approach the coral cylinder, and press F. Battle scene and build entry were created separately.");
        }

        private static void CreateBattleScene(Scene overworld, Font font, InputActionAsset actions, InputActionReference[] references)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                new GameObject("Battle Camera", typeof(Camera), typeof(AudioListener)).transform.position = new Vector3(0f, 0f, -10f);
                GameObject events = new GameObject("Battle EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                InputSystemUIInputModule module = events.GetComponent<InputSystemUIInputModule>();
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
                GameObject canvasObject = new GameObject("Battle Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280f, 720f);
                scaler.matchWidthOrHeight = 0.5f;
                GameObject root = FishTimingSetup.CreateUI("Battle UI", canvasObject.transform);
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                FishTimingSetup.AddImage(root, new Color(0.03f, 0.08f, 0.1f, 1f)).raycastTarget = true;
                FishTimingSetup.CreateText("Title", root.transform, "FISH! - Test Battle", font, 34, new Vector2(0f, 285f), new Vector2(1100f, 50f));
                Text result = FishTimingSetup.CreateText("Turn and Result", root.transform, "", font, 22, new Vector2(0f, 215f), new Vector2(1150f, 64f));
                Text playerInfo = CreateCard(root.transform, "Player Fish", font, -300f, new Color32(71, 123, 86, 255));
                Text enemyInfo = CreateCard(root.transform, "Enemy Fish", font, 300f, new Color32(155, 91, 71, 255));
                Text feedback = FishTimingSetup.CreateText("Action Feedback", root.transform, "", font, 23, new Vector2(0f, -135f), new Vector2(1150f, 100f));
                Button attack = Button(root.transform, "Attack", font, new Vector2(-150f, -235f));
                Button defend = Button(root.transform, "Defend", font, new Vector2(150f, -235f));
                Button retry = Button(root.transform, "Retry", font, new Vector2(-150f, -315f));
                Button returnButton = Button(root.transform, "Return to Shore", font, new Vector2(150f, -315f));
                FishBattleScene controller = Undo.AddComponent<FishBattleScene>(root);
                var serialized = new SerializedObject(controller);
                Set(serialized, "playerInfo", playerInfo);
                Set(serialized, "enemyInfo", enemyInfo);
                Set(serialized, "feedback", feedback);
                Set(serialized, "result", result);
                Set(serialized, "attack", attack);
                Set(serialized, "defend", defend);
                Set(serialized, "retry", retry);
                Set(serialized, "returnToBeach", returnButton);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                FishBattleSpecialSetup.ConfigureSpecialUI(controller, font);
                if (!EditorSceneManager.SaveScene(scene, FishBattleEncounter.BattleScenePath))
                    throw new System.InvalidOperationException("Could not save the new battle scene.");
            }
            finally
            {
                SceneManager.SetActiveScene(overworld);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Text CreateCard(Transform parent, string name, Font font, float x, Color color)
        {
            GameObject card = FishTimingSetup.CreateUI(name, parent);
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(x, 45f);
            rect.sizeDelta = new Vector2(470f, 220f);
            FishTimingSetup.AddImage(card, color);
            return FishTimingSetup.CreateText("Fish and Health", card.transform, name, font, 26, Vector2.zero, new Vector2(440f, 200f));
        }

        private static Button Button(Transform parent, string label, Font font, Vector2 position)
        {
            GameObject item = FishTimingSetup.CreateUI(label, parent);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(260f, 60f);
            Image image = FishTimingSetup.AddImage(item, new Color32(71, 123, 86, 255));
            image.raycastTarget = true;
            Button button = Undo.AddComponent<Button>(item);
            button.targetGraphic = image;
            FishTimingSetup.CreateText("Label", item.transform, label, font, 24, Vector2.zero, new Vector2(250f, 55f));
            return button;
        }

        private static T[] Find<T>(Transform[] objects) where T : Component =>
            objects.Select(item => item.GetComponent<T>()).Where(item => item != null).ToArray();
        private static InputActionReference Reference(InputActionReference[] references, string name) =>
            references.First(reference => reference.action != null && reference.action.actionMap.name == "UI" && reference.action.name == name);
        private static void Set(SerializedObject serialized, string field, Object value) => serialized.FindProperty(field).objectReferenceValue = value;
    }
}
