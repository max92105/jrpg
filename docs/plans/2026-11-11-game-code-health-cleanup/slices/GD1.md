# GD1. One `EventPresenters.Over` builder for the 20 hand-built presenters

Game tests, about 60 lines (one small helper, 20 call sites shortened), no production change. Builds on design.md, "Dead code survey" and "Risks" (GD1's optional parameters). Before GT3, which moves some of these tests.

## Goal
Tests build an `EventPresenter` the same way in one place: the repository's screen text, fresh UI textures, the settings and the voice channel a test may set.

## New types
- `tests/Jrpg.Game.Tests/Exploration/EventPresenters` (static, about 20 lines): `public static EventPresenter Over(ExplorationState exploration, PlayerSettings? settings = null, VoiceChannel? voice = null)`, summary "An event presenter over the exploration, drawing with the repository's screen text, with default settings and its own voice channel unless a test passes them."

## Changes
The 20 sites (`Grep "new EventPresenter\("` in `tests/Jrpg.Game.Tests`) become `EventPresenters.Over(exploration)`, passing `settings:` where a site sets one (`TextSpeed = TextSpeed.Instant` or a theory's speed) and `voice:` where a site keeps the channel to read it afterward:
- `EventPresenterTests` (11), `EventPresenterForgeTests` (2), `EventPresenterInnTests`, `EventPresenterShopTests`, `EventPresenterQuestTests`, `EventPresenterRestTests`, `EventPresenterSoundTests`, `EventPresenterStepSoundTests`, `StartMapScenarios` (one each).
- A local `look` built only to pass it (`EventPresenterSoundTests`, `EventPresenterStepSoundTests`) goes with the call; if the test reads `look` afterward, the site keeps its constructor call and the Done note says why.
- `docs/plans/backlog.md`: delete the sound pass's "20 times in 9 files" line.

## Tests
None new. The runner's totals per project equal before and after (Done note); no test moves.

## Verify by hand
None: tests only.
