# Charcoal artwork

Three portrait texture sources created with the built-in image_gen tool for this project. Full prompts are in GENERATION.json. The source images are kept intact.

At runtime StudyDrawing prepares a paper-backed charcoal target from each source texture. The pencil follows contour and hatching paths extracted by Tools/trace_charcoal.py. Pigment is deposited only inside a brush footprint along the current path. Stroke duration follows path length, the hand lifts between strokes, and the final stroke completes residual coverage. No uniform whole-image opacity reveal is used.

The studies are stylized interpretations of a teaching skeleton and a traditional plaster-and-pears still life. They are not medical anatomy illustrations.

Measured model body-to-head ratio in Play Mode, using transformed bone mesh vertices excluding the metal stand: 7.52 heads.
