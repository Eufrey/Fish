using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fish
{
    public sealed class FishTimingMinigame : MonoBehaviour
    {
        [SerializeField] private RectTransform indicator;
        [SerializeField] private Text feedback;
        [SerializeField] private Button retryButton;
        [SerializeField] private FishCollection collection;
        [SerializeField] private FishSpecies catchSpecies;
        private readonly FishTimingAttempt attempt = new FishTimingAttempt();
        private bool catchAdded;
        private bool resultSoundPlayed;
        [SerializeField] private FishCatchCatalog catalog;
        private FishSpecies chosenSpecies;
        private FishFishingDial dial;
        private FishFigmaFishing figmaView;
        private bool catchRequested;
        public void RequestCatch() => catchRequested = true;
        public void ConfigureCatalog(FishCatchCatalog value) => catalog = value;
        public bool HasCollectionSetup => collection != null && (catalog != null ? catalog.IsValid() : catchSpecies != null);

        public void ConfigureCollection(FishCollection ownedFish, FishSpecies species)
        {
            collection = ownedFish;
            catchSpecies = species;
        }

        public void Configure(RectTransform movingIndicator, Text resultText, Button retry)
        {
            indicator = movingIndicator;
            feedback = resultText;
            retryButton = retry;
        }

        private void OnEnable()
        {
            if (indicator == null || feedback == null || retryButton == null)
            {
                Debug.LogError("Timing UI needs setup. Run FISH! > Setup Milestone 1C.", this);
                enabled = false;
                return;
            }
            FishPanelLayout.Fishing(transform, feedback, retryButton);
            dial = GetComponentInChildren<FishFishingDial>(true);
            figmaView = GetComponent<FishFigmaFishing>(); catchRequested = false;
            retryButton.onClick.AddListener(Retry);
            attempt.Begin(Time.frameCount);
            catchAdded = false; chosenSpecies = null; resultSoundPlayed = false;
            ClearSelection();
            RefreshUI();
        }

        // EventSystem processes button clicks in Update. Evaluate afterwards so
        // clicking Close and pressing Space together cancels before any reward.
        private void LateUpdate()
        {
            if (!Application.isFocused) return;
            Keyboard keyboard = Keyboard.current;
            // The spot owns closing; cancellation wins over a simultaneous catch press.
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) return;
            attempt.Tick(Time.unscaledDeltaTime, Time.frameCount,
                catchRequested || (keyboard != null && keyboard.spaceKey.wasPressedThisFrame),
                keyboard != null && keyboard.spaceKey.isPressed);
            catchRequested = false;
            if (!resultSoundPlayed && (attempt.State == FishTimingAttempt.AttemptState.Success || attempt.State == FishTimingAttempt.AttemptState.Failure))
            {
                resultSoundPlayed = true;
                FishPresentation.Play(attempt.State == FishTimingAttempt.AttemptState.Success ? FishSound.FishingSuccess : FishSound.FishingFailure);
            }
            if (!catchAdded && attempt.State == FishTimingAttempt.AttemptState.Success && HasCollectionSetup)
            {
                if (chosenSpecies == null) chosenSpecies = catalog != null ? catalog.Choose() : catchSpecies;
                if (collection.TryAddCatch(attempt, chosenSpecies)) catchAdded = true;
            }
            RefreshUI();
        }

        private void Retry()
        {
            if (!attempt.Retry(Time.frameCount)) return;
            catchAdded = false; chosenSpecies = null; resultSoundPlayed = false;
            ClearSelection();
            RefreshUI();
        }

        private static void ClearSelection()
        {
            // Space is also UI Submit. Do not let a selected Retry/Close button
            // consume the catch key after the next attempt starts.
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        private void RefreshUI()
        {
            Vector2 anchor = new Vector2(attempt.Position, 0.5f);
            indicator.anchorMin = indicator.anchorMax = anchor;
            indicator.anchoredPosition = Vector2.zero;
            if (dial != null) dial.ShowPosition(attempt.Position);
            if (figmaView != null) figmaView.Show(attempt.Position);
            bool resolved = attempt.State == FishTimingAttempt.AttemptState.Success ||
                attempt.State == FishTimingAttempt.AttemptState.Failure;
            retryButton.gameObject.SetActive(resolved);
            // A catch press that doubles as UI Submit cannot immediately retry.
            retryButton.interactable = resolved && Time.frameCount > attempt.ResolvedFrame;

            if (attempt.State == FishTimingAttempt.AttemptState.Success)
            {
                string name = chosenSpecies != null ? chosenSpecies.displayName : "Placeholder Fish";
                feedback.text = "Success! Caught: " + name + ".\n" +
                    (catchAdded ? "Added one fish to your collection." : "Collection not configured; no fish stored.");
                feedback.color = FishUITheme.Success;
            }
            else if (attempt.State == FishTimingAttempt.AttemptState.Failure)
            {
                feedback.text = "Missed! No fish caught.\nChoose Retry or Return to beach.";
                feedback.color = FishUITheme.Failure;
            }
            else
            {
                feedback.text = "Hit the green zone.\nSpace to catch  /  Esc to leave";
                feedback.color = Color.white;
            }
            if (figmaView != null) figmaView.Result(resolved, attempt.State == FishTimingAttempt.AttemptState.Success);
        }

        private void OnDisable()
        {
            // An unresolved cancellation cannot claim a catch. Stored successes remain owned.
            attempt.Cancel();
            if (retryButton != null) retryButton.onClick.RemoveListener(Retry);
        }
    }
}
