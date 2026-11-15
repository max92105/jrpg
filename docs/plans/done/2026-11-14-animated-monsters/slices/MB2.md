# MB2. The Enemies window's idle boxes and animated preview

Editor, about 210 lines (about 90 of them tests). Needs MB1. Decision 631. Design: "Structure" (Editor,
`EnemyArtSources`), tunable "New idle boxes".

## Goal
The Art tab has an **Idle animation** group: **Animated** (a check box), **Frames** and **Frame time (ms)** boxes, kept
as typed; the battle sprite preview shows the loop playing on the editor's animation clock (mirrored as the battle
mirrors it), with "8 frames of 100 x 128 px" under it, or "1000 px doesn't split into 6 frames" when the strip doesn't.

## New types
- `Editor/Battle/EnemyArtSources` (internal sealed record): what the Art tab reads and copies pictures with:
  `Func<string, IReadOnlyList<string>, SheetImportResult> Import`, `Func<string, BitmapSource?> BattlerImageOf`,
  `TileAnimationTime AnimationTime`. Keeps `EnemyCatalogWindow` at four constructor parameters.

## Changes
- `Editor/Battle/EnemyArtViewModel` (71 → about 115): `[ObservableProperty]` `IsAnimated`, `FrameCountText`,
  `FrameMillisecondsText` (from the record, or "2" and "100" when it has none); `string? Problem` (a box that isn't a
  whole number while animated, through `Catalogs/TypedNumber`; ranges are Data's); `ApplyTo` sets `IdleAnimation`
  (null when not animated); `CopyFrom` copies them (Duplicate); `TileAnimation? PreviewLoop` (the boxes as read, null
  while one can't be: what the preview plays, never a throw, `.claude/rules/editor.md`). Its summary names the loop.
- `Editor/Battle/EnemyViewModel`: its `Problem` chain asks the art's problem after the numbers'.
- `EnemyCatalogWindow.xaml` (190 → about 205): the group under the battle sprite's box, greyed boxes while not
  animated.
- `EnemyCatalogWindow.xaml.cs` (114 → about 145): takes `EnemyArtSources` instead of `import` and `battlerImageOf`;
  while the chosen enemy is animated and its strip reads, the preview shows the frame `PreviewLoop.FrameAt(time)` as a
  `CroppedBitmap` of the strip, on `AnimationTime`'s `FramesChanged`-style tick (unsubscribed on `Closed`); the size
  line as above.
- `Shell/MainWindow.Database.cs`: builds the `EnemyArtSources` (`_viewModel.AnimationTime!`).
- `docs/map.md`: the Editor `Battle` row names the idle boxes and `EnemyArtSources`.

## Tests
`EnemyArtViewModelTests` (65), enemies built in the test:
- `New_EnemyWithLoop_FillsTheBoxes` (6 frames, 120 ms: a fixture other than the defaults).
- `New_EnemyWithoutLoop_IsNotAnimatedWithDefaultBoxes`.
- `ApplyTo_Animated_SetsIdleAnimation`; `ApplyTo_NotAnimated_ClearsIt` (an enemy that had one).
- `Problem_AnimatedFramesNotANumber_NamesTheBox`; `Problem_NotAnimatedFramesNotANumber_IsNull` (the guard's other side).
- `PreviewLoop_BoxNotANumber_IsNull`.
- `CopyFrom_Animated_CopiesTheLoop`.
`EnemyCatalogViewModelTests` (120): `Problem_FrameCountOne_IsDatasMessage`.

## Verify by hand
1. Run the editor. **Database > Enemies...**, choose `goblin`, **Duplicate**, type an id `strip_test`, **Art** tab.
2. Check **Animated**, Frames `2`, Frame time `300`: the preview alternates the left and right halves of
   `Goblin.png` every 0.3 s, mirrored as in battle; the line says "2 frames of ... px".
3. Frames `3`: the line says the width doesn't split into 3 frames (the preview still plays what it can, OK still
   applies: the game's start refuses it). Type `x`: the problem names Frames and OK greys.
4. **Cancel**: nothing changes.

- **Done, with deviations:** the preview follows `CompositionTarget.Rendering` and `AnimationTime.Elapsed`, as the
  New NPC window's walk does: `FramesChanged` fires only for the tiles' loops. `PreviewLoop` is also null while the
  frames or the time is under 1 (a zero would divide by zero in `FrameAt`), with two tests more
  (`PreviewLoop_NoFrames_IsNull`, `PreviewLoop_Animated_IsTheBoxes`). The shell opens the window only with the
  clock, beside the enemy editing guard, instead of `AnimationTime!`. `EnemyArtViewModel.cs` 71 → 118, the window's
  code 114 → 167, its XAML 190 → 205.
  Review: no Critical or Warning; the slice's `TypedAnimation` is named `PreviewLoop` (what the preview plays, beside
  the private `TypedLoop` the record gets); the window reads the sprite only when its file or the folders change and
  keeps the loop between render ticks.
