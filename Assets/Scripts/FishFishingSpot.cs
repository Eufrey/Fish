using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fish
{
    // Apply the interaction gate before the player's movement Update.
    [DefaultExecutionOrder(-100)]
    public sealed class FishFishingSpot : MonoBehaviour
    {
        [SerializeField] private FishExplorationPlayer player;
        [SerializeField] private FishOrbitCamera orbitCamera;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private GameObject prompt;
        [SerializeField] private GameObject fishingPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private float interactionRadius = 2.5f;
        [SerializeField] private FishCollectionPanel collectionPanel;
        [SerializeField] private FishTeamPanel teamPanel;

        private InputAction interact;
        private bool panelOpen;
        private bool previousMovementInput;
        private bool previousCameraInput;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;
        public GameObject FishingPanel => fishingPanel;
        public Button CloseButton => closeButton;
        private int closedFrame = -1;
        public bool BlocksCollection => panelOpen || Time.frameCount == closedFrame;

        public void ConfigureCollectionPanel(FishCollectionPanel panel) => collectionPanel = panel;
        public void ConfigureTeamPanel(FishTeamPanel panel) => teamPanel = panel;

        public void Configure(FishExplorationPlayer explorationPlayer, FishOrbitCamera camera,
            InputActionAsset actions, GameObject nearbyPrompt, GameObject panel, Button close)
        {
            player = explorationPlayer;
            orbitCamera = camera;
            inputActions = actions;
            prompt = nearbyPrompt;
            fishingPanel = panel;
            closeButton = close;
        }

        private void OnEnable()
        {
            InputAction interactSource = inputActions != null ? inputActions.FindAction("Player/Interact") : null;
            if (player == null || orbitCamera == null || interactSource == null ||
                prompt == null || fishingPanel == null || closeButton == null)
            {
                Debug.LogError("Fishing needs its scene references and the Player/Interact action. Check Milestone 1B setup.", this);
                enabled = false;
                return;
            }

            interact = interactSource.Clone();
            interact.Enable();
            closeButton.onClick.AddListener(ClosePanel);
            prompt.SetActive(false);
            fishingPanel.SetActive(false);
        }

        private void Update()
        {
            if (panelOpen)
            {
                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                    ClosePanel();
                return;
            }

            Vector3 offset = player.transform.position - transform.position;
            bool nearby = offset.sqrMagnitude <= interactionRadius * interactionRadius;
            bool canInteract = nearby && player.ExplorationInputEnabled && orbitCamera.ExplorationInputEnabled &&
                (collectionPanel == null || !collectionPanel.BlocksFishing) &&
                (teamPanel == null || !teamPanel.BlocksOtherPanels);
            prompt.SetActive(canInteract);

            // WasPressedThisFrame responds immediately, even though the template action has Hold.
            if (canInteract && Application.isFocused && interact.WasPressedThisFrame())
                OpenPanel();
        }

        private void OpenPanel()
        {
            if (panelOpen || (collectionPanel != null && collectionPanel.BlocksFishing) ||
                (teamPanel != null && teamPanel.BlocksOtherPanels)) return;
            previousMovementInput = player.ExplorationInputEnabled;
            previousCameraInput = orbitCamera.ExplorationInputEnabled;
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            panelOpen = true;
            player.SetExplorationInputEnabled(false);
            orbitCamera.SetExplorationInputEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            prompt.SetActive(false);
            fishingPanel.SetActive(true);
        }

        public void ClosePanel()
        {
            if (!panelOpen) return;
            panelOpen = false;
            closedFrame = Time.frameCount;
            if (fishingPanel != null) fishingPanel.SetActive(false);
            if (player != null) player.SetExplorationInputEnabled(previousMovementInput);
            if (orbitCamera != null) orbitCamera.SetExplorationInputEnabled(previousCameraInput);
            Cursor.lockState = Application.isFocused ? previousCursorLock : CursorLockMode.None;
            Cursor.visible = !Application.isFocused || previousCursorVisible;
        }

        private void OnDisable()
        {
            ClosePanel();
            if (prompt != null) prompt.SetActive(false);
            if (closeButton != null) closeButton.onClick.RemoveListener(ClosePanel);
            interact?.Dispose();
            interact = null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
