using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Text;

namespace Fish
{
    public sealed class FishBattleScene : MonoBehaviour
    {
        [SerializeField] private Text playerInfo;
        [SerializeField] private Text enemyInfo;
        [SerializeField] private Text feedback;
        [SerializeField] private Text result;
        [SerializeField] private Button attack;
        [SerializeField] private Button defend;
        [SerializeField] private Button specialAbility;
        [SerializeField] private Text specialInfo;
        [SerializeField] private Button retry;
        [SerializeField] private Button returnToBeach;
        private FishBattleSession session;
        private FishBattleRules battle => session?.Battle;
        private Action onReturn;
        private bool resolving;
        private GameObject targetPanel;
        private FishBattleRules.BattleAction pendingAction;
        private int inputFrame;
        private bool inputReleased;
        private bool FreshInput => inputReleased && Time.frameCount > inputFrame;
        private void GateInput() { inputReleased = false; inputFrame = Time.frameCount; }
        private bool outcomeSoundPlayed;
        private Text[] memberLabels;
        private FishBattleVisuals visuals;
        private FishSpecies[] playerSpecies;
        private FishSpecies[] enemySpecies;
        private FishBattleSession.SessionPhase displayedPhase;
        public Button SpecialButton => specialAbility;
        public Text SpecialInfo => specialInfo;

        public void ConfigureSpecialUI(Button button, Text info)
        {
            specialAbility = button;
            specialInfo = info;
        }

        private void Awake()
        {
            if (playerInfo == null || enemyInfo == null || feedback == null || result == null ||
                attack == null || defend == null || retry == null || returnToBeach == null)
            {
                Debug.LogError("Battle scene references are missing. Check Milestone 3A setup.", this);
                enabled = false;
                return;
            }
            EnsureSpecialUI();
            FishPanelLayout.Battle(transform, feedback, specialInfo, attack, defend, specialAbility, retry, returnToBeach);
            FishUITheme.Apply(gameObject);
            result.fontSize = 32;
            result.rectTransform.sizeDelta = new Vector2(1150, 56);
            result.rectTransform.anchoredPosition = new Vector2(0, 300);
            var title = transform.Find("Title"); if (title != null) title.gameObject.SetActive(false);
            attack.onClick.AddListener(Attack);
            defend.onClick.AddListener(Defend);
            if (specialAbility != null) specialAbility.onClick.AddListener(Special);
            retry.onClick.AddListener(Retry);
            returnToBeach.onClick.AddListener(Return);
            Text retryLabel = retry.GetComponentInChildren<Text>(true);
            if (retryLabel != null) { retryLabel.text = "Recover and Retry"; retryLabel.fontSize = 20; }
            Text returnLabel = returnToBeach.GetComponentInChildren<Text>(true);
            if (returnLabel != null) returnLabel.text = "Return to Shore";
            feedback.text = "Start from the overworld encounter marker with a confirmed team.";
            Refresh();
        }

        // Older saved battle scenes predate 3C. Fill in only the missing UI,
        // using the existing canvas and EventSystem; never clone action listeners.
        private void EnsureSpecialUI()
        {
            if (specialAbility == null)
            {
                Transform existing = transform.Find("Special Ability");
                if (existing != null) specialAbility = existing.GetComponent<Button>();
                if (specialAbility == null && existing == null)
                {
                    RectTransform rect = CreateSpecialRect("Special Ability", transform,
                        new Vector2(300f, -250f), new Vector2(260f, 60f));
                    Image image = rect.gameObject.AddComponent<Image>();
                    image.color = new Color32(71, 123, 86, 255);
                    specialAbility = rect.gameObject.AddComponent<Button>();
                    specialAbility.targetGraphic = image;
                    CreateSpecialText("Label", rect, "Special Ability", 24,
                        Vector2.zero, new Vector2(250f, 55f));
                    attack.GetComponent<RectTransform>().anchoredPosition = new Vector2(-300f, -250f);
                    defend.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -250f);
                }
            }
            if (specialInfo == null)
            {
                Transform existing = transform.Find("Special Description");
                if (existing != null) specialInfo = existing.GetComponent<Text>();
                if (specialInfo == null && existing == null)
                {
                    specialInfo = CreateSpecialText("Special Description", transform,
                        "Once per fish per battle", 18, new Vector2(0f, -185f), new Vector2(1150f, 45f));
                    feedback.rectTransform.anchoredPosition = new Vector2(0f, -120f);
                    feedback.rectTransform.sizeDelta = new Vector2(1150f, 80f);
                }
            }
            if (specialAbility == null || specialInfo == null)
                Debug.LogError("Conflicting Special Ability or Special Description object. Check the battle UI and run FISH! > Setup Milestone 3C outside Play Mode.", this);
        }

        private static RectTransform CreateSpecialRect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private Text CreateSpecialText(string name, Transform parent, string content, int fontSize, Vector2 position, Vector2 size)
        {
            Text label = CreateSpecialRect(name, parent, position, size).gameObject.AddComponent<Text>();
            label.font = feedback.font != null ? feedback.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = content;
            return label;
        }

        public bool Begin(FishBattleSession encounterSession, Action returnAction, FishSpecies[] players = null, FishSpecies[] enemies = null)
        {
            if (!enabled || session != null || encounterSession == null || returnAction == null || !encounterSession.TryBeginBattle()) return false;
            session = encounterSession;
            playerSpecies = players;
            enemySpecies = enemies;
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                Camera camera = root.GetComponentInChildren<Camera>();
                if (camera == null) continue;
                visuals = gameObject.AddComponent<FishBattleVisuals>();
                visuals.Initialize(camera, transform, playerInfo, enemyInfo);
                break;
            }
            memberLabels = new Text[battle.Players.Count + battle.Enemies.Count];
            for (int i = 0; i < memberLabels.Length; i++)
            {
                var label = CreateSpecialText("Fish name and HP " + i, transform, "", 20, Vector2.zero, new Vector2(270, 88));
                label.fontStyle = FontStyle.Bold;
                label.rectTransform.pivot = new Vector2(.5f, 0);
                label.alignment = TextAnchor.LowerCenter;
                label.gameObject.AddComponent<Shadow>().effectColor = new Color(0, .08f, .1f, .95f);
                memberLabels[i] = label;
            }
            playerInfo.gameObject.SetActive(false); enemyInfo.gameObject.SetActive(false);
            onReturn = returnAction;
            ShowFreshAttempt();
            return true;
        }

        private void ShowFreshAttempt()
        {
            outcomeSoundPlayed = false;
            CloseTargets();
            GateInput();
            if (visuals != null) visuals.BindTeams(battle, playerSpecies, enemySpecies);
            StopAllCoroutines();
            resolving = false;
            feedback.text = "Choose an action.";
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(attack.gameObject);
            Refresh();
        }

        private void Update()
        {
            if (!inputReleased && Time.frameCount > inputFrame &&
                !(Keyboard.current?.enterKey.isPressed ?? false) && !(Keyboard.current?.spaceKey.isPressed ?? false) &&
                !(Gamepad.current?.buttonSouth.isPressed ?? false) && !(Mouse.current?.leftButton.isPressed ?? false)) inputReleased = true;
            if (targetPanel != null && (Keyboard.current?.escapeKey.wasPressedThisFrame ?? false)) CancelTargetSelection();
            if (session == null || session.Phase == displayedPhase) return;
            if (displayedPhase == FishBattleSession.SessionPhase.Returning && session.Phase == FishBattleSession.SessionPhase.Battling)
                feedback.text = "Return could not finish. Try Return to Shore again; check the Console.";
            Refresh();
        }

        private void LateUpdate()
        {
            if (memberLabels == null || visuals == null) return;
            var canvas = GetComponentInParent<Canvas>();
            Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            for (int i = 0; i < memberLabels.Length; i++)
            {
                bool visible = visuals.TryGetLabelPosition(i, out Vector3 screenPosition);
                memberLabels[i].gameObject.SetActive(visible);
                if (visible && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)transform, screenPosition, uiCamera, out Vector2 position))
                    memberLabels[i].rectTransform.anchoredPosition = position;
            }
        }

        private void Attack() => PlayerAction(FishBattleRules.BattleAction.Attack);
        private void Defend() => PlayerAction(FishBattleRules.BattleAction.Defend);
        private void Special() => PlayerAction(FishBattleRules.BattleAction.Special);

        private void PlayerAction(FishBattleRules.BattleAction action)
        {
            if (!FreshInput || targetPanel != null || battle == null || session.Phase != FishBattleSession.SessionPhase.Battling || resolving || battle.State != FishBattleRules.BattleState.PlayerTurn) return;
            if (action == FishBattleRules.BattleAction.Special && !battle.ActivePlayer.CanUseSpecial) return;
            if (action == FishBattleRules.BattleAction.Attack ||
                (action == FishBattleRules.BattleAction.Special && battle.ActivePlayer.SpecialEffect == FishSpecialEffect.StrongerAttack))
            { BeginTargetSelection(action); return; }
            resolving = true;
            Refresh();
            StartCoroutine(ResolveRound(action, -1));
        }

        private void BeginTargetSelection(FishBattleRules.BattleAction action)
        {
            pendingAction = action;
            targetPanel = CreateSpecialRect("Choose Target", transform, new Vector2(0, -290), new Vector2(1000, 92)).gameObject;
            targetPanel.AddComponent<Image>().color = new Color(.03f,.16f,.17f,1);
            var row = targetPanel.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 12; row.padding = new RectOffset(12,12,12,12);
            for (int i = 0; i < battle.Enemies.Count; i++)
            {
                int index = i;
                var fish = battle.Enemies[i];
                var button = CreateTargetButton(fish.Name + "\nHP " + fish.Health + "/" + fish.MaxHealth);
                button.interactable = fish.Health > 0;
                button.onClick.AddListener(() => ConfirmTarget(index));
                var events = button.gameObject.AddComponent<EventTrigger>();
                foreach (var eventType in new[] { EventTriggerType.Select, EventTriggerType.PointerEnter })
                {
                    var entry = new EventTrigger.Entry { eventID = eventType };
                    entry.callback.AddListener(_ => { if (visuals != null && fish.Health > 0) visuals.HighlightTarget(index); });
                    events.triggers.Add(entry);
                }
            }
            CreateTargetButton("Back").onClick.AddListener(CancelTargetSelection);
            feedback.text = "Choose a target · Esc to cancel";
            GateInput();
            Refresh();
            Button first = null;
            foreach (var candidate in targetPanel.GetComponentsInChildren<Button>()) if (candidate.interactable) { first = candidate; break; }
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(first.gameObject);
        }
        private Button CreateTargetButton(string label)
        {
            var rect = CreateSpecialRect(label, targetPanel.transform, Vector2.zero, new Vector2(240,90));
            var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.12f,.38f,.32f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var text = CreateSpecialText("Label", rect, label, 20, Vector2.zero, Vector2.zero);
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(8,8); text.rectTransform.offsetMax = new Vector2(-8,-8);
            return button;
        }
        private void ConfirmTarget(int index)
        {
            if (!FreshInput || resolving || targetPanel == null || battle.Enemies[index].Health == 0) return;
            resolving = true;
            var action = pendingAction;
            CloseTargets();
            Refresh();
            StartCoroutine(ResolveRound(action, index));
        }
        private void CloseTargets()
        {
            if (targetPanel != null) { targetPanel.SetActive(false); Destroy(targetPanel); targetPanel = null; }
            if (visuals != null) visuals.HighlightTarget(-1);
        }
        private void CancelTargetSelection() { CloseTargets(); GateInput(); Refresh(); FocusAction(); }
        private IEnumerator ResolveRound(FishBattleRules.BattleAction action, int targetIndex)
        {
            if (session.TryPlayerAction(action, targetIndex, out string message))
            {
                feedback.text = message;
                if (session.LastAction.Offensive) FishPresentation.Play(FishSound.Attack);
                if (visuals != null) yield return visuals.PlayAction(session.LastAction);
                if (visuals != null) visuals.RefreshFormation(battle);
                Refresh();
                if (!battle.IsFinished && session.TryEnemyTurn(out message))
                {
                    feedback.text = message;
                    if (session.LastAction.Offensive) FishPresentation.Play(FishSound.Attack);
                    if (visuals != null) yield return visuals.PlayAction(session.LastAction);
                    if (visuals != null) visuals.RefreshFormation(battle);
                }
            }
            resolving = false;
            GateInput();
            Refresh();
            FocusAction();
        }
        private void FocusAction()
        {
            if (EventSystem.current == null) return;
            Button choice = battle.IsFinished ? (battle.State == FishBattleRules.BattleState.Defeat ? retry : returnToBeach) : attack;
            EventSystem.current.SetSelectedGameObject(choice.gameObject);
        }
        private void Retry()
        {
            if (session == null || resolving || !session.TryRetry()) return;
            ShowFreshAttempt();
        }

        private void Return()
        {
            if (session == null || resolving || onReturn == null || !session.TryBeginReturn()) return;
            feedback.text = "Returning to Shore...";
            Refresh();
            onReturn();
        }

        private void Refresh()
        {
            if (battle != null && battle.IsFinished && !resolving && !outcomeSoundPlayed)
            {
                outcomeSoundPlayed = true;
                FishPresentation.Play(battle.State == FishBattleRules.BattleState.Victory ? FishSound.Victory : FishSound.Defeat);
                if (battle.State == FishBattleRules.BattleState.Defeat)
                    feedback.text = "Recover and Retry heals your team free. Return keeps remaining HP.";
            }
            bool ready = battle != null && targetPanel == null && !resolving && session.Phase == FishBattleSession.SessionPhase.Battling;
            if (session != null) displayedPhase = session.Phase;
            attack.interactable = defend.interactable = ready && battle.State == FishBattleRules.BattleState.PlayerTurn;
            FishBattleFighter active = battle?.ActivePlayer;
            if (specialAbility != null)
                specialAbility.interactable = ready && battle.State == FishBattleRules.BattleState.PlayerTurn && active != null && active.CanUseSpecial;
            if (specialInfo != null)
                specialInfo.text = active == null || active.SpecialEffect == FishSpecialEffect.None ? "No special ability available." :
                    active.SpecialName + " · " + active.SpecialDescription + (active.SpecialUsed ? " (Used)" : " (Once per battle)");
            attack.transform.parent.gameObject.SetActive(targetPanel == null && (battle == null || !battle.IsFinished));
            retry.transform.parent.gameObject.SetActive(battle != null && battle.IsFinished && !resolving);
            specialInfo.transform.parent.gameObject.SetActive(targetPanel == null && battle != null && !battle.IsFinished);
            retry.interactable = ready && battle.State == FishBattleRules.BattleState.Defeat;
            returnToBeach.interactable = ready && battle.IsFinished && onReturn != null;
            retry.gameObject.SetActive(battle != null && battle.State == FishBattleRules.BattleState.Defeat);
            returnToBeach.gameObject.SetActive(battle != null && battle.IsFinished);
            if (battle == null) { playerInfo.text = "Player"; enemyInfo.text = "Enemy"; result.text = "No encounter loaded"; return; }
            if (memberLabels != null)
                for (int i = 0; i < memberLabels.Length; i++)
                {
                    bool playerSide = i < battle.Players.Count;
                    int slot = playerSide ? i : i - battle.Players.Count;
                    var fish = playerSide ? battle.Players[slot] : battle.Enemies[slot];
                    bool acting = playerSide ? battle.State == FishBattleRules.BattleState.PlayerTurn && slot == battle.PlayerIndex : battle.State == FishBattleRules.BattleState.EnemyTurn && slot == battle.EnemyIndex;
                    memberLabels[i].text = fish.Name + "\n" + fish.Health + " / " + fish.MaxHealth +
                        (acting ? "  •" : fish.IsDefending ? "  Guard" : "");
                    memberLabels[i].color = acting ? new Color(1,.85f,.3f) : Color.white;
                }
            playerInfo.text = TeamText(battle.Players, battle.PlayerIndex, battle.State == FishBattleRules.BattleState.PlayerTurn);
            enemyInfo.text = TeamText(battle.Enemies, battle.EnemyIndex, battle.State == FishBattleRules.BattleState.EnemyTurn);
            result.text = battle.State == FishBattleRules.BattleState.Victory ? "Victory!" :
                battle.State == FishBattleRules.BattleState.Defeat ? "Defeat" :
                resolving ? "Resolving action..." : "Your turn";
            result.color = battle.State == FishBattleRules.BattleState.Victory ? FishUITheme.Success :
                battle.State == FishBattleRules.BattleState.Defeat ? FishUITheme.Failure : Color.white;
        }

        private static string TeamText(System.Collections.Generic.IReadOnlyList<FishBattleFighter> team, int actor, bool turn)
        {
            var text = new StringBuilder();
            for (int i = 0; i < team.Count; i++)
                text.AppendLine((turn && i == actor ? "> " : "") + team[i].Name + "  " + team[i].Health + "/" + team[i].MaxHealth +
                    (team[i].Health == 0 ? " Defeated" : team[i].IsDefending ? " Defending" : ""));
            return text.ToString();
        }
        private static string FighterText(FishBattleFighter fish, int index, int count) => fish == null
            ? "All fish defeated"
            : fish.Name + "\nFish " + (index + 1) + " / " + count + "    HP " + fish.Health + " / " + fish.MaxHealth +
                (fish.IsDefending ? "\nDefending" : "");

        private void OnEnable()
        {
            if (session == null || battle == null) return;
            if (battle.State == FishBattleRules.BattleState.EnemyTurn && session.Phase == FishBattleSession.SessionPhase.Battling)
                session.TryEnemyTurn(out _);
            resolving = false;
            if (visuals != null) { visuals.HighlightTarget(-1); visuals.RefreshFormation(battle); }
            GateInput(); Refresh(); FocusAction();
        }
        private void OnDisable()
        {
            CloseTargets();
            StopAllCoroutines();
            resolving = false;
        }

        private void OnDestroy()
        {
            if (attack != null) attack.onClick.RemoveListener(Attack);
            if (defend != null) defend.onClick.RemoveListener(Defend);
            if (specialAbility != null) specialAbility.onClick.RemoveListener(Special);
            if (retry != null) retry.onClick.RemoveListener(Retry);
            if (returnToBeach != null) returnToBeach.onClick.RemoveListener(Return);
        }
    }
}
