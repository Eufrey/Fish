using UnityEditor;
using UnityEngine;
namespace Fish.Editor
{
    public static class FishVisualSetup
    {
        [MenuItem("FISH!/Setup Milestone 5C Visuals")]
        public static void Setup()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) { Debug.LogWarning("Exit Play Mode before setting up visuals."); return; }
            Shader shader=Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null) { Debug.LogError("URP Unlit is required; render pipeline was not changed."); return; }
            const string folder="Assets/FishPlaceholderVisuals";
            if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets","FishPlaceholderVisuals");
            foreach(string guid in AssetDatabase.FindAssets("t:FishSpecies",new[]{"Assets/Data/Fish"}))
            {
                string assetPath=AssetDatabase.GUIDToAssetPath(guid);
                var species=AssetDatabase.LoadAssetAtPath<FishSpecies>(assetPath);
                if(species.model!=null) continue;
                // GUID-based filenames remain stable when display names change.
                string prefabPath=folder+"/"+guid+".prefab";
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if(prefab==null)
                {
                    if(AssetDatabase.LoadMainAssetAtPath(prefabPath)!=null) { Debug.LogWarning("Occupied visual path: "+prefabPath); continue; }
                    Material body=MaterialAt(folder+"/"+guid+"Body.mat",shader,FishPrimitiveVisual.ColorFor(species));
                    Material accent=MaterialAt(folder+"/"+guid+"Accent.mat",shader,FishPrimitiveVisual.IsBoss(species)?Color.cyan:new Color(.08f,.16f,.22f));
                    if(body==null || accent==null) continue;
                    GameObject temporary=FishPrimitiveVisual.Create(body,accent,FishPrimitiveVisual.IsBoss(species));
                    try { prefab=PrefabUtility.SaveAsPrefabAsset(temporary,prefabPath); }
                    finally { Object.DestroyImmediate(temporary); }
                }
                if(prefab==null) continue;
                Undo.RecordObject(species,"Assign placeholder visual"); species.model=prefab;
                EditorUtility.SetDirty(species); AssetDatabase.SaveAssetIfDirty(species);
            }
            Debug.Log("5C visuals assigned to empty species model fields. Existing imported models preserved. Battle preview is installed at runtime; no scene replacement needed.");
        }
        private static Material MaterialAt(string path,Shader shader,Color color)
        {
            Object existing=AssetDatabase.LoadMainAssetAtPath(path);
            if(existing!=null) return existing as Material;
            var material=new Material(shader); material.SetColor("_BaseColor",color);
            AssetDatabase.CreateAsset(material,path); return material;
        }
    }
}
