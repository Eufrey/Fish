# Milestone 2A: fish data and basic collection

## Unity setup

1. Open `Assets/Scenes/CoastalPrototype.unity` as the only loaded scene in the
   existing Unity **6000.6.3f1** Editor. Let scripts compile.
2. Ensure the 1A, 1B, and 1C setups already exist. Run only missing setup commands.
3. Outside Play Mode, choose **FISH! > Setup Milestone 2A**.
4. Review the additions, save the scene, and enter Play Mode.

The command reuses the existing canvas, EventSystem, timing game, exploration
player, camera, and setup UI helpers. It adds one collection component and UI,
connects fishing rewards and panel exclusion, and creates/reuses
`Assets/Data/Fish/PlaceholderFish.asset`. Rerunning the command preserves the
existing setup. It stops on conflicting or partial setup instead of duplicating
objects or overwriting references. Immediate Undo restores scene edits as one
group; a newly created species asset remains. The scene is not saved automatically.

No packages, input assets, scenes, render settings, or existing gameplay objects
are replaced. No second Unity instance is needed.

## Data and behavior

- `FishSpecies` is shared ScriptableObject data. The initial asset is explicitly
  named **Placeholder Fish (unresearched)** and marked as a placeholder.
- Supported fields: stable species ID; display/local/English/scientific names;
  local-name language/region; icon/model; rarity; fictional health, attack,
  defense and speed; special ability name/effect; habitat, description,
  educational fact; and reference notes/URLs.
- Unresearched names, biological facts, habitat, descriptions, references, and
  abilities are left empty. Stats stay at zero until deliberately authored;
  they are not used by this milestone. No real species is asserted or approved.
- Each success creates a separate `FishOwnedCatch` with its own ID and a reference
  to the shared species asset. Duplicate species catches are allowed.
- `FishCollection` holds these records in memory, independently of the panel.
  No catch data is written to the species asset or disk. A successful attempt
  can claim exactly one reward; failure and unresolved cancellation claim none.
- Closing a resolved success keeps its already-earned catch. Retrying starts a
  new attempt; it does not remove or re-award the previous one.
- **C** opens the collection from exploration. The panel shows species names,
  quantities, and total owned catches. It refreshes on each opening, without
  clearing stored catches. **C**, **Escape**, or **Return to beach** closes it.
- Movement and mouse orbit are blocked while the collection is open, and the
  cursor is visible/unlocked. Closing restores the prior input/cursor state.
- C is ignored during fishing; E is ignored during collection. Simultaneous
  C/E from exploration opens collection only. A close-frame guard prevents one
  panel from opening as the other closes on that same input frame.
- Data lasts while the collection component exists in the current Play session.
  There is no scene-transition persistence, disk saving, or battle integration.

## Unity tests to perform

1. Press C before catching anything. Confirm the empty state, visible cursor,
   and blocked WASD/orbit. Close using C, Escape, and the button. Test opening
   with the exploration cursor both locked and already released.
2. Catch successfully at the dock. Confirm the feedback says one fish was added.
   Close fishing, press C, and confirm **Placeholder Fish (unresearched) x 1**.
3. Reopen/close collection several times. The quantity must stay at 1.
4. Catch the same placeholder again, using Retry if desired. Confirm quantity 2
   and total 2. Press/hold Space repeatedly after a success: no extra catches.
5. Fail a catch; close and check that the count is unchanged. Cancel an unresolved
   attempt with Escape and with Return to beach; neither may add anything.
   Also test Escape + Space together, clicking Return to beach while pressing
   Space on the same frame, and opening/retrying while holding Space.
6. While fishing, press C. Collection must not open or steal the cursor. While
   viewing collection near the dock, press E: fishing must not open. Test C+E,
   Escape+E, and rapid panel toggles. Only one panel may own exploration input.
7. After both panels close, verify WASD, orbit, and cursor capture/release still
   work. Switch application focus while each panel is open and retest closing.
8. Inspect the placeholder asset: biological content/reference fields must be
   blank. Catching fish must not modify the asset.
9. Stop Play Mode, rerun setup, and confirm no duplicate collection, canvas,
   EventSystem, player, or camera. Save/reopen the scene and test again. With the
   normal scene/domain reload settings, a new Play session starts empty.

## Checks performed and limitations

- Runtime and Editor source compiled against installed Unity/Input System/uGUI
  assemblies with no source errors. The external reference set still produces
  the known CS1701 netstandard compatibility warning.
- **30 standalone timing/claim checks passed**, including same-input safeguards,
  success claimed once, repeat successes, and no claim for running, failed,
  closed, or cancelled attempts. They do not instantiate Unity collection/UI objects.
- Available standalone command (.NET 10 SDK):
  `dotnet run --project Tests/FishTimingAttemptChecks.csproj`. The checks above
  were compiled and executed directly with the installed compiler/runtime;
  this project entry point itself has not been exercised.
- The existing saved scene was preserved. No Unity Editor tool is connected;
  the 2A setup command has not been executed. Asset creation/serialization,
  actual owned records and displayed quantities, panel layout, button clicks,
  exclusion/input ordering, Undo, and Play Mode behavior remain **unverified in
  the Editor**. Follow the checklist before considering 2A verified in Unity.
