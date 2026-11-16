using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Fish.Editor
{
    public static class FishVarietySetup
    {
        [MenuItem("FISH!/Setup Milestone 5A")]
        public static void Setup()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.sceneCount != 1 || scene.path != "Assets/Scenes/CoastalPrototype.unity")
            { Debug.LogWarning("Outside Play Mode, open only CoastalPrototype.unity."); return; }
            var roots = scene.GetRootGameObjects();
            var games = roots.SelectMany(r => r.GetComponentsInChildren<FishTimingMinigame>(true)).ToArray();
            var panels = roots.SelectMany(r => r.GetComponentsInChildren<FishCollectionPanel>(true)).ToArray();
            var original = AssetDatabase.LoadAssetAtPath<FishSpecies>("Assets/Data/Fish/PlaceholderFish.asset");
            if (games.Length != 1 || panels.Length != 1 || original == null)
            { Debug.LogWarning("Complete the collection/fishing setup first. Need one minigame and collection panel."); return; }
            const string path = "Assets/Data/Fish/CatchCatalog.asset";
            Object existing = AssetDatabase.LoadMainAssetAtPath(path);
            if (existing != null && !(existing is FishCatchCatalog)) { Debug.LogError("CatchCatalog path is occupied."); return; }
            var catalog = existing as FishCatchCatalog;
            if (catalog == null)
            {
                for (int i = 2; i <= 5; i++)
                {
                    Object asset = AssetDatabase.LoadMainAssetAtPath("Assets/Data/Fish/CatchPlaceholder" + i + ".asset");
                    if (asset != null && !(asset is FishSpecies)) { Debug.LogError("Species path occupied; nothing changed."); return; }
                }
                catalog = ScriptableObject.CreateInstance<FishCatchCatalog>();
                catalog.entries = new FishCatchCatalog.Entry[5];
                int[] weights = { 40, 30, 15, 10, 5 };
                for (int i = 0; i < 5; i++)
                {
                    FishSpecies species = original;
                    if (i > 0)
                    {
                        string speciesPath = "Assets/Data/Fish/CatchPlaceholder" + (i+1) + ".asset";
                        species = AssetDatabase.LoadAssetAtPath<FishSpecies>(speciesPath);
                        if (species == null)
                        {
                            species = ScriptableObject.CreateInstance<FishSpecies>();
                            species.speciesId = "catch-placeholder-" + (i+1);
                            species.displayName = "Placeholder Fish " + (i+1) + " (unresearched)";
                            species.isPlaceholder = true;
                            species.rarity = i == 1 ? FishSpecies.Rarity.Common : i == 4 ? FishSpecies.Rarity.Rare : FishSpecies.Rarity.Uncommon;
                            species.health = 24; species.attack = 8; species.defense = 2;
                            species.specialAbilityName = "Harbor Burst";
                            species.specialAbilityDescription = "A fictional strike with 6 bonus Attack before defense.";
                            species.specialAbilityType = FishSpecialEffect.StrongerAttack; species.specialAbilityPower = 6;
                            AssetDatabase.CreateAsset(species,speciesPath);
                        }
                    }
                    catalog.entries[i] = new FishCatchCatalog.Entry { species = species, weight = weights[i] };
                }
                AssetDatabase.CreateAsset(catalog,path);
            }
            if (!catalog.IsValid()) { Debug.LogError("Repair duplicate/missing species IDs or invalid positive weights in CatchCatalog before setup."); return; }
            Undo.RecordObject(games[0],"Configure catch catalog"); Undo.RecordObject(panels[0],"Configure encyclopedia");
            games[0].ConfigureCatalog(catalog); panels[0].ConfigureCatalog(catalog);
            EditorUtility.SetDirty(games[0]); EditorUtility.SetDirty(panels[0]);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("5A configured. Save shore. N opens encyclopedia, C opens collection. Existing assets/tuning are preserved on repeat setup.");
        }
    }
}
