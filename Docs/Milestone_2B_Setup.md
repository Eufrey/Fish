# Milestone 2B: battle team selection

## Unity setup

1. Let scripts compile in the existing Unity **6000.6.3f1** Editor.
2. Open only `Assets/Scenes/CoastalPrototype.unity`, outside Play Mode.
3. Ensure the 1A–2A setup is present, then run **FISH! > Setup Milestone 2B**.
4. Review the new UI and references, save, and enter Play Mode.

The command reuses the existing canvas, EventSystem/InputSystemUIInputModule,
player, camera, collection, species data, and UI construction helpers. It adds
one session team store to the player, one Team UI root, and mutual-exclusion
references on the fishing and collection controllers. It does not add another
input asset/handler, player, camera, scene, or package.

Running setup again preserves existing objects and tuning. Conflicting/missing
prerequisites stop setup before edits; manually deleted children are not repaired.
The command never saves automatically. Immediately after setup, Undo removes the
scene changes as one group. The existing saved scene was not edited by the code
implementation; this menu command must be run to apply the new UI.

## Controls and behavior

- **T** opens team selection during exploration. **T**, **Escape**, or
  **Return to beach** closes it. Closing retains both draft selections and order.
- With no catches, the panel explains that fishing is needed and Confirm is
  disabled. Catch fish at the dock first.
- The left side shows individual catches, four per page. **Previous/Next**
  accesses additional catches. Catch numbers identify entries in collection order.
- Click an **Add** row to append that owned catch. A selected catch is disabled
  in the available list; all additions are disabled when three slots are filled.
- The right side shows three ordered slots with catch number, species name and fictional
  **HP, ATK, DEF, SPD**. The placeholder currently has zero stats; no new stats,
  species facts, or biological content were authored.
- Click a filled team slot (marked with `>`) and choose **Remove**, **Up**, or
  **Down**. Removing frees the slot and leaves the owned catch in collection.
- **Confirm team** accepts one to three valid owned catches and displays a
  confirmation message. It does not start a battle or transition scenes.
  Adding, removing, or reordering afterwards requires confirmation again.
- Separate owned catches of the same species can fill separate slots. The same
  owned-catch ID cannot appear twice. Species assets are never copied into slots.
- Selection and confirmation live on `FishBattleTeam`, independently of the UI,
  and survive panel reopen/disable during the current Play session. Nothing is
  saved to disk; no battle behavior is implemented.

The panel uses the existing movement/camera input gates and cursor restore
behavior. Fishing, collection, and team panels cannot open over each other.
Simultaneous T/C/E during exploration gives Team priority; closing a panel blocks
other panels for that frame. Inputs do not switch panels until the current one
has closed. The existing Input System UI module handles button clicks.

## Files changed

- `Assets/Scripts/FishTeamSelection.cs`: ordered ID list, ownership/duplicate and
  capacity validation, removal, reorder, confirmation.
- `Assets/Scripts/FishBattleTeam.cs`: connects the session team to real owned
  catches in the existing collection; resolves species/stats from those catches.
- `Assets/Scripts/FishTeamPanel.cs`: selection UI, paging, input/cursor management.
- `Assets/Editor/FishTeamSetup.cs`: repeatable menu setup using existing UI helpers.
- `FishFishingSpot.cs` and `FishCollectionPanel.cs`: team panel exclusion only.
- Script metadata, `Tests/FishTeamSelectionChecks.cs`/`.csproj`, this guide, and
  `Docs/PROJECT_STATUS.md`. Existing catch/reward rules and species assets unchanged.

## Unity tests to perform

1. Open T with no catches. Confirm must be disabled; closing restores controls.
2. Catch at least four fish. Open T and add one: name/stats must match its shared
   species, and Confirm must work with just one fish.
3. Try selecting the same owned catch again: its Add button must be disabled.
   Add two different catches of the same placeholder species: all three slots
   should be allowed. A fourth must be blocked, including from another page.
4. Select each slot and test Up/Down. Confirm the order changes as expected and
   first/last bounds disable invalid moves. Remove a middle fish and confirm the
   remaining order compacts, quantity in collection is unchanged, and the removed
   fish can be selected again.
5. Confirm with one, two, and three members. Edit after confirming: reconfirmation
   should be required. Remove all members: Confirm must become disabled.
6. Close/reopen repeatedly through T, Escape, and the button. Names, catch IDs,
   order, and valid confirmation should persist. Check Next/Previous with five or
   more catches; entries must remain individually identifiable and selectable.
7. While Team is open, E and C must not open other panels. While fishing or
   collection is open, T must do nothing. Test simultaneous T/C/E and close+open
   inputs; only one panel may be visible or own controls.
8. During Team selection, WASD/orbit must be blocked and cursor visible. Closing
   must restore the prior cursor state, including opening from an already
   released cursor. Switch focus away/back and test closing again.
9. Stop/restart Play Mode, inspect Console errors, rerun setup outside Play Mode,
   and verify one team store/UI plus the original single EventSystem. Save/reopen
   and check references and layout at multiple Game view sizes.

## Verification performed and limits

- **25 standalone team-rule checks passed**: ownership, empty/one/three/four-member
  boundaries, duplicate IDs, ordered moves, removal/re-addition, confirmation
  invalidation, read-only member exposure, and retained selection on reads.
  Distinct IDs represent separate catches (including repeat species).
- All runtime and Editor scripts compiled against installed Unity/Input System/
  uGUI assemblies with no source errors. The external compiler reports the known
  CS1701 framework-reference warning and CS0649 for fields assigned by the Editor
  setup/Unity serialization rather than C# assignments.
- Available command (.NET 10 SDK):
  `dotnet run --project Tests/FishTeamSelectionChecks.csproj`.
  The tests were compiled/run directly with the installed compiler/runtime;
  that project entry point has not itself been exercised.
- **Not verified in Unity:** setup execution/Undo, serialized connections, real
  owned-catch resolution, button interaction, layout, paging, panel exclusion,
  control/cursor restoration, and Play Mode persistence. No Editor-control tool
  was connected and no additional Unity instance was launched.
