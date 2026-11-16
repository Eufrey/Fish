# Step 17 single-slot saves

Refresh scripts in the existing Unity Editor and restart Play. The existing shore pause menu (P) now includes Continue, Save, Confirm New Game and Cancel. Continue becomes enabled when the main save or its backup validates against the loaded species data. New Game first displays a replacement question. A second click confirms it, and Cancel or closing pause dismisses it. Resume preserves the current session.

Gameplay is stored at `Application.persistentDataPath/fish-save.json`. With current Windows project settings this is `%USERPROFILE%/AppData/LocalLow/DefaultCompany/Fish!_Blue Horizons/fish-save.json`. The sibling `.bak` is the previous validated save and `.tmp` is used during writing. Only master volume uses PlayerPrefs.

The version 1 JSON stores stable catch and species IDs, independent current/max HP, ordered team IDs, discovered species IDs, total committed catches, progression stage and introduction/ending presentation claims. Discovery and total counts are checked against owned catches because this project retains all catches. Continue restores stored HP and the scene's initial shore spawn, with camera snap. Team confirmation is requested again before battle entry.

Autosave requests follow committed catches, team confirmation/recovery and story presentation changes. Completed battle outcomes are captured after the existing HP/progression callbacks finish. The save comparison prevents repeated terminal-frame writes. Active attempts and scene transitions block autosave. Pending shore saves resume after returning. Manual Save is available on shore.

Existing disk progress protects itself from an uninitialized session until Continue or explicit New Game. Validation checks save version, complete arrays, stable/unique catch IDs, species existence, team ownership/order/uniqueness, HP bounds, discovery, counts and progression. Loading builds replacement catches first and clears existing runtime catches/team, preventing duplication. Shared species assets supply references.

Writes flush a temporary file before filesystem replacement. A valid previous primary becomes the backup. Corrupt primaries are preserved unless New Game is explicitly confirmed. Continue can recover a valid backup. When the primary is unreadable, saving remains blocked and feedback explains this. Preserve a copy of both files before manually repairing a damaged primary. Failed automatic writes report feedback and can be retried using manual Save. Unsupported versions are rejected.

## Verification performed

- All runtime/Editor scripts compiled externally against installed Unity assemblies with zero errors.
- 18 save checks passed using an isolated GUID-named temporary directory. Tests cover missing/incomplete/corrupt/unsupported data, backup recovery, IDs, independent zero/injured HP, team order, introduction state, repeated load, HP/team rejection, battle gating and write failure.
- 11 health, 34 session and 25 story regression checks passed.
- Save tests use System.Text.Json and Unity/session stubs. Unity JsonUtility serialization, UI input, lifecycle and actual platform storage still require Editor/build testing.
- Available command is `dotnet run --project Tests/FishSaveChecks.csproj`. Sources were compiled/run directly with the installed compiler/runtime during this task. Temporary responses/logs are under `Temp/Fish17Validation`.
- Personal save files were untouched. No additional Editor instance was launched.

## Manual Unity checks

Use a separate test profile or preserve personal files before destructive tests. Catch duplicate species, injure one and knock out another, reorder/confirm the team and save. Restart Play, choose Continue and verify exact IDs/counts/order, discovery, progression and independent HP. Repeat Continue and check for duplication. Check full-health recovery only through the existing explicit recovery actions.

Verify catch/failure/cancel behavior, terminal victory/defeat saves, repeated retry and return, unfinished-battle exit, opening/ending once-only behavior and recovered shore appearance. Loading returns to the authored initial shore spawn, so verify its ground clearance after manual scene edits.

Cancel New Game and compare both current session and disk state. Confirm New Game and verify fresh story/catches with the previous valid primary retained as backup. Check manual Save, pause cursor/control restoration and button spacing at 1920x1080, 1280x720 and 800x450. Test corrupt/unsupported data and filesystem write denial using disposable saves, inspect feedback, and verify damaged originals remain intact.
