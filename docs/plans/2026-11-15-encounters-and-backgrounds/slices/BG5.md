# BG5. The battle plays an animated background; a map's backgrounds decoded on entry, the last map's released

Core, Game; about 250 lines with tests (Game code estimated at 1.6x). Needs BG1, BG3, BG4. Decisions 899, 900.
Design: "Animated backgrounds (ADR 0059)", "Replacement mapping", "Risks" (the decode stall, a release while drawn).

## Goal
A battle on a map (or encounter) naming an animated background plays it: the frame for the time since the battle
screen opened, from frame 0, looping, cover-fit and point-sampled under the vignette and the battlers, and still
playing behind the victory, escape and defeat screens. A still background draws as today. Entering a map decodes the
backgrounds its battles can show, so the battle's first frame never waits on a decode, and releases the previous map's
that the new map doesn't list, so video memory holds one map's backgrounds, not every one visited.

## New types
- Game `Battle/BattleBackdrop` (internal sealed): the part of the background image shown at the battle's time.
  - `BattleBackdrop(string file, AnimatedBackgroundDefinition? animation)`;
  - `string File`; `void Advance(TimeSpan delta)` (adds the delta; a still ignores it);
  - `Rectangle SourceIn(Point imageSize)`: a still's `CoverFit.SourceIn(imageSize, screen)`; an animated one's cell at
    `animation.FrameOrigin(animation.FrameAt(elapsed), imageSize.X)`, with `CoverFit.SourceIn(frameSize, screen)`
    offset to it. No allocation (a struct out, no list).
- Game `Exploration/BackgroundRelease` (internal static): which of the last map's backgrounds to release on entering
  another. `static IReadOnlyList<string> Of(IReadOnlyList<string> previous, IReadOnlyList<string> next)`: the files in
  `previous` that `next` doesn't hold, never `UiArtFiles.Backgrounds.Title` or `.Menu` (decision 900; the menu's 200 ms
  decode is done once at startup on purpose, `JrpgGame.LoadContent`). Pure, called once per map entry.

## Changes
- Core `Exploration/EncounterBackground`: `public static IReadOnlyList<string> PossibleOn(MapSettings map,
  IReadOnlyDictionary<string, EncounterDefinition> encounters)`: the map's background, then each listed encounter's
  own, each file once, in that order; empty when the map names none and has no random battles.
- Game `Rendering/MapArt`: `IReadOnlyDictionary<string, EncounterDefinition> Encounters` (the catalog's), its param doc
  adding "and the encounters whose backgrounds a map's battles show", so the scene can work out a map's list with no
  fifth constructor parameter (`ExplorationScene` is at four).
- Game `Rendering/TextureLibrary` (about +10): `void Release(string folder, string file)`: disposes the cached texture
  and forgets it; nothing when it isn't cached. Its summary says a texture is cached until released.
- Game `Battle/BattleScene` (219 lines, about +8): `_backgroundFile` becomes `_backdrop = new BattleBackdrop(file,
  session.Catalog.AnimatedBackgrounds.GetValueOrDefault(file))` (BG1's `BackgroundFile` accessor reads `_backdrop.File`);
  `Update` calls `_backdrop.Advance(delta)` beside `_outcome.Advance(delta)`, before the finished branch returns, so the
  outcome screens keep it playing; `DrawBackdrop` draws `textures.Get(ArtFolders.Backgrounds, _backdrop.File)` over
  the screen with `_backdrop.SourceIn(...)` instead of `CoverFit.DrawOverScreen`. The class summary says "the
  encounter's background, animated or still".
- Game `Exploration/ExplorationScene` (213 lines, about +18): `EnterCurrentMap` stores
  `EncounterBackground.PossibleOn(_map.Settings, _art.Encounters)` as `_battleBackgrounds` and keeps the previous list;
  at the start of `Draw`, once per entry, `textures.Release` for each of `BackgroundRelease.Of(previous, current)`, then
  `textures.Get(ArtFolders.Backgrounds, file)` for each of the current list (index loops, no enumerator). Its comment
  says why: a sheet takes 20 to 200 ms to decode, and the battle's first frame must not (decision 900, the menu's
  precedent); released because every texture is otherwise cached for the session.
- `docs/map.md`: Game `Battle` names `BattleBackdrop`; Game `Exploration` names `BackgroundRelease`; Core `Exploration`
  names `EncounterBackground`; `Rendering` says `TextureLibrary.Release`.

## Tests
- `BattleBackdropTests` (Game, new; records built in the test, 960 x 540 frames, 4 in a 2 x 2 grid of 1920 x 1080):
  - `SourceIn_StillImage_IsTheCoverFitOfTheWholeImage` (a 2000 x 1000 image: cropped at the sides)
  - `SourceIn_AnimatedAtTheStart_IsTheFirstCell`
  - `SourceIn_AnimatedAfterOneFrameTime_IsTheSecondCell`
  - `SourceIn_AnimatedFrameOnTheSecondRow_IsOneRowDown` (after 2 frame times: column 0, row 1)
  - `SourceIn_AnimatedAfterTheWholeLoop_IsTheFirstCellAgain`
  - `SourceIn_FrameNarrowerThanTheScreen_CropsTopAndBottomWithinTheCell` (a 4:3 frame)
  - `Advance_StillImage_SourceUnchanged`
- `BackgroundReleaseTests` (Game, new):
  - `Of_FileOnlyOnThePreviousMap_IsReleased`
  - `Of_FileOnBothMaps_IsKept`
  - `Of_MenuBackgroundOnThePreviousMap_IsNeverReleased`
  - `Of_NoPreviousMap_ReleasesNothing`
- `EncounterBackgroundTests` (Core, created by BG1):
  - `PossibleOn_MapAndEncountersWithTheirOwn_ListsEachFileOnce` (two encounters naming the same arena)
  - `PossibleOn_EncounterWithoutItsOwn_AddsNothing`
  - `PossibleOn_NoBackgroundAndNoBattles_IsEmpty`
- The scene's wiring (the advance before the finished branch, the release and decode on entry) is draw-side and
  covered by the hand check's steps 1 and 3.

## Verify by hand
1. In the editor, open `milestone_outside`, **Map > Settings...**, **Battle background**: `ForestPulse.png`, **OK**,
   **Ctrl+S** (BG2's box). Run the game, walk until a battle: the forest slowly darkens and brightens behind the
   battlers, a 960 x 540 frame point-scaled at 2x, under the vignette; the HUD doesn't change. Win: it keeps pulsing
   behind the victory screen.
2. Run away from the next battle and enter another: the pulse starts from its brightest frame again.
3. Warp from `milestone_outside` into `milestone` and back: watch both fade-ins for a jump (the release and the decode on
   entry, design.md Risks); note what you see in the Done note. Open the menu right after: no stall (its background is
   never released).
4. F12 captures at two moments of one battle, a frame apart in brightness.
5. Revert `milestone_outside`'s background to `Forest.png` (**Ctrl+Z**, **Ctrl+S**), and `git status` shows it
   unchanged.
