using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fish.Editor
{
    public static class FishBattleSpecialSetup
    {
        [MenuItem("FISH!/Setup Milestone 3C")]
        public static void Setup()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.sceneCount != 1 ||
                scene.path != FishBattleEncounter.BattleScenePath)
            {
                Debug.LogWarning("Outside Play Mode, open only Assets/Scenes/FishBattle.unity. Save existing shore work before switching scenes.");
                return;
            }
            FishBattleScene controller = null;
            int count = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (FishBattleScene candidate in root.GetComponentsInChildren<FishBattleScene>(true)) { controller = candidate; count++; }
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (count != 1 || font == null)
            {
                Debug.LogWarning("The battle scene must have exactly one battle controller and a built-in UI font. Nothing was changed.");
                return;
            }
            ConfigurePlaceholderAbilities();
            if (controller.SpecialButton != null && controller.SpecialInfo != null)
            {
                Debug.Log("Milestone 3C UI already exists. Existing layout and ability tuning were preserved.");
                return;
            }
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup FISH! Milestone 3C specials");
            if (!ConfigureSpecialUI(controller, font)) return;
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = controller.gameObject;
            Debug.Log("Milestone 3C added. Review/save the battle scene, then start Play from the overworld encounter.");
        }

        internal static bool ConfigureSpecialUI(FishBattleScene controller, Font font)
        {
            if (controller.SpecialButton != null && controller.SpecialInfo != null) return true;
            var serialized = new SerializedObject(controller);
            Button attack = serialized.FindProperty("attack").objectReferenceValue as Button;
            Button defend = serialized.FindProperty("defend").objectReferenceValue as Button;
            Text feedback = serialized.FindProperty("feedback").objectReferenceValue as Text;
            Transform existingButton = controller.transform.Find("Special Ability");
            Transform existingInfo = controller.transform.Find("Special Description");
            if (attack == null || defend == null || feedback == null ||
                (existingButton != null && existingButton.GetComponent<Button>() == null) ||
                (existingInfo != null && existingInfo.GetComponent<Text>() == null))
            {
                Debug.LogWarning("Resolve missing battle references or conflicting special UI objects first. Nothing was changed.");
                return false;
            }

            Button special = controller.SpecialButton;
            if (special == null && existingButton != null) special = existingButton.GetComponent<Button>();
            if (special == null)
            {
                GameObject item = FishTimingSetup.CreateUI("Special Ability", controller.transform);
                RectTransform rect = item.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(300f, -250f);
                rect.sizeDelta = new Vector2(260f, 60f);
                Image image = FishTimingSetup.AddImage(item, new Color32(71, 123, 86, 255));
                image.raycastTarget = true;
                special = Undo.AddComponent<Button>(item);
                special.targetGraphic = image;
                FishTimingSetup.CreateText("Label", item.transform, "Special Ability", font, 24, Vector2.zero, new Vector2(250f, 55f));
            }
            Text info = controller.SpecialInfo;
            if (info == null && existingInfo != null) info = existingInfo.GetComponent<Text>();
            if (info == null)
                info = FishTimingSetup.CreateText("Special Description", controller.transform, "Once per fish per battle", font,
                    18, new Vector2(0f, -185f), new Vector2(1150f, 45f));
            RectTransform attackRect = attack.GetComponent<RectTransform>();
            RectTransform defendRect = defend.GetComponent<RectTransform>();
            Undo.RecordObject(attackRect, "Position Attack beside special");
            Undo.RecordObject(defendRect, "Position Defend beside special");
            Undo.RecordObject(feedback.rectTransform, "Make space for special description");
            attackRect.anchoredPosition = new Vector2(-300f, -250f);
            defendRect.anchoredPosition = new Vector2(0f, -250f);
            feedback.rectTransform.anchoredPosition = new Vector2(0f, -120f);
            feedback.rectTransform.sizeDelta = new Vector2(1150f, 80f);
            Undo.RecordObject(controller, "Connect special UI");
            controller.ConfigureSpecialUI(special, info);
            EditorUtility.SetDirty(controller);
            return true;
        }

        internal static void ConfigurePlaceholderAbilities()
        {
            ConfigureDefault("Assets/Data/Fish/PlaceholderFish.asset", FishSpecialEffect.StrongerAttack, "Harbor Burst",
                "A fictional strike with 6 bonus Attack before defense.", 6);
            ConfigureDefault("Assets/Data/Fish/PlaceholderEnemy.asset", FishSpecialEffect.SelfHeal, "Patch-Up Pulse",
                "A fictional pulse that restores up to 8 of this fish's HP.", 8);
        }

        private static void ConfigureDefault(string path, FishSpecialEffect effect, string name, string description, int power)
        {
            FishSpecies species = AssetDatabase.LoadAssetAtPath<FishSpecies>(path);
            if (species == null || !species.isPlaceholder || species.specialAbilityType != FishSpecialEffect.None ||
                !string.IsNullOrEmpty(species.specialAbilityName) || !string.IsNullOrEmpty(species.specialAbilityDescription)) return;
            Undo.RecordObject(species, "Author fictional placeholder special");
            species.specialAbilityName = name;
            species.specialAbilityDescription = description;
            species.specialAbilityType = effect;
            species.specialAbilityPower = power;
            EditorUtility.SetDirty(species);
            AssetDatabase.SaveAssetIfDirty(species);
        }
    }
}
