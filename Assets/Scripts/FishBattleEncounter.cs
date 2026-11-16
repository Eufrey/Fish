using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fish
{
    // F is independent of fishing's E. The earlier execution order claims input
    // before exploration panels when keys are pressed together.
    [DefaultExecutionOrder(-130)]
    public sealed class FishBattleEncounter : MonoBehaviour
    {
        public const string BattleScenePath = "Assets/Scenes/FishBattle.unity";
        [SerializeField] private FishBattleTeam team;
        [SerializeField] private FishEnemyTeam enemyTeam;
        [SerializeField, Range(1, 3)] private int storyEncounter = 1;
        public int StoryEncounter => storyEncounter;
        private FishStoryProgress Story => team.Collection.Story;
        [SerializeField] private FishExplorationPlayer player;
        [SerializeField] private FishOrbitCamera orbitCamera;
        [SerializeField] private FishFishingSpot fishingSpot;
        [SerializeField] private FishCollectionPanel collectionPanel;
        [SerializeField] private FishTeamPanel teamPanel;
        [SerializeField] private Text prompt;
        [SerializeField] private GameObject markerVisual;
        [SerializeField] private float interactionRadius = 2.5f;
        [SerializeField] private Transform shoreReturnPoint;
        [SerializeField] private Collider shoreGround;
        private readonly List<GameObject> suspendedRoots = new List<GameObject>();
        private static FishBattleEncounter activeEncounter;
        public static bool HasActiveEncounter => activeEncounter != null;
        public static bool ResultReady => activeEncounter != null && activeEncounter.session != null && activeEncounter.session.Phase == FishBattleSession.SessionPhase.Battling && activeEncounter.session.Battle != null && activeEncounter.session.Battle.IsFinished;
        private FishBattleSession session;
        private bool inBattle => session != null && session.Phase != FishBattleSession.SessionPhase.Closed;
        private bool returning;
        private int resumedFrame = -1;
        private FishStoryHUD storyUI;
        private bool introductionConfirmed;
        private Vector3 entryPosition;
        private Quaternion entryRotation;
        private bool previousMovementInput;
        private bool previousCameraInput;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;
        private bool previousMarkerActive;
        private Scene overworld;
        private Scene battleScene;
        public Transform ShoreReturnPoint => shoreReturnPoint;
        public Collider ShoreGround => shoreGround;

        public void ConfigureShoreReturn(Transform point, Collider ground)
        {
            shoreReturnPoint = point;
            shoreGround = ground;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEncounterLock() => activeEncounter = null;

        private void OnEnable()
        {
            if (team == null || team.Collection == null || enemyTeam == null || player == null || orbitCamera == null ||
                fishingSpot == null || collectionPanel == null || teamPanel == null || prompt == null || markerVisual == null ||
                shoreReturnPoint == null || shoreGround == null)
            {
                Debug.LogError("Encounter or shore return references are missing. Check FISH! > Setup Milestone 3B.", this);
                enabled = false;
                return;
            }
            prompt.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (inBattle || Time.frameCount == resumedFrame) return;
            if (activeEncounter != null) { prompt.gameObject.SetActive(false); return; }
            markerVisual.SetActive(Story.CanEnter(storyEncounter));
            if (!Story.CanEnter(storyEncounter)) { prompt.gameObject.SetActive(false); return; }
            bool available = player.ExplorationInputEnabled && orbitCamera.ExplorationInputEnabled &&
                !fishingSpot.BlocksCollection && !collectionPanel.BlocksFishing && !teamPanel.BlocksOtherPanels &&
                (player.transform.position - transform.position).sqrMagnitude <= interactionRadius * interactionRadius;
            prompt.gameObject.SetActive(available);
            if (!available) return;
            bool confirmed = team.Selection.IsConfirmed && team.Selection.CanConfirm && team.HasUsableFish;
            prompt.text = introductionConfirmed && confirmed ? "Press F: " + Story.Objective : "Press F: encounter introduction";
            Keyboard keyboard = Keyboard.current;
            if (Application.isFocused && keyboard != null && keyboard.fKey.wasPressedThisFrame && !keyboard.escapeKey.wasPressedThisFrame)
            {
                if (introductionConfirmed && confirmed) StartCoroutine(EnterBattle());
                else
                {
                    if (storyUI == null) storyUI = FindFirstObjectByType<FishStoryHUD>();
                    if (storyUI != null) storyUI.ShowEncounter(storyEncounter, () => introductionConfirmed = true);
                    else Debug.LogWarning("Story HUD missing. Run Milestone 4 setup before story encounters.", this);
                }
            }
        }

        private IEnumerator EnterBattle()
        {
            if (!Story.CanEnter(storyEncounter) || inBattle || activeEncounter != null || SceneManager.GetSceneByPath(BattleScenePath).isLoaded ||
                !team.Selection.IsConfirmed || !team.Selection.CanConfirm || !team.HasUsableFish) yield break;
            if (!Application.CanStreamedLevelBeLoaded(BattleScenePath) || !enemyTeam.TryCreateFighters(out var enemies))
            {
                Debug.LogError("The battle scene must be enabled in the build scene list and the enemy team must contain species.", this);
                yield break;
            }
            var players = new FishBattleFighter[team.Selection.Members.Count];
            var playerSpecies = new FishSpecies[players.Length];
            var enemySpecies = (FishSpecies[])enemyTeam.members.Clone();
            for (int i = 0; i < players.Length; i++)
            {
                FishOwnedCatch owned = team.GetMember(i);
                if (owned == null || owned.Species == null) yield break;
                FishSpecies species = owned.Species;
                playerSpecies[i] = species;
                players[i] = new FishBattleFighter(species.displayName, owned.MaximumHP, species.attack, species.defense, owned.CatchId,
                    species.specialAbilityType, species.specialAbilityPower, species.specialAbilityName, species.specialAbilityDescription, owned.CurrentHP);
            }

            int encounterId = storyEncounter;
            session = new FishBattleSession(encounterId.ToString(), players, enemies,
                id => { if (id == encounterId.ToString()) Story.ApplyVictory(encounterId); }, enemyTeam.useSpecialAtHalfHealth);
            session.CommitHealth = fighters => {
                foreach (var fighter in fighters)
                    foreach (var owned in team.Collection.Catches)
                        if (owned.CatchId == fighter.OwnedCatchId) owned.SetHealth(fighter.Health);
            };
            var selectedCatches = new List<FishOwnedCatch>();
            for (int i = 0; i < team.Selection.Members.Count; i++) selectedCatches.Add(team.GetMember(i));
            session.RecoverTeam = () => { foreach (var owned in selectedCatches) owned.Recover(); };
            activeEncounter = this;
            returning = false;
            overworld = gameObject.scene;
            entryPosition = player.transform.position;
            entryRotation = player.transform.rotation;
            previousMovementInput = player.ExplorationInputEnabled;
            previousCameraInput = orbitCamera.ExplorationInputEnabled;
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            previousMarkerActive = markerVisual.activeSelf;
            player.SetExplorationInputEnabled(false);
            orbitCamera.SetExplorationInputEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            markerVisual.SetActive(false);
            prompt.gameObject.SetActive(false);

            // Keep this encounter root alive to coordinate the additive load/unload.
            // Disabling the other roots avoids duplicate cameras/EventSystems while
            // retaining their session collection/team objects in the original scene.
            suspendedRoots.Clear();
            foreach (GameObject root in overworld.GetRootGameObjects())
            {
                if (root == transform.root.gameObject || !root.activeSelf) continue;
                suspendedRoots.Add(root);
                root.SetActive(false);
            }
            AsyncOperation loading = null;
            try { loading = SceneManager.LoadSceneAsync(BattleScenePath, LoadSceneMode.Additive); }
            catch (Exception exception) { Debug.LogException(exception, this); }
            if (loading == null)
            {
                battleScene = SceneManager.GetSceneByPath(BattleScenePath);
                session.TryBeginReturn(true);
                yield return ExitBattle();
                yield break;
            }
            yield return loading;
            battleScene = SceneManager.GetSceneByPath(BattleScenePath);
            var controllers = new List<FishBattleScene>();
            if (battleScene.IsValid())
            {
                SceneManager.SetActiveScene(battleScene);
                foreach (GameObject root in battleScene.GetRootGameObjects())
                {
                    controllers.AddRange(root.GetComponentsInChildren<FishBattleScene>(true));
                }
            }
            if (controllers.Count != 1 || !controllers[0].Begin(session, () => StartCoroutine(ExitBattle()), playerSpecies, enemySpecies))
            {
                Debug.LogError("Battle scene needs exactly one valid controller; returning to the overworld.", this);
                session.TryBeginReturn(true);
                yield return ExitBattle();
            }
        }

        private IEnumerator ExitBattle()
        {
            if (returning || session == null || session.Phase != FishBattleSession.SessionPhase.Returning) yield break;
            returning = true;
            if (overworld.IsValid() && overworld.isLoaded) SceneManager.SetActiveScene(overworld);
            if (battleScene.IsValid() && battleScene.isLoaded)
            {
                AsyncOperation unloading = null;
                try { unloading = SceneManager.UnloadSceneAsync(battleScene); }
                catch (Exception exception) { Debug.LogException(exception, this); }
                if (unloading != null) yield return unloading;
                if (battleScene.isLoaded)
                {
                    // Never reactivate a second camera/EventSystem while battle is still loaded.
                    if (battleScene.isLoaded) SceneManager.SetActiveScene(battleScene);
                    session.CancelFailedReturn();
                    returning = false;
                    Debug.LogError("Battle scene did not unload. Return can be attempted again; overworld remains suspended.", this);
                    yield break;
                }
            }
            RestoreOverworld();
        }

        private void RestoreOverworld()
        {
            foreach (GameObject root in suspendedRoots)
                if (root != null) root.SetActive(true);
            suspendedRoots.Clear();
            if (markerVisual != null) markerVisual.SetActive(previousMarkerActive);
            Physics.SyncTransforms();
            if (player != null)
            {
                Vector3 destination = entryPosition;
                Quaternion rotation = entryRotation;
                if (session != null && session.Attempt > 0)
                {
                    rotation = Quaternion.Euler(0f, shoreReturnPoint.eulerAngles.y, 0f);
                    if (!TryFindShorePosition(rotation, out destination))
                    {
                        destination = entryPosition;
                        rotation = entryRotation;
                        Debug.LogWarning("Nearby shore points are blocked. Player kept at the original encounter entry position.", this);
                    }
                }
                player.TeleportTo(destination, rotation);
            }
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            if (orbitCamera != null) orbitCamera.SnapToTarget();
            if (player != null) player.SetExplorationInputEnabled(previousMovementInput);
            if (orbitCamera != null) orbitCamera.SetExplorationInputEnabled(previousCameraInput);
            Cursor.lockState = Application.isFocused ? previousCursorLock : CursorLockMode.None;
            Cursor.visible = !Application.isFocused || previousCursorVisible;
            session?.CompleteReturn();
            session = null;
            battleScene = default;
            if (activeEncounter == this) activeEncounter = null;
            resumedFrame = Time.frameCount;
            returning = false;
        }

        private bool TryFindShorePosition(Quaternion rotation, out Vector3 position)
        {
            CharacterController body = player.GetComponent<CharacterController>();
            Vector3[] candidates = { shoreReturnPoint.position, transform.position + Vector3.back * 3f,
                transform.position + Vector3.right * 3f, transform.position + Vector3.left * 3f,
                transform.position + Vector3.forward * 3f, entryPosition };
            Vector3 scale = player.transform.lossyScale;
            float radius = body.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float halfHeight = Mathf.Max(radius, body.height * Mathf.Abs(scale.y) * 0.5f);
            Vector3 centerOffset = rotation * Vector3.Scale(body.center, scale);
            foreach (Vector3 candidate in candidates)
            {
                if (!shoreGround.Raycast(new Ray(candidate + Vector3.up * 4f, Vector3.down), out RaycastHit hit, 12f) || hit.normal.y < 0.7f) continue;
                Vector3 destination = hit.point + Vector3.up * (halfHeight - centerOffset.y + body.skinWidth + 0.02f);
                Vector3 center = destination + centerOffset;
                Vector3 axis = Vector3.up * (halfHeight - radius);
                bool blocked = false;
                foreach (Collider obstacle in Physics.OverlapCapsule(center - axis, center + axis, radius + body.skinWidth, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (obstacle == shoreGround || obstacle.transform.IsChildOf(player.transform)) continue;
                    blocked = true;
                    break;
                }
                if (blocked) continue;
                position = destination;
                return true;
            }
            position = entryPosition;
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}



