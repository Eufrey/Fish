# Water Lineup Battle Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal** — Extend FISH!'s existing battle system with alternating fish turns, selectable enemy targets and opposing formations in a full 3D coastal water arena.

**Architecture** — FishBattleRules remains the authority for damage, actor rotation and terminal results. FishBattleSession retains attempt snapshots, HP commitment and progression callbacks. The scene controller owns input and recorded action playback, while FishBattleVisuals renders formations without mutating gameplay state.

**Tech Stack** — Unity 6000.6.3f1, C#, URP, uGUI, new Input System, existing .NET 10 standalone checks.

**Spec** — `Docs/superpowers/specs/2026-11-16-water-lineup-battle-design.md`, approved 2026-11-16. Read both documents before execution.

## Global Constraints

- Player first, alternating sides after each accepted action, independent cyclic order per side, skip zero-HP fish.
- Player teams remain one to three fish. Preserve existing data-defined enemy team sizes, including the single boss.
- Damage is max(1, Attack + offensive bonus - Defense). Guard halves upward with minimum one until the defender's next individual turn.
- One special use per individual fish per attempt. Keep HP commitment, recovery, stable IDs and save/progression ownership in existing systems.
- Use current URP, Input System and assets. Keep scene changes repeatable and preserve manual edits.
- Fishing retains its independent collection flow. Existing save format and settings remain compatible.
- Verify 1920x1080, 1280x720 and 800x450 with 24 reference-pixel safe insets.
- Source inspection found no project AGENTS.md and no .git directory. Recheck at execution. Follow `.cursor/rules/fish-workflow.mdc`. Use repository commits only if a repository is available and authorized.

## Review Focus

- Identical species in separate owned catches must retain separate slots, HP, special usage and save identities. Task 2 checks this.
- A defeated next actor must be skipped without skipping another survivor or clearing a guard early. Task 1 checks this.
- Held Submit, rapid clicks and Escape plus Submit cannot consume two actions or select a target with the opening input. Task 3 checks this.
- A terminal hit followed by scene disable, retry or failed return must retain one committed result and clean up presentation. Tasks 4 and 6 check this.
- Large imported meshes, missing models, long localized names and legacy scene layouts must fit safely and rerun setup cleanly. Tasks 4 and 5 check this.

## Files and responsibilities

Modify `Assets/Scripts/FishBattleRules.cs`, `FishBattleSession.cs`, `FishBattleScene.cs`, `FishBattleVisuals.cs` for rules, session, input and presentation respectively. Add a small immutable `Assets/Scripts/FishBattleActionResult.cs` for recorded action data. Adapt `FishPanelLayout.cs` and, only where needed, `FishUITheme.cs` for battle layout.

Extend `Assets/Editor/FishBattleSetup.cs` with an existing-scene upgrade command. Inspect `FishBattleSpecialSetup.cs` and `FishPresentationSetup.cs` for compatible reruns. Modify their battle-specific paths only when required. Inspect `FishBattleEncounter.cs` and `FishSave.cs` integration, preserving their existing ownership.

Extend `Tests/FishBattleChecks.cs`, `FishBattleSessionChecks.cs`, `FishSpecialChecks.cs`, `FishHealthChecks.cs` and their explicit Compile lists when necessary. Reuse `FishSaveChecks.cs` and `FishStoryChecks.cs` for connected regressions. Add setup/testing instructions in `Docs/Water_Lineup_Battle_Setup.md` at delivery and update `Docs/PROJECT_STATUS.md` with actual results.

## Task 1 — Cyclic actors and explicit targets

Interfaces in `FishBattleRules.cs`

- Add `TryAct(bool isPlayer, BattleAction action, int targetIndex, out string message)`.
- Preserve `TryAct(bool isPlayer, BattleAction action, out string message)` as a compatibility wrapper targeting the opposing next actor for offensive actions.
- Retain `PlayerIndex`, `EnemyIndex`, `ActivePlayer`, `ActiveEnemy`. Each denotes its side's next living actor, normalized after every accepted action. A terminal empty side returns a null active fighter.
- Use targetIndex -1 for self-actions. Validate an offensive target against the opposing list, independent of that side's actor cursor.

- [ ] Add failing checks in `FishBattleChecks.cs` using 100-HP, 1-Attack, 0-Defense fighters. Capture actor names before each action and assert P1,E1,P2,E2,P3,E3,P1. Assert 1v3 produces P1,E1,P1,E2,P1,E3 and 3v1 produces P1,E1,P2,E1,P3,E1.
- [ ] Add arbitrary-target KO checks. Killing E2 while E1 is pending leaves E1 next, then E3. Killing the pending E1 chooses E2. Killing the final enemy ends immediately. Zero-HP starting players are skipped.
- [ ] Add invalid-target checks for -1, count, and a defeated index on offensive actions. Assert false and unchanged HP, guards, special usage, state and cursors. Self-actions accept -1.
- [ ] Run `dotnet run --project Tests/FishBattleChecks.csproj`. Record the expected new-rule failures before implementation.
- [ ] Implement cyclic rotation bounded by each list's count. Normalize the other side after targeted damage. Call BeginTurn only on entry to the newly granted turn. Preserve the existing damage routine and immediate terminal detection.
- [ ] Add a guard check where P1 defends and multiple enemies target P1 before its next turn. Assert reduced damage for every hit and expiration exactly on P1's next turn. Update replacement-only assertions that the approved design supersedes.
- [ ] Run battle and special projects. Require exit code zero and their reported passed-check totals. Review the change before moving on.

## Task 2 — Recorded actions and session integrity

Interfaces

- Create immutable `FishBattleActionResult` with ActorIsPlayer, ActorIndex, TargetIsPlayer, TargetIndex, Action, HealthBefore, HealthAfter and Message properties. The target is the actor for self-actions. Keep stable list indices and snapshot primitive values.
- Add `FishBattleSession.LastAction` of that type, null before the first action or after retry.
- Add `TryPlayerAction(FishBattleRules.BattleAction action, int targetIndex, out string message)`. Keep the old overload for existing callers. `TryEnemyTurn(out string message)` retains its signature and targets Battle.PlayerIndex.
- Capture actor and target before TryAct rotates cursors. Set LastAction only for accepted commands. Result commitment continues through the current CommitResult path.

- [ ] Add session tests asserting a P1-to-E3 attack records indices 0 and 2 even when P2 becomes next. Rejected commands leave LastAction and attempt state unchanged.
- [ ] Add tests for two catches of the same species with different owned IDs and initial HP. Assert separate HP and specials, correct exact-ID commitment once on victory/defeat, and unchanged source snapshots.
- [ ] Add retry assertions for player-first order, both cursor resets, cleared guards/LastAction, restored selected HP and fresh specials. Assert enemy AI's every-third-action Defend and configured half-health special still operate on the current enemy actor.
- [ ] Add the action-result source to every test project compiling FishBattleSession. Run `dotnet run --project Tests/FishBattleSessionChecks.csproj` to establish failures.
- [ ] Implement session overloads and snapshot capture. Increment EnemyTurns only after an accepted action. Keep existing victory callback and terminal HP commitment ordering.
- [ ] Run session, special and health projects. Inspect FishBattleEncounter's species-array ordering against session member indices and exact owned IDs. Adjust only a demonstrated mismatch.

## Task 3 — Action and target selection

Interfaces in `FishBattleScene.cs`

- Add private `BeginTargetSelection(FishBattleRules.BattleAction action)`, `ConfirmTarget(int enemyIndex)`, and `CancelTargetSelection()` methods.
- Adapt `ResolveRound` to accept action and targetIndex and consume session.LastAction after each accepted player/enemy command.
- Use explicit controller UI phases for action choice, target choice, resolving and result. SessionPhase and BattleState remain authoritative for permission to act.

- [ ] Capture the current action-entry and listener wiring. Define manual failing cases for selecting Attack, canceling, and targeting a non-active enemy against the existing scene.
- [ ] Add living enemy target buttons in the existing Canvas with name/current-max HP. Use the existing InputSystemUIInputModule for click, navigation and Submit. Target hover/selection later calls the visuals highlight API from Task 4.
- [ ] Route Attack/offensive Special to target choice. Route Defend/self-heal with targetIndex -1. Escape/Back clears only pending targeting. Set initial keyboard focus deliberately.
- [ ] Add a fresh-input gate across entering targeting, leaving cancellation, retry and completing action playback. Suppress callbacks in the opening frame and until relevant held Submit/pointer input releases. Keep one synchronous resolving guard before session mutation.
- [ ] Update Refresh to render all member HP/defeated states and current actor, while deriving sound and hit feedback from LastAction rather than the now-advanced ActivePlayer. Delay result controls and outcome sound until terminal playback ends.
- [ ] In the existing Editor, test click and keyboard targeting, rapid double-click, held Submit, Escape+Submit, canceled offensive special, depleted special, and defeated target. Assert no action on cancellation and exactly one action per confirmed target. Record untested cases if Editor access is unavailable.

## Task 4 — Full 3D water stage and formation playback

Interfaces in `FishBattleVisuals.cs`

- Retain the existing Initialize entry point while migrating its internals to the scene camera.
- Add `BindTeams(FishBattleRules battle, FishSpecies[] playerSpecies, FishSpecies[] enemySpecies)` for initial/retry roster binding.
- Add `RefreshFormation(FishBattleRules battle)` and `HighlightTarget(int enemyIndex)` with -1 clearing the target.
- Add `IEnumerator PlayAction(FishBattleActionResult action)` for short read-only presentation. BindTeams resets pending visual state. Defer defeated-model removal until the recorded hit has played, then RefreshFormation reconciles final state.

- [ ] Replace the narrow preview/RenderTexture with the existing camera's world view. Create or reuse the owned arena root, shallow-water surface and formation anchors. Use existing URP-compatible materials and camera/light ownership.
- [ ] Spawn one holder per member using species.model or FishPrimitiveVisual. Retain existing collider cleanup and model bounds fitting. Preserve slot indices even when zero-HP members have no model. Support the configured enemy count without truncation.
- [ ] Frame opposing rows with an elevated diagonal perspective camera, accounting for screen-safe HUD areas and the larger boss. Expose stage/camera placement as authored settings preserved by repeated initialization/setup.
- [ ] Implement short swim-forward/return for attacks and stronger specials, and a brief highlight for Defend/heal. Suggested initial duration is 0.4 seconds per action using unscaled time. PlayAction reads the recorded result and never calls damage/heal methods.
- [ ] Connect ResolveRound to player playback, then one accepted enemy action/playback, then fresh input. After a terminal hit, show results and stop. Restore transforms and destroy owned runtime materials/models on retry/unload or interruption.
- [ ] Manually verify 3v3, 1v3, 3v1, single boss, duplicate species, missing models and oversized imported meshes. Check input locking, KO removal timing, no repeated audio, camera bounds at all target resolutions, and no HP changes caused by spawning.

## Task 5 — Repeatable battle scene upgrade and compact HUD

Interfaces

- Add `FishBattleSetup.UpgradeWaterBattle()` with menu `FISH!/Upgrade Water Battle`, limited to the open `Assets/Scenes/FishBattle.unity` outside Play Mode.
- Extend the existing `FishPanelLayout.Battle` entry point to build or adapt member-card/action/feedback sections. Preserve serialized controls and listener ownership.

- [ ] Upgrade legacy Battle Layout objects explicitly. Existing name-based early returns must recognize and migrate the old layout once while preserving later authored edits. Reuse existing buttons, controller, camera, Canvas and EventSystem.
- [ ] Organize member cards along upper safe edges, actor/turn feedback compactly, actions in a fixed footer and the arena in the center. Wrap long names and scroll lengthy special descriptions while primary actions remain visible. Use theme typography already available in Unity and the approved Figma hierarchy where supported.
- [ ] Implement the Editor upgrade with Undo, missing-reference validation and create-only-missing owned objects. Mark the scene dirty for review and manual save. Run the same construction path for newly generated battle scenes. Retain configured species/model/audio/encounter references.
- [ ] Check older 3C and Step 6C setup reruns cannot restore the preview, duplicate Special, or overwrite the new layout. Adapt their battle-only paths if needed.
- [ ] In Unity run upgrade twice, edit a supported camera/anchor setting, rerun, Undo and reload. Assert edits survive and component/object counts remain stable. Check long names, button bounds and actor/target differentiation at 1920x1080, 1280x720 and 800x450.

## Task 6 — Integration acceptance and delivery

- [ ] Run `dotnet run --project Tests/FishBattleChecks.csproj`, `Tests/FishBattleSessionChecks.csproj`, `Tests/FishSpecialChecks.csproj`, and `Tests/FishHealthChecks.csproj` as separate commands. Require zero exit codes and explicit passing totals.
- [ ] Run `dotnet run --project Tests/FishSaveChecks.csproj` and `dotnet run --project Tests/FishStoryChecks.csproj`. Keep save tests isolated in disposable temporary directories. Verify changed member HP and ordered IDs round-trip without healing.
- [ ] Compile runtime and Editor source through the existing Editor. If using the project's previous external compiler workflow, identify actual installed assembly paths and record the exact invocation. Clearly distinguish external compilation from Editor import/build verification.
- [ ] In the existing Unity instance, complete a victory, defeat, Recover and Retry, Return to Shore and consecutive battle. Verify exact-ID HP, fresh temporary state, once-only progression, save timing and fishing after return. Test target cancellation, failed return recovery and disabling/unloading during presentation.
- [ ] Count active camera/listener/EventSystem and model roots across repeated attempts/returns. Check single ownership, clean release of obsolete preview resources and restored shore controls/cursor.
- [ ] Write `Docs/Water_Lineup_Battle_Setup.md` with the actual menu, controls, completed checks, manual checklist and limitations. Update PROJECT_STATUS with actual file paths, commands/results and remaining validation. Update the approved design only for documented implementation clarifications.
- [ ] Review source changes for plan coverage and preserve unrelated edits. If a repository is available, checkpoint reviewed tasks and provide the final diff. Otherwise report file-level changes without creating a repository solely for this work.

## Plan verification and handoff

Self-review maps turn rules to Task 1, result/HP integrity to Task 2, targeting/input to Task 3, water and playback to Task 4, layout/setup to Task 5, and saves/transitions/documentation to Task 6. All five review-focus cases have checks assigned. API additions preserve compatibility wrappers until existing callers are audited.

This planning task inspected source and test entry points. Runtime tests, compilation and Unity visual checks are pending implementation. Native execution in this chat is recommended because these tasks share closely coupled actor indices, action records and UI phases. Subagent-driven execution is also available with a fresh implementation/review context per task.
