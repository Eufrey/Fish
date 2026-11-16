# Water lineup battle setup and testing

## Setup

1. Refresh scripts in the existing Unity Editor and resolve any import errors.
2. Open Assets/Scenes/FishBattle.unity outside Play Mode and choose **FISH! > Upgrade Water Battle**. This validates the existing camera/controller. Runtime adaptation supplies the formation and target UI each battle. The command preserves saved scene objects and manual edits.
3. Start from the shore scene, confirm a usable team and interact with an available encounter. Existing encounter data can contain one enemy or several. Three-member fixtures should be tested using disposable development data.

## Controls

Choose Attack or an offensive Special, then click a living enemy card or navigate with the existing UI keyboard controls and Submit. Back or Escape cancels targeting. Defend and self-heal act immediately. Each side rotates through living fish independently. A smaller team acts more frequently per individual fish. Recover and Retry restores the selected team and starts a fresh attempt.

## Implemented changes

FishBattleRules rotates actors and validates explicit targets. FishBattleSession records the accepted actor/target before cursors advance and retains existing result/HP ownership. FishBattleScene adds target mode, input release gating, member cards and keyboard focus. FishBattleVisuals uses the battle camera, shallow-water placeholders, every member's existing model/fallback and short swim-forward presentation. Defeated members disappear after playback. Species and catch identity are reused.

The action record is nested inside FishBattleSession to preserve existing standalone project compile lists. The new Editor command validates prerequisites because runtime adaptation already constructs the presentation. It does not rewrite the saved scene. Camera framing is currently a runtime default and requires visual tuning in Unity. Figma frames were not edited in this implementation.

## Checks actually performed

- Expected failing rotation test observed before changing rules.
- 58 battle, 35 session, 38 special, 11 health, 18 save and 25 story checks passed. Save tests used an isolated temporary directory.
- Runtime and Editor source compiled externally with zero errors. Existing obsolete API and serialized-field warnings remain. Logs and compiler response files are under Temp/WaterBattle.
- Independent source review identified keyboard focus loss, post-defeat Special dereference and interrupted enemy-turn playback. Corrections were applied, including a post-defeat Special regression check.
- No Unity Play Mode or visual verification performed.

Run checks using `dotnet run --project Tests/FishBattleChecks.csproj` and corresponding FishBattleSessionChecks, FishSpecialChecks, FishHealthChecks, FishSaveChecks and FishStoryChecks projects.

## Manual Unity acceptance still required

- Inspect 1920x1080, 1280x720 and 800x450 for card wrapping, target buttons, water transparency, actor marker, boss/imported-model framing and footer bounds.
- Exercise mouse/keyboard targets, Back/Escape, held Submit, rapid clicks, same-frame cancellation and repeated specials. Check one accepted action per confirmation and fresh focus after every action/cancel/retry.
- Exercise 3v3, 1v3 and 3v1, defeating a pending actor and a different target. Verify guards until the defender's own next turn.
- Complete victory, defeat, retry, return and another battle. Verify independent owned HP, exact team order, saved HP, one progression event, restored shore controls and usable fishing.
- Disable/re-enable during playback, repeat setup, reload scenes and inspect active camera/listener/EventSystem and visual cleanup. Interrupted presentation resumes pending enemy resolution once on re-enable.
- Placeholder water and procedural fish require visual acceptance. Fish are stylized placeholders. Imported final assets and polished animation remain future work.

## Arena-first layout update

Refresh scripts and restart Play from shore. The battle now fills the camera view. Name/HP labels track the fish, while actions and short feedback sit in a compact footer. Reapplying the theme keeps the arena visible. Verify long-name wrapping, moving labels, larger formations and target/result states at the three supported resolutions. A batch screenshot attempt failed before scene loading, so this layout still needs visual confirmation in Unity.
