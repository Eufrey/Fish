using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Fish.Editor
{
    public static class FishWaterBattleSetup
    {
        [MenuItem("FISH!/Upgrade Water Battle")]
        public static void Upgrade()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != FishBattleEncounter.BattleScenePath)
            {
                Debug.LogWarning("Open Assets/Scenes/FishBattle.unity outside Play Mode first.");
                return;
            }
            int controllers = 0, cameras = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                controllers += root.GetComponentsInChildren<FishBattleScene>(true).Length;
                cameras += root.GetComponentsInChildren<Camera>(true).Length;
            }
            if (controllers != 1 || cameras != 1)
            {
                Debug.LogError("Water battle requires one existing battle controller and one camera. Resolve duplicate or missing objects before Play.");
                return;
            }
            Debug.Log("Water battle is ready for runtime adaptation. Start from a confirmed shore encounter. Existing scene edits and references are preserved.");
        }
    }
}
