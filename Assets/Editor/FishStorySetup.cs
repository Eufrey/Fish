using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fish.Editor
{
    public static class FishStorySetup
    {
        [MenuItem("FISH!/Setup Milestone 4")]
        public static void Setup()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.sceneCount != 1 ||
                scene.path != "Assets/Scenes/CoastalPrototype.unity")
            { Debug.LogWarning("Outside Play Mode, open only CoastalPrototype.unity."); return; }
            var encounters = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<FishBattleEncounter>(true)).ToArray();
            var stores = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<FishCollection>(true)).ToArray();
            if (stores.Length != 1 || encounters.Length < 1 || encounters.Length > 3 ||
                encounters.GroupBy(e => e.StoryEncounter).Any(g => g.Count() != 1) ||
                !encounters.Any(e => e.StoryEncounter == 1) || encounters.Any(e => e.ShoreReturnPoint == null || e.ShoreGround == null))
            { Debug.LogWarning("Complete 2B, 3A and 3B first. Need one collection and unique encounter IDs 1-3 with shore references."); return; }
            FishBattleEncounter first = encounters.First(e => e.StoryEncounter == 1);
            var firstData = new SerializedObject(first);
            Text firstPrompt = firstData.FindProperty("prompt").objectReferenceValue as Text;
            if (firstPrompt == null || first.transform.parent != null)
            { Debug.LogWarning("The existing encounter must be a scene root with its prompt assigned."); return; }
            for (int id = 1; id <= 3; id++)
            {
                Object speciesAsset = AssetDatabase.LoadMainAssetAtPath("Assets/Data/Fish/Story" + id + "Species.asset");
                Object teamAsset = AssetDatabase.LoadMainAssetAtPath("Assets/Data/Fish/Story" + id + "Team.asset");
                if ((speciesAsset != null && !(speciesAsset is FishSpecies)) ||
                    (teamAsset != null && !(teamAsset is FishEnemyTeam)))
                { Debug.LogWarning("A story asset path contains unrelated data. Nothing was changed."); return; }
            }
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Setup FISH! story");
            string[] names = { "First Disturbance", "Growing Threat", "Tangle of Tides" };
            int[] health = { 14, 30, 64 };
            int[] attack = { 4, 7, 8 };
            for (int id = 1; id <= 3; id++)
            {
                FishEnemyTeam enemy = EnemyData(id, names[id - 1], health[id - 1], attack[id - 1]);
                FishBattleEncounter encounter = encounters.FirstOrDefault(e => e.StoryEncounter == id);
                bool created = encounter == null;
                if (created)
                {
                    GameObject clone = Object.Instantiate(first.gameObject);
                    Undo.RegisterCreatedObjectUndo(clone, "Create story marker");
                    clone.name = "FISH " + names[id - 1];
                    clone.transform.position = first.transform.position + Vector3.right * ((id - 1) * 7f);
                    encounter = clone.GetComponent<FishBattleEncounter>();
                }
                var data = new SerializedObject(encounter);
                // Preserve tuned teams on repeat runs; only replace the old test fixture.
                var oldTeam = data.FindProperty("enemyTeam").objectReferenceValue;
                if (created || oldTeam == null || AssetDatabase.GetAssetPath(oldTeam) == "Assets/Data/Fish/TestEnemyTeam.asset")
                    data.FindProperty("enemyTeam").objectReferenceValue = enemy;
                data.FindProperty("storyEncounter").intValue = id;
                if (created)
                {
                    Text prompt = FishTimingSetup.CreateText(names[id - 1] + " Prompt", firstPrompt.transform.parent,
                        "", font, 24, Vector2.zero, new Vector2(900f, 50f));
                    prompt.rectTransform.anchorMin = prompt.rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    prompt.rectTransform.anchoredPosition = new Vector2(0f, 140f);
                    prompt.gameObject.SetActive(false);
                    data.FindProperty("prompt").objectReferenceValue = prompt;
                }
                data.ApplyModifiedProperties();
            }
            var huds = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<FishStoryHUD>(true)).ToArray();
            if (huds.Length == 0)
            {
                GameObject root = FishTimingSetup.CreateUI("Story HUD", firstPrompt.transform.parent);
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -55f);
                Text objective = FishTimingSetup.CreateText("Objective", root.transform, "Catch three fish.", font, 24,
                    Vector2.zero, new Vector2(1100f, 45f));
                Text notice = FishTimingSetup.CreateText("Story Notice", root.transform, "", font, 20,
                    new Vector2(0f, -70f), new Vector2(1100f, 100f));
                FishHUDLayout.Story(objective, notice);
                Undo.AddComponent<FishStoryHUD>(root).Configure(stores[0], objective, notice);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(group);
            Debug.Log("Story setup ready. Review three marker/return positions and HUD, then save the shore. Existing battle scene is reused.");
        }

        private static FishEnemyTeam EnemyData(int id, string name, int health, int attack)
        {
            string prefix = "Assets/Data/Fish/Story" + id;
            FishSpecies species = AssetDatabase.LoadAssetAtPath<FishSpecies>(prefix + "Species.asset");
            if (species == null)
            {
                species = ScriptableObject.CreateInstance<FishSpecies>();
                species.speciesId = "story-placeholder-" + id;
                species.displayName = name + " (fictional placeholder)";
                species.isPlaceholder = true;
                species.health = health; species.attack = attack; species.defense = id == 1 ? 0 : 2;
                if (id == 3)
                {
                    species.specialAbilityName = "Debris Patch";
                    species.specialAbilityDescription = "Restore up to 10 HP, once per battle at half HP or lower.";
                    species.specialAbilityType = FishSpecialEffect.SelfHeal;
                    species.specialAbilityPower = 10;
                }
                AssetDatabase.CreateAsset(species, prefix + "Species.asset");
            }
            FishEnemyTeam team = AssetDatabase.LoadAssetAtPath<FishEnemyTeam>(prefix + "Team.asset");
            if (team == null)
            {
                team = ScriptableObject.CreateInstance<FishEnemyTeam>();
                team.members = new[] { species };
                team.useSpecialAtHalfHealth = id == 3;
                AssetDatabase.CreateAsset(team, prefix + "Team.asset");
            }
            return team;
        }

        // Editor-only fixture goes through the same success/claim path as fishing.
        [MenuItem("FISH!/Development/Add Three Test Catches (Play Mode)")]
        private static void AddTestCatches()
        {
            if (!EditorApplication.isPlaying || SceneManager.GetSceneByPath(FishBattleEncounter.BattleScenePath).isLoaded) return;
            var stores = Object.FindObjectsByType<FishCollection>(FindObjectsSortMode.None);
            FishSpecies species = AssetDatabase.LoadAssetAtPath<FishSpecies>("Assets/Data/Fish/PlaceholderFish.asset");
            if (stores.Length != 1 || species == null) return;
            for (int i = 0; i < 3; i++)
            {
                var attempt = new FishTimingAttempt();
                attempt.Begin(0);
                attempt.Tick(0.7f / FishTimingAttempt.IndicatorSpeed, 1, false, false);
                attempt.Tick(0f, 2, true, true);
                stores[0].TryAddCatch(attempt, species);
            }
            Debug.Log("Added three session-only test catches. Confirm a team normally; stop Play to reset.");
        }
    }
}

