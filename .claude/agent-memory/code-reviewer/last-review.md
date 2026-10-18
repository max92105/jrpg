---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 1b (Input mapping)
Round: 1 (verdict: Ready to commit; caller said this is the single review round)

Accepted (don't re-raise): ButtonSet bitset struct not in plan; StickReader.FromKeys renamed FromDirectionButtons taking held ButtonSet; tab buttons have no keyboard key (question 50); Esc maps to both Cancel and Menu (legacy).

- open (Suggestion): StickReader.FromStick uses `< Deadzone`, so exactly 0.2 moves; the boundary is untested; pin it with a 0.2f test
- open (Suggestion): InputFrame exposes Held/Pressed positional properties next to IsHeld/IsPressed; two ways to ask the same thing
- open (Suggestion): InputBindings.Keyboard/GamePad properties shadow the MonoGame Keyboard/GamePad types; KeyBindings/PadBindings reads clearer
