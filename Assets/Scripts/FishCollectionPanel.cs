using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fish
{
    // Before the fishing spot (-100), so simultaneous C/E has one consistent owner.
    [DefaultExecutionOrder(-110)]
    public sealed class FishCollectionPanel : MonoBehaviour
    {
        [SerializeField] private FishCollection collection;
        [SerializeField] private FishExplorationPlayer player;
        [SerializeField] private FishOrbitCamera orbitCamera;
        [SerializeField] private FishFishingSpot fishingSpot;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text contents;
        [SerializeField] private Text explorationHint;
        [SerializeField] private Button closeButton;
        [SerializeField] private FishTeamPanel teamPanel;
        private bool panelOpen;
        [SerializeField] private FishCatchCatalog catalog;
        private FishEncyclopediaView encyclopediaView;
        private bool encyclopediaOpen;
        public void ConfigureCatalog(FishCatchCatalog value) => catalog = value;
        private bool previousMovementInput;
        private bool previousCameraInput;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;
        private int closedFrame = -1;
        public bool BlocksFishing => panelOpen || Time.frameCount == closedFrame;
        public void ConfigureTeamPanel(FishTeamPanel value) => teamPanel = value;

        public void Configure(FishCollection ownedFish, FishExplorationPlayer explorationPlayer,
            FishOrbitCamera camera, FishFishingSpot spot, GameObject collectionPanel,
            Text namesAndQuantities, Text hint, Button close)
        {
            collection = ownedFish;
            player = explorationPlayer;
            orbitCamera = camera;
            fishingSpot = spot;
            panel = collectionPanel;
            contents = namesAndQuantities;
            explorationHint = hint;
            closeButton = close;
        }

        private void OnEnable()
        {
            if (explorationHint != null) FishHUDLayout.Hint(explorationHint, 0);
            if (collection == null || player == null || orbitCamera == null || fishingSpot == null ||
                panel == null || contents == null || explorationHint == null || closeButton == null)
            {
                Debug.LogError("Collection UI references are missing. Run FISH! > Setup Milestone 2A.", this);
                enabled = false;
                return;
            }
            panel.SetActive(false);
            if (encyclopediaView == null)
                encyclopediaView = new FishEncyclopediaView(panel, contents, closeButton, collection, catalog);
            explorationHint.text = catalog != null ? "C: Collection   N: Encyclopedia" : "C: Collection";
            explorationHint.fontSize = 20;
            explorationHint.rectTransform.sizeDelta = new Vector2(400, 34);
            closeButton.onClick.AddListener(ClosePanel);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (panelOpen)
            {
                if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.cKey.wasPressedThisFrame || keyboard.nKey.wasPressedThisFrame))
                    ClosePanel();
                return;
            }

            bool canOpen = !fishingSpot.BlocksCollection && player.ExplorationInputEnabled && orbitCamera.ExplorationInputEnabled &&
                (teamPanel == null || !teamPanel.BlocksOtherPanels);
            explorationHint.gameObject.SetActive(canOpen);
            if (canOpen && Application.isFocused && keyboard != null &&
                (keyboard.cKey.wasPressedThisFrame || (catalog != null && keyboard.nKey.wasPressedThisFrame)) && !keyboard.escapeKey.wasPressedThisFrame)
            { encyclopediaOpen = keyboard.nKey.wasPressedThisFrame; OpenPanel(); }
        }

        private void OpenPanel()
        {
            if (panelOpen || fishingSpot.BlocksCollection || (teamPanel != null && teamPanel.BlocksOtherPanels)) return;
            previousMovementInput = player.ExplorationInputEnabled;
            previousCameraInput = orbitCamera.ExplorationInputEnabled;
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            panelOpen = true;
            player.SetExplorationInputEnabled(false);
            orbitCamera.SetExplorationInputEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            explorationHint.gameObject.SetActive(false);
            encyclopediaView?.Show(encyclopediaOpen);
            if (!encyclopediaOpen) RefreshContents();
            panel.SetActive(true);
            encyclopediaView?.ResetScroll();
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        private void LateUpdate()
        {
            if (explorationHint != null)
                explorationHint.gameObject.SetActive(!panelOpen && player.ExplorationInputEnabled && orbitCamera.ExplorationInputEnabled &&
                    !fishingSpot.BlocksCollection && (teamPanel == null || !teamPanel.BlocksOtherPanels));
        }

        private void RefreshContents()
        {
            if (collection.Count == 0)
            {
                contents.text = "No fish collected yet.\nCatch a fish at the dock.";
                return;
            }
            var quantities = new Dictionary<FishSpecies, int>();
            foreach (FishOwnedCatch owned in collection.Catches)
            {
                if (owned.Species == null) continue;
                if (!quantities.ContainsKey(owned.Species)) quantities.Add(owned.Species, 0);
                quantities[owned.Species]++;
            }
            var text = new StringBuilder();
            text.AppendLine("Owned catches: " + collection.Count);
            foreach (var entry in quantities)
                text.AppendLine(entry.Key.displayName + "  x " + entry.Value);
            text.AppendLine("\nIndividual health:");
            int number = 0;
            foreach (var fish in collection.Catches)
                text.AppendLine("Catch #" + (++number) + ": " + (fish.Species != null ? fish.Species.displayName : "Missing species") +
                    "\nHP " + fish.CurrentHP + "/" + fish.MaximumHP + "\n");
            contents.text = text.ToString();
        }

        public void ClosePanel()
        {
            if (!panelOpen) return;
            panelOpen = false;
            closedFrame = Time.frameCount;
            if (panel != null) panel.SetActive(false);
            if (player != null) player.SetExplorationInputEnabled(previousMovementInput);
            if (orbitCamera != null) orbitCamera.SetExplorationInputEnabled(previousCameraInput);
            Cursor.lockState = Application.isFocused ? previousCursorLock : CursorLockMode.None;
            Cursor.visible = !Application.isFocused || previousCursorVisible;
        }

        private void OnDisable()
        {
            ClosePanel();
            if (explorationHint != null) explorationHint.gameObject.SetActive(false);
            if (closeButton != null) closeButton.onClick.RemoveListener(ClosePanel);
        }
    }
}



