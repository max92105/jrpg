# NB5. The idle loop in the Character sprites window

**Later: not in the approved plan** (README, "Later"). A draft kept with NB4.

Editor, about 180 lines (90 of them tests). Builds on NB4 and SI4.

## Goal
The setup gets an **Idle** group: a "Has idle" box, the four idle rows, the idle frame count and time; the preview's **Walk / Idle** switch plays either loop for the four facings. OK writes the idle loop with the record.

## New types
None.

## Changes
- `Editor/Sprites/FieldSpriteSetupViewModel`: `HasIdle` and `Idle` (a second `FacingCycleSetup`, SI2's type, starting from the rows under the walk's: walk rows + 4, 2 frames, 500 ms, when the image has the rows; else the walk's); `IsPreviewingIdle`; `Problem` asks `Idle.ProblemWithin` while `HasIdle`, before Data's message; `ToSheet` writes `Idle` only with `HasIdle`; `PreviewCellAt` reads the idle loop while `IsPreviewingIdle`.
- `Editor/Sprites/FieldSpriteWindow.xaml`: the Idle group under the walk's, the switch over the preview.

## Tests
`FieldSpriteSetupIdleTests` (new sibling): `From_RecordWithIdle_ReadsIt`; `ToSheet_HasIdleOff_WritesNone`; `ForNew_SevenRowSheet_GuessesIdleRowsUnderTheWalk`; `Problem_IdleRowPastRows_NamesIt`; `PreviewCellAt_PreviewingIdle_StepsTheIdleRow`; `HasIdle_TurnedOff_ClearsIdleProblem` (the note refreshed).

## Verify by hand
1. Objects > Character sprites..., choose the bought sheet: tick Has idle, set the four idle rows to its two-frame poses as numbered on the image, 2 frames, 500 ms; switch the preview to Idle: the four facings breathe (or whatever the poses do).
2. OK, Ctrl+S, start the game: the NPC idles on the map. Ctrl+Z in the editor before saving takes the idle loop away with the rest of the session.
3. Revert with `git checkout -- content` if this was only a check.
