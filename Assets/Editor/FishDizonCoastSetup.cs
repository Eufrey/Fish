using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fish.Editor
{
    public static class FishDizonCoastSetup
    {
        private const string RootName = "FISH Dizon Coast";
        private const string Output = "Assets/Art/DizonCoast";

        [MenuItem("FISH!/Integrate Dizon Coast")]
        public static void Setup()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/CoastalPrototype.unity")
                throw new InvalidOperationException("Open CoastalPrototype outside Play Mode first.");
            var objects = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            if (objects.Any(t => t.name == RootName)) { Polish(objects); Validate(); return; }
            var ground = objects.Single(t => t.name == "Beach Ground");
            if (Vector3.Distance(ground.position, new Vector3(0,-.5f,0)) > .1f || Vector3.Distance(ground.lossyScale,new Vector3(24,1,20)) > .1f)
                throw new InvalidOperationException("Beach dimensions have been edited. Review placement before integrating this compact layout.");
            var source = AssetDatabase.LoadAssetAtPath<TerrainData>("Assets/New Terrain.asset");
            if (source == null) throw new InvalidOperationException("Import DizonTerrain before running setup.");
            System.IO.Directory.CreateDirectory(Output);
            AssetDatabase.Refresh();
            Undo.IncrementCurrentGroup();
            int undo = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Integrate Dizon coastal environment");
            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create coastal environment");
            var sand = MaterialAt("Sand", new Color32(243,214,154,255));
            var wood = MaterialAt("Driftwood", new Color32(153,112,69,255));
            var rock = MaterialAt("Warm rock", new Color32(126,143,129,255));
            var data = UnityEngine.Object.Instantiate(source);
            data.name = "Dizon compact shore";
            data.size = new Vector3(48,6,40);
            data.treeInstances = new TreeInstance[0];
            data.treePrototypes = new TreePrototype[0];
            data.detailPrototypes = new DetailPrototype[0];
            int resolution = data.heightmapResolution;
            var heights = data.GetHeights(0,0,resolution,resolution);
            for (int z = 0; z < resolution; z++)
                for (int x = 0; x < resolution; x++)
                {
                    float worldX = x * 48f / (resolution-1) - 24;
                    float worldZ = z * 40f / (resolution-1) - 20;
                    float edge = Mathf.SmoothStep(0,1,Mathf.Clamp01((Mathf.Abs(worldX)-12)/10));
                    float back = Mathf.SmoothStep(0,1,Mathf.Clamp01((-worldZ-10)/8));
                    float dune = Mathf.Max(edge,back) * (1.3f + Mathf.PerlinNoise(worldX*.13f+10,worldZ*.13f+10));
                    float shore = Mathf.SmoothStep(0,1,Mathf.Clamp01((worldZ-9)/5));
                    heights[z,x] = (1.97f + dune*(1-shore) - shore*1.7f) / 6;
                }
            data.SetHeights(0,0,heights);
            string dataPath = AssetDatabase.GenerateUniqueAssetPath(Output+"/CoastalTerrain.asset");
            AssetDatabase.CreateAsset(data,dataPath);
            var terrainObject = Terrain.CreateTerrainGameObject(data);
            terrainObject.name = "Dizon sandy shore";
            terrainObject.transform.SetParent(root.transform,false);
            terrainObject.transform.position = new Vector3(-24,-2,-20);
            var terrain = terrainObject.GetComponent<Terrain>();
            var terrainMaterial = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
            AssetDatabase.CreateAsset(terrainMaterial,AssetDatabase.GenerateUniqueAssetPath(Output+"/Terrain.mat"));
            terrain.materialTemplate = terrainMaterial;
            var texture = new Texture2D(4,4);
            texture.SetPixels(Enumerable.Repeat(sand.color,16).ToArray()); texture.Apply();
            AssetDatabase.CreateAsset(texture,AssetDatabase.GenerateUniqueAssetPath(Output+"/Sand.asset"));
            var layer = new TerrainLayer { diffuseTexture=texture, tileSize=new Vector2(5,5) };
            AssetDatabase.CreateAsset(layer,AssetDatabase.GenerateUniqueAssetPath(Output+"/SandLayer.terrainlayer"));
            var grassTexture = new Texture2D(4,4);
            grassTexture.SetPixels(Enumerable.Repeat(new Color32(131,184,121,255),16).Select(c=>(Color)c).ToArray());
            grassTexture.Apply();
            AssetDatabase.CreateAsset(grassTexture,AssetDatabase.GenerateUniqueAssetPath(Output+"/Grass.asset"));
            var grassLayer = new TerrainLayer { diffuseTexture=grassTexture, tileSize=new Vector2(5,5) };
            AssetDatabase.CreateAsset(grassLayer,AssetDatabase.GenerateUniqueAssetPath(Output+"/GrassLayer.terrainlayer"));
            data.terrainLayers = new[] { layer, grassLayer };
            var alpha = new float[data.alphamapHeight,data.alphamapWidth,2];
            for(int y=0;y<data.alphamapHeight;y++) for(int x=0;x<data.alphamapWidth;x++)
            {
                float worldX=x*48f/(data.alphamapWidth-1)-24;
                float worldZ=y*40f/(data.alphamapHeight-1)-20;
                float grass=Mathf.Clamp01((Mathf.Max(Mathf.Abs(worldX)-13,-worldZ-10))/5) * (1-Mathf.Clamp01((worldZ-4)/6));
                alpha[y,x,0]=1-grass; alpha[y,x,1]=grass;
            }
            data.SetAlphamaps(0,0,alpha);
            Undo.RecordObject(ground.GetComponent<Renderer>(), "Use terrain beach surface");
            ground.GetComponent<Renderer>().enabled=false;
            AddProp(root.transform,"Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Rocks/PT_Menhir_Rock_02.prefab",new Vector3(-16,.4f,5),3.5f,rock,25);
            AddProp(root.transform,"Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Rocks/PT_Menhir_Rock_02.prefab",new Vector3(16,.4f,3),4,rock,-40);
            AddProp(root.transform,"Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Rocks/PT_Menhir_Rock_02.prefab",new Vector3(14,.3f,-12),2.7f,rock,80);
            AddProp(root.transform,"Assets/Low Poly Boats & Raft/Low Poly Row Boats/Prefabs/NormalBoat.prefab",new Vector3(-8,0,8),3.5f,wood,25);
            AddProp(root.transform,"Assets/FantasyEnvironments/Environments/Town/Prefabs/storage_barrel_fish.prefab",new Vector3(-9,0,-3),1,wood,0);
            AddProp(root.transform,"Assets/WoodlandHarbour/Prefabs/014_shore_cargo_gangway_cleated.prefab",new Vector3(-15,.2f,-4),3,wood,35);
            var leaves=MaterialAt("Palm green",new Color32(71,123,86,255));
            Palm(root.transform,new Vector3(-14,1,-8),wood,leaves);
            Palm(root.transform,new Vector3(14,1,-8),wood,leaves);
            Palm(root.transform,new Vector3(-15,1,1),wood,leaves);
            var water = objects.FirstOrDefault(t => t.name == "Water");
            if(water == null) water=objects.FirstOrDefault(t=>t.name.Contains("Water") && t.GetComponent<Renderer>()!=null);
            if(water!=null) { Undo.RecordObject(water.GetComponent<Renderer>(),"Coastal water color"); water.GetComponent<Renderer>().sharedMaterial=MaterialAt("Lagoon",new Color32(119,205,187,255)); }
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            Polish(scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray());
            Undo.CollapseUndoOperations(undo);
            Validate();
            Selection.activeGameObject=root;
            Debug.Log("Dizon coast integrated. Review the open scene and save. Repeat setup preserves authored edits.");
        }
        private static Material MaterialAt(string name, Color color)
        {
            string path=Output+"/"+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material!=null) return material;
            material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color=color; material.SetFloat("_Smoothness",.15f);
            AssetDatabase.CreateAsset(material,path); return material;
        }
        private static void Polish(Transform[] objects)
        {
            var root=objects.Single(t=>t.name==RootName);
            if(root.Find("Dock integration")!=null) return;
            foreach(var item in objects)
                if(item.name=="Beach to dock path" || item.name=="Encounter approach path" || item.name.EndsWith(" edge marker") ||
                    item.name=="Beach label" || item.name=="Shelter sign" || item.name=="Bangka sign")
                {
                    Undo.RecordObject(item.gameObject,"Remove blockout markings");
                    item.gameObject.SetActive(false);
                }
            var water=objects.First(t=>t.name=="Water Placeholder");
            Undo.RecordObject(water,"Extend coastal water");
            water.position=new Vector3(0,water.position.y,50);
            water.localScale=new Vector3(100,water.localScale.y,100);
            var dock=objects.Single(t=>t.name=="Placeholder Dock");
            var area=dock.GetComponent<Collider>().bounds;
            var visual=new GameObject("Dock integration").transform;
            visual.SetParent(root,false);
            AddProp(visual,"Assets/WoodlandHarbour/Prefabs/014_shore_cargo_gangway_cleated.prefab",area.center,
                Mathf.Max(area.size.x,area.size.z),MaterialAt("Driftwood",new Color32(153,112,69,255)),0);
            var renderers=visual.GetComponentsInChildren<Renderer>();
            var bounds=renderers[0].bounds;
            foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            visual.localScale=new Vector3(area.size.x/bounds.size.x,.15f/Mathf.Max(.01f,bounds.size.y),area.size.z/bounds.size.z);
            bounds=renderers[0].bounds;
            foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            visual.position+=new Vector3(area.center.x-bounds.center.x,area.max.y+.01f-bounds.max.y,area.center.z-bounds.center.z);
            Undo.RecordObject(dock.GetComponent<Renderer>(),"Use imported dock visual");
            dock.GetComponent<Renderer>().enabled=false;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
        private static void Palm(Transform parent, Vector3 position, Material trunk, Material leaf)
        {
            var root=new GameObject("Stylized coastal palm").transform;
            root.SetParent(parent,false); root.position=position;
            var stem=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.transform.SetParent(root,false);
            stem.transform.localPosition=new Vector3(0,2,0);
            stem.transform.localScale=new Vector3(.24f,2,.24f);
            stem.GetComponent<Renderer>().sharedMaterial=trunk;
            UnityEngine.Object.DestroyImmediate(stem.GetComponent<Collider>());
            for(int i=0;i<6;i++)
            {
                float angle=i*60*Mathf.Deg2Rad;
                var frond=GameObject.CreatePrimitive(PrimitiveType.Sphere);
                frond.transform.SetParent(root,false);
                frond.transform.localPosition=new Vector3(Mathf.Sin(angle),4,Mathf.Cos(angle));
                frond.transform.localRotation=Quaternion.Euler(15,i*60,0);
                frond.transform.localScale=new Vector3(.7f,.15f,2.8f);
                frond.GetComponent<Renderer>().sharedMaterial=leaf;
                UnityEngine.Object.DestroyImmediate(frond.GetComponent<Collider>());
            }
        }
        private static void AddProp(Transform parent,string path,Vector3 position,float length,Material material,float yaw)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab==null) throw new InvalidOperationException("Missing Dizon prop " + path);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);
            instance.transform.localRotation=Quaternion.Euler(0,yaw,0);
            var renderers=instance.GetComponentsInChildren<Renderer>();
            if(renderers.Length==0) throw new InvalidOperationException("Prop has no renderers " + path);
            var bounds=renderers[0].bounds;
            foreach(var renderer in renderers) { bounds.Encapsulate(renderer.bounds); renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>material).ToArray(); }
            float scale=length/Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
            instance.transform.localScale*=scale;
            bounds=renderers[0].bounds; foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            instance.transform.position+=position-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            foreach(var collider in instance.GetComponentsInChildren<Collider>()) collider.enabled=false;
            foreach(var script in instance.GetComponentsInChildren<MonoBehaviour>()) if(script!=null) script.enabled=false;
        }
        [MenuItem("FISH!/Validate Dizon Coast")]
        public static void Validate()
        {
            Physics.SyncTransforms();
            var scene=SceneManager.GetActiveScene();
            var objects=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            if(objects.Count(t=>t.GetComponent<Terrain>()!=null)!=1) throw new InvalidOperationException("Expected one coastal terrain.");
            var player=objects.Single(t=>t.GetComponent<FishExplorationPlayer>()!=null);
            var ground=objects.Single(t=>t.name=="Beach Ground").GetComponent<Collider>();
            if(!ground.enabled || !ground.Raycast(new Ray(player.position+Vector3.up*2,Vector3.down),out _,10))
                throw new InvalidOperationException("Player has no shore ground support.");
            var dock=objects.Single(t=>t.name=="Placeholder Dock").GetComponent<Collider>();
            if(dock==null || !dock.enabled || dock.isTrigger) throw new InvalidOperationException("Dock needs solid collision.");
            if(objects.Count(t=>t.GetComponent<Camera>()!=null)!=1) throw new InvalidOperationException("Expected one shore camera.");
            if(objects.Count(t=>t.name=="Water Placeholder")!=1) throw new InvalidOperationException("Expected one water surface.");
            for(int z=-6;z<=6;z+=2)
                if(!ground.Raycast(new Ray(new Vector3(0,5,z),Vector3.down),out _,10))
                    throw new InvalidOperationException("The central approach has a ground gap.");
            foreach(var encounter in objects.Select(t=>t.GetComponent<FishBattleEncounter>()).Where(e=>e!=null))
            {
                if(encounter.ShoreGround==null || encounter.ShoreReturnPoint==null) throw new InvalidOperationException("Missing encounter return references.");
                if(!encounter.ShoreGround.Raycast(new Ray(encounter.ShoreReturnPoint.position+Vector3.up*5,Vector3.down),out _,20))
                    throw new InvalidOperationException("Encounter return point has no shore ground support.");
            }
            Debug.Log("Dizon validation passed: one terrain/camera/water, solid dock, central approach, player and encounter return ground support.");
        }
        public static void Batch()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/CoastalPrototype.unity");
            Setup();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            var cameraObject=new GameObject("Coast verification camera",typeof(Camera));
            var camera=cameraObject.GetComponent<Camera>();
            camera.transform.position=new Vector3(25,24,-30);
            camera.transform.LookAt(new Vector3(0,0,1));
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color32(168,226,213,255);
            var target=new RenderTexture(1280,720,24);
            var previous=RenderTexture.active;
            try
            {
                camera.targetTexture=target;
                ShaderUtil.allowAsyncCompilation=false;
                camera.Render();
                camera.Render();
                RenderTexture.active=target;
                var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,1280,720),0,0); image.Apply();
                System.IO.Directory.CreateDirectory("Logs");
                System.IO.File.WriteAllBytes("Logs/DizonCoastPreview.png",image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }
            finally
            {
                camera.targetTexture=null; RenderTexture.active=previous;
                target.Release(); UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
