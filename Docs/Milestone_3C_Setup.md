# Milestone 3C: simple fish special abilities

## Unity setup

**Missing-button fix (2026-11-11):** let Unity recompile, then enter battle from
the overworld again. Older saved battle scenes now create missing Special Ability
and description controls at runtime, before registering the button listener.
Existing assigned/named controls are reused; no extra EventSystem is created.
The button appears to the right of Defend. These runtime additions are not saved.
To author the controls permanently outside Play Mode, use the optional steps below.

1. Let scripts/assets import in the existing Unity **6000.6.3f1** Editor.
2. Outside Play Mode, **save any unsaved overworld work before switching scenes**.
3. Open only `Assets/Scenes/FishBattle.unity` and run
   **FISH! > Setup Milestone 3C**.
4. Review/save the battle scene, then open the overworld and start Play there.
   Catch/select/confirm fish and enter via F, as in 3A–3B.

The command adds one Special Ability button beside Attack/Defend and one ability
description label. It repositions those action controls and the feedback area
to fit the existing canvas. It reuses the controller, UI helpers, and Input
System EventSystem. It creates no input handler, manager, scene, camera, or
ability asset. Repeating setup preserves configured UI/layout and species tuning.
Immediate Undo restores the scene upgrade as one group; save manually.

Fresh 3A battle-scene generation now includes the new UI automatically. Existing
battle scenes now fill missing controls at runtime; 3C setup remains available to
save/edit their layout. No existing serialized button references were renamed.

## Shared data and rules

Species data now has:

- `specialAbilityName` and `specialAbilityDescription` (fictional gameplay text).
- `specialAbilityType`: **None**, **StrongerAttack**, or **SelfHeal**.
- `specialAbilityPower`: positive bonus Attack or restored HP, according to type.

The old textual `specialAbilityEffect` field migrates to the description through
`FormerlySerializedAs`; no description is interpreted as executable code.

The current shared assets are assigned:

- **PlaceholderFish:** **Harbor Burst**, StrongerAttack, Power **6**.
- **PlaceholderEnemy:** **Patch-Up Pulse**, SelfHeal, Power **8**.

All names/effects are fictional. Biological names, habitat, description, facts,
and reference fields remain empty; combat stats were not changed. The setup
only supplies defaults for missing/unconfigured placeholder abilities, preserving
already authored definitions and tuning.

Stronger attack uses the existing damage rules with a flat Attack bonus:
`base = max(1, Attack + Power - target.Defense)`. Guard halves damage, rounded up,
with minimum one, exactly as for normal Attack. Large numeric results saturate
to the integer range rather than overflow. Special damage uses the existing
immediate KO/replacement/victory behavior.

Self-heal restores `min(Power, MaxHealth - Health)` HP to the active fish only.
Using it at full HP restores zero and **still spends its use and turn**. Actual
restored HP is reported. No revival, team heal, status effect, or energy exists.

Each individual battle fighter has its own temporary `SpecialUsed` flag. Both
specials consume the turn and can be used once per fish per battle. A replacement
fish has its own availability, even if it references the same species. The UI
disables Special during resolution, after use, outside the player's turn, and
for unconfigured effects. UI and battle rules both reject repeated requests.

Definitions are captured with the existing battle snapshot. New battle/retry
copies preserve the definition and reset usage/HP/guard state. Species assets
and persistent owned catches contain no usage state. Enemy AI remains its
existing Attack/Attack/Defend cycle and does not use the assigned enemy special.

## Files changed

- `FishSpecies.cs`, new `FishSpecialEffect.cs`, and the two placeholder `.asset`
  files: shared definitions/configuration and legacy description migration.
- `FishBattleFighter.cs`, `FishBattleRules.cs`: temporary use state and the two
  effects, reusing damage/defense/turn/KO handling.
- `FishBattleEncounter.cs`, `FishEnemyTeam.cs`: include abilities in stat snapshots.
- `FishBattleScene.cs`: button/listener, availability/description, and feedback;
  rejected actions cannot advance the UI's enemy-action sequence.
- `Assets/Editor/FishBattleSpecialSetup.cs` and `FishBattleSetup.cs`: repeatable
  upgrade and automatic controls for newly generated battle scenes.
- Script metadata, special test source/project, existing battle/session test
  project enum dependency, this guide, and `Docs/PROJECT_STATUS.md`.

## Unity testing checklist

0. With the existing older battle scene, enter battle without running setup:
   verify exactly one Special Ability button beside Attack/Defend and one
   description. Return and reenter; verify no duplicates. Then optionally save
   the Editor setup and repeat, confirming the authored controls are reused.

1. Inspect both placeholder assets: name, description, enum, and Power must match
   above. Unresearched biological fields must still be empty. Run 3C setup twice;
   verify only one new button/label and one original EventSystem.
2. Enter battle with Harbor Burst. Confirm its description is visible and the
   button is available. Compare its damage with `Attack + 6 - Defense`, including
   guard/minimum rules. The feedback must include the ability name and damage.
3. Spam Special clicks/Submit during resolution. One effect and one player turn
   must resolve. On later turns the button stays disabled, while Attack/Defend
   remain usable. No extra enemy turn may follow a rejected request.
4. Use two separate owned catches of the same placeholder. Spend the first one's
   special, let it be defeated, and confirm the second fish has its own use. A
   special that defeats the final enemy must produce victory without retaliation.
5. To test SelfHeal UI while keeping enemy AI unchanged, outside Play Mode
   temporarily copy the four ability fields from PlaceholderEnemy onto
   PlaceholderFish. Enter a fresh battle, take damage, then use the special.
   Confirm actual restoration, maximum-HP cap, spent use, and consumed turn.
   Test at full HP too. **Restore the original player ability fields afterwards.**
6. Defeat and Retry after spending specials. All player fish must have fresh
   availability, and definitions/team/order must remain the same. Return to Shore
   and enter a new battle; availability must reset again.
7. Check buttons remain disabled during action resolution, enemy turns, terminal
   results and returning. Verify the original Enemy Attack/Attack/Defend pattern.
8. Confirm collection quantities/IDs and species definitions are unchanged after
   using abilities; no shared/owned usage flag should exist. Check shore return,
   fishing, cursor behavior, Console errors, layout at different Game view sizes,
   and save/reload of the upgraded scene.

## Verification performed and limits

- Missing-button recheck: saved scene had no special controls/references despite
  configured species data. Added runtime completion in `FishBattleScene.cs`.
  All runtime/Editor scripts compiled externally (zero errors, existing
  CS1701/CS0649 warnings); 38 standalone special checks passed again.
  Runtime UI creation, layout, and clicking still require the Unity checks above.
- **38 special-ability checks passed**: bonus/defense/guard/minimum damage,
  capped/self-only healing, full-health spending, turn consumption, rejection of
  repeated/invalid uses, individual catch availability, KO/victory, new battle/
  retry reset, definition retention, unchanged enemy AI, and overflow safety.
- **45 battle-rule + 34 session checks passed again.** All runtime/Editor source
  compiled against installed Unity/Input System/uGUI assemblies without errors.
  Known external reference/Editor-serialized field warnings remain CS1701/CS0649.
- Available .NET 10 command: `dotnet run --project Tests/FishSpecialChecks.csproj`.
  Existing battle/session test projects now include the shared effect enum.
  Checks were compiled/run directly with the local compiler/runtime; project
  entry points have not themselves been exercised.
- Static inspection confirms placeholder ability fields are assigned and
  biological/reference fields remain empty. Saved scenes were preserved; the
  actual UI upgrade has **not** been executed in Unity.
- No Unity-control tool is connected. Serialization/migration in the Editor,
  layout, button locking under real UI events, replacement/retry display,
  scene round trips and cursor controls remain **unverified in the Editor**.
  No second Editor instance, disk saving, story progression, animation, energy,
  or complex status-effect system was added.
