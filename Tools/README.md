Optional asset generators. Run with Python 3 from any directory.
Dependencies: numpy, scipy, scikit-image, Pillow.

- hands.py: creates the continuous posed hand meshes used by the V4 viewmodel.
- sculpt.py: creates skull, jaw, portrait and palm OBJ meshes.
- sketch_paths.py: creates blank paper and procedural fallback studies.
- trace_charcoal.py: reads ArtV3 charcoal images and creates length-weighted drawing paths for the game. Run this after sketch_paths.py so the final strokeBook.json describes the charcoal sources.

All generated assets are included. No Python is required to play or build in Unity.

- build_blender_hands.py: V7 anatomy, poses, continuous wrists and exports. Run in Blender 5.1: `blender --background --factory-startup --python Tools/build_blender_hands.py`. Uses Blender-bundled numpy, no external Python setup. Writes editable Blender/StudyHands.blend and ArtV7 OBJ files.
