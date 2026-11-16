# Milestone 5A: variety and encyclopedia

## Setup

Outside Play Mode, open only `Assets/Scenes/CoastalPrototype.unity` in the existing
Editor. Allow compilation, then choose **FISH! > Setup Milestone 5A** and save.
Existing collection/fishing setup is required. This reuses the current canvas,
Input System EventSystem, collection panel and its input/cursor gates. No new
manager, player, camera, package, input handler or battle scene is created.
Repeat setup preserves assets and tuning. Scene wiring supports Undo; newly
created assets remain after Undo. No existing scene is overwritten automatically.

Setup creates `Assets/Data/Fish/CatchCatalog.asset` referencing the unchanged
`PlaceholderFish.asset` plus `CatchPlaceholder2.asset` through
`CatchPlaceholder5.asset`. Existing species GUIDs/IDs and owned-catch IDs are not
changed. Story/enemy species remain separate encounter fixtures, not catch entries.
No researched species entries were found in the existing assets.

The five default weights are **40, 30, 15, 10, 5**. Categories are Common, Common,
Uncommon, Uncommon, Rare. Rarity describes gameplay frequency only, never biological
abundance or conservation status. Weights control selection independently of the
rarity label; changing a label alone does not change catch frequency. Each enabled
entry requires an integer weight from 1–100000. Invalid/missing or duplicate species
IDs invalidate the catalog rather than silently excluding an enabled species.
Disabled entries have no catch chance. Integer selection gives every enabled
entry at least one reachable ticket. Default timing difficulty is unchanged.

Species selection happens only after success and is retained for that attempt.
The existing claim gate awards exactly one owned catch; retry clears the selection.
Failure/cancellation do not choose or award a species. Without 5A wiring, legacy
single-species scenes retain their prior behavior.

## Controls and presentation

- **N**: encyclopedia; **C**: collection. These are two views of the same panel,
  so they cannot stack. N/C/Escape or Return to beach closes it.
- Previous/Next Entry browse exactly one catalog entry per species. Undiscovered
  entries reveal only a hidden placeholder and entry number. Discovery derives
  from owned catches using stable species IDs; duplicates do not duplicate entries.
- Discovered entries show available names, rarity, image, biological fields,
  reference metadata and battle stats from the same shared species assets.
  Missing text says Not researched yet; missing images have a text fallback.
- Long names/details wrap inside a scrollable area. Wheel/drag scrolls the text.
  Footer navigation and return controls remain outside the scroll area.
- Team, fishing and encounter gates reuse collection's existing open/close-frame
  exclusion. Story HUD and ending hide/pause while this panel is open.
- Session only: battle round trips retain catches/discoveries; fresh Play resets.
  No backstory text or disk saving has been added.

## Files

New runtime: `FishCatchCatalog.cs`, `FishCatchWeights.cs`, `FishEncyclopediaView.cs`.
Updated: `FishTimingMinigame.cs` (single selection/award), `FishCollectionPanel.cs`
(shared N/C mode), `FishStoryHUD.cs` (visibility), `FishSpecies.cs` (rarity tooltip).
Setup: `Assets/Editor/FishVarietySetup.cs`. Tests: `Tests/FishCatchWeightChecks.cs`
and project file. Gameplay timing, owned-catch/team/progression rules unchanged.

## Verification

External compilation of all runtime/Editor scripts passed (existing reference/
serialized-field warnings). Direct compiler/runtime checks passed: exhaustive
100-ticket default distribution, disabled/invalid tickets, smallest positive
weight reachability and success/failure/cancel claims; **30 timing checks** and
**21 story checks** passed again. Available .NET 10 command:
`dotnet run --project Tests/FishCatchWeightChecks.csproj` (entry point itself not run).

**Not verified in Unity:** setup/import/serialization, actual random draws, UI
input and layout, discovery display, real catch awards and scene transitions.
No additional Editor instance was launched. Check manually:

1. Run setup twice: five catalog entries, one panel/input system, preserved tuning.
2. Fresh N shows five hidden entries. Catch a species, reopen: only that entry
   unlocks. Repeat the species: quantity increases by one, entry count stays five.
3. For reachability testing, temporarily enable one entry at a time, catch it,
   then restore all five enabled and original weights. This tests real award/UI
   wiring without relying on random waiting. Do not use this fixture as a claim
   that random frequency has been measured.
4. Fail/cancel, Escape+Space and Close+Space: zero rewards. Retry/held Space cannot
   double award or reroll the displayed species after success. Difficulty unchanged.
5. N/C/T/E/F simultaneous input, close-frame inputs, focus changes, return button:
   no stacked panels, no moving/orbiting while open, correct cursor restoration.
6. Select catches into team, complete/retry/return from battles: same IDs/team,
   discoveries and story count. Fishing remains usable; no auto-battle on catch.
7. Test 1280x720, 1920x1080 and smaller 16:9. Check footer, long names/details,
   image/no-image, missing fields, scrolling and story/ending visibility.

## Research still needed

All five catchable species are unresearched placeholders: Placeholder Fish and
Placeholder Fish 2, 3, 4, 5. Verified local/common/scientific names, regional naming,
habitat, descriptions, educational facts, sources and suitable images remain
empty. Their stats, rarity and ability names are fictional gameplay values.
