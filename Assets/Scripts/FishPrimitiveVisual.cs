using UnityEngine;
namespace Fish
{
    // Visual-only primitive assembly. No gameplay components or stat writes.
    public static class FishPrimitiveVisual
    {
        public static bool IsBoss(FishSpecies species) => species != null && species.speciesId == "story-placeholder-3";
        public static Color ColorFor(FishSpecies species)
        {
            switch (species != null ? species.speciesId : "")
            {
                case "placeholder-fish": return new Color32(76, 193, 235, 255);
                case "catch-placeholder-2": return new Color32(246, 195, 74, 255);
                case "catch-placeholder-3": return new Color32(139, 209, 102, 255);
                case "catch-placeholder-4": return new Color32(200, 135, 230, 255);
                case "catch-placeholder-5": return new Color32(240, 115, 115, 255);
                case "story-placeholder-3": return new Color32(112, 71, 145, 255);
                default: return new Color32(235, 157, 87, 255);
            }
        }
        public static GameObject Create(Material body, Material accent, bool boss)
        {
            var root = new GameObject(boss ? "Stylized Environmental Boss" : "Stylized Fish Placeholder");
            Part(root, "Body", PrimitiveType.Sphere, Vector3.zero, new Vector3(2.4f,1f,.7f), 0, body);
            Part(root, "Tail upper", PrimitiveType.Cube, new Vector3(-1.35f,.22f,0), new Vector3(.65f,.35f,.18f), -35, accent);
            Part(root, "Tail lower", PrimitiveType.Cube, new Vector3(-1.35f,-.22f,0), new Vector3(.65f,.35f,.18f), 35, accent);
            Part(root, "Dorsal fin", PrimitiveType.Cube, new Vector3(-.2f,.55f,0), new Vector3(.65f,.35f,.12f), -20, accent);
            Part(root, "Side fin", PrimitiveType.Cube, new Vector3(.1f,-.2f,-.4f), new Vector3(.6f,.2f,.12f), 25, accent);
            Part(root, "Eye", PrimitiveType.Sphere, new Vector3(.8f,.15f,-.33f), Vector3.one*.15f, 0, accent);
            Part(root, "Far side fin", PrimitiveType.Cube, new Vector3(.1f,-.2f,.4f), new Vector3(.6f,.2f,.12f), 25, accent);
            Part(root, "Far eye", PrimitiveType.Sphere, new Vector3(.8f,.15f,.33f), Vector3.one*.15f, 0, accent);
            if (boss)
                for (int i=0;i<3;i++)
                    Part(root,"Fictional debris ridge " + i,PrimitiveType.Cube,new Vector3(-.65f+i*.55f,.7f,0),new Vector3(.25f,.65f,.4f),i*15-15,accent);
            return root;
        }
        private static void Part(GameObject root,string name,PrimitiveType type,Vector3 position,Vector3 scale,float angle,Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name=name; part.transform.SetParent(root.transform,false);
            part.transform.localPosition=position; part.transform.localScale=scale;
            part.transform.localRotation=Quaternion.Euler(0,0,angle);
            var collider=part.GetComponent<Collider>(); collider.enabled=false;
            if(Application.isPlaying) Object.Destroy(collider); else Object.DestroyImmediate(collider);
            part.GetComponent<Renderer>().sharedMaterial=material;
        }
    }
}
