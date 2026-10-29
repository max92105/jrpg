# Music and sound effects: design

Part of [the music and sound effects plan](README.md).

## Tunables and placeholders

Every value is a placeholder for the developer to tune by ear.

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Volume steps | `Hosting/VolumeLevel.Max` | 10 | levels 0 (silent) to 10 (full) |
| Volume gain | `Hosting/VolumeLevel.GainOf` | `level / 10`, linear | what a level multiplies an instance's volume by |
| Music default | `Hosting/PlayerSettings.MusicVolume` | 7 | |
| Effects default | `PlayerSettings.EffectsVolume` | 8 | |
| Voices default | `PlayerSettings.VoiceVolume` | 10 | today's voice-over plays at full |
| Crossfade | `Audio/MusicMix.FadeLength` (P3) | 1.0 s | one track out while the next comes in; every change (Q2) |
| Typing tick | `Dialogue/TypingTicks.CharactersPerTick` (D1) | 2 | a tick every second character typed, none on a space (Q8) |
| Title theme | `content/audio.json` `titleThemeFile` | `PlaceholderTitle.wav` | a slow two-note chord, 4 s loop |
| Battle theme | `audio.json` `battleThemeFile` | `PlaceholderBattle.wav` | a faster pulse, 4 s loop |
| Town track | `maps/start.json`, `maps/interior_1.json` `musicFile` | `PlaceholderTown.wav` | shared, so a door between them keeps it playing |
| Forest track | `maps/forest.json` `musicFile` | `PlaceholderForest.wav` | |
| Intro map | `maps/intro.json` | none | silent: its voice-over plays (Q5) |
| Boss hook | `encounters.json` `training_dummy` `musicFile` | `PlaceholderBoss.wav` | the practice enemy, so the hook can be heard (Q6) |
| Effects | `audio.json` `effectFiles` | one `Placeholder<Cue>.wav` per id | short tones of different pitches, 0.05-0.4 s, so each cue can be told apart |
| Placeholder format | `.claude/scripts/new-placeholder-tone.ps1` | 16-bit PCM mono 22 050 Hz WAV, 5 ms fade in and out | small (a 4 s loop is about 176 KB); no click at the loop point |

The effect ids (Data's `Audio/SoundEffectIds`, C2): `menuMove`, `menuConfirm`, `menuCancel`, `textTyping`, `battleHit`, `battleCritical`, `battleMiss`, `statusApplied`, `victory`. `menuCancel` is pending Q7.

## Design overview

### What exists
- `Data/Audio/AudioFolders` (`voices`, `RootIn(contentRoot)`) and `AudioReferenceCheck.FindProblems(catalog, audioRoot)`: every voice step's file exists by exact name (`Content/ExactFileName`), called from `Program.FirstScene` beside `ArtReferenceCheck`.
- `Game/Audio`: `VoiceChannel` (a file or null), `SoundLibrary(audioRoot)` (`Get(folder, file)`, loads once, a bad file reported once and silent), `VoicePlayer(sounds).Follow(voice)` (one `SoundEffectInstance`, restarted when the file changes). `JrpgGame.StartAudio` probes the device once; without one, `_sounds` and `_voicePlayer` stay null and the game is silent. `JrpgGame.Update` calls `_voicePlayer?.Follow(_voice)` after the root scene's update.
- The voice channel is built by `SceneFactory.Voice`, exposed as `GameFlow.Voice`, returned by `Program.FirstScene` with the root scene and given to `JrpgGame`'s constructor; `EventPresenter` writes it (15 test sites build `new VoiceChannel()` for it).
- `Hosting/PlayerSettings`: `IsFullScreen`, `TextSpeed`, "for this run only (question 89)"; one object from `Program`. `Menus/OptionsPage` (162 lines): two rows, Left and Right change the selected one, values read when drawing.
- `Core/Maps/MapSettings` (`Id`, `DisplayName`, `RandomEncounters`), `Core/Battle/EncounterDefinition` (`Id`, `EnemyIds`, `BackgroundFile`), `Core/Content/ContentCatalog` (`required init` members). `Data/Content/AbsentWhenNullFilter` leaves an optional field added later out of a written file while null; the editor writes map settings back byte for byte.
- `Exploration/ExplorationState.PendingEncounter` is set when a step or a step's roll starts a battle and cleared by `GameSession.FinishBattle`, which `BattleScene.Finish` calls when the outcome screen opens; the battle scene is popped later, at black, on `BattleFinished` (or replaced by a load or the title after a defeat).
- `Battle/ActionPlayback.Start(events, messages)` reads each turn's events once; the impact is its first beat, at `Elapsed` zero, which `BattleScene.ShowPlayback` shows. `Scenes/SceneStack` updates only the top scene. `Ui/ListCursor` wraps and skips disabled entries; `Ui/PipRow` draws diamond pips lit or dim.
- Editor: `MapSettingsViewModel(map, encounterIds)` with `ToSettings()` and `AreSettingsChanged`, built by `MainViewModel.StartEditingSettings()`, applied as one `SetMapSettingsEdit` (undoable).

### Which MonoGame types carry music
- **Music is `SoundEffect` + `SoundEffectInstance`**, as voices are: loaded by `SoundLibrary.Get(AudioFolders.Music, file)` with `SoundEffect.FromFile`, played by an instance with `IsLooped = true` (set before its first `Play`), its `Volume` set every update, `Pause()`/`Resume()` for the track left for a battle, `Stop()` when faded out. Two instances at once make the crossfade.
- **Files are WAV only** (16-bit PCM, any encoding ADR 0018 lists loads). **`.ogg` isn't needed**: only `Song` reads it, and `Song`/`MediaPlayer` aren't used (one global song, no crossfade, static state; ADR candidate "music playback"). If real tracks make WAV too big, only `Audio/MusicPlayer` changes.
- **Effects are `SoundEffect.Play(volume, 0, 0)`**, fire and forget; a false return (too many sounds at once) is ignored.

### Core changes
- `Maps/MapSettings.MusicFile` (C1): `string?`, init, declared after `DisplayName`; the track in `content/audio/music/`, null for silence.
- `Battle/EncounterDefinition.MusicFile` (C1): `string?`, init; the encounter's own theme (the boss hook), null for the battle theme.
- `Content/AudioDefinition` (C2): public sealed record; the game-wide audio files content names. `TitleThemeFile`, `BattleThemeFile` (`string?`), `EffectFiles` (`IReadOnlyDictionary<string, string>`, effect id to file); every member defaults to absent, so `new AudioDefinition()` is silence.
- `Content/ContentCatalog.Audio` (C2): `{ get; init; } = new()`, not `required`, so no test catalog changes.

No rule, random or state change: determinism and saves are untouched.

### Structure and folder dependencies

Core: `Content` → `Maps`, `Battle` (exist); `AudioDefinition` depends on nothing.

Data:

| Folder | Depends on | New |
|---|---|---|
| `Audio` | Core `Content`, `Events`, `Maps`, `Battle`; Data `Content` (`ExactFileName`, `ContentValidationException`), `Maps` (`MapEventSteps`) | `SoundEffectIds`, `AudioDefinitionValidator`; `AudioFolders.Music`, `.Effects`, `.FilesIn` |
| `Content` | `Audio` (new edge: the loader calls `AudioDefinitionValidator`) | `audio.json` read by `ContentCatalogLoader` |

`Content` ↔ `Audio` is the pair `Data/Loot` and `Data/Crafting` already have with `Content`.

Game:

| Folder | Depends on | New |
|---|---|---|
| `Audio` | Data `Audio`; `Hosting` (exists: `DeveloperLog`; new: `VolumeLevel`) | `AudioChannels`, `MusicChannel`, `EffectChannel`, `MusicPlayer`, `MusicMix`, `MixedTrack`, `SoundEffectPlayer` |
| `Hosting` | (none new) | `VolumeLevel` |
| `Scenes` | (none new) | `IScene.Sounds` (default empty), `SceneStack.TopSounds`, `SceneStack.TopmostOf<T>()` |
| `Ui` | Data `Audio` (ids only) | `AskedSounds` |
| `Flow` | `Audio`, `Battle` (exist) | `MusicSelection`, `AudioCues` |
| `Title`, `NewGame`, `Menus`, `Battle`, `Crafting`, `Exploration`, `Dialogue` | `Ui` (`AskedSounds`), Data `Audio` (`SoundEffectIds`) | `Battle/ImpactSounds`, `Dialogue/TypingTicks` |

No feature folder references `Audio`: scenes only report ids (`Ui/AskedSounds`); the flow writes the channels and the host plays them. `Audio` never references `Flow`, a feature folder or Core.

### Key decisions
1. **Three channels, one host pass.** `AudioChannels` holds `Voice` (exists), `Music` and `Effects`. During an update the flow and the event presenter write them; after it, `JrpgGame` calls `VoicePlayer.Follow`, `MusicPlayer.Follow` and `SoundEffectPlayer.Play` with each channel and its volume. Every channel holds file names, so the players never read content.
2. **Music follows the flow's state** (ADR candidate "how the game asks"). `Flow/MusicSelection.TrackFor(GameSession? session, EncounterDefinition? fought, AudioDefinition audio)` is pure: no session → title theme; an encounter fought → its `MusicFile ?? audio.BattleThemeFile`; else `session.Exploration.CurrentMap.Settings.MusicFile`. `Flow/AudioCues` finds `fought` as the battle screen's encounter while one is on the stack (`SceneStack.TopmostOf<BattleScene>()?.Encounter`), else `PendingEncounter`, so the battle theme starts at the flash and lasts until the battle screen is popped, though `FinishBattle` clears `PendingEncounter` when the outcome screen opens.
3. **Scenes report effects as values.** `IScene` gains `IReadOnlyList<string> Sounds => []` (default: none), the effect ids its last update asks for; `IMenuPage` the same, which `GameMenuScene` adds to its own. Each scene keeps an `Ui/AskedSounds`, cleared as its update starts. After `_scenes.Update`, `AudioCues` writes `SceneStack.TopSounds` into `Effects` by file (`AudioDefinition.EffectFiles[id]`; an id content lacks is reported once and skipped). No scene, page or presenter gains a constructor parameter (most are at four: `BattleScene`, `ForgeScene`, `GameMenuScene`, `InventoryPage`, `PartyPage`, `StatusPage`, `SystemPage`).
4. **The effect ids are Data's closed list** (`SoundEffectIds`, ADR candidate "game-wide audio names"): `AudioDefinitionValidator` refuses `audio.json` unless every id is named once with a file and no other id appears, so a missing cue fails at load with the file named, and the Game uses the same constants.
5. **Crossfade and resume are a pure mix.** `Audio/MusicMix` (P3) says which tracks sound at what gain as time passes: the wanted track rises over `FadeLength` while the one before falls; the one faded out is kept paused (one only), and wanting it again resumes it. `MusicPlayer` applies the mix to its instances.
6. **Volumes are levels, gains are computed.** `PlayerSettings` keeps three `int` levels (0..10); `VolumeLevel.GainOf(level)` turns one into an instance volume. Players multiply it in every update, so a change in the Options page is heard at once.
7. **Placeholders are generated.** `.claude/scripts/new-placeholder-tone.ps1 -Path -Frequencies -Seconds` writes a 16-bit PCM mono WAV (one or more sine frequencies, 5 ms fades); every generated file's name starts with `Placeholder`.

### Extension points
- More cues: an id in `SoundEffectIds`, its file in `audio.json`, and the presenter asking for it. A battle happening: a case in `Battle/ImpactSounds`.
- A victory fanfare that silences the battle theme (Q4): `BattleScene.IsShowingOutcome` read by `AudioCues`, `MusicSelection` returning null.
- A cut instead of a crossfade for some changes (Q2): `MusicChannel` carries the change's fade length.
- Streaming music: only `MusicPlayer` (and `SoundLibrary` for `Song`) changes; channels and selection stay.

### Replacement mapping
| Existing | Becomes | Slice |
|---|---|---|
| `SceneFactory.Voice`, `GameFlow.Voice` | `SceneFactory.Audio`, `GameFlow.Audio` (`AudioChannels`, its `Voice` the same channel) | P1 |
| `Program.FirstScene` returning `(IScene, VoiceChannel)` | returning `(IScene, AudioChannels)` | P1 |
| `JrpgGame(rootScene, contentRoot, settings, VoiceChannel voice)` | `JrpgGame(rootScene, contentRoot, settings, AudioChannels audio)` | P1 |
| `VoicePlayer.Follow(voice)` at full volume | `Follow(voice, gain)` | V1 |
| `OptionsPage.Change`'s two-way branch | a switch over five rows | V1 |
| `AudioReferenceCheck`'s voices loop | voices, map and encounter tracks (C1), themes and effects (C2) | C1, C2 |
| `MusicPlayer`'s cut (P2) | the crossfade through `MusicMix` | P3 |
| Volume values as text | `Ui/PipRow` meters | V2 |

## Risks
- **Repository size**: WAV in git keeps every version. Placeholders are small; real tracks (about 5 MB a minute mono, 10 stereo) would not be, and `SoundEffect.FromFile` holds a whole track in memory (a 4-minute stereo track is about 40 MB). The music ADR names the switch to `Song`/OGG.
- **Loop seams**: a looped `SoundEffectInstance` loops the whole file; a track whose end doesn't meet its start clicks. Placeholders fade 5 ms at each end; real tracks must be cut to loop.
- **OpenAL limits**: many effects at once can fail to start (`Play` returns false); one sound per cue per update keeps the count low.
- **Files near 300 lines** (README "Guiding rules"): each slice adds a line or two to the big scenes and puts its rule in a new type; `Editor/Shell/MainViewModel` is already 490 lines, so E1 changes one line there; if the verify script refuses any change to a file over 300, the music list is built in `MapSettingsViewModel`'s caller in `MainWindow.xaml.cs` instead (E1 says so).
- **Content written back**: `musicFile` is added to three maps' settings, so the round-trip writer tests compare them; the field is declared after `displayName` and written in that order; a map without one leaves it out (`AbsentWhenNullFilter`).
- **Test content**: the Data and Game test projects copy `content/` (the audio folder included, as `AudioReferenceCheckTests` reads `voices/Placeholder.wav`); new files must be in the copied globs. Test catalogs built in code get `new AudioDefinition()` (silent, no ids), so `AudioCues` skips their ids: tests that assert effects use `tests/Jrpg.Game.Tests/Audio/TestAudio.Definition` (every id mapped to a file named as the id).
- **Settings not saved (question 89)**: Q1. Without V3, volumes reset each run like the other options.
- **Determinism**: none; audio reads no random and no wall clock, and the flow's state alone chooses the music.
- **Save format**: none. **Content format**: optional fields and one new required file (`audio.json`); a content folder without it fails at load with the file named, which `--content <folder>` users must know.
