# TN0. `BattleScene`'s outcome screens into `BattleOutcomeScreens`

Game, about 110 lines moved (no behavior change). Decision 663.

## Goal
`Game/Battle/BattleScene.cs` (290 lines) gives up its outcome part before this plan adds the wind-up's sounds, the swing
and the light: the victory, escape and defeat screens, their music cue and their sounds move into one type, and the
scene hands it the finished battle. Nothing on screen or in the speakers changes.

## New types
- `Game/Battle/BattleOutcomeScreens` (internal sealed): which screen a finished battle shows (the victory's rewards, the
  escape's notice or the defeat's choices), with the music and the sounds that go with it. Constructor `(SaveSlots
  saves, ScreenText text, UiTextures ui)`.
  - `void Show(BattleSpoils? spoils, BattlePhase outcome)`: the scene calls it once, with `FinishBattle`'s spoils; it
    picks the screen as `Finish` did and asks its sound.
  - `void Advance(TimeSpan delta)` (the music's time, every update, as `_music.Advance` now) and `SceneRequest?
    Update(TimeSpan delta, InputFrame input)` (the shown screen's, as `UpdateOutcomeScreen`).
  - `IReadOnlyList<string> Sounds` (what its last `Show` or `Update` asked, cleared at each) and `BattleMusicCue
    MusicCue`.
  - `bool CoversTheHud` (a victory or defeat screen shows), `DrawOverTheBattlers` (that screen) and `DrawOverTheHud`
    (the escaped notice), keeping today's draw order.

## Changes
- `BattleScene` (290 → about 195): `_victory`, `_victoryView`, `_defeat`, `_defeatView`, `_escaped`, `_escapedText`,
  `_escapedFont`, `_music`, the escaped constants, `UpdateOutcomeScreen`, `UpdateDefeat`, `DrawEscaped` and `Finish`'s
  three branches move; `_isFinished`, the `FinishBattle` call, `Sounds` (now also asking the outcome's) and `MusicCue`
  (now the outcome's) stay. `Draw` asks `CoversTheHud` where it tested the two views. Unused usings deleted bottom-up.
- `docs/map.md`: the Game `Battle` row names `BattleOutcomeScreens`.

## Tests
- None new: `BattleSceneTests`, `BattleSceneSoundTests` (the outcome sounds and music cues reach the scene),
  `BattleMusicTests`, `VictoryScreen*Tests` and `DefeatScreen*Tests` pass unchanged, with the same test counts before
  and after (the guard of a pure move).

## Verify by hand
Before and after F12 captures of the three outcomes, compared, with their jingles heard: a won battle's rewards and
drops; Run's "Escaped"; a lost battle's defeat menu.

- **Done:** as planned: `BattleScene.cs` 290 → 208, `BattleOutcomeScreens.cs` new; `Jrpg.Game.Tests` 1469 before and after. The escaped notice's text position became its own line (the moved draw line was over 130 columns).
