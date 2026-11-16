# Milestone 3A: reusable turn-based battle foundation

## Setup in the existing Unity Editor

1. Let scripts compile in Unity **6000.6.3f1**. Do not open another Editor instance.
2. Open only `Assets/Scenes/CoastalPrototype.unity`, outside Play Mode.
3. Ensure 1A–2B are set up. The latest saved scene inspected still had 1A–2A;
   run **FISH! > Setup Milestone 2B** first if its team UI is absent.
4. Run **FISH! > Setup Milestone 3A**. Review and save the overworld.
5. Enter Play Mode, catch fish at the dock, open **T**, choose 1–3 fish, and
   **Confirm team**. Close the panel.
6. Approach the coral cylinder at **(-7, 1, 4)** and press **F** to enter battle.

Setup creates and saves the new `Assets/Scenes/FishBattle.unity` separately and
enables its build-scene entry, retaining existing entries. It restores the active
overworld scene without saving over it. An existing battle scene is preserved.
It creates/reuses `Assets/Data/Fish/PlaceholderEnemy.asset` and
`Assets/Data/Fish/TestEnemyTeam.asset`; the initial enemy team has two separate
battle fighters referencing the same placeholder enemy species data.

For a usable test, setup gives the original **all-zero, flagged placeholder**
player species fictional base stats **HP 24, Attack 8, Defense 2**. It preserves
any stats already edited. A newly created enemy has **HP 16, Attack 6, Defense 1**.
These are gameplay numbers, not biological information. Names/habitat/facts and
reference fields remain unresearched and empty. Inspect/change the asset stats
outside Play Mode to test other matchups. Runtime safety clamps max HP to at
least one and Attack/Defense to at least zero.

Setup adds one standalone encounter root and prompt, reusing the overworld
canvas/team/collection and Input System UI asset. Repeating setup detects the
existing marker and preserves it; it does not repair partial edits. Undo of the
encounter setup removes its overworld edits. Created scene/data/material assets
and build-list changes remain. No packages or render-pipeline settings change.

## Entry, scene reuse, and session data

- Fishing uses E and only awards catches. **Only the separate F encounter** starts
  battle, and only with a nonempty, valid, confirmed owned-fish team.
- Fishing, collection, or team UI blocks encounter entry. F takes priority if
  pressed simultaneously with other panel keys while exploration is available.
- The encounter snapshots selected players in team order and enemies from its
  `FishEnemyTeam` asset. Change that reference to reuse the same battle scene for
  other data-defined enemy teams; no separate battle system is required.
- Battle loads **additively**. Original overworld roots are temporarily disabled,
  retaining collection/team objects while avoiding simultaneous cameras and
  EventSystems. The encounter root remains alive to coordinate loading/return.
- On victory or defeat, **Return to beach** unloads battle and restores the
  previously active overworld roots, exploration input, cursor state, and team.
- After defeat, **Retry** creates a fresh battle with all fish at full HP and
  no guard state. Re-entering the encounter also starts full health.
- Nothing writes battle HP/guard into a species asset or owned catch. There is
  no disk saving, story progression, reward, special ability, or battle animation.
- Starting the battle scene directly has no encounter/team context: action
  buttons stay disabled and the UI directs you to start from the overworld.

## Battle rules

- One active player fish and one active enemy; other team members are reserves.
- Player acts first. Each Attack or Defend gives the other side its next turn.
  The simple enemy attacks twice, defends once, then repeats this pattern.
- Damage: `base = max(1, attacker.Attack - defender.Defense)`.
- While defending: `damage = max(1, ceil(base / 2))`. Minimum damage is always one.
- Defend sets a boolean guard; it cannot stack. Guard remains until that fish's
  **next turn begins**, then expires even if no attack hit it. This follows the
  milestone's explicit duration requirement.
- After damage, HP clamps to zero. A defeated active fish is immediately replaced
  by its next surviving teammate in order; the next side still takes its turn.
- Defeating the last enemy immediately yields Victory, with no retaliation.
  Losing the last player fish immediately yields Defeat.
- Attack/Defend are disabled throughout resolution and at a terminal result.
  A small real-time feedback pause separates actions; it is not an event timer.
  No polished animation or extra combat mechanic is involved.

## Files

- `Assets/Scripts/FishBattleFighter.cs`, `FishBattleRules.cs`: isolated temporary
  stats/HP and deterministic turn/damage/replacement rules.
- `FishEnemyTeam.cs`: enemy team ScriptableObject data.
- `FishBattleEncounter.cs`: confirmed-team proximity entry, additive transition,
  and overworld restoration.
- `FishBattleScene.cs`: placeholder fighter cards, HP, action locking, enemy
  turns, results, defeat retry, and return controls.
- `Assets/Editor/FishBattleSetup.cs`: scene/data/encounter setup using existing UI
  helpers and Input System actions.
- `Tests/FishBattleChecks.cs`/`.csproj`, script metadata, this guide, and status.

## Unity testing checklist

1. After setup/save, confirm one encounter marker, the separate battle scene in
   the enabled build scene list, and correct enemy data references. Rerun setup:
   there must be no extra marker or overwritten scene/data tuning.
2. Approach the marker with an empty/unconfirmed team. F must not load battle.
   Add/confirm fish through T; editing the team afterwards must require reconfirmation.
3. Enter battle with one, two, and three selected catches. Check team order,
   active names, max HP, and one active fighter per side. Duplicate species from
   distinct catches must have independent HP. Confirm the enemy team matches its asset.
4. Attack: compare damage to the documented formula. Spam click/Submit during
   feedback pauses; buttons must remain disabled and no extra action may resolve.
5. Defend: verify reduced incoming damage and expiry on your next turn. Test
   enemy guarding too, including when the other side defends instead of attacking.
   Set high Defense/low Attack in test assets to verify minimum damage one.
6. Defeat the first enemy: the next enemy must enter immediately and act next.
   Defeat the last enemy: Victory must appear without an extra enemy attack.
7. Repeatedly Defend or tune enemies stronger to lose. A defeated player must be
   replaced automatically; losing the last player must immediately show Defeat.
8. Retry from defeat. Every fish must start full HP, player acts first, and no
   guard survives. Return after both Victory and Defeat; collection quantities,
   owned-catch IDs, team order, and confirmation must remain intact.
9. Verify restored movement, orbit, C/T/E panels, cursor state (including entry
   from an already released cursor), and focus changes. Re-enter battle several
   times. Check only one active camera/AudioListener and EventSystem at a time;
   battle scenes must unload on return.
10. Catch successfully at the dock repeatedly: no battle may start. Test F near
    the marker while another panel is open and simultaneous F/T/C inputs.
11. Save/reopen the overworld and retest. Check the Console, UI readability at
    different Game view sizes, and that shared species/owned data did not acquire HP.

## Checks performed and limits

- **45 standalone battle-rule checks passed**: first/alternating turns, duplicate
  action rejection, damage/rounding/minimum, guard duration/non-stacking,
  immediate KO/replacement/victory/defeat, no post-result action, fresh HP/guard
  on restart, independent instances, numeric safety, and invalid team rejection.
- All runtime/Editor scripts compiled against installed Unity/Input System/uGUI
  assemblies with no source errors. External compiler warnings remain CS1701
  (framework references) and CS0649 (Editor-serialized fields).
- Available .NET 10 command: `dotnet run --project Tests/FishBattleChecks.csproj`.
  Tests were compiled/run directly with the installed compiler/runtime; the
  project entry point itself has not been exercised.
- Existing overworld scene, build settings, and species files were left unchanged
  by implementation; their scene/asset changes happen when setup runs in Unity.
- **Not verified in the Editor:** setup/new-scene serialization, additive load/
  unload, Input System UI behavior and action-button lock timing, session
  collection/team retention, cursor/control restoration, UI layout, repeat
  entry/retry, and Undo. No Editor-control tool is connected and no second
  Unity instance was launched. Follow the checklist above before approval.
