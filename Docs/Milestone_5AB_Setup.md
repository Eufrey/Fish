# Milestone 5AB: owned health

This replaces the previous full-health-on-every-battle requirement.

## Setup and controls
No setup menu is required. Exit Play, let the existing Editor recompile and restart
Play. Existing catches within a new Play session start at maximum HP. In **T: Team**,
press **R: Recover Team** to restore all selected catches for free. The status text
explains recovery. Confirm a usable team afterward. Zero-HP members remain selected
but are skipped during battle; an entirely exhausted team cannot start.
Defeat offers **Recover and Retry** (full selected team recovery) or **Return to
Shore** (retains remaining HP). No items, costs, timers, or healing locations.

## Implementation
- `FishOwnedCatch`: CurrentHP and per-catch MaximumHP snapshot; clamped writes.
  Existing catch/species IDs unchanged. Shared species/base stats are not written.
- `FishBattleFighter`, `FishBattleRules`: snapshot starting HP, skip zero-HP fish,
  reject all-zero player teams. Fresh copies reset guards and special usage.
- `FishBattleSession`: once-per-attempt terminal health callback on victory/defeat;
  clears guards. Recover/retry heals captured selection and resets starting HP.
- `FishBattleEncounter`: maps results to exact owned IDs; captures recovery targets;
  existing shore scene retains the collection across additive battle transitions.
- Collection and team UI show individual current/max HP; battle retry label updated.
  Enemies remain local snapshots. Self-heals are included in committed result HP.

## Step 17 save/load requirements (not implemented)
Persist each owned CatchId, speciesId, CurrentHP and MaximumHP snapshot together,
plus selected team IDs and progression. Restore exact IDs; do not create new catches
or silently reset HP on load. Validate species references, positive MaximumHP and
clamp CurrentHP to [0, MaximumHP]. Define explicit migration defaults for legacy
records missing HP (full valid max). Do not serialize Defend/special usage as owned
state. Test independent same-species catches, zero HP, injured HP, recovery and team
references across save/load. Saving mid-battle requires a separately specified
policy; this milestone commits only terminal victory/defeat results.

## Checks and remaining manual tests
External runtime/Editor compilation passed. 11 health, 34 session, 38 special and
21 story standalone checks passed via direct compiler/runtime. Health tests use a
minimal species stub with actual owned/battle classes, not Unity objects.
Command available: `dotnet run --project Tests/FishHealthChecks.csproj` (entry point
itself not run). No Unity instance launched.

Manual Unity checks still required: injure two same-species catches independently;
return and enter the next encounter with stored HP; verify self-heal and exact
identity commits; zero-HP skipping/all-zero entry denial; R recovery restores only
selected fish; repeated Recover and Retry resets guards/specials/enemies without
advancing story; return does not heal; collection/team rows show correct HP; fishing,
cursor and panel behavior remains intact. Check recovery label/layout at all target
resolutions. No disk saving or backstory introduced.
