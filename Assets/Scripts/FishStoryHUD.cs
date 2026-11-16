using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fish
{
    // All narrative presentation reuses the shore HUD and its existing UI canvas.
    [DefaultExecutionOrder(-140)]
    public sealed class FishStoryHUD : MonoBehaviour
    {
        [SerializeField] private FishCollection collection;
        [SerializeField] private Text objective;
        [SerializeField] private Text notification;
        [SerializeField] private Renderer recoveryWater;
        [SerializeField] private GameObject[] recoveryLitter = new GameObject[0];
        private FishTeamPanel teamPanel;
        private FishCollectionPanel collectionPanel;
        private FishFishingSpot fishing;
        private FishExplorationPlayer player;
        private FishOrbitCamera orbit;
        private GameObject panel;
        private Text heading, body;
        private Button proceed, close;
        private ScrollRect storyScroll;
        private Action proceedAction, afterClose;
        private bool open, ownsInput, oldMovement, oldCamera, oldVisible;
        private CursorLockMode oldLock;
        private int openedFrame, releaseFrame = -1;
        private float remaining;
        private MaterialPropertyBlock originalWater;
        private bool[] originalLitter;
        public void Configure(FishCollection source, Text objectiveText, Text noticeText)
        { collection = source; objective = objectiveText; notification = noticeText; }
        private void Start()
        {
            teamPanel = FindFirstObjectByType<FishTeamPanel>();
            collectionPanel = FindFirstObjectByType<FishCollectionPanel>();
            fishing = FindFirstObjectByType<FishFishingSpot>();
            player = FindFirstObjectByType<FishExplorationPlayer>();
            orbit = FindFirstObjectByType<FishOrbitCamera>();
            if (collection == null || objective == null || notification == null || teamPanel == null ||
                collectionPanel == null || fishing == null || player == null || orbit == null)
            { Debug.LogError("Story presentation needs the existing shore HUD and 1A-4 references.", this); enabled = false; return; }
            originalLitter = new bool[recoveryLitter.Length];
            for (int i = 0; i < recoveryLitter.Length; i++) originalLitter[i] = recoveryLitter[i] != null && recoveryLitter[i].activeSelf;
            if (recoveryWater == null) {
                foreach (var root in gameObject.scene.GetRootGameObjects())
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                        if (renderer.name == "WaterPlaceholder") recoveryWater = renderer;
            }
            originalWater = new MaterialPropertyBlock();
            if (recoveryWater != null) recoveryWater.GetPropertyBlock(originalWater);
            FishHUDLayout.Story(objective, notification);
            BuildPanel();
        }
        private bool CanOpen => !ownsInput && player.ExplorationInputEnabled && orbit.ExplorationInputEnabled &&
            !teamPanel.BlocksOtherPanels && !collectionPanel.BlocksFishing && !fishing.BlocksCollection;
        private void LateUpdate()
        {
            if (panel == null) return;
            objective.gameObject.SetActive(CanOpen);
            notification.gameObject.SetActive(CanOpen);
        }
        private void Update()
        {
            if (panel == null) return;
            if (releaseFrame >= 0)
            {
                if (Time.frameCount <= releaseFrame) return;
                releaseFrame = -1;
                RestoreInput();
                var next = afterClose; afterClose = null; next?.Invoke();
            }
            objective.text = collection.Story.Objective;
            bool hudVisible = CanOpen;
            objective.gameObject.SetActive(hudVisible);
            notification.gameObject.SetActive(hudVisible);
            if (open)
            {
                if (Application.isFocused && Time.frameCount > openedFrame && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Close(null);
                return;
            }
            if (!CanOpen) return;
            if (collection.Story.TryPresentOpening())
            {
                Show("Welcome to the shore", "You have arrived in a fictional coastal barangay in Negros. The community depends on the sea, but a disturbance is unsettling its fish. Meet fish companions and help protect the shoreline.\n\nWASD or arrow keys: move. Mouse: look around. At the fishing dock, press E, then Space when the indicator reaches the target.\n\nFirst objective: catch three fish. C opens Collection; N opens the Encyclopedia. Escape releases the exploration cursor; click to resume.", "Explore the shore", null);
                return;
            }
            if (collection.Story.TryPresentEnding())
            {
                RecoverShore();
                Show("A beginning, together", "The disturbance has eased. The shoreline is beginning to recover, but caring for it takes time and shared effort.\n\nReduce litter, protect coastal habitats, and support responsible fishing. Our barangay thanks you for helping its fish companions and looking after the sea.\n\nThank you for playing FISH! You can return to shore and keep exploring.", "Return to Shore", null);
                return;
            }
            remaining -= Time.unscaledDeltaTime;
            if (remaining <= 0)
            {
                if (collection.Story.TryTakeNotice(out string text))
                { notification.text = text == "Completed." ? "" : text; remaining = 8; }
                else notification.text = "";
            }
        }
        public void ShowEncounter(int id, Action confirmed)
        {
            if (panel == null || !CanOpen || !collection.Story.CanEnter(id)) return;
            string title = id == 1 ? "First Disturbance" : id == 2 ? "Growing Threat" : "The environmental boss";
            string text = id == 1 ? "A troubled fish has approached the beach. This first encounter is a chance to learn Attack, Defend and your fish's Special Ability." :
                id == 2 ? "A stronger opponent has appeared. The disturbance is spreading along the coast. Prepare your companions before you face it." :
                "The Tangle of Tides embodies pollution, overfishing and damaged habitats in this fictional battle. Face it when your team is ready.";
            text += "\n\nChoose 1-3 fish and confirm your team. R in Team recovers selected fish for free. Then close Team and press F again at this marker to begin.\n\nYou can return to shore now and fish or prepare first. Choosing a team does not start battle.";
            Show(title, text, "Choose Team", () => teamPanel.OpenForStory(confirmed));
        }
        private void Show(string title, string text, string actionLabel, Action action)
        {
            FishSave.Request();
            oldMovement = player.ExplorationInputEnabled; oldCamera = orbit.ExplorationInputEnabled;
            oldLock = Cursor.lockState; oldVisible = Cursor.visible; ownsInput = true;
            player.SetExplorationInputEnabled(false); orbit.SetExplorationInputEnabled(false);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            heading.text = title; body.text = text; proceed.GetComponentInChildren<Text>().text = actionLabel;
            proceedAction = action; openedFrame = Time.frameCount; open = true;
            objective.gameObject.SetActive(false); notification.gameObject.SetActive(false);
            panel.SetActive(true); panel.transform.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            storyScroll.verticalNormalizedPosition = 1;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }
        private void Close(Action next)
        {
            if (!open || Time.frameCount <= openedFrame) return;
            open = false; panel.SetActive(false); afterClose = next;
            // Keep input claimed through the next frame so close/Submit cannot also interact.
            releaseFrame = Time.frameCount + 1;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }
        private void RestoreInput()
        {
            if (!ownsInput) return;
            ownsInput = false;
            if (player != null) player.SetExplorationInputEnabled(oldMovement);
            if (orbit != null) orbit.SetExplorationInputEnabled(oldCamera);
            Cursor.lockState = Application.isFocused ? oldLock : CursorLockMode.None;
            Cursor.visible = !Application.isFocused || oldVisible;
        }
        private void OnDisable()
        { open = false; releaseFrame = -1; afterClose = null; if (panel != null) panel.SetActive(false); RestoreInput(); }
        private void OnDestroy() { if (panel != null) Destroy(panel); }
        public void RefreshSavedShore()
        {
            remaining = 0;
            notification.text = "";
            if (recoveryWater != null && originalWater != null) recoveryWater.SetPropertyBlock(originalWater);
            if (originalLitter != null)
                for (int i = 0; i < recoveryLitter.Length; i++)
                    if (recoveryLitter[i] != null) recoveryLitter[i].SetActive(originalLitter[i]);
            if (collection.Story.Stage == 4) RecoverShore();
        }
        private void RecoverShore()
        {
            foreach (var litter in recoveryLitter) if (litter != null) litter.SetActive(false);
            if (recoveryWater == null)
                foreach (var root in gameObject.scene.GetRootGameObjects())
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                        if (renderer.name == "Water Placeholder") recoveryWater = renderer;
            if (recoveryWater == null || recoveryWater.sharedMaterial == null || !recoveryWater.sharedMaterial.HasProperty("_BaseColor")) return;
            var properties = new MaterialPropertyBlock(); recoveryWater.GetPropertyBlock(properties);
            Color original = recoveryWater.sharedMaterial.GetColor("_BaseColor");
            properties.SetColor("_BaseColor", Color.Lerp(original, new Color(.25f,.82f,.95f,original.a), .65f));
            recoveryWater.SetPropertyBlock(properties); // Instance-only change, never edits the material asset.
        }
        private void BuildPanel()
        {
            var root = Rect("Story Presentation", GetComponentInParent<Canvas>().transform);
            panel = root.gameObject; root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = root.offsetMax = Vector2.zero;
            root.gameObject.AddComponent<Image>().color = new Color(.03f,.08f,.1f,.97f);
            var layout = root.gameObject.AddComponent<VerticalLayoutGroup>(); layout.padding = new RectOffset(32,32,28,28);
            layout.spacing = 20; layout.childControlHeight = layout.childControlWidth = true; layout.childForceExpandHeight = false;
            heading = Label("Title",root,32); heading.gameObject.AddComponent<LayoutElement>().preferredHeight = 58;
            var viewport = Rect("Story Scroll",root); viewport.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;
            viewport.gameObject.AddComponent<RectMask2D>(); viewport.gameObject.AddComponent<Image>().color = new Color(0,0,0,.01f);
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.viewport = viewport;
            storyScroll = scroll;
            body = Label("Text",viewport,26); body.alignment = TextAnchor.UpperLeft;
            body.rectTransform.anchorMin = new Vector2(0,1); body.rectTransform.anchorMax = Vector2.one;
            body.rectTransform.pivot = new Vector2(.5f,1); body.rectTransform.sizeDelta = Vector2.zero;
            body.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize; scroll.content = body.rectTransform;
            var footer = Rect("Actions",root); footer.gameObject.AddComponent<LayoutElement>().preferredHeight = 62;
            var row = footer.gameObject.AddComponent<HorizontalLayoutGroup>(); row.spacing = 16; row.childControlWidth = row.childControlHeight = true; row.childForceExpandWidth = true;
            proceed = Button("Continue",footer); close = Button("Return to Shore",footer);
            proceed.onClick.AddListener(() => { if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) Close(proceedAction); });
            close.onClick.AddListener(() => Close(null)); FishUITheme.Apply(panel); panel.SetActive(false);
        }
        private static RectTransform Rect(string name,Transform parent)
        { var rect = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent,false); return rect; }
        private Text Label(string name,Transform parent,int size)
        {
            var label = Rect(name,parent).gameObject.AddComponent<Text>(); label.font = objective.font;
            label.fontSize = size; label.color = Color.white; label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Overflow; label.raycastTarget = false; return label;
        }
        private Button Button(string name,Transform parent)
        {
            var rect = Rect(name,parent); var image = rect.gameObject.AddComponent<Image>(); image.color = new Color32(71,123,86,255);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var label = Label("Label",rect,24); label.text = name; label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(16,8); label.rectTransform.offsetMax = new Vector2(-16,-8); return button;
        }
    }
}




