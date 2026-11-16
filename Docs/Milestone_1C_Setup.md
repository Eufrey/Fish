# Milestone 1C: timing-based fishing

One fixed-difficulty timing attempt now replaces the 1B panel's placeholder text.
It uses the existing fishing spot, panel, Close button, movement/camera gates,
cursor restoration, and Input System UI module. Fishing never starts a battle.

## Unity setup

1. Open this project in Unity **6000.6.3f1** and wait for scripts to compile.
2. Open **Assets/Scenes/CoastalPrototype.unity** as the only loaded scene.
3. Outside Play Mode, run **FISH! > Setup Milestone 1A** and then
   **FISH! > Setup Milestone 1B** if those setups are not already present.
4. Run **FISH! > Setup Milestone 1C**.
5. Save the scene, enter Play Mode, and approach the dock to the north.

The 1C command upgrades the existing panel in place: it hides the known 1B
placeholder message, adds the timing UI under `Timing Minigame`, and positions
the existing Close button beside Retry. It preserves the dock, player, camera,
EventSystem, input mappings, and URP settings. No packages or input assets are added.

Rerunning 1C detects the existing component and makes no changes. It does not
repair manually removed UI children. The command refuses missing 1B references
or duplicate/incompatible EventSystems rather than adding another input handler.
It never saves the scene automatically. Immediately after setup, Undo restores
the previous panel as one scene-edit group.

## Controls and rules

- **E near the dock:** open a fresh attempt. Exploration input is paused and the
  cursor is unlocked and visible, as in 1B.
- **Space:** attempt a catch when the white indicator's center is within the
  green target zone. The zone spans 60–80% of the bar, including both edges.
- The indicator bounces left/right at 0.65 bar lengths per second. Difficulty
  does not change between attempts.
- A successful attempt displays **Success! Caught: Placeholder Fish.**
- A missed attempt displays **Missed! No fish caught.**
- Each attempt resolves once and freezes the indicator. More Space presses
  cannot change its result or produce additional catches.
- **Retry:** starts a fresh attempt at the left end. It appears after a result.
- **Escape** or **Return to beach:** closes the panel and restores the previous
  exploration input/cursor state. Cancelling an unresolved attempt produces no
  catch. Escape takes priority over Space when both are pressed together.

Opening/retrying ignores input for that frame. Space must then be released
before a new press can attempt a catch. UI selection is cleared when an attempt
starts because Space is also bound to UI Submit. Retry cannot trigger on the
same frame that a catch resolves. The panel uses one existing Input System UI
module; there is no second Submit/Cancel input module or legacy Input API.

The placeholder fish is feedback only: there is no inventory, persistent catch
count, species database, reward callback, save data, or battle trigger. Closing
or retrying clears the temporary attempt result.

## Files changed

- `Assets/Scripts/FishTimingAttempt.cs`: pure C# attempt state, timing, result,
  retry, and cancellation rules.
- `Assets/Scripts/FishTimingMinigame.cs`: bar updates, Space input, feedback,
  and Retry button lifecycle.
- `Assets/Scripts/FishFishingSpot.cs`: exposes existing panel/Close references
  for the setup command; its exploration/cursor close flow is retained.
- `Assets/Editor/FishTimingSetup.cs`: repeatable panel upgrade.
- New Unity script metadata, standalone rule checks in `Tests`, and this guide.

## Unity testing checklist

1. Outside fishing range, E does nothing and the prompt stays hidden. Near the
   dock, E opens the timing bar; WASD and mouse orbit must remain blocked.
2. Wait for the white indicator's center to enter the green zone, then tap
   Space. Confirm success text naming Placeholder Fish and a stopped indicator.
3. Press/hold Space repeatedly after success. The result and position must stay
   unchanged. Click Retry, then catch outside the green zone to verify failure.
4. Confirm the indicator stops after failure too. Retry several times from both
   outcomes; the bar starts from the left each time and produces one result.
5. Hold Space before pressing E. The attempt must remain running until Space is
   released and pressed again. Also test pressing E and Space together.
6. After a result, hold Space while clicking Retry. The next attempt must keep
   running until Space is released and pressed anew. Try keyboard Submit on
   Retry as well: that Submit must not immediately resolve the new attempt.
7. Press Escape during a running attempt. Confirm no catch feedback is awarded
   and the panel closes. Reopen: it must start fresh. Test Escape + Space together.
8. Close through both Escape and Return to beach while running and after each
   outcome. Confirm movement, mouse orbit, and cursor behavior restore every time.
9. Open with the cursor already released (Escape during exploration, then E).
   Closing should leave it released; click the Game view to resume exploration.
10. Repeat open/retry/close at least ten times. Switch focus away/back while open,
    stop/restart Play Mode, and check for stuck controls or Console errors.
11. Rerun setup outside Play Mode. Confirm one fishing UI, one timing minigame,
    and one EventSystem. Save/reopen the scene and repeat the main checks. Inspect
    the panel at different Game view sizes to verify layout and readability.

## Verification performed and limits

Twenty standalone rule checks passed for opening-frame protection, held-input
protection, fixed speed, success/failure, inclusive target edges, stopped motion,
single resolution, same-frame retry rejection, retry/reopen reset, cancellation,
and bouncing within the bar. These test `FishTimingAttempt`, without Unity UI.
With a .NET 10 SDK, rerun them using:

```powershell
dotnet run --project Tests/FishTimingAttemptChecks.csproj
```

All runtime and Editor scripts compiled against the installed Unity, uGUI, and
Input System assemblies. The external compiler reports the same framework
reference compatibility warning as previous milestones, with no source errors.
The saved scene file was left unchanged; setup must be applied through the menu.

No Unity Editor control tool is connected. The setup command, Undo, scene
serialization, rendering/layout, input ordering with EventSystem Submit,
button clicks, cursor restoration, and actual Play Mode behavior have **not**
been verified in the Editor. Use the checklist above before considering this
milestone verified in Unity.
