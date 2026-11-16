using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace Fish
{
    // Read-only presentation, using the battle scene's existing camera.
    public sealed class FishBattleVisuals : MonoBehaviour
    {
        private Camera cameraView;
        private Transform stage;
        private GameObject[] models;
        private FishBattleFighter[] displayed;
        private int playerCount;
        private Material[] fallbackMaterials;
        public void Initialize(Camera camera, Transform ui, Text playerInfo, Text enemyInfo)
        {
            if (stage != null) return;
            cameraView=camera;
            stage=new GameObject("Battle Visual Stage").transform;
            // Keep world geometry independent of CanvasScaler and RectTransform scale.
            SceneManager.MoveGameObjectToScene(stage.gameObject, gameObject.scene);
            stage.position=new Vector3(0,10000,0);
            camera.transform.position = stage.position + new Vector3(0, 17, -18);
            camera.transform.LookAt(stage.position + new Vector3(0, 0, -1));
            camera.orthographic = false;
            camera.fieldOfView = 40;
            camera.rect = new Rect(0, 0, 1, 1);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.targetTexture = null;
            camera.ResetAspect();
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 100;
            camera.backgroundColor = new Color(.12f, .48f, .55f);
            bool hasLight = false;
            foreach (var root in gameObject.scene.GetRootGameObjects())
                foreach (var light in root.GetComponentsInChildren<Light>())
                    hasLight |= light.isActiveAndEnabled && light.intensity > 0;
            if (!hasLight)
            {
                var lightObject = new GameObject("Battle key light", typeof(Light));
                lightObject.transform.SetParent(stage, false);
                lightObject.transform.rotation = Quaternion.Euler(50, -30, 0);
                var light = lightObject.GetComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.3f;
            }
            var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            water.name = "Shallow water placeholder";
            water.transform.SetParent(stage, false);
            water.transform.localPosition = new Vector3(0, -1, 0);
            water.transform.localScale = new Vector3(40, .1f, 40);
            Destroy(water.GetComponent<Collider>());
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = new Color(.12f, .56f, .58f);
            water.GetComponent<Renderer>().sharedMaterial = material;
            waterMaterial = material;
            var surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = "Clear water surface";
            surface.transform.SetParent(stage, false);
            surface.transform.localPosition = new Vector3(0,1.2f,0);
            surface.transform.localScale = new Vector3(40,.01f,40);
            Destroy(surface.GetComponent<Collider>());
            surfaceMaterial = new Material(material);
            surfaceMaterial.SetFloat("_Surface",1);
            surfaceMaterial.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            surfaceMaterial.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            surfaceMaterial.SetFloat("_ZWrite",0);
            surfaceMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            surfaceMaterial.renderQueue = 3000;
            surfaceMaterial.color = new Color(.3f,.85f,.8f,.12f);
            surface.GetComponent<Renderer>().sharedMaterial = surfaceMaterial;
            ArrangeInfo(playerInfo, -310);
            ArrangeInfo(enemyInfo, 310);
            foreach (var panel in ui.GetComponentsInChildren<Image>())
                if (panel.transform == ui) panel.color = Color.clear;
        }
        private Material waterMaterial;
        private Material surfaceMaterial;
        public void BindTeams(FishBattleRules battle, FishSpecies[] players, FishSpecies[] enemies)
        {
            if (models != null)
            {
                foreach (var model in models) if (model != null) Destroy(model);
                foreach (var material in fallbackMaterials) if (material != null) Destroy(material);
            }
            playerCount = battle.Players.Count;
            int count = playerCount + battle.Enemies.Count;
            models = new GameObject[count];
            displayed = new FishBattleFighter[count];
            fallbackMaterials = new Material[count * 2];
            for (int i = 0; i < playerCount; i++)
                Show(i, battle.Players[i], players != null && i < players.Length ? players[i] : null);
            for (int i = 0; i < battle.Enemies.Count; i++)
                Show(playerCount + i, battle.Enemies[i], enemies != null && i < enemies.Length ? enemies[i] : null);
            RefreshFormation(battle);
        }
        public void RefreshFormation(FishBattleRules battle)
        {
            if (models == null) return;
            for (int i = 0; i < models.Length; i++)
                if (models[i] != null) models[i].SetActive(displayed[i].Health > 0);
        }
        public bool TryGetLabelPosition(int index, out Vector3 screenPosition)
        {
            screenPosition = Vector3.zero;
            if (models == null || index >= models.Length || models[index] == null || !models[index].activeSelf) return false;
            screenPosition = cameraView.WorldToScreenPoint(models[index].transform.position + Vector3.up * 1.35f);
            return screenPosition.z > 0;
        }
        public void HighlightTarget(int index)
        {
            if (models == null) return;
            for (int i = 0; i < models.Length; i++)
                if (models[i] != null) models[i].transform.localPosition = SlotPosition(i) +
                    (i == playerCount + index && index >= 0 ? Vector3.up * .4f : Vector3.zero);
        }
        private Vector3 SlotPosition(int index)
        {
            bool player = index < playerCount;
            int slot = player ? index : index - playerCount;
            int count = player ? playerCount : models.Length - playerCount;
            return new Vector3(player ? -4.8f : 4.8f, 0, (slot - (count - 1) * .5f) * 4.2f);
        }
        public IEnumerator PlayAction(FishBattleSession.ActionResult action)
        {
            int index = action.ActorIsPlayer ? action.ActorIndex : playerCount + action.ActorIndex;
            if (models == null || index >= models.Length || models[index] == null) yield break;
            Transform actor = models[index].transform;
            Vector3 start = SlotPosition(index);
            Vector3 offset = Vector3.up * .5f;
            if (action.Offensive)
            {
                int target = action.ActorIsPlayer ? playerCount + action.TargetIndex : action.TargetIndex;
                offset = (SlotPosition(target) - start) * .65f;
            }
            float elapsed = 0;
            while (elapsed < .4f)
            {
                elapsed += Time.unscaledDeltaTime;
                actor.localPosition = start + offset * Mathf.Sin(Mathf.Clamp01(elapsed / .4f) * Mathf.PI);
                yield return null;
            }
            actor.localPosition = start;
        }
        private static void ArrangeInfo(Text info,float x)
        {
            var backdrop=info.transform.parent.GetComponent<Image>(); if(backdrop!=null) backdrop.enabled=false;
            info.transform.SetParent(info.transform.parent.parent,false);
            info.rectTransform.anchorMin=info.rectTransform.anchorMax=new Vector2(.5f,.5f);
            info.rectTransform.sizeDelta=new Vector2(560,150); info.rectTransform.anchoredPosition=new Vector2(x,205);
            info.fontSize=20; info.horizontalOverflow=HorizontalWrapMode.Wrap;
        }
        public void Show(int side,FishBattleFighter fighter,FishSpecies species)
        {
            if(displayed[side]==fighter) return;
            displayed[side]=fighter;
            if(models[side]!=null) { models[side].SetActive(false); Destroy(models[side]); }
            for(int i=side*2;i<side*2+2;i++) if(fallbackMaterials[i]!=null) { Destroy(fallbackMaterials[i]); fallbackMaterials[i]=null; }
            if(fighter==null) return;
            var holder=new GameObject(side==0 ? "Active Player Visual" : "Active Enemy Visual"); holder.transform.SetParent(stage,false);
            models[side]=holder;
            GameObject model=species!=null && species.model!=null ? Instantiate(species.model,holder.transform) : null;
            if(model!=null) model.SetActive(true);
            if(model==null || model.GetComponentsInChildren<Renderer>().Length==0)
            {
                if(model!=null) { model.SetActive(false); Destroy(model); }
                Shader shader=Shader.Find("Universal Render Pipeline/Unlit");
                if(shader==null) { Debug.LogError("URP Unlit shader missing. Run 5C setup to include placeholder materials."); return; }
                var body=new Material(shader); body.SetColor("_BaseColor",FishPrimitiveVisual.ColorFor(species));
                var accent=new Material(shader); accent.SetColor("_BaseColor",FishPrimitiveVisual.IsBoss(species) ? Color.cyan : new Color(.08f,.16f,.22f));
                fallbackMaterials[side*2]=body; fallbackMaterials[side*2+1]=accent;
                model=FishPrimitiveVisual.Create(body,accent,FishPrimitiveVisual.IsBoss(species)); model.transform.SetParent(holder.transform,false);
            }
            foreach(var collider in model.GetComponentsInChildren<Collider>(true)) { collider.enabled=false; Destroy(collider); }
            foreach(var body in model.GetComponentsInChildren<Rigidbody>(true)) { body.isKinematic=true; body.detectCollisions=false; }
            foreach(var animator in model.GetComponentsInChildren<Animator>(true)) animator.enabled=false;
            holder.transform.localRotation=Quaternion.Euler(0,side < playerCount ? 0 : 180,0);
            var renderers=model.GetComponentsInChildren<Renderer>();
            Bounds bounds=renderers[0].bounds; foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            bool boss=FishPrimitiveVisual.IsBoss(species);
            // Each half of the view has a fixed box. The boss gets a taller box;
            // even unusually sized imported meshes are fitted, including depth.
            float scale = Mathf.Min((boss ? 4.4f : 3.6f) / Mathf.Max(.01f, bounds.size.x),
                2f / Mathf.Max(.01f, bounds.size.y), 2f / Mathf.Max(.01f, bounds.size.z));
            Vector3 center = holder.transform.InverseTransformPoint(bounds.center);
            model.transform.localPosition -= center;
            holder.transform.localScale = Vector3.one * scale;
            holder.transform.localPosition = SlotPosition(side);
        }
        private void OnDestroy()
        {
            if(cameraView!=null) cameraView.targetTexture=null;
            if(stage!=null) Destroy(stage.gameObject);
            if (surfaceMaterial != null) Destroy(surfaceMaterial);
            if (waterMaterial != null) Destroy(waterMaterial);
            if (fallbackMaterials != null) foreach(var material in fallbackMaterials) if(material!=null) Destroy(material);
        }
    }
}




