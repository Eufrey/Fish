# Milestone 4: session story progression

## Required setup

In the existing Editor, exit Play Mode and allow scripts to compile. Open only
`Assets/Scenes/CoastalPrototype.unity`. Preserve unsaved work. Complete any missing
2B, 3A and 3B setup first, then choose **FISH! > Setup Milestone 4** and save.
Do not rerun 3B after adding the three story markers (its older tool expects one).
The existing battle scene/build entry must remain enabled. The 3C runtime button
fallback still works; no battle scene replacement is needed.

Setup upgrades the old test marker and adds markers seven and fourteen units to
its right. Review all three positions, safe return points and HUD layout within
your edited beach. Repeating setup preserves marker positions, enemy tuning and
HUD, and does not add cameras, input handlers, EventSystems or managers. Scene
changes support Undo; new data assets remain on disk after scene Undo.

Generated configurable assets: `Assets/Data/Fish/Story1Species.asset` through
`Story3Species.asset` and corresponding `Story1Team.asset` through `Story3Team.asset`.
Defaults are one enemy per fight: HP/ATK/DEF 14/4/0, 30/7/2, and 64/8/2.
All are fictional placeholders; unresearched biological fields stay empty.
The boss, Tangle of Tides, has **Debris Patch**, healing up to 10 HP. Its team opts
into using Special on its first enemy turn at half maximum HP or less, once per
fish per battle. It consumes the turn, cannot revive, and resets on retry.
Other enemies keep Attack/Attack/Defend. Existing species assets are unchanged.

## Behavior and files

- `FishStoryProgress.cs`: single authoritative stage plus queued one-time story
  notices. Owned by the existing `FishCollection`, alongside catches; no singleton.
- `FishCollection.cs`: only successfully claimed catches update the threshold;
  three individual catches count even when their species is identical.
- `FishStoryHUD.cs`: non-blocking ten-second introduction/unlock/ending notices
  and objective. No modal input or cursor changes. Notices pause while shore is
  suspended, so victories display their notice/ending after Return to Shore.
- `FishBattleEncounter.cs`: stage-specific entry gate both at prompt and loading;
  captures its encounter ID for victory application. Locked/completed visuals
  are hidden. Only the available marker responds to F with a valid confirmed team.
- `FishBattleSession.cs`: reports a successful terminal victory once; defeat,
  rejected actions, retry and returns do not report victory. Enemy special policy
  is captured with the encounter and retained on retry.
- `FishEnemyTeam.cs`: configurable half-health special policy. All three encounters
  reuse the existing battle scene, health snapshots and return logic.
- `Assets/Editor/FishStorySetup.cs`: setup and an Editor-only test fixture.

Confirm a team using T, close the panel, approach the unlocked marker, then press
F. Confirmation alone never launches battle. Escape/closing team selection never
launches it; after editing, confirmation is required again. Fishing remains E.
No catches are removed on loss. All progress resets with a fresh Play session;
no disk saving or persistence across unrelated scene replacement is provided.

## Safe development testing

Only in Play Mode on the shore, **FISH! > Development > Add Three Test Catches
(Play Mode)** adds three placeholder catches through the existing successful
attempt/claim path. No debug button or shortcut is present in player UI/builds.
It refuses to run while the battle scene is loaded. Stop Play to discard the
fixture. Normal fishing should still be tested separately; this fixture does
not verify real input/timing. Use actual battles to exercise subsequent stages.

## Manual Unity checklist — not yet verified

1. Fresh Play: read introduction and objective. Locked markers cannot start.
   Fail/cancel fishing, then catch twice: first encounter stays locked. Third
   successful catch (same species allowed) unlocks it with one notification.
   More catches must not repeat that notification or start battle.
2. Open T and cancel/close; no battle. Select/confirm 1–3 fish, close, then press F
   nearby. Only encounter 1 starts. Simultaneous F/T/E/Escape must not bypass UI.
3. Lose, Retry, and Return: same stage, catches and team; full HP and fresh special
   on retry. Fish again on shore. No progression from defeat or a failed load.
4. Win first: second unlocks. Spam actions/Return; only one notice after returning.
   First cannot restart, third stays locked. Win second to unlock boss similarly.
5. Boss heals only on its turn at half HP or below, once per attempt; damage and
   defense still apply. Lose/retry to check reset. Win: Completed immediately in
   state, one placeholder ending notice upon shore return; no encounter reentry.
6. Return preserves IDs/team/order, movement, camera/cursor, fishing and collection.
   Check one active EventSystem/camera, repeated setup, Undo, save/reopen, different
   Game view sizes and the three near-marker return placements.

## Checks performed

All runtime/Editor source compiled against installed Unity assemblies: no errors,
existing CS1701/CS0649 warnings. Direct compiler/runtime execution passed **21 story,
34 session, 45 battle and 38 special checks**. The story checks cover ordering,
thresholds, one-time notices/results, defeat/retry/abort, completed locks and boss
restriction. Available .NET 10 command: `dotnet run --project Tests/FishStoryChecks.csproj`;
that entry point has not itself been executed. No Unity Editor instance launched;
setup, serialization, UI, physics and actual scene transitions remain unverified.

## Narrative deferred by request
The backstory introduction and narrative unlock/ending text have been removed. New sessions start directly with the catch objective. Unlock notices are functional and the ending only says Completed. Earlier introduction/ending prose descriptions above are superseded. Add backstory only when explicitly instructed.

