# MB2. The banner drawn plain at the top right

Game, about 150 lines raw, about 240 at the 1.6x a view type costs. Builds on MB1 and design.md, "What exists" (entering a map in the game). Decisions 527, 528, 529.

## Goal
Entering a map whose settings show the banner shows its display name and, when it has one, its area level ("Lv 3") at the top right of the screen, fading with `MapBanner`; with no level, the name alone; it waits while an event runs (the warp's fade, an arrival cutscene) and shows after. The look is plain here (body font, white text, no band); MB3 gives it the reference's look.

## New types
- `Game/Exploration/MapBannerView`: draws the banner at the top right, its text measured once per shown map.
  - Constructor `(ScreenText text, MapBanner banner)`; `Update()` re-measures only when `banner.Shown` changed (a reference check, as `InteractionPromptView` does); `Draw(SpriteBatch batch, TextureLibrary textures)` draws at `banner.Opacity`, nothing at 0.
  - The level line from `text.Messages` (`MessageIds.MapBannerLevel`, "Lv {level}"), formatted when the map changes, never in `Draw`.
- `Game/Exploration/FieldOverlays` (sealed record `(InteractionPromptView Prompt, MapBanner Banner, MapBannerView BannerView)`): what the exploration scene shows over the field besides events. The scene drives `Banner` and draws `BannerView`, which reads the same `MapBanner` (given to its constructor), so nothing forwards.

## Changes
- `Game/Exploration/ExplorationScene` (194 lines): the fourth constructor parameter `InteractionPromptView prompt` becomes `FieldOverlays overlays` (still 4). `EnterCurrentMap` calls `overlays.Banner.Enter(_map)`; `Update` calls `overlays.Banner.Advance(delta)` after the presenter's update only while `!_presenter.IsBusy`, then `overlays.BannerView.Update()`; `Draw` draws the banner view after the prompt and before the presenter (an event's message box covers it).
- `Game/Flow/SceneFactory.CreateExploration`: `var banner = new MapBanner();` then `new FieldOverlays(new InteractionPromptView(...), banner, new MapBannerView(_text, banner))`.
- `tests/Jrpg.Game.Tests/Exploration/StartMapScenarios` (line 133): the same construction (the one other site).
- `content/text/ui.json`: `"map_banner_level": "Lv {level}"` (laid out as the file is); `Game/Ui/MessageIds.MapBannerLevel`; the message id test covers it as for every id.
- `Ui/UiTheme`: `MapBannerRight`, `MapBannerTop` (design.md tunables).
- `docs/map.md`: the Game Exploration row adds `MapBanner`, `MapBannerView`, `FieldOverlays`.

## Tests
`Game.Tests/Exploration/ExplorationSceneBannerTests` (new; through `StartMapScenarios` on the test world, 1/60 s updates):
- `Update_NewGameStart_ShowsTheStartMap` (the `MapBanner` the test built into `FieldOverlays` shows the start map, opacity above 0 once its wait passed).
- `Update_EventRunning_BannerWaits` (an arrival or interact event running: opacity stays 0 for longer than the wait).
- `Update_WarpToAnotherMap_ShowsThatMap` (a test world warp run to its end).
- `Update_BattleFinished_DoesNotRestart` (the same map reference: no restart).
- A map with the banner off is MB1's `Enter_MapNotShowingTheBanner_ShowsNothing` (the frozen test world has no such map, ADR 0037).
`Game.Tests/Exploration/MapBannerViewTests` (new; the formatting read through a small internal `LevelLine` accessor, the fonts as `RepositoryScreenText` gives them to the other view tests):
- `Update_MapWithLevel_FormatsTheLevelLine` ("Lv 3" from a test map with 3).
- `Update_MapWithoutLevel_HasNoLevelLine`.

- **Done, with deviations:** `MapBannerView.Draw` takes the batch alone (the plain look needs no texture; MB3 adds it with the band); the plain size is `UiTheme.MapBannerTextPixels` (36), beside `MapBannerRight` and `MapBannerTop` in `UiTheme.Dialogue.cs`. `StartMapScenarios.SceneOf` gained an overload taking the `FieldOverlays` (`OverlaysFor` builds them as the factory does), so a scene test reads the view too (`NameLine`, added for that). The battle test is left out: a battle pushes over the scene (`BattleTransitions`), which never enters the map again, and MB1 pins the same-map rule; a scene stepped past its banner guards nothing. Not captured: the look is the hand check's.

## Verify by hand
1. Give "start" an area level (AL1) or not, F5, New game, pass the intro: after the fade from black, "Start map name" and "Lv n" appear at the top right, hold about three seconds and fade out.
2. Walk through a door to another map: after the fade from black the other map's name shows; back through the door: the first map's name shows again.
3. Open the menu while the banner shows: it stays still under the menu and carries on after.
4. Win a battle: no banner on returning to the map.
5. Load a save: the saved map's name shows.
6. A map without an area level: its name alone. A map with Show banner on entry unticked (AL2): nothing shows on entering it, and the next map entered shows its banner.
