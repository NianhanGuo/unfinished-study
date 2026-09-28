# V7 Blender hands

HoldingHand.obj and WritingHand.obj are posed, wrist-remodeled hand meshes created in Blender 5.1.1. No nail geometry, nail material, nail tips or decoration is included.

The anatomy begins with the MIT-licensed generic-hand source from immersive-web/webxr-input-profiles (see LICENSE.md). Anatomical joint poses, a continuous oval wrist/forearm, voxel joining, localized wrist smoothing and subdivision are created by the Blender generator. Both posed meshes and the editable joint rigs are saved in HandModels-v7/StudyHands.blend. Inspection props are not exported into the hand meshes.

OBJ coordinates are the game's board-local metres. StudioRealism reads those coordinates directly, bypassing generic OBJ axis conversion. Blender inspection layout mirrors the presentation coordinates to match the game's handedness.

Open StudyHands.blend in Blender. The visible HoldingHand and WritingHand are the baked export meshes. Hidden editable skin / pose rig objects retain deform groups and joint poses; unhide them to edit. Re-export using Tools/build_blender_hands.py in the supplied Unity source project. Inspection board, charcoal and camera are display aids only. Game meshes are static poses moved by the existing drawing animation.
