# Water arena and alternating fish battles

Approved by the user — 2026-11-16. Implementation pending.

## Intent and agreed direction

Present the selected fish as a facing formation in a full 3D water arena. Give each surviving fish its own turn and let the player choose the enemy it attacks. Use the requested Pokémon Scarlet/Violet reference for scene framing and action emphasis.

The user chose alternating individual turns between sides, player-selected targets, and a focused extension of the existing battle system.

## Approaches considered

- Extend FishBattleRules, FishBattleSession and their existing presentation. Reuse owned-fish HP, abilities, encounter transitions and saves. This is the selected approach and limits migration work.
- Introduce a Speed-based initiative framework. This offers broader scheduling options but adds balancing and migration work. The selected alternating order already defines the needed scheduling.

## Existing project and design requirements

The root FISH_Game_Design_Document.md section 6 currently specifies one active fish on each side and replacement after defeat. The approved implementation of this proposal will supersede those formation and targeting rules. The GDD must receive a focused update when this design is approved.

FishBattleRules currently advances indices only on defeat. FishBattleSession owns attempt snapshots, one-time HP commitment, retry and victory callbacks. FishBattleScene handles action locking, feedback and UI. FishBattleVisuals renders two fish into a narrow RenderTexture preview. FishBattleEncounter owns scene transitions and owned-catch mapping.

The current persistent HP and once-per-battle ability requirements remain acceptance constraints. Fishing and story-triggered encounters continue to use their existing entry points. Existing species IDs, catch IDs, save format, selected team order and progression state remain compatible.

## Approved defaults

These implementation choices were proposed in the design and included in the user's approval.

- Teams continue to support one to three selected fish. Existing smaller enemy encounters and the single boss remain valid. Three-fish teams display all three members.
- Use shallow, clear coastal water with fish visible just beneath the surface and an elevated diagonal camera. Water opacity and framing prioritize readable silhouettes. This is a compact battle stage.
- Each side cycles independently through its surviving fish. A smaller team cycles more frequently because sides always alternate. One enemy can therefore act between every player action.
- Enemy offensive actions target the next player fish due to act. Keep the existing Attack/Attack/Defend action pattern and configured boss special restriction.
- Keep each member's original formation slot after defeat. Remove its model and show its HUD entry as defeated. Survivors stay in their established positions.

## Turn scheduling

The first surviving player fish acts first. Each side has a cursor pointing to its next living actor. After an accepted action, advance that side's cursor once, wrapping through its original team order and skipping zero-HP members. Hand control to the other side, resolving its next living actor from its own cursor.

For full teams, the opening order is P1, E1, P2, E2, P3, E3. If E2 is defeated by P1, the next enemy actor is E1 and its following actor is E3. If a pending actor is defeated, skip it before granting its turn.

The acting fish is fixed for that turn. Target selection never changes the acting fish or consumes a turn. Check victory or defeat immediately after damage. When a team has no survivors, end the attempt before scheduling further actions.

Call BeginTurn exactly once when a fish receives its turn. UI refreshes and target selection must never expire Defend. A guard lasts through all incoming attacks until that defender's next turn, does not stack, and is cleared when the battle ends. Per-fish special availability resets at each new attempt.

## Actions and target selection

- Attack and stronger-attack Special enter target selection. Show selectable living enemy cards with corresponding world highlights. Clicking a card confirms and resolves that action.
- Defend and self-heal Special act immediately on the current actor.
- Escape or a Back control cancels target selection and returns to action selection without changing HP, consuming a turn, or spending a special.
- Keep the existing InputSystemUIInputModule. Mouse and UI keyboard navigation must select the same targets. World models are highlighted through the selected card, avoiding extra physics input handlers.
- Distinguish actor and target with labels and markers as well as color. Keep names and current/max HP visible.
- Validate side, actor health, target membership, target health and special availability before accepting an action. Invalid commands leave every field and cursor unchanged.
- Once accepted, lock actions and targeting synchronously. One command commits one outcome. Discard repeated submissions from the same frame/interaction until presentation has completed and a fresh input can begin.

Keep the existing damage formula, max(1, Attack + offensive bonus - Defense). Defend halves the resulting damage with upward rounding and a minimum of one. Self-healing is capped at maximum HP. Each fish has its own once-per-battle special usage.

## Presentation and HUD

Use the existing battle scene camera for a perspective overview. Position opposing rows facing inward, with sufficient separation and staggered screen-space placement to see all members. Fit imported models and procedural fallbacks inside formation bounds. Keep the larger boss framed safely.

Reuse FishBattleVisuals as the read-only presentation owner. Replace its preview rendering with the world arena and per-member model holders. Reuse FishPrimitiveVisual and species.model references. Visual construction reads fighter state and never initializes HP or changes data.

Use a brief swim-forward-and-return movement for offensive actions. Defend and healing use simple highlight feedback. Resolve rules once, then animate the recorded actor, target and outcome. The presentation may display the recorded pre-hit and post-hit HP at the impact beat. It never reapplies damage. Keep input locked through enemy action playback. A terminal hit finishes its short presentation before result controls appear.

The HUD uses the established tropical palette and the approved bold, compact direction. Show up to three member cards per side, actor name/turn indicator, current/max HP, short feedback and Attack/Defend/Special controls. Target mode replaces the action prompt with a clear target instruction and Back. Long names wrap within cards. Use icons alongside action labels, restrained borders and clear disabled states.

Reuse uGUI, CanvasScaler, existing theme helpers and the active EventSystem. Keep action controls within a 24-reference-pixel safe inset. Validate 1920x1080, 1280x720 and 800x450. Keep the center available for the arena and avoid covering fish with floating labels. The existing Figma battle frame requires adaptation during presentation implementation.

## Health, results and scene lifecycle

Initialize players from stored individual current HP. Exclude zero-HP fish from acting and targeting while retaining stable member identity for result commitment. Enemy health starts from encounter data per attempt.

Commit each player's final HP by exact owned-catch ID through the existing one-time result path. Self-healing is included. Award victory progression once. Save only after committed terminal results through the existing safe-point integration.

Recover and Retry heals the captured selected team through existing recovery, resets both turn cursors, player-first ordering, guards, special usage, pending targeting and presentation. Enemy state resets from the encounter snapshot. Return to Shore preserves remaining HP and restores the existing safe return position, input, cursor and UI.

Unload/retry cleans up model instances, runtime materials, highlights and action playback. Ensure one active camera/listener/EventSystem for the battle and correct restoration afterward. Close/repeated transition events cannot apply damage, HP commitment or victory twice.

## Implementation boundaries and source ownership

- FishBattleRules.cs adds independent cyclic actor selection and explicit offensive targets. Audit every caller of ActivePlayer/ActiveEnemy and the action API so actor scheduling and target selection have clear meanings.
- FishBattleSession.cs passes target identity, chooses enemy targets, and retains attempt/result ownership.
- FishBattleScene.cs handles action/target selection, fresh-input gating, recorded action playback and results.
- FishBattleVisuals.cs manages formation slots, camera framing, water-stage visuals and actor/target emphasis.
- FishPanelLayout.cs and FishUITheme.cs supply focused battle layout/style updates.
- Assets/Editor/FishBattleSetup.cs and connected battle setup tools gain a repeatable upgrade path for existing battle scenes. Preserve manual scene edits. Reuse existing cameras, lights, materials and UI references, create only missing owned objects, and leave scene saving to the user.
- FishBattleEncounter.cs, owned data, save and progression receive changes only where their existing interfaces require adaptation.

Use the current URP pipeline, new Input System and existing assets. Scope excludes new battle mechanics beyond rotation/targeting, new status types, Speed initiative, area attacks, free swimming, packages and asset downloads.

## Verification plan

Extend existing standalone battle/session/special/health checks for the specific changed rules. Cover 3v3 order, 1v3 and 3v1 cycling, defeated pending actors, arbitrary-target defeat, all-dead terminal detection, invalid target rejection without mutation, cancellation, Defend across several incoming attacks, independent same-species fighters, healing cap and per-fish special reset.

Verify results and retries commit exact owned IDs once, preserve partial HP across consecutive battles, and retain existing progression/save safe points. Run connected save round trips with disposable files. Audit previous tests for assumptions about replacement-only actors before updating their expected behavior.

Compile runtime and Editor source. In Unity verify three models per full team, correct facing and slot mapping, targeting via mouse/keyboard, spam and held-input behavior, animation interruption, model cleanup, transparent-water readability, boss framing, all resolutions, repeat setup, retries and repeated shore returns. Use the existing Editor instance or available connected tools.

## Delivery sequence after approval

1. Extend rules and session targeting with regression checks.
2. Adapt battle UI and input to actor/target selection.
3. Integrate the water stage, formations and short action presentation.
4. Upgrade setup safely, exercise persistence/transitions, and document manual acceptance.

The implementation plan follows design approval. This document records source inspection and proposed behavior. Unity visual and gameplay verification is pending.
