# Dizon coastal scene integration

Open `Assets/Scenes/CoastalPrototype.unity`. The integrated scene is already saved.

The compact shore uses a sculpted copy of the package terrain data, warm sand, grassy dunes, selected package rocks, a rowboat, fishing barrel and dock mesh. Three stylized primitive palms frame the edges. The existing shelter and decorative bangka remain landmarks. The central beach stays open for movement and encounters.

`FISH! > Integrate Dizon Coast` creates the layout once. Later runs validate the existing integration and preserve authored placement. `FISH! > Validate Dizon Coast` checks terrain/camera/water counts, dock collision and ground support at the player, central approach and battle return points. The original package and terrain data remain available. Generated working assets are under `Assets/Art/DizonCoast`.

The original beach collider remains the flat playable ground and battle-return reference. Its renderer is disabled. The visible terrain lies slightly below that surface in the playable area and rises into perimeter dunes. Existing boundary collision remains active. New scenery colliders are disabled and props are placed away from the central routes. The original dock collider supports the imported dock visual.

Compatibility adjustments replaced removed GetInstanceID calls with GetEntityId in imported AQUAS_Lite_Reflection.cs and WaterVolumeBase.cs. Duplicate legacy TutorialInfo/Scripts readme classes were excluded because the project already provides those classes. The supplied unitypackage retains those original files.

Verified in Unity 6000.6.3f1 through successful batch compilation, integration, scene save, repeat setup and final validation. Rendered and reviewed `Logs/DizonCoastPreview.png`. The shader warmup capture revealed the old blockout cross/rails, which were hidden in the finished scene. Unity's native serialized scene emits some empty-value trailing spaces.

Manual Play Mode checks remain for walking the full perimeter, dock traversal, camera clearance, fishing, all story encounter approaches and battle return. The setup checks establish ground support and object counts, not a completed gameplay session.
