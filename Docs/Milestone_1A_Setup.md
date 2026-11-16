   # Milestone 1A: third-person exploration

This implements the movement foundation from the root `FISH_Game_Design_Document.md`.
The existing scene, input asset, packages, lighting, and URP configuration are preserved.
There is no jumping, swimming, fishing, or decoration in this milestone.

## Unity setup

1. Open this project in Unity **6000.6.3f1** and let scripts import.
2. Open **Assets/Scenes/CoastalPrototype.unity** as the only loaded scene.
3. Outside Play Mode, choose **FISH! > Setup Milestone 1A**.
4. Review the generated objects and save the scene with **Ctrl+S**.
5. Enter Play Mode and click the Game view.

No package installation or Input System changes are required. The project already uses
the new Input System. If Unity has not discovered the command, use **Assets > Refresh**
and check the Console for compilation errors.

The setup reuses the existing camera and attaches `FishOrbitCamera`. It adds one
`FISH Exploration Test Area` root containing a capsule, a 24-by-20-unit beach,
water scenery, and four invisible boundary colliders. The player uses a
`CharacterController` instead of a Rigidbody or a second capsule collider.
Water has no collider and is outside the enclosed walking area.

Three simple URP materials are created in `Assets/FishPrototypeMaterials` when
the command runs. Existing materials at those paths are reused without edits.
The script never saves or replaces a scene automatically.

Running the command again detects `FishExplorationArea` and leaves the existing
setup and Inspector values alone. It does not repair a manually deleted child;
use Undo for accidental edits. Setup refuses multiple loaded scenes, multiple
cameras, an inactive camera, or a pre-existing player/controller rather than
adding conflicting objects. It creates no PlayerInput, EventSystem, or input asset.

Immediately after setup, **Edit > Undo** removes the scene additions as one group.
Generated material assets remain available for reuse.

## Controls and tuning

- **WASD:** move relative to the camera. The existing Move action also supplies
  arrow keys and gamepad movement; no extra traversal actions are enabled.
- **Mouse:** orbit. Vertical pitch is limited to 10–65 degrees.
- **Escape:** release the cursor and stop horizontal movement.
- **Left click in the Game view:** capture the cursor and resume exploration.
- **Space:** no action.

Select the player to adjust movement speed, rotation speed, and gravity.
Select Main Camera to adjust orbit distance, sensitivity, target height, and pitch limits.
Defaults are 4 units/second movement and a 6-unit camera distance. The capsule
spawns slightly above the sand so gravity settles it onto the ground.

The player clones and enables only the existing `Player/Move` action and disposes
that clone on disable. Camera input reads the new Input System's Mouse/Keyboard
devices. No legacy Input API or extra input manager is used.

## Play Mode checks

1. Confirm one player and one camera. The camera should follow the capsule.
2. Walk with WASD, orbit roughly 90 degrees, then press W: movement should follow
   the new camera direction. Diagonal movement should not be faster.
3. Move the mouse far up/down: the camera must stop at the pitch limits and stay
   above the flat test ground.
4. Confirm the spawn settles onto the ground. Walk into all four edges and
   corners, including while moving diagonally: the player must stay on the beach.
   The north edge blocks entry into the water.
5. Press Space: there should be no jump. Escape releases the cursor; clicking
   resumes. Switching application focus releases the cursor as well.
6. Exit Play Mode and run setup again. Object counts, transforms, and tuned
   Inspector values should remain unchanged. Save, reopen, and retest the scene.
7. Check the Console for errors during these steps.

## Validation performed during implementation

All four new C# files compiled together against the installed Unity and Input
System assemblies using the local C# compiler. The external compiler reported
one framework reference compatibility warning from the project's reference set;
it reported no source compilation errors. Existing WASD bindings, package versions,
and scene contents were inspected. No existing scene, input asset, package file,
or render setting was edited.

There was no connected Unity Editor tool. The menu command has not been executed
and Play Mode behavior, Undo, material appearance, collision, cursor capture, and
repeat-run behavior still require the Editor checks above. Camera obstacle
avoidance is outside this flat, undecorated milestone and will be needed if
obstructing scenery is introduced later.
