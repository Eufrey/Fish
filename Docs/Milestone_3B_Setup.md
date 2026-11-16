# Milestone 3B: battle retry and return to shore

## Required Unity setup

1. Let scripts compile in the existing Unity **6000.6.3f1** Editor. Preserve any
   unsaved scene work; do not launch another Editor instance.
2. Outside Play Mode, keep only `CoastalPrototype.unity` loaded. Ensure the 2B
   team and 3A encounter setups exist; run only missing setup commands.
3. Choose **FISH! > Setup Milestone 3B**. Review the **Shore Return Point** child
   of the encounter marker, then save the overworld.
4. The point defaults three units behind the marker: **(-7, 1, 1)** for the
   standard test area. Its height is a search origin, not the capsule's final
   height. Keep it near the marker over the beach and away from colliders/water.

3B upgrades the existing encounter with a return-point transform and the beach
ground collider reference. It adds no manager, camera, EventSystem, scene, input
handler, or species asset. Repeating setup reuses the existing references/point
without moving tuned points. Immediate Undo restores the setup edits as one
group. The scene is never saved automatically.

Fresh 3A setup now includes these return references automatically. Existing 3A
encounters must run 3B setup before Play Mode. Existing battle scene references
remain compatible: the serialized `returnToBeach` button field keeps its name,
while its visible label becomes **Return to Shore** at runtime. No replacement
battle scene is needed.

## Behavior

- Defeat offers **Retry** and **Return to Shore**. Victory offers **Return to
  Shore**; Retry is hidden. Options stay disabled while actions are resolving.
- `FishBattleSession` captures the encounter, player catch IDs/order/stats, and
  enemy team once. Retry uses that same snapshot, scene, and session. It does
  not reread a changed selection or enemy asset and does not load another scene.
- Retry constructs fresh battle health, guard flags, active indices, turn state,
  and enemy AI counter. Player acts first. All player/enemy fish start at full HP.
- Starting/returning/retrying is guarded by session phases and UI state. One
  static active-encounter reference prevents competing loads across markers;
  it resets on Play startup and clears after returning. No persistent manager
  object or disk state is introduced. Already-loaded battle scenes cannot be
  loaded again, and a battle scene must contain exactly one controller.
- Return unloads battle **before** enabling the suspended overworld roots, so
  the two scenes' cameras/EventSystems cannot become active together. If unload
  fails, the overworld stays suspended; after a terminal battle the Return option
  can be tried again, with an error in the Console.
- The same overworld collection and selected-team objects are re-enabled.
  Defeat never removes catches, clears the team, or changes shared species data.
- Return raycasts against the assigned beach collider, checks capsule clearance,
  and places the player above the ground at the return point. If blocked, it tries
  other nearby positions and the original entry location. If all checks fail, it
  keeps the original entry position and logs a warning rather than forcing a
  teleport into an obstruction. The standard test area's ground/bounds must still
  be verified in Play Mode, especially after moving the marker or point.
- The capsule's fall speed is reset; the camera snaps to follow immediately.
  Previous movement/camera input and cursor states are restored, UI selection
  is cleared, and movement/camera ignore input for the restoration frame. The
  encounter ignores that frame too, preventing the Return input from re-entering.
- Collection/team/fishing panels remain closed after the round trip and can be
  opened normally. Catching fish still never starts a battle.

## Files changed

- `Assets/Scripts/FishBattleSession.cs`: captured encounter/team and retry/return
  lifecycle; simple enemy-turn counter moved here so retry resets it explicitly.
- `FishBattleScene.cs`: uses the session, result options, guarded retries/return,
  and cleared UI selection. Existing serialized button references are preserved.
- `FishBattleEncounter.cs`: single active encounter/load guards, safe placement,
  checked unload, and ordered shore restoration.
- `FishExplorationPlayer.cs`: safe CharacterController teleport/reset and one-frame
  input suppression. `FishOrbitCamera.cs`: reusable immediate target snap.
- `Assets/Editor/FishBattleReturnSetup.cs`: repeatable **Setup Milestone 3B**;
  `FishBattleSetup.cs` wires return points for new encounters.
- Script metadata, `Tests/FishBattleSessionChecks.cs`/`.csproj`, this guide, and
  `Docs/PROJECT_STATUS.md`.

## Unity verification checklist

1. Record collection quantities and the selected catch IDs/order/confirmation.
   Enter through F with a confirmed team. Force defeat by defending repeatedly
   or using a stronger test enemy. Confirm Retry and Return to Shore are offered.
2. Retry several times. Confirm same encounter/enemies and same selected player
   fish/order, full HP for **every** fish, no guard state, active indices reset,
   player first, and the enemy action sequence restarts. No extra scene/manager
   may appear. Spam click/Submit: only one retry or action may resolve per input.
3. Return after defeat. Verify safe placement near the marker, no overlap with
   the cylinder/boundaries/water, and no accumulated fall speed or delayed camera
   jump. Confirm all recorded catches and team entries are intact.
4. Win another battle. Retry must be hidden; Return to Shore must work. Repeat
   entry/defeat/retry/return and victory/return cycles at least ten times.
5. Test Return click/Submit spam and F held/pressed around return. There must be
   no repeated load/unload or immediate re-entry. Check one active camera,
   AudioListener, and EventSystem; the battle scene must unload before shore UI
   becomes active. Repeated 3B setup must add no second return point or manager.
6. Verify WASD, mouse orbit, and cursor restoration. Enter both with a locked
   cursor and with the exploration cursor already released. Switch focus away/
   back before returning. Open/close C and T afterwards; their controls must work.
7. Walk to the dock after both victory and defeat. E, Space, Retry, Escape, and
   Close must work, with exactly one catch on success and none on failure/cancel.
8. Temporarily place a collider on the return point to test nearby fallback.
   Move the point outside the beach to check ground rejection; restore it after
   the test. Confirm no unsafe forced teleport, and inspect any warning.
9. Save/reopen the scene and restart Play Mode. Check Console errors and serialized
   return references. If an unload error occurs, verify another Return attempt
   is allowed and the overworld camera/EventSystem remains inactive meanwhile.

## Verification performed and limitations

- **34 new battle-session checks passed** for snapshot identity/order, retries,
  full HP/guard reset, first turn/active indices/AI reset, duplicate transitions,
  terminal choices, return failure recovery, and failed-load abort.
- **45 existing battle-rule checks passed again.** All runtime/Editor source
  compiled against installed Unity/Input System/uGUI assemblies with no errors.
  Expected external compiler warnings remain CS1701 and CS0649. Compilation
  initially found obsolete `GetInstanceID`; the encounter uses `GetEntityId`,
  supported by the installed Unity version.
- Available .NET 10 commands:
  `dotnet run --project Tests/FishBattleSessionChecks.csproj` and
  `dotnet run --project Tests/FishBattleChecks.csproj`. Checks were compiled/run
  directly with the local compiler/runtime; project entry points remain untested.
- The saved battle scene/enemy assets/build entry already exist. The saved
  overworld still lacks 2B/3A objects; live unsaved Editor work is unknown. Save
  the existing Editor's configured scene rather than discarding unsaved work.
- No Unity-control tool is connected. Physics/shore placement, scene loading/
  unloading, active manager counts, serialized setup, outcome buttons, cursor/UI
  restoration, retained collection/team objects, and fishing after return remain
  **unverified in the Editor**. No scenes or existing data were overwritten and
  no second Unity instance was launched. No disk saving/story progression added.
