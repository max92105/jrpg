# Music and sound effects: design

Part of [the music and sound effects plan](README.md).

## Tunables and placeholders

Every value is a placeholder for the developer to tune by ear.

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Volume steps | `Hosting/VolumeLevel.Max` | 10 | levels 0 (silent) to 10 (full), decision 424 |
| Volume taper | `Hosting/VolumeLevel.GainOf` | `(level / 10)²` | squared, so the low steps aren't all loud |
| Music default | `Hosting/PlayerSettings.MusicVolume` | 7 | |
| Effects default | `PlayerSettings.EffectsVolume` | 8 | |
| Voices default | `PlayerSettings.VoiceVolume` | 10 | today's voice-over plays at full |
| Resume fade-in | `Audio/MusicMix.ResumeFadeIn` (P3) | 0.5 s | the map's track coming back after a battle (decision 416) |
| Fanfare hold | `Battle/BattleMusic.FanfareHold` (B4) | 3.0 s | silence under the victory fanfare before the map's track returns; the placeholder fanfare's length |
| Typing tick | `Dialogue/TypingTicks.CharactersPerTick` (D1) | 2 | decision 422 |
| Typing gap | `Dialogue/TypingTicks.MinimumGap` (D1) | 60 ms | decision 422 |
| Title theme | `Ui/UiSoundFiles.Music.Title` (P1) | `PlaceholderTitle.wav` | a slow two-note chord, 4 s loop |
| Battle theme | `UiSoundFiles.Music.Battle` (P1) | `PlaceholderBattle.wav` | a faster pulse, 4 s loop |
| Town track | `maps/start.json`, `maps/interior_1.json` `musicFile` (C1) | `PlaceholderTown.wav` | shared, so the door between them keeps it playing |
| Forest track | `maps/forest.json` `musicFile` (C1) | `PlaceholderForest.wav` | |
| Intro map | `maps/intro.json` | none | silent (decision 419); its voice-over plays |
| Boss theme | `encounters.json` `goblin_chief` `musicFile` (C2) | `PlaceholderBoss.wav` | decision 420 |
| Interface sounds | `UiSoundFiles.Effects` (S1, D1) | `PlaceholderMenuMove.wav`, `...MenuConfirm`, `...MenuCancel`, `...TextTyping` | short tones: confirm higher, cancel lower, typing a click |
| Battle sounds | `UiSoundFiles.Battle` (B1, B2, B4) | one `Placeholder<Name>.wav` each | design "Key decisions" 5 lists them |
| Skill sounds | `skills.json` `soundFile` (B3) | `PlaceholderSkillStrike.wav` (damage), `PlaceholderSkillMend.wav` (healing), `PlaceholderSkillCharm.wav` (anything else) | shared by kind until each skill gets its own |
| Placeholder format | `.claude/scripts/new-placeholder-tone.ps1` (C1) | 16-bit PCM mono 22 050 Hz WAV | music: whole sine cycles per loop, no fades, so the loop has no seam; effects: 5 ms fades at each end |

## Design overview

### What exists
- `Data/Audio/AudioFolders` (`voices`, `RootIn(contentRoot)`) and `AudioReferenceCheck.FindProblems(catalog, audioRoot)`: every voice step's file exists by exact name (`Content/ExactFileName`), called from `Program.FirstScene` beside `ArtReferenceCheck`.
- `Game/Audio`: `VoiceChannel` (a file or null), `SoundLibrary(audioRoot)` (`Get(folder, file)`, loads once, a bad file reported once and silent), `VoicePlayer(sounds).Follow(voice)` (one `SoundEffectInstance`, restarted when the file changes). `JrpgGame.StartAudio` probes the device once; without one, `_sounds` and `_voicePlayer` stay null and the game is silent. `JrpgGame.Update` calls `_voicePlayer?.Follow(_voice)` after the root scene's update. `JrpgGame.LoadContent` already preloads the menu's background to avoid a hitch on first use.
- The voice channel is built by `SceneFactory.Voice`, exposed as `GameFlow.Voice`, returned by `Program.FirstScene` with the root scene and given to `JrpgGame`'s constructor; `EventPresenter` writes it (15 test sites build `new VoiceChannel()` for it; they stay).
- `Ui/UiArtFiles`: images no content names, as constants grouped by folder, checked by `UiArtFilesTests` (decision 84).
- `Hosting/PlayerSettings`: `IsFullScreen`, `TextSpeed`, "for this run only (question 89)"; one object from `Program`. `Menus/OptionsPage` (162 lines): two rows, Left and Right change the selected one, values read when drawing.
- `Core/Maps/MapSettings` (`Id`, `DisplayName`, `RandomEncounters`), `Core/Battle/EncounterDefinition` (`Id`, `EnemyIds`, `BackgroundFile`), `Core/Skills/SkillDefinition` (23 skills in `skills.json`; `IconFile` required). `Data/Content/AbsentWhenNullFilter` leaves an optional field added later out of a written file while null; the editor writes map settings back byte for byte (it writes no encounters and no skills).
- `Exploration/ExplorationState.PendingEncounter` is set when a step starts a battle and cleared by `GameSession.FinishBattle`, which `BattleScene.Finish` calls when the outcome screen opens; the battle scene is popped later, at black, on `BattleFinished` (or replaced by a load or the title after a defeat). While the encounter's flash plays, `GameFlow.Update` returns before updating the scenes.
- `Battle/ActionPlayback.Start(events, messages)` reads each turn's events once into beats (`PlaybackBeatKind`: Banner, Impact, Defeat, Skip, Backfire, Tick; the enemy plan adds Phase); a private `StartOf(kind)` gives when a beat starts. Live-state events (`ResourceChanged`, `StatusEnded`, `PlacesSwapped`, `ExtraTurnReadied`, `BattleEnded`) get no beat. An unknown event throws.
- `Scenes/SceneStack` updates only the top scene. `Ui/ListCursor` wraps and skips disabled entries; `Ui/PipRow` draws diamond pips lit or dim.
- Editor: `MapSettingsViewModel(map, encounterIds)` with `ToSettings()` and `AreSettingsChanged`, built by `MainViewModel.StartEditingSettings()`, applied as one `SetMapSettingsEdit` (undoable).

### Which MonoGame types carry music
- **Music is `SoundEffect` + `SoundEffectInstance`**, as voices are: loaded by `SoundLibrary.Get(AudioFolders.Music, file)` with `SoundEffect.FromFile`, played by an instance with `IsLooped = true` (set before its first `Play`), its `Volume` set every update, `Pause()` when a battle cuts in and `Resume()` when its track comes back, `Stop()` and `Dispose()` when it leaves the mix.
- **Files are WAV only** (16-bit PCM; any encoding ADR 0018 lists loads). **`.ogg` isn't needed**: only `Song` reads it, and `Song`/`MediaPlayer` aren't used (ADR 0035). If real tracks make WAV too big, `DynamicSoundEffectInstance` streaming changes only `MusicPlayer` and `SoundLibrary`.
- **Effects are `SoundEffect.Play(volume, 0, 0)`**, fire and forget; a false return (too many sounds at once) is ignored.

### Core changes
- `Maps/MapSettings.MusicFile` (C1): `string?`, init, declared after `DisplayName`; the track in `content/audio/music/`, null for silence.
- `Battle/EncounterDefinition.MusicFile` (C2): `string?`, init; the encounter's own theme, null for the battle theme.
- `Skills/SkillDefinition.SoundFile` (B3): `string`, init, default `""` (so no test that builds a skill changes); Data refuses an empty one in content. The file in `content/audio/effects/`.

No rule, random or state change: determinism and saves are untouched.

### Structure and folder dependencies

Core: no new folder or edge.

Data: `Audio` → Core `Maps`, `Battle`, `Skills`, `Events` (Battle and Skills new, for the startup check), Data `Content`, `Maps` (exist). `Skills/SkillCatalogValidator` gains a rule, no edge.

Game:

| Folder | Depends on | New |
|---|---|---|
| `Audio` | Data `Audio`; `Hosting` (`DeveloperLog` exists) | `AudioChannels`, `MusicChannel`, `EffectChannel`, `MusicPlayer` (told by the host which tracks stay loaded), `MusicMix`, `SoundEffectPlayer` |
| `Hosting` | (none new) | `VolumeLevel` |
| `Scenes` | (none new) | `IScene.Sounds` (default empty), `SceneStack.TopSounds`, `SceneStack.Top` |
| `Ui` | Data `Audio` (folders, for the summaries only) | `UiSoundFiles`, `AskedSounds` |
| `Flow` | `Audio`, `Battle` (exist), `Ui` (exists) | `MusicSelection` |
| `Battle` | `Ui` (exists) | `BattleSounds`, `TimedSound`, `PlaybackSoundQueue`, `BattleMusic`, `BattleMusicCue` |
| `Dialogue` | (none new) | `TypingTicks` |
| `Title`, `NewGame`, `Menus`, `Crafting`, `Exploration`, `Saving` | `Ui` (exists) | |

No feature folder references `Audio`: scenes only report files (`Ui/AskedSounds`), the flow writes the channels, the host plays them. `Audio` never references `Flow`, a feature folder or Core. `Ui` never references `Audio` or a feature folder.

### Key decisions
1. **Three channels, one host pass.** `AudioChannels` holds `Voice` (exists), `Music` (P1) and `Effects` (S1). The flow and the event presenter write them during an update; after it, `JrpgGame` calls `VoicePlayer.Follow`, `MusicPlayer.Follow` and `SoundEffectPlayer.Play`, each with its volume's gain. Every channel holds file names, so the players never read content.
2. **Music follows the flow's state** (ADR 0035). `Flow/MusicSelection.TrackFor(GameSession? session, BattleMusicCue? battleCue)` is pure: no session → `UiSoundFiles.Music.Title`; the battle screen's cue `Silence` → null, `Field` → the map's track; a pending encounter (from the flash until `FinishBattle`) → its `MusicFile ?? UiSoundFiles.Music.Battle`; else the current map's `MusicFile`. `GameFlow` calls it on every update path (the flash's included) and writes `Audio.Music.Want(...)`. P1 starts with the session alone; B4 adds the cue, read as `(_scenes.Top as BattleScene)?.MusicCue`.
3. **Scenes report effects as values** (ADR 0035). `IScene.Sounds => []` (default: none): the effect files the scene's last update asks for; `IMenuPage.Sounds` the same, which `GameMenuScene` adds to its own. Each sounding scene keeps a `Ui/AskedSounds`, cleared as its update starts. `GameFlow.Update` clears `Audio.Effects` first; right after `_scenes.Update` it copies `SceneStack.TopSounds` into it, before `Start(request)`: a request that opens the menu or the Forge or starts a fade doesn't lose the sound, and the flash (which skips the scenes) replays nothing. No scene, page or presenter gains a constructor parameter.
4. **The pause and resume are a pure mix** (P3). `Audio/MusicMix` keeps the track playing, its gain and one paused track: wanting a new track cuts to it at full gain and pauses the one before (dropping any older paused one); wanting the paused one resumes it from where it paused, its gain rising over `ResumeFadeIn`, and drops the one playing; wanting silence drops the playing one and keeps the paused one (the victory's fanfare). `MusicPlayer` applies the mix to instances and releases a dropped track (`SoundLibrary.Release`), except the tracks the host told it to keep loaded (the two themes, preloaded in P2).
5. **Battle sounds come from the events, each at its beat's start** (B1, B2). `Battle/BattleSounds.Of(events, startOf)` maps every event kind (exhaustive; an unknown one throws, as `ActionPlayback` does) to `TimedSound(File, At)`, `At` the start of the beat that shows it (`ActionPlayback`'s `StartOf`, passed as a function; zero for live-state events). Impact: `Critical` if any hit was critical else `Hit`, `Miss`, `Block`, `Resisted`, `StatusApplied`, `Heal` (restore, revive, life stolen). Others: `Fall` (defeat beat), `PoisonTick`/`RegenTick` (tick beat), `Backfire`, `Stunned` (skip beat), `ResourceGain` (a gain; a cost is silent), `StatusEnded`, `Move` (places swapped), `ExtraTurn`, `ItemUse` and `Defend` (the action's start), a skill's own `SoundFile` (the action's start, B3). `BattleEnded` is silent here: the outcome screens sound `Victory`, `Defeat` and `Escape` (B4). `ActionPlayback.Sounds` holds them sorted by `At`; `Battle/PlaybackSoundQueue` asks each once as the playback's `Elapsed` reaches it.
6. **The battle screen's music cue** (B4). `Battle/BattleMusic` says what the screen calls for: `Theme` while fighting, `Silence` from the victory screen for `FanfareHold` then `Field` (decision 418), `Silence` on the defeat screen, `Field` on the escaped notice.
7. **The files the code plays are constants** (decision 425): `Ui/UiSoundFiles.Music` (in `music/`), `.Effects` and `.Battle` (in `effects/`), checked by `UiSoundFilesTests` by exact name, case included, as `UiArtFilesTests` checks `UiArtFiles`. A missing file at run time is the `SoundLibrary`'s: reported once, silent.
8. **Volumes are levels, gains are computed.** `PlayerSettings` keeps three `int` levels (0..10); `VolumeLevel.GainOf(level)` turns one into an instance volume. Players set it every update, so a change in Options is heard at once.
9. **Placeholders are generated** by `.claude/scripts/new-placeholder-tone.ps1 -Path -Frequencies -Seconds [-FadeMilliseconds]`: music with whole sine cycles per loop (each frequency rounded so frequency × seconds is whole) and no fade; effects with 5 ms fades. Every generated file's name starts with `Placeholder`.

### Extension points
- A new battle event kind: a case in `BattleSounds` (its default throws) and a constant with its file. The enemy plan's `EnemyPhaseEntered` (its B2/B3) is the first: whichever plan lands second adds its case (an `Enraged` sound at the `Phase` beat).
- A new skill: its `soundFile` in `skills.json` (the validator refuses one without). The enemy plan's skills are the first.
- A `playMusic` event step: Core state read by `MusicSelection` (ADR 0035).
- Streaming music: `MusicPlayer` and `SoundLibrary` only.

### Replacement mapping
| Existing | Becomes | Slice |
|---|---|---|
| `SceneFactory.Voice`, `GameFlow.Voice` | `SceneFactory.Audio`, `GameFlow.Audio` (`AudioChannels`, its `Voice` the same channel) | P1 |
| `Program.FirstScene` returning `(IScene, VoiceChannel)` | returning `(IScene, AudioChannels)` | P1 |
| `JrpgGame(rootScene, contentRoot, settings, VoiceChannel voice)` | `JrpgGame(rootScene, contentRoot, settings, AudioChannels audio)` | P1 |
| `VoicePlayer.Follow(voice)` at full volume | `Follow(voice, gain)` | V1 |
| `OptionsPage.Change`'s two-way branch | a switch over five rows | V1 |
| `AudioReferenceCheck`'s voices loop | a shared exact-name loop: voices, map tracks (C1), encounter themes (C2), skill sounds (B3) | C1 |
| `MusicPlayer`'s cut with a stop (P2) | the pause and resume through `MusicMix` | P3 |
| Volume values as text | `Ui/PipRow` meters | V2 |

## Risks
- **Cross-plan order** (the enemy plan is approved beside this one): C2 needs its C1 (the Goblin Chief's encounter). If its skills (Cleave, Field Dressing, Crushing Blow, War Cry) land after B3, each needs a `soundFile`; if they land before, B3 gives them the placeholders. Its `EnemyPhaseEntered` needs a `BattleSounds` case in whichever plan's slice lands second (B1's default throws on it).
- **Repository size and memory**: WAV in git keeps every version, and `SoundEffect.FromFile` holds a whole track in memory (a 4-minute stereo track is about 40 MB). Placeholders are small; real tracks make ADR 0035's streaming the next step. Releasing a track when it leaves the mix keeps one map's tracks loaded at a time, plus the two themes.
- **Loop seams**: a looped instance loops the whole file; a track whose end doesn't meet its start clicks. Placeholder music is whole cycles with no fade; real tracks must be cut to loop.
- **OpenAL limits**: many sounds at once can fail to start (`Play` returns false); one sound per file per update keeps the count low.
- **Files near 300 lines** (README "Guiding rules"): each slice adds a line or two to the big scenes and puts its rule in a new type. `BattleScene` (254) grows through S2, B1, B4, B5 to about 280; `ActionPlayback` (252) by about 10 here and the enemy plan's B3 phase beat. `Editor/Shell/MainViewModel` is already 490: E1 changes one line; if the verify script refuses any change to a file over 300, `MainWindow.xaml.cs` builds the music list instead.
- **Content written back**: `musicFile` is added to three maps' settings, so the round-trip writer tests compare them; the field is declared after `displayName` and written in that order; a map without one leaves it out (`AbsentWhenNullFilter`). `EncounterDefinition` and `SkillDefinition` get no filter entry: nothing writes them.
- **Skill content fixtures**: the validator's new rule (a non-empty `soundFile`) runs last in `FindProblem`, so the `TestContent` fixtures that fail for another reason still fail for theirs; test skills built in code keep `""` and are never validated; B3 budgets the validator tests' skill helper.
- **Test content**: the Data and Game test projects copy `content/` (the audio folder included, as `AudioReferenceCheckTests` reads `voices/Placeholder.wav`); new folders must be in the copied globs. Test catalogs have no tracks and no skill sounds: the Game skips an empty or null file, so they stay silent.
- **Typing under a voice**: the voice channel holds its file until the event ends or is skipped, so ticks stay muted for the rest of an event once its voice step started (README, "Noted").
- **Determinism**: none; audio reads no random and no wall clock, and the flow's state alone chooses the music.
- **Save format**: none.
