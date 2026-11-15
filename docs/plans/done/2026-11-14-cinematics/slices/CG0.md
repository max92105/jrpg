# CG0. `EventPresenter`'s cutscene part into `CutscenePresenter`

Game, about 110 lines moved, none new. A pure move. Design: "Structure" (Game).

## Goal
`Game/Exploration/EventPresenter.cs` (294 lines) has room for the cinematic: its still and voice handling leaves for a
`CutscenePresenter` it owns. The backlog line suggests moving the rest's handling instead; the cutscene part moves here
because the next slices grow it (noted in the backlog line's place).

## New types
- `Game/Exploration/CutscenePresenter` (internal sealed): shows the running event's cutscene: its stills, the black
  around them and the voice it asks for. Constructor `(ExplorationState exploration, StillView view, VoiceChannel
  voice)`. Takes from `EventPresenter`: the `StillRun` field, `IsShowingStill`, the lead-out branch of `Update`
  (`AdvanceLeadOut`), `ShowStill`, `FollowStartedVoice`, the stills' part of `Blackness` and of `Draw`. It returns
  whether the run just ended (`bool Show(StillStep, TimeSpan, InputFrame)` → false once not running) so the presenter
  keeps `StayBlackIntoAWarp` (it owns the warp fade).

## Changes
- `EventPresenter` (294 → about 240): builds the `CutscenePresenter` in its constructor (no new parameter: it already
  takes the voice channel and `EventLook`); its summary's cutscene sentences move to the new type's and one line says
  it delegates.
- `docs/map.md`: the Game `Exploration` row names `CutscenePresenter`.
- `docs/plans/backlog.md`: the `EventPresenter` line says the cutscene part moved and the size now.

## Tests
None new. `EventPresenterStillsTests`, `EventPresenterSoundTests`, `EventPresenterFadeSoundTests` and the other
`EventPresenter*Tests` pass unchanged through `EventPresenters.Over` (runner totals for `Jrpg.Game.Tests` before and
after in the slice's notes).

## Verify by hand
`dotnet run --project src/Jrpg.Game`, New game, choose Ragnir: the placeholder intro plays as before (black, three
stills with captions, the voice, black, the milestone). Start again and press Confirm during a still: it skips to the
milestone.

- **Done, with deviations:** `EventPresenter.cs` is 276 lines, not about 240: `StillRun` already held most of the stills, so what moves is the run's ownership, the lead-out branch's calls and the voice following. `CutscenePresenter.AdvanceLeadOut` returns whether the black still shows, as `Show` returns whether the stills run. `Jrpg.Game.Tests` 1435 tests before and after.
