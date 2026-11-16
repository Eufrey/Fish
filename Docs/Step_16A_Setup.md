# Step 16A: final story presentation

## Setup

Allow the existing Unity Editor to recompile and restart Play from the shore.
No new setup command, art asset, package or scene replacement is required.
Existing Milestone 4 Story HUD plus exploration, fishing, collection and team
setup must be present. The existing HUD creates one modal on its existing canvas
at runtime and reuses the Input System EventSystem. Saved scene edits are preserved.

This step explicitly supersedes the earlier request to defer backstory: the new
opening, two encounter introductions, boss introduction and ending are now enabled.
Text is English; no unverified local expressions were added.

## Flow

- Fresh Play: a brief opening explains the fictional Negros coastal barangay,
  disturbance and purpose. It teaches the verified keyboard controls: WASD/arrows,
  mouse look, E at dock, Space timing catch, C collection, N encyclopedia, and
  Escape/click cursor behavior. First objective remains catch three fish.
- Approach an unlocked marker and press F: introduction, Choose Team or Return
  to Shore. Locked/completed encounters remain inaccessible. No unlock or story
  button launches a battle automatically.
- Choose Team closes story first, then opens the existing team UI. Confirm a
  usable team, return to shore, and press F again to enter. Closing/canceling team
  without confirming does not grant introduction confirmation. Recovery remains
  R in Team. Editing still invalidates team confirmation as before.
- Defeat, recovery/retry and return behave as before. Introduction confirmation
  is session-only per marker; retries do not repeat introductions. No automatic
  encounter chaining occurs.
- Boss victory retains the existing Completed progression result. After Return
  to Shore, the ending opens once when other menus are closed. It thanks the
  community/player, explains that recovery takes continued effort, and asks for
  litter reduction, habitat protection and responsible fishing.

## Recovery visual

At ending presentation, the existing Water Placeholder receives a brighter cyan
instance tint through MaterialPropertyBlock. This is a symbolic placeholder cue,
not a claim that real environmental recovery happens instantly. No shared material
asset changes. With the modal closed, the changed shore remains visible for the
session. No litter was found in the inspected scene, so none is invented.
Optionally assign existing litter objects to Story HUD's Recovery Litter list to
hide those at the same point; do not assign gameplay roots/colliders/markers.
Recovery Water can also explicitly reference an existing water renderer. Automatic
fallback uses the existing Water Placeholder renderer; if absent, no new water is
created. Resetting Play restores the original scene appearance.

## Input and layout

Story claims movement/camera input and unlocks the cursor. The team, collection,
encyclopedia, fishing and encounter controls already honor these gates. HUD text
hides while story or another panel is active. Closing defers restoration through
the next frame to prevent the same Submit/interaction input opening another menu
or starting battle. Team transfer happens only after restoration, so its existing
cursor/input snapshot stays correct. Escape cancels; close-button actions never
start battle. Disable/teardown restores held input ownership.

One opaque modal uses uGUI vertical/horizontal layout groups, 32px horizontal and
28px vertical padding, 20px section gaps, 16px button gaps, fixed header/footer and
scrollable wrapped body text. It shares the existing CanvasScaler. Reopening
resets scrolling to the top. No timers dismiss story panels.

## Files and state

- FishStoryHUD.cs: replaces the old timed ending card with modal presentation,
  input ownership, opening/introductions/ending and instance-only recovery tint.
- FishStoryProgress.cs: adds only once-per-session opening/ending presentation
  claims. Existing progression stages, catch threshold and victory rules unchanged.
- FishTeamPanel.cs: optional story confirmation callback; cancellation clears it.
- FishBattleEncounter.cs: F introduction before explicit confirmed-team entry.
- Tests/FishStoryChecks.cs: once-only opening/ending and no early ending coverage.

Owned catches, current HP, battle stat snapshots, recovery/retry and victory commits
are untouched. For future Step 17 persistence, save presentation flags with story
progress and reapply the completed shore appearance on load without replaying an
already-seen ending. Disk saving remains unimplemented.

## Verification and manual acceptance

External compile of all runtime/Editor source passed (existing reference/serialized
field warnings). **25 story + 11 health + 34 session checks passed** using the local
compiler/runtime. Input bindings were inspected directly. Available entry points:
`dotnet run --project Tests/FishStoryChecks.csproj`, `Tests/FishHealthChecks.csproj`
and `Tests/FishBattleSessionChecks.csproj` (project entry points not themselves run).

**Not verified in Unity:** modal construction/layout, actual input event ordering,
flow to/from team, cursor restoration, scene suspension and water appearance.
No connected Editor tool or extra Editor instance used. Test:

1. Fresh Play: opening once, correct instructions; close/Explore/Escape restores
   controls. Try simultaneous T/C/N/E/F and UI Submit while story is open/closing.
2. Each unlocked marker: introduction readable, Return/Escape causes no battle;
   Choose Team has no overlap. Cancel team without confirming: next F shows intro.
   Confirm, close, then F: exactly one battle. Empty/all-zero team cannot enter.
3. Defeat/retry/return and normal victories: correct unlocks and notices, retained
   catches/HP, no narrative-triggered recovery or progression changes.
4. Boss win/return: ending once, no stacked menu, persistent water tint afterward;
   reopening other menus or returning focus cannot replay ending. Verify fishing.
5. 1280x720, 1920x1080 and smaller 16:9: title/body/footer in bounds, scrolling,
   normal cursor/focus changes and no overlapping HUD. Check water tint directly
   after closing ending and original appearance after a fresh Play session.
