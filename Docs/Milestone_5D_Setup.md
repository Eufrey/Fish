# Milestone 5D: compact coastal environment

## Inspection and scope

No imported terrain/environment prefabs or model files were found in Assets.
The saved shore contains the existing 24 x 20 beach blockout, one water placeholder,
one camera/light, dock, boundary colliders and three story encounters. It also
contains manually authored/imported character objects; these are preserved.
This milestone adds to that shore rather than generating an island or replacing
source assets. Runtime gameplay/health/input code is unchanged.

## Required Editor setup

1. Exit Play Mode. Open only `Assets/Scenes/CoastalPrototype.unity` in the existing
   Editor and preserve/save any ongoing manual work.
2. Choose **FISH! > Setup Milestone 5D Coast**.
3. Review the created `FISH Coastal Placeholders 5D` root and encounter labels.
   Choose **FISH! > Validate Milestone 5D Coast** and address Console warnings.
4. Walk the routes in Play Mode, then save the scene manually outside Play.

The command requires the existing exploration/fishing setup. It reuses the 3B
return-point helper to fill only missing return references for existing encounters.
It creates no terrain, water, camera, light, player, EventSystem or gameplay manager.
Materials use the existing URP Lit shader and live in `Assets/FishPrototypeMaterials`.

Repeat setup detects its root, preserves all manual edits and runs validation only.
It does not rebuild deleted pieces or relocate objects. Undo groups the scene
additions; generated material assets remain after Undo. Existing transforms,
materials, interaction references, sources and scene files are not overwritten.
The tool refuses fixed-position placement if the beach transform/dimensions have
changed; adapt the props manually for a custom environment instead.

## Layout

- Existing beach and north-side dock remain connected by the original ground.
  The dock top is 0.2 units above sand; original player step height is 0.25.
- Small open shelter near (-7, 0, -5): low floor, four posts and simple roof.
- Decorative bangka near (7, 0, 7): colored hull, crossbeams and two outriggers.
  This is explicitly a stylized placeholder, not a sailing object or accurate
  final boat model. No interaction or swimming is added.
- Flat non-colliding path markings distinguish the central dock route and the
  encounter approach area. Existing encounter positions/IDs remain intact.
- Fishing and encounter signs identify controls and confirmation requirements.
  Location labels provide setting context only; no backstory dialogue was added.
- Low non-colliding perimeter rails explain the existing invisible boundaries.
  Boundary colliders remain authoritative and are neither duplicated nor removed.
- Shelter floor/posts/roof and boat hull are solid; thin decorations and path
  markings have no colliders. Default spawn and return approaches remain clear.

Manual editing: keep the central route and each encounter's return point clear.
If moving the dock, maintain a connected approach below the player's step height
(or add a gentle ramp), keep its fishing point within reach, and move its sign/path
as needed. Preserve the existing boundary colliders and shore-ground references.

## Verification

`Assets/Editor/FishCoastSetup.cs` contains both commands; no runtime system changed.
All runtime/Editor sources compiled externally with zero errors (existing reference
and serialized-field warnings). Static saved-scene audit found exactly one collider
on the beach, dock, four boundaries and exploration player, and one camera/light.
The saved spawn is (0, 1.1, -3); dock and beach retain their existing geometry.
Gameplay tests were not rerun because only Editor environment authoring was added.

**Not run in Unity:** setup/Undo/save/repeat behavior, physical clearance validator,
actual collision/step traversal, route connectivity, signs/materials and camera
visibility. No connected Editor tool was available; no second Editor was launched.
The scene is unchanged on disk until you run setup and save.

Validator scope: checks named solid colliders, player/return-point projected capsule
clearance and dock step connection. It reports problems without relocating authored
objects. It does not prove all paths are connected, validate camera obstruction,
or simulate movement. Existing battle return logic still checks multiple ground
candidates at runtime and restores the original entry position if all are blocked.

## Manual acceptance checklist

- Run setup twice: one coast root, no extra water/terrain/cameras/lights; tuned
  object positions remain unchanged. Check Undo and save/reload.
- Spawn safely; walk to the dock, all three encounter approaches and around the
  shelter/boat. Step onto/off the dock without jumping. Walk all four boundaries:
  no escape into water or through the edges.
- Fish, open collection/encyclopedia/team, recover selected fish: no regressions
  in prompts, cursor, movement or camera. Inspect signs from normal approach angles.
- Battle at each available marker, lose/retry/return and win/return. Verify safe
  near-marker placement, no trapped capsule, unchanged owned IDs/current HP/team,
  expected progression and usable fishing after return.
- Check roof/boat do not obscure the orbit camera or block routes at target window
  sizes. Adjust only new props if the manually edited character/environment needs
  additional clearance; rerunning setup will preserve your adjustments.

## Missing final assets

Final coastal terrain/sand/water art, accurate bangka model, shelter art, dock detail,
polished signs/textures and environment dressing remain absent. This functional
blockout deliberately adds no islands, sailing, swimming, packages or downloads.
