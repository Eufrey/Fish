# Milestone 1B: fishing spot interaction

This adds one nearby interaction and a placeholder fishing panel. It does not
catch fish, run a minigame, or trigger battles. The existing exploration scene,
URP settings, input asset, player, and camera are preserved.

## Setup in Unity

1. Open the project in Unity **6000.6.3f1**. Wait for script compilation.
2. Open **Assets/Scenes/CoastalPrototype.unity** as the only loaded scene.
3. Outside Play Mode, run **FISH! > Setup Milestone 1A** if the exploration
   player and camera have not been set up yet. The saved scene inspected during
   implementation still contained only the original camera, light, and volume.
4. Run **FISH! > Setup Milestone 1B**.
5. Review the additions, save the scene, and enter Play Mode.

The new menu command adds a `FISH Fishing Interaction` root with:

- A simple wooden-colored box dock near the water at the north beach edge.
- A `Fishing Interaction Point` at world position **(0, 1.2, 8)**, with a
  default interaction radius of **2.5 units**. Select it to see the radius gizmo.
- A screen-space UI canvas containing the nearby prompt and fishing panel.
- One EventSystem with `InputSystemUIInputModule`, only if none exists.

The dock is inside the existing test-area boundary. The boundary remains intact,
so this milestone cannot introduce swimming or walking into the water.

Existing compatible EventSystems are reused without changing their mappings.
If an EventSystem has no input module, setup adds one using the existing asset's
UI action references. Duplicate, inactive, or legacy input modules cause setup
to stop before editing the scene; read the Console message and resolve that
configuration first. No StandaloneInputModule, PlayerInput, or new input asset
is created.

Running setup again preserves the existing fishing interaction, UI, and tuning.
It does not rebuild manually removed children. Undo the setup immediately to
remove its scene additions as one group; the generated `Dock.mat` asset remains
available. Scene changes are never saved automatically. Existing materials at
the generated material path are reused without modification.

## Behavior and controls

- Approach the dock: **Press E to Fish** appears only within range and while
  exploration input is enabled. It is hidden while the panel is open.
- Press **E** once to open the panel immediately. This reuses `Player/Interact`
  through a private action clone. `WasPressedThisFrame` bypasses the template's
  Hold delay without modifying the shared action asset.
- While open, horizontal movement, turning, mouse orbit, and camera cursor
  recapture are blocked. Gravity and camera following continue normally.
- The cursor is unlocked and visible. The placeholder panel has a clickable
  **Return to beach** button using the new Input System UI module.
- Press **Escape** or click **Return to beach** to close. The previous movement
  input, camera input, cursor lock, and cursor visibility states are restored.
- If you opened the panel with the exploration cursor already released, closing
  leaves it released. Click the Game view to resume exploration as in 1A.

Escape closing is handled only by the fishing interaction. The camera skips
input for that frame so the same Escape press cannot immediately release the
restored cursor. Closing with a UI click likewise cannot also orbit the camera.
The action and button listener are cleaned up when the interaction is disabled.

## What changed

- `FishExplorationPlayer.cs`: explicit input gate; collision and gravity retained.
- `FishOrbitCamera.cs`: explicit input gate and one-frame protection on restore.
- `FishFishingSpot.cs`: proximity, prompt, panel lifecycle, and input/cursor restoration.
- `FishFishingSetup.cs`: repeatable Editor setup and compatible UI input wiring.
- New script metadata and this guide. Existing scene and input asset files were
  not edited by the implementation; the menu commands apply the scene setup.

## Test in Play Mode

1. At the spawn point, confirm there is no fishing prompt. Press E: nothing opens.
2. Walk north with W to the dock. Confirm the prompt appears only near the point,
   disappears when backing away, and reappears when approaching again.
3. Tap E briefly. Confirm the placeholder panel opens without holding E and the
   prompt disappears. Holding E must not toggle or reopen the panel repeatedly.
4. While open, hold WASD, move the mouse, and click the background. The player
   must not walk/turn, the orbit must not change, and the cursor must stay visible.
5. Press Escape. Confirm the panel closes, the cursor returns to its previous
   state, WASD works, and the mouse orbits again. It must not require a second
   click when the cursor was locked before opening.
6. Open again and click Return to beach. Confirm the button works and controls
   restore. Repeat both close methods at least ten times; check the Console.
7. Near the dock, press Escape during exploration to release the cursor, then E
   to open and Escape to close. The cursor should remain released until clicked.
8. Open the panel, switch application focus away and back, then close it. Check
   that there is no stuck cursor/input state. Stop and restart Play Mode as well.
9. Outside Play Mode, rerun setup. Confirm there is still one fishing interaction,
   one dock, one fishing canvas, one player, one camera, and one EventSystem with
   one compatible UI input module. Save/reopen the scene and repeat steps 1–6.
10. Confirm all four original beach boundaries still stop the player.

## Verification limits

The runtime and Editor scripts were compiled against the locally installed
Unity, uGUI, and Input System assemblies. There were no source compilation
errors. The external compiler reports the same framework-reference compatibility
warning seen in Milestone 1A. Existing Interact and UI bindings and imported
action-reference support were inspected in the project/package source.

No Unity Editor control tool was connected. The menu command, layout, click
handling, proximity, Play Mode control restoration, Undo, repeat setup, and
save/reload persistence still need the Unity checks above. No packages were
installed, and no battle or collection system was added.
