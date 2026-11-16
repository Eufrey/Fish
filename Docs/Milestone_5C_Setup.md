# Milestone 5C: procedural placeholder battle visuals

## Required Unity setup

1. Exit Play Mode in the existing Unity Editor and allow compilation.
2. Choose **FISH! > Setup Milestone 5C Visuals**. This existing tool creates
   reusable prefabs/materials under `Assets/FishPlaceholderVisuals` and assigns
   only empty `FishSpecies.model` references. It also covers story enemies/boss.
   Run this before a player build so the URP Unlit materials/shader are included.
3. Start Play from the shore and enter an unlocked encounter with a usable,
   confirmed team. The existing battle scene installs its preview at runtime;
   no scene replacement or additional camera/EventSystem is required.

Setup saves generated assets and model assignments. Repeating it reuses existing
prefabs/materials and preserves assigned imported models. Undoing assignments
leaves the generated assets intact. Source asset names/IDs remain unchanged.
Current saved species model references were empty during implementation; the
setup command has **not been executed here**.

## Visuals and integration

The reusable primitive builder makes an ellipsoid body, split block tail, dorsal
and side fins, and eyes on both sides. Primitive colliders are removed immediately
in Editor creation, or disabled then destroyed at runtime. Colors intentionally
identify gameplay entries rather than biological appearance:

- Bangus: cyan-blue.
- Asohos: golden yellow.
- Samaral: green.
- Kitang: lilac.
- Mangagat: coral red.
- Environmental boss: deep purple with cyan fins/debris ridges and larger framing.

Names and current/max HP appear above the models. A visible caption explicitly
says these are stylized placeholders, not accurate species representations.
URP Unlit materials preserve the existing render pipeline; no real-fish textures,
custom icons or animations are needed. Missing encyclopedia icons retain the
existing text fallback; missing/unrenderable battle models use primitive fish.

The existing battle camera renders a fixed-aspect preview into the battle UI.
Renderer bounds fit each model into a reserved half-view box, including imported
models with unusual scales/pivots. The stage is a separate world root so UI canvas
scaling does not affect its geometry. The boss has a larger box. Both opposing
fish have visible fins/eyes. Previous visuals are deactivated before deferred
destruction; fighter identity changes trigger replacement, including new retry
attempts and separate same-species catches. Defeated final fish leave an empty side.
Temporary materials/render texture/stage are cleaned up on scene teardown.

The encounter passes species arrays aligned with the captured battle team. Active
battle indices choose the matching model, including skipping zero-HP fish. Visual
creation reads species/fighter data only; it cannot call recovery, commit HP,
award catches or change stats, team IDs or progression.

## Imported prefabs later

Assign a **visual-only prefab** to the existing species Model field. Orient it
along local +X, with Y up; the enemy side is turned to face the player. Include
active renderers and URP-compatible materials. Do not include gameplay scripts,
cameras, input managers or scene controllers in a visual prefab. Renderer bounds
are normalized automatically. Runtime presentation disables colliders, Rigidbody
collision/simulation and Animator playback on the instance. The original prefab
is not edited. Setup will not replace an already assigned model. The global
placeholder disclaimer remains until a future art milestone explicitly replaces it.

## Relevant files

Reused existing partial 5C work: `FishPrimitiveVisual.cs`, `FishBattleVisuals.cs`,
`FishVisualSetup.cs`, species-array integration in `FishBattleEncounter.cs` and
`FishBattleScene.cs`. Finished here: canvas-independent world stage, explicit
preview aspect, cleanup, larger boss framing, bilateral fins/eyes; documented
prefab expectations on `FishSpecies.model`. No gameplay rule or species asset
values changed in this pass.

## Checks actually performed

- All runtime and Editor scripts compiled externally against installed Unity
  assemblies: zero errors; existing reference/serialized-field warnings remain.
- **11 health + 34 battle-session + 21 story checks passed** through the local
  C# compiler/runtime. These verify rule regressions, not Unity rendering.
- Static check of species IDs and palette mapping: all five catchables and boss
  match existing data. Visual/setup code has no health/recovery/stat-write calls.
- Calculated maximum fitted boxes remain within the orthographic camera:
  at least 1.03 horizontal and 0.35 vertical world-unit margin. This calculation
  does not establish actual rendered appearance.

No connected Unity Editor tool is available. No extra Editor instance was launched.
**Unity setup, prefab serialization, rendering, shader inclusion, UI text clipping,
actual replacements/cleanup and imported-model fitting remain unverified.**

## Manual acceptance checks

- Run setup twice: one prefab/material pair per unassigned species, no duplicates
  on repeat; no colliders in generated prefabs; imported assignment preserved.
- Enter battle with each species (use existing catch catalog test procedure):
  correct distinct color and visible name, disclaimer readable, HP unchanged.
- Use two same-species catches with different HP: replacement changes the active
  name/HP instance correctly. Skip a zero-HP lead. Defeat enemy/player and verify
  only one visual per active side; last defeat clears that side.
- Recover and Retry: full HP only because recovery was requested, fresh visual and
  special usage. Return and reenter: stored HP is preserved. Fishing/team/story
  behavior must remain unchanged.
- Boss: larger purple/cyan ridged fish, no clipping, correct boss name/HP.
- At 1280x720, 1920x1080 and smaller 16:9 Game views: check preview, labels, action
  buttons, feedback and disclaimer do not overlap. Check imported off-center/large
  prefab and missing-model fallback. Check a build for missing/pink shaders.
