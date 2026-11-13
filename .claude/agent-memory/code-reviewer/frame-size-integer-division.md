---
name: frame-size-integer-division
description: Sprite setup derives columns/rows as image / frame size with integer division, silently changing the frame size
metadata:
  type: project
---

Seen twice (SI2 `FieldSpriteSetupViewModel.From`, SI3 `CopySetupFrom`): `Columns = imageWidth / frameWidth` then
`FrameWidth = GridCut.LengthAlong(imageWidth, Columns)`. When the image has spare pixels the frame size comes back
different (128 / 48 -> 2 columns -> 64 px) and GridCut reports no problem, because any whole column count splits.

**Why:** decision 570 wants a problem when the image doesn't split into the copied frame size; Data's HoldsFrames is >=.
**How to apply:** for any new code turning a frame size into columns/rows, test an image with a remainder
(e.g. 144 px over 32 px frames) and check the frame size and Problem afterwards.
