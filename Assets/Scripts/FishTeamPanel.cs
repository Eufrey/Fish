using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fish
{
    // Resolve simultaneous T/C/E before collection (-110) and fishing (-100).
    [DefaultExecutionOrder(-120)]
    public sealed class FishTeamPanel : MonoBehaviour
    {
        [SerializeField] private FishBattleTeam team;
        [SerializeField] private FishCollection collection;
        [SerializeField] private FishExplorationPlayer player;
        [SerializeField] private FishOrbitCamera orbitCamera;
        [SerializeField] private FishFishingSpot fishingSpot;
        [SerializeField] private FishCollectionPanel collectionPanel;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text explorationHint;
        [SerializeField] private Text pageLabel;
        [SerializeField] private Text status;
        [SerializeField] private Button[] catchButtons;
        [SerializeField] private Text[] catchLabels;
        [SerializeField] private Button[] slotButtons;
        [SerializeField] private Text[] slotLabels;
        [SerializeField] private Button previousPage;
        [SerializeField] private Button nextPage;
        [SerializeField] private Button remove;
        [SerializeField] private Button moveUp;
        [SerializeField] private Button moveDown;
        [SerializeField] private Button confirm;
        [SerializeField] private Button close;
        private readonly List<Button> boundButtons = new List<Button>();
        private readonly List<UnityAction> callbacks = new List<UnityAction>();
        private bool panelOpen;
        private System.Action storyConfirmation;
        public void OpenForStory(System.Action confirmed)
        {
            if (!isActiveAndEnabled || panelOpen) return;
            storyConfirmation = confirmed;
            OpenPanel();
        }
        private bool previousMovementInput;
        private bool previousCameraInput;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;
        private int closedFrame = -1;
        private int page;
        private int selectedSlot = -1;
        public bool BlocksOtherPanels => panelOpen || Time.frameCount == closedFrame;

        // Setup uses serialized references, as with the earlier milestone panels.
        private void OnEnable()
        {
            if (explorationHint != null) FishHUDLayout.Hint(explorationHint, 1);
            if (team == null || collection == null || team.Collection != collection || player == null ||
                orbitCamera == null || fishingSpot == null || collectionPanel == null || panel == null ||
                explorationHint == null || pageLabel == null || status == null || previousPage == null ||
                nextPage == null || remove == null || moveUp == null || moveDown == null || confirm == null || close == null ||
                !ValidRows(catchButtons, catchLabels) || !ValidRows(slotButtons, slotLabels) ||
                slotButtons.Length != FishTeamSelection.MaximumMembers)
            {
                Debug.LogError("Team UI references are missing or inconsistent. Check FISH! > Setup Milestone 2B.", this);
                enabled = false;
                return;
            }
            FishTeamLayout.Apply(panel);
            panel.SetActive(false);
            for (int i = 0; i < catchButtons.Length; i++)
            {
                int row = i;
                Bind(catchButtons[i], () => AddCatch(row));
            }
            for (int i = 0; i < slotButtons.Length; i++)
            {
                int slot = i;
                Bind(slotButtons[i], () => { selectedSlot = slot; Refresh(); });
            }
            Bind(previousPage, () => { page--; Refresh(); });
            Bind(nextPage, () => { page++; Refresh(); });
            Bind(remove, RemoveSelected);
            Bind(moveUp, () => MoveSelected(-1));
            Bind(moveDown, () => MoveSelected(1));
            Bind(confirm, () => {
                if (!team.HasUsableFish || !team.Selection.Confirm()) return;
                var callback = storyConfirmation;
                storyConfirmation = null;
                callback?.Invoke();
                Refresh();
                status.text = "Team confirmed. Return to shore, then press F at the marker to start.";
                status.color = FishUITheme.Success; FishSave.Request();
            });
            Bind(close, ClosePanel);
        }

        private static bool ValidRows(Button[] buttons, Text[] labels)
        {
            if (buttons == null || labels == null || buttons.Length == 0 || buttons.Length != labels.Length) return false;
            for (int i = 0; i < buttons.Length; i++)
                if (buttons[i] == null || labels[i] == null) return false;
            return true;
        }

        private void Bind(Button button, UnityAction callback)
        {
            button.onClick.AddListener(callback);
            boundButtons.Add(button);
            callbacks.Add(callback);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (panelOpen)
            {
                if (Application.isFocused && keyboard != null && keyboard.rKey.wasPressedThisFrame)
                {
                    team.RecoverTeam();
                    Refresh();
                    if (team.Selection.Members.Count > 0)
                    {
                        status.text = "Team recovered to full HP, free. Confirm your team when ready.";
                        status.color = FishUITheme.Success; FishSave.Request();
                    }
                }
                if (keyboard != null && (keyboard.tKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)) ClosePanel();
                return;
            }
            bool canOpen = !fishingSpot.BlocksCollection && !collectionPanel.BlocksFishing &&
                player.ExplorationInputEnabled && orbitCamera.ExplorationInputEnabled;
            explorationHint.gameObject.SetActive(canOpen);
            if (canOpen && Application.isFocused && keyboard != null &&
                keyboard.tKey.wasPressedThisFrame && !keyboard.escapeKey.wasPressedThisFrame) OpenPanel();
        }

        private void OpenPanel()
        {
            if (panelOpen || fishingSpot.BlocksCollection || collectionPanel.BlocksFishing) return;
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
            status.text = team.Selection.IsConfirmed ? "Team confirmed. Editing requires confirmation again." : "Select 1-3 owned fish. Click a team slot to remove or reorder it.";
            Refresh();
            panel.SetActive(true);
            FishTeamLayout.FitRows(catchButtons);
            FishTeamLayout.FitRows(slotButtons);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        private void LateUpdate()
        {
            if (explorationHint != null)
                explorationHint.gameObject.SetActive(!panelOpen && player.ExplorationInputEnabled && orbitCamera.ExplorationInputEnabled &&
                    !collectionPanel.BlocksFishing && !fishingSpot.BlocksCollection);
        }

        private void AddCatch(int row)
        {
            int index = page * catchButtons.Length + row;
            if (index < 0 || index >= collection.Count) return;
            if (!team.Selection.TryAdd(collection.Catches[index].CatchId)) return;
            selectedSlot = team.Selection.Members.Count - 1;
            Refresh();
            status.text = "Fish added. Confirm when your team is ready. R: Recover Team for free.";
        }

        private void RemoveSelected()
        {
            if (!team.Selection.RemoveAt(selectedSlot)) return;
            selectedSlot = Mathf.Min(selectedSlot, team.Selection.Members.Count - 1);
            Refresh();
            status.text = "Fish removed from team; it remains in your collection.";
        }

        private void MoveSelected(int direction)
        {
            int destination = selectedSlot + direction;
            if (!team.Selection.Move(selectedSlot, destination)) return;
            selectedSlot = destination;
            Refresh();
            status.text = "Team order changed. Confirm again when ready.";
        }

        private void Refresh()
        {
            status.color = Color.white;
            int pageCount = Mathf.Max(1, (collection.Count + catchButtons.Length - 1) / catchButtons.Length);
            page = Mathf.Clamp(page, 0, pageCount - 1);
            pageLabel.text = "Owned catches - page " + (page + 1) + "/" + pageCount;
            previousPage.interactable = page > 0;
            nextPage.interactable = page + 1 < pageCount;
            var catches = collection.Catches;
            for (int row = 0; row < catchButtons.Length; row++)
            {
                int index = page * catchButtons.Length + row;
                bool exists = index < catches.Count;
                catchButtons[row].gameObject.SetActive(exists);
                if (!exists) continue;
                FishOwnedCatch owned = catches[index];
                bool selected = team.Selection.Contains(owned.CatchId);
                catchLabels[row].text = "Catch #" + (index + 1) + ": " + (owned.Species != null ? owned.Species.displayName : "Missing species") +
                    "\nHP " + owned.CurrentHP + "/" + owned.MaximumHP + (selected ? " [Selected]" : " [Add]");
                catchButtons[row].interactable = owned.Species != null && !selected && team.Selection.Members.Count < FishTeamSelection.MaximumMembers;
            }
            for (int slot = 0; slot < slotButtons.Length; slot++)
            {
                FishOwnedCatch owned = team.GetMember(slot);
                var colors = slotButtons[slot].colors;
                colors.normalColor = selectedSlot == slot && owned != null ? FishUITheme.Selected : new Color32(71, 123, 86, 255);
                slotButtons[slot].colors = colors;
                slotButtons[slot].interactable = owned != null;
                if (owned == null) { slotLabels[slot].text = (slot + 1) + ". Empty"; continue; }
                FishSpecies species = owned.Species;
                int catchNumber = 0;
                for (int index = 0; index < catches.Count; index++)
                    if (catches[index].CatchId == owned.CatchId) { catchNumber = index + 1; break; }
                slotLabels[slot].text = (selectedSlot == slot ? "> " : "") + (slot + 1) + ". Catch #" + catchNumber + ": " + species.displayName +
                    "\nHP " + owned.CurrentHP + "/" + owned.MaximumHP + "   ATK " + species.attack + "   DEF " + species.defense + "   SPD " + species.speed;
            }
            bool hasSelection = selectedSlot >= 0 && selectedSlot < team.Selection.Members.Count;
            remove.interactable = hasSelection;
            moveUp.interactable = hasSelection && selectedSlot > 0;
            moveDown.interactable = hasSelection && selectedSlot + 1 < team.Selection.Members.Count;
            confirm.interactable = team.Selection.CanConfirm && team.HasUsableFish;
            status.text = "R: Recover Team (free full HP). Zero-HP fish are skipped in battle.";
            if (collection.Count == 0) status.text = "No owned fish yet. Catch a fish at the dock first.";
            FishTeamLayout.FitRows(catchButtons);
            FishTeamLayout.FitRows(slotButtons);
        }

        public void ClosePanel()
        {
            storyConfirmation = null;
            if (!panelOpen) return;
            panelOpen = false;
            closedFrame = Time.frameCount;
            if (panel != null) FishTeamLayout.Apply(panel);
            panel.SetActive(false);
            if (player != null) player.SetExplorationInputEnabled(previousMovementInput);
            if (orbitCamera != null) orbitCamera.SetExplorationInputEnabled(previousCameraInput);
            Cursor.lockState = Application.isFocused ? previousCursorLock : CursorLockMode.None;
            Cursor.visible = !Application.isFocused || previousCursorVisible;
        }

        private void OnDisable()
        {
            ClosePanel();
            if (explorationHint != null) explorationHint.gameObject.SetActive(false);
            for (int i = 0; i < boundButtons.Count; i++)
                if (boundButtons[i] != null) boundButtons[i].onClick.RemoveListener(callbacks[i]);
            boundButtons.Clear();
            callbacks.Clear();
        }
    }
}






