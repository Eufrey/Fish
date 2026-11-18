using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace Fish.Editor
{
    public static class FishImportedCoastSetup
    {
        [MenuItem("FISH!/Inspect Imported Coast %&i")]
        public static void Inspect()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Terrain.prefab");
            var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var children = root.transform.Cast<Transform>().ToArray();
                foreach (var child in children) child.SetParent(null, true);
                root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                root.transform.localScale = Vector3.one;
                foreach (var child in children) child.SetParent(root.transform, true);
                var report = new StringBuilder();
                var terrain = root.GetComponent<Terrain>();
                report.AppendLine("Terrain size " + terrain.terrainData.size);
                foreach (Transform child in root.transform)
                {
                    report.AppendLine(child.name + " position " + child.position + " rotation " + child.eulerAngles);
                    foreach (var renderer in child.GetComponentsInChildren<Renderer>())
                        report.AppendLine("  " + renderer.name + " " + renderer.bounds);
                }
                for (int z = 160; z <= 230; z += 5)
                {
                    report.Append(z + " | ");
                    for (int x = 250; x <= 320; x += 5)
                        report.Append(terrain.terrainData.GetInterpolatedHeight(x / 500f, z / 500f).ToString("F1") + " ");
                    report.AppendLine();
                }
                Directory.CreateDirectory("Temp/ImportedCoast");
                File.WriteAllText("Temp/ImportedCoast/Inspection.txt", report.ToString());
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}

