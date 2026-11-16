using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fish.Editor
{
    public static class FishCoastSetup
    {
        private const string RootName = "FISH Coastal Placeholders 5D";
        [MenuItem("FISH!/Setup Milestone 5D Coast")]
        public static void Setup()
        {
            if (!TryScene(out Scene scene, out Transform[] objects)) return;
            if (objects.Any(t => t.name == RootName))
            { Debug.Log("5D already exists. All manual edits preserved; running validation only."); Validate(); return; }
            if (objects.Count(t => t.name == "Beach Ground") != 1 || objects.Count(t => t.name == "Placeholder Dock") != 1 ||
                objects.Count(t => t.GetComponent<FishExplorationPlayer>() != null) != 1 ||
                objects.Count(t => t.GetComponent<FishFishingSpot>() != null) != 1)
            { Debug.LogWarning("Need the existing beach, dock, player and fishing point. No replacement environment was created."); return; }
            // This small layout is tailored to the inspected blockout; don't place props over edited terrain.
            Transform ground = objects.First(t => t.name == "Beach Ground");
            Collider groundCollider = ground.GetComponent<Collider>();
            if (groundCollider == null || Vector3.Distance(ground.position,new Vector3(0,-.5f,0)) > .1f ||
                Vector3.Distance(ground.lossyScale,new Vector3(24,1,20)) > .1f || Quaternion.Angle(ground.rotation,Quaternion.identity)>1)
            { Debug.LogWarning("Beach dimensions/transform were edited. Preserve that layout: place the documented 5D props manually instead of applying fixed blockout positions."); return; }
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) { Debug.LogError("URP Lit required; pipeline unchanged."); return; }
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Setup compact coast");
            Material wood = MaterialAt("CoastWood",new Color32(133,91,57,255),shader);
            Material roof = MaterialAt("CoastRoof",new Color32(180,143,79,255),shader);
            Material paint = MaterialAt("CoastPaint",new Color32(67,155,172,255),shader);
            Material path = MaterialAt("CoastPath",new Color32(225,193,133,255),shader);
            if (wood==null || roof==null || paint==null || path==null) return;
            Transform root = Node(RootName,null,Vector3.zero);
            Transform shelter = Node("Shelter - simple placeholder",root,new Vector3(-7,0,-5));
            Box("Floor",shelter,new Vector3(0,.05f,0),new Vector3(4,.1f,3),wood,true);
            foreach(float x in new[]{-1.8f,1.8f}) foreach(float z in new[]{-1.3f,1.3f})
                Box("Post",shelter,new Vector3(x,1.35f,z),new Vector3(.18f,2.7f,.18f),wood,true);
            Box("Roof placeholder",shelter,new Vector3(0,2.8f,0),new Vector3(4.4f,.25f,3.4f),roof,true);
            Sign("Shelter sign",shelter,new Vector3(0,2.35f,-1.55f),"Coastal shelter\nPlaceholder",2.8f);
            Transform boat = Node("Decorative bangka - stylized placeholder",root,new Vector3(7,0,7));
            Box("Hull",boat,new Vector3(0,.4f,0),new Vector3(.9f,.55f,3),paint,true);
            Box("Interior",boat,new Vector3(0,.7f,0),new Vector3(.6f,.08f,2.5f),wood,false);
            foreach(float z in new[]{-.8f,.8f}) Box("Outrigger crossbeam",boat,new Vector3(0,.7f,z),new Vector3(3.3f,.1f,.12f),wood,false);
            foreach(float x in new[]{-1.5f,1.5f}) Box("Outrigger float",boat,new Vector3(x,.3f,0),new Vector3(.2f,.2f,3.2f),wood,false);
            Sign("Bangka sign",boat,new Vector3(0,1.5f,-1.8f),"Decorative bangka\nStylized placeholder - no sailing",3.5f);
            // Thin non-colliding markings keep the existing walkable surfaces intact.
            Box("Beach to dock path",root,new Vector3(0,.006f,1),new Vector3(1.7f,.01f,8),path,false);
            Box("Encounter approach path",root,new Vector3(0,.007f,1),new Vector3(16,.01f,1.5f),path,false);
            Transform dock = objects.First(t => t.name == "Placeholder Dock");
            Transform dockSign = Node("Dock wayfinding",root,new Vector3(dock.position.x-2.3f,0,dock.position.z-1));
            Box("Post",dockSign,new Vector3(0,.85f,0),new Vector3(.12f,1.7f,.12f),wood,true);
            Sign("Dock label",dockSign,new Vector3(0,2,0),"Fishing dock\nE: Fish",2.5f);
            Sign("Beach label",root,new Vector3(0,2,-7.8f),"Negros coastal barangay\nBeach - placeholder environment",5);
            foreach (var encounter in objects.Select(t=>t.GetComponent<FishBattleEncounter>()).Where(e=>e!=null))
            {
                FishBattleReturnSetup.ConfigureReturnPoint(encounter,objects);
                // Attached to the encounter root, so it follows authored marker movements.
                Sign("Encounter area label 5D",encounter.transform,new Vector3(0,1.6f,0),
                    "Encounter " + encounter.StoryEncounter + "\nF when unlocked - confirm team with T",4);
            }
            // Existing invisible boundary colliders remain authoritative; low visible rails explain their edges.
            foreach (string side in new[]{"North","South","East","West"})
            {
                Transform boundary = objects.FirstOrDefault(t=>t.name==side+" Boundary");
                if(boundary==null) continue;
                bool horizontal=side=="North" || side=="South";
                Box(side+" edge marker",root,new Vector3(boundary.position.x,.35f,boundary.position.z),
                    horizontal?new Vector3(24,.12f,.15f):new Vector3(.15f,.12f,20),wood,false);
            }
            EditorSceneManager.MarkSceneDirty(scene); Undo.CollapseUndoOperations(group);
            Selection.activeGameObject=root.gameObject;
            Validate();
            Debug.Log("5D added to existing coast. Review props/paths, run validation, then save manually. No terrain, water, cameras or lights added.");
        }
        private static bool TryScene(out Scene scene,out Transform[] objects)
        {
            scene=SceneManager.GetActiveScene(); objects=null;
            if(EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.sceneCount!=1 || scene.path!="Assets/Scenes/CoastalPrototype.unity")
            { Debug.LogWarning("Outside Play Mode, open only CoastalPrototype.unity."); return false; }
            objects=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray(); return true;
        }
        [MenuItem("FISH!/Validate Milestone 5D Coast")]
        public static void Validate()
        {
            if(!TryScene(out _,out Transform[] objects)) return;
            Physics.SyncTransforms(); int issues=0;
            foreach(string name in new[]{"Beach Ground","Placeholder Dock","North Boundary","South Boundary","East Boundary","West Boundary"})
            {
                var matches=objects.Where(t=>t.name==name).ToArray();
                if(matches.Length!=1 || matches[0].GetComponent<Collider>()==null || !matches[0].GetComponent<Collider>().enabled ||
                    matches[0].GetComponent<Collider>().isTrigger || !matches[0].gameObject.activeInHierarchy)
                { issues++; Debug.LogWarning("Check exactly one active solid collider: "+name); }
            }
            var players=objects.Select(t=>t.GetComponent<FishExplorationPlayer>()).Where(p=>p!=null).ToArray();
            var ground=objects.FirstOrDefault(t=>t.name=="Beach Ground")?.GetComponent<Collider>();
            if(players.Length==1 && ground!=null)
            {
                var body=players[0].GetComponent<CharacterController>();
                if(!SafeFootprint(body,ground,players[0].transform.position)) { issues++; Debug.LogWarning("Player spawn footprint is not clear/supported."); }
                foreach(var encounter in objects.Select(t=>t.GetComponent<FishBattleEncounter>()).Where(e=>e!=null))
                    if(encounter.ShoreReturnPoint==null || encounter.ShoreGround==null || !SafeFootprint(body,encounter.ShoreGround,encounter.ShoreReturnPoint.position))
                    { issues++; Debug.LogWarning("Check safe return footprint: "+encounter.name,encounter); }
                var dock=objects.FirstOrDefault(t=>t.name=="Placeholder Dock")?.GetComponent<Collider>();
                if(dock!=null && (dock.bounds.max.y-ground.bounds.max.y > body.stepOffset || dock.bounds.min.y > ground.bounds.max.y))
                { issues++; Debug.LogWarning("Dock needs a connected ramp or step within the controller step height."); }
            }
            else { issues++; Debug.LogWarning("Need one player and beach ground to validate safe points."); }
            Debug.Log("5D collider/clearance checks: "+issues+" issue(s). Walk all paths in Play Mode; this is not a navigation or camera test.");
        }
        private static bool SafeFootprint(CharacterController body,Collider ground,Vector3 point)
        {
            if(!ground.Raycast(new Ray(point+Vector3.up*4,Vector3.down),out RaycastHit hit,12) || hit.normal.y<.7f) return false;
            float radius=body.radius*Mathf.Max(Mathf.Abs(body.transform.lossyScale.x),Mathf.Abs(body.transform.lossyScale.z));
            float half=Mathf.Max(radius,body.height*Mathf.Abs(body.transform.lossyScale.y)*.5f);
            Vector3 center=hit.point+Vector3.up*(half+body.skinWidth+.02f);
            return !Physics.OverlapCapsule(center-Vector3.up*(half-radius),center+Vector3.up*(half-radius),radius+body.skinWidth,~0,QueryTriggerInteraction.Ignore)
                .Any(c=>c!=ground && !c.transform.IsChildOf(body.transform));
        }
        private static Transform Node(string name,Transform parent,Vector3 position)
        { var go=new GameObject(name); Undo.RegisterCreatedObjectUndo(go,"Create "+name); go.transform.SetParent(parent,false); go.transform.localPosition=position; return go.transform; }
        private static void Box(string name,Transform parent,Vector3 position,Vector3 scale,Material material,bool solid)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube); Undo.RegisterCreatedObjectUndo(go,"Create "+name);
            go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=material;
            if(!solid) Undo.DestroyObjectImmediate(go.GetComponent<Collider>());
        }
        private static void Sign(string name,Transform parent,Vector3 position,string message,float width)
        {
            if(parent.Find(name)!=null) return;
            Transform node=FishTimingSetup.CreateUI(name,parent).transform; node.localPosition=position;
            var canvas=Undo.AddComponent<Canvas>(node.gameObject);canvas.renderMode=RenderMode.WorldSpace;
            var rect=node.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(width*100,70);node.localScale=Vector3.one*.01f;
            Transform label=FishTimingSetup.CreateUI("Label",node).transform;var text=Undo.AddComponent<Text>(label.gameObject);
            text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=24;text.alignment=TextAnchor.MiddleCenter;text.color=new Color(.08f,.16f,.18f);
            text.text=message;text.raycastTarget=false;text.rectTransform.sizeDelta=rect.sizeDelta;
        }
        private static Material MaterialAt(string name,Color color,Shader shader)
        {
            string path="Assets/FishPrototypeMaterials/"+name+".mat";
            Object existing=AssetDatabase.LoadMainAssetAtPath(path);if(existing!=null)return existing as Material;
            var material=new Material(shader);material.name=name;material.SetColor("_BaseColor",color);material.SetFloat("_Smoothness",0);
            AssetDatabase.CreateAsset(material,path);return material;
        }
    }
}
