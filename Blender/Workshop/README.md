# Design workshop artwork — 2026-10-11

Open `Workshop.blend` to edit the individual Worktable, Cabinet, Cutter, Ruler, PaperStack, LeftHand and RightHand collections. The file includes packed material images and a table inspection camera; other asset collections are hidden from that camera render, not removed. `Worktable.png` is an offline inspection render, not a game screenshot.

The table matches the existing cover collision dimensions. Its bevels, square steel tubing, recessed fasteners and rubber feet are visual details. The closed workstation cabinet explains its existing solid collider. The cutter carries 5 mm model stock; paper stacks use thin offset leaves; the ruler has engraved divisions. Imported artwork adds no gameplay colliders.

Five authored 1024 px material sets (Wood, Mat, Paper, Floor, Cork) contain base color, tangent normal and packed metallic/smoothness maps. Surface RGB is metallic; alpha is smoothness for Unity Standard. Cork is used for the pin-up screens, wood for furniture, matte printed grid for cutting mats, and staggered board ends for the floor. Steel, powder coat, knit sleeves and skin have separate material responses.

## Rebuild

1. From the project directory, run Blender in background with `--python Tools/build_workshop_art.py`.
2. In Unity, choose **Study > Import Workshop Artwork**. This imports the JSON into Mesh assets and prefabs, preserving existing asset references. Normal/surface texture color-space settings are configured by the importer.
3. Run the `designqa` bridge command to check work interactions, cover escape, pursuit and the new resource/material assignments. Run `menuqa` for the entry and return flow.

The runtime loads the generated `Resources/Workshop` prefabs; Blender and the source JSON are not needed at runtime. Exported geometry preserves per-corner normals around bevels.

## Hand attribution

Hands derive from the existing [immersive-web/webxr-input-profiles generic-hand](https://github.com/immersive-web/webxr-input-profiles/tree/main/packages/assets/profiles/generic-hand), MIT License, Copyright (c) 2019 Amazon. The full required license is retained at `../GenericHand-LICENSE.md` and `../../Assets/Study/ArtV5/GenericHand-LICENSE.md`. This version changes the pose, smooths the surface and adds separate ribbed cuffs. It is a static posed mesh animated by the workshop interaction, not a new fully rigged character.

Current triangle totals: worktable 3,948; cabinet 1,316; cutter 752; ruler 1,108; paper stack 564; each hand including cuff 14,268. These counts are asset metadata, not a frame-rate benchmark.
