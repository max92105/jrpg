# Sound pass: design

Part of [the sound pass plan](README.md).

## Tunables and placeholders

Every value is a placeholder for the developer to tune by ear.

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Format | the script's output | 16-bit PCM, mono, 44 100 Hz WAV | ADR 0018 lists the encodings the game reads; the rate is ADR 0036's: about 2 MB for every effect; the tunes stay 22 050 Hz |
| Peak level | `sound-sources.json` `peakDb` | measured in P1 from today's tones (`new-placeholder-tune.pl measure`), about -12 dBFS | every converted effect's peak, so a swap keeps the level |
| Per-sound gain | an entry's `gainDb` (optional) | 0 dB | on top of the peak level, where a sound reads too loud or quiet at the same peak |
| Lead-in trim | the script | below the measured peak minus 40 dB (`detection=peak`) | an uploader's lead-in; ffmpeg already drops the MP3's encoder delay when the header carries it |
| Start offset | an entry's `startSeconds` (optional) | 0 | skips into a longer recording before the trim; with the id, the entry's key |
| Length cap | an entry's `maxSeconds` | each slot's brief | the conversion cuts there |
| Fade in | the script | 1 ms; 5 ms when `startSeconds` cuts into the sound | against a click at the start |
| Fade out | an entry's `fadeOutSeconds` (optional) | 0.01 s | longer where the cap cuts a sound mid-ring |
| Preview quality | an entry's `preview` | the `-hq.mp3` preview (128 kbps) | the best file freesound serves without a login |
| Licenses allowed | the script, P2's test | `CC0-1.0`, `CC-BY-3.0`, `CC-BY-4.0` (SPDX ids) | decision 427 |
| Skill families | `skills.json` `soundFile` (K1) | nine files | decision 431 |
| New slots | `Ui/UiSoundFiles` | `Effects.Deal` (SH3); `Field.ItemGot`, `.Recruit`, `.QuestUpdated`, `.QuestCompleted` (FE2), `.Warp`, `.Rest` (FE3); `Battle.LevelUp` (V1) | each `Placeholder<Name>.wav` |

## Design overview

### What exists
- **Audio** (ADR 0018, ADR 0035; the music plan's design): effects are reported by scenes as values (`IScene.Sounds`, `IMenuPage.Sounds`), each scene keeping a `Ui/AskedSounds` cleared as its update starts (`Ask` keeps one of each file per update; `AskMoveIfChanged`); `GameFlow` copies the top scene's into the effect channel after `_scenes.Update`; `SoundEffectPlayer` plays them at the effects volume. `Battle/PlaybackSoundQueue.Follow(playback, sounds)` writes into the scene's list it's given. The files the code plays are constants in `Ui/UiSoundFiles` (`Music`, `Effects`, `Battle`), checked by `UiSoundFilesTests` against the folder's listing, case included, with `EveryGroup_IsCheckedAbove` counting the groups (3). Content names map tracks, encounter themes and skill sounds (`skills.json` `soundFile`), checked at startup by `Data/Audio/AudioReferenceCheck`.
- **Files**: `content/audio/effects/` holds 30 `Placeholder*.wav`: 28 sine tones from `.claude/scripts/new-placeholder-tone.ps1`, and `PlaceholderVictory.wav` and `PlaceholderDefeat.wav`, tunes from `.claude/scripts/new-placeholder-tune.pl` (which also renders the five tracks in `music/` and has a `measure <wav>` mode: peak, loudness, biggest jump). `.claude/rules/content.md` says placeholder music comes from the tune script. `tests/Jrpg.Data.Tests` and `tests/Jrpg.Game.Tests` copy all of `content/` to their output; the game's publish copies all of `content/` too.
- **Shop** (`Trade/`): `ShopScene` (296 lines) owns the tabs, the rails, the member preview (`_memberIndex`, `_previewedRow`, `_previewedMember`, `_comparison`, `SwitchMember`, `RefreshPreview`) and the close; `ShopCounter` (128) holds both lists and the open deal (`QuantityPicker` or a priced `ConfirmPrompt`), settling through Core (`Shop.TryBuy`, `ItemSales.TrySell`). No sound anywhere. `Crafting/ForgeScene` (241) asks its sounds, but its salvage (after a `ConfirmPrompt`'s Yes) is silent, pinned by `ForgeSceneSoundTests.Update_YesInTheSalvagePrompt_AsksNothing`.
- **Field events** (`Exploration/`): `EventPresenter` (296) switches on the current step: Warp (its `ScreenFade`, started by `ShowWarp` or already black after a cutscene by `StayBlackIntoAWarp`), the page steps (`ShowPages`/`StartReading`: Dialogue, GiveItem, Recruit, the three quest steps; Rest's line), Still, Rest (`RestPlayback`: a free rest fades at once, an inn asks then fades on a paid Yes). `SwapObjectStep` and `VoiceStep` never reach it: Core applies them inside the run. A found item's receipt can say nothing was added (`Added == 0`: `FoundItemText`'s "couldn't carry it" line). Its `Sounds` are the typing ticks (`TypingSound`); `ExplorationScene` copies them (tested by `ExplorationSceneSoundTests`).
- **Victory** (`Battle/`): `BattleScene` (288) opens the outcome in `Finish` (victory, escape, defeat) and updates it in `UpdateOutcomeScreen` (`return victory.Update(delta, input);`) and `UpdateDefeat` (which copies the defeat screen's sounds); `VictoryScreen` (96) fills `ExperienceBarAnimation`s (each with `LevelUps`), skips them on Confirm, then shows the drops or leaves. It asks no sound; the fanfare is asked by `BattleScene.Finish`.

### Core changes
None. No rule, random, state, content record or save changes; determinism and the save format are untouched.

### The pipeline
**`content/audio/sound-sources.json`** (P1), laid out as `ContentWriter` would write it (one field per line, entries sorted by `file`, CRLF):

```json
{
  "peakDb": -12,
  "tunes": [
    "effects/PlaceholderDefeat.wav",
    "effects/PlaceholderVictory.wav"
  ],
  "sounds": [
    {
      "file": "effects/PlaceholderMenuMove.wav",
      "freesoundId": 123456,
      "name": "the sound's title on freesound",
      "author": "the uploader's name",
      "license": "CC0-1.0",
      "page": "https://freesound.org/s/123456/",
      "preview": "https://cdn.freesound.org/previews/123/123456_7890-hq.mp3",
      "fetched": "2026-11-02",
      "maxSeconds": 0.08,
      "startSeconds": 0.2,
      "fadeOutSeconds": 0.02,
      "gainDb": -2
    }
  ]
}
```

`file` is a path under `content/audio/`, so a later voice or ambient placeholder needs no new field. `startSeconds`, `fadeOutSeconds` and `gainDb` are optional. `page` uses freesound's short form (no username: names change, ids don't); `preview` is copied from the page's HTML, as served, and never rebuilt from the id. `fetched` is the date the entry's preview was downloaded, written by whoever adds or changes the entry. `tunes` names the effects the tune script renders, so P2's check knows every other effect must be sourced.

**`.claude/scripts/fetch-placeholder-sounds.ps1 [-Only <file>]`** (P1, under about 160 lines):
1. Finds `ffmpeg` on the PATH, else stops with "ffmpeg is missing: run `winget install Gyan.FFmpeg`, then open a new terminal." (decision 428).
2. Reads the manifest and refuses it, naming the entry, when a license isn't one of the three allowed or two entries share (`freesoundId`, `startSeconds`). With `-Only effects/PlaceholderHit.wav` it converts that entry alone (a swap or a gain change doesn't rewrite every WAV, so two PCs with different ffmpeg builds don't churn the repository); without, every entry.
3. Downloads each preview once with `curl.exe -fsSL` into its own cache folder (`$env:TEMP\jrpg-freesound\<id>.mp3`), reused on reruns. Downloads are untrusted data: nothing is run from that folder, paths are passed to ffmpeg as arguments.
4. Converts in two ffmpeg passes. The first measures the source's peak P (`atrim=start=<startSeconds>`, `volumedetect`). The second: `atrim=start=<startSeconds>`, `asetpts=PTS-STARTPTS`, `silenceremove=start_periods=1:start_threshold=<P - 40>dB:detection=peak`, `atrim=end=<maxSeconds>`, `afade=t=in:d=<0.001, or 0.005 with a startSeconds>`, `areverse,afade=t=in:d=<fadeOutSeconds>,areverse`, `volume=<peakDb - P + gainDb>dB`, written with `-ac 1 -ar 44100 -c:a pcm_s16le -fflags +bitexact -flags:a +bitexact -map_metadata -1` (a plain RIFF with only `fmt` and `data`, which `SoundEffect.FromFile` reads).
5. Prints each written file through `perl .claude/scripts/new-placeholder-tune.pl measure <wav>` (length and peak at a glance; a cap that cut the loudest part shows as a low peak).
6. Rewrites `content/audio/CREDITS.md` from the whole manifest (always, `-Only` or not) with `[IO.File]::WriteAllText(path, text, [Text.UTF8Encoding]::new($false))` and CRLF lines: a "generated by the script from `sound-sources.json`, don't edit" line, a sentence saying the music and the two jingles are generated and need no credit, then a table sorted by file: file, the sound's name linked to its page, author, license linked to its deed (`https://creativecommons.org/publicdomain/zero/1.0/`, `.../licenses/by/3.0/`, `.../licenses/by/4.0/`), "modified: trimmed, resampled to mono, level-adjusted", fetched.
7. Fails naming the entry on a failed download or conversion; a file already written stays.

**Finding a sound** (decision 437; every content slice):
1. Search the public site with the CC0 filter first, shortest first (`https://freesound.org/search/?q=<terms>&f=license:"Creative Commons 0"&s=duration_asc`); widen to CC-BY only when nothing CC0 fits the brief.
2. Choose one per slot from freesound's signals against the brief: the description (what was recorded, how), the duration (near the brief's length; a long take only with a clear `startSeconds`), the rating and the download count (both high suggests a clean, usable recording). Prefer a dry, single hit with a clean attack. Refuse any upload that names or imitates a commercial game or film (the rip rule, decision 427), and any NC, SA or Sampling+ license.
3. Open the sound's page: license, length, author, and the preview URL from its HTML. Add the entry with today's `fetched`, run `-Only`, check the printed length and peak.
4. Each slice's report lists the slots with the chosen page links and flags every CC-BY sound.
5. At the phase's listening (the phase's last slice), the developer plays the route and vetoes by ear. For each vetoed slot the implementer offers a shortlist of three candidates with their page links, to play in the browser; the developer picks one (or asks for three more), the entry changes, `-Only` reruns. Vetoes left at the phase's end go to L1.

### Structure and folder dependencies

Core, Data: no new type, folder or edge. Data.Tests: `Audio/SoundSourcesTests` (reads the manifest and the credits with `System.Text.Json`; no production code reads them).

Game:

| Folder | Depends on | New |
|---|---|---|
| `Ui` | (none new) | `UiSoundFiles.Effects.Deal`, `UiSoundFiles.Field` (a fourth group), `UiSoundFiles.Battle.LevelUp` |
| `Trade` | `Menus` (`EquipmentComparison`, exists), `Ui` (exists) | `GearPreview` |
| `Exploration` | `Dialogue`, `Audio`, `Ui` (exist) | `MessageReading` |
| `Crafting`, `Battle` | `Ui` (exists) | |

No new edge: every new type uses what its folder already reads.

### Key decisions
1. **Sound names stay `Placeholder...`** and a slot's file name doesn't change when its source does, so a swap touches only the manifest, the WAV and the credits. The Mend skill sound keeps its name (K1), so `AudioReferenceCheckTests` and the heal row of `ContentCatalogLoaderSkillSoundTests` don't change.
2. **The manifest has no Data loader.** The game never reads it, so a loader and validator in `Jrpg.Data` would be code with no caller; the content rule "every content type has a loader + validator" is about content the game loads. The script refuses what it can't write (a license, a repeated key); P2's Data test checks what the script can't see (an effect file with no entry, credits not regenerated).
3. **The credits are generated, then checked.** The script writes them; P2's test fails when an entry's file isn't in `CREDITS.md`, so a manifest edit committed without its rerun is caught.
4. **New sounds are asked where the thing happens, by the type that sees it** (decisions 421, 435):
   - the Shop's navigation by `ShopScene` (SH2); a deal by `ShopCounter` where `Settle` sees Core accept it, the picker's ticks and cancel by the counter around the picker's update; the counter writes into the scene's `AskedSounds`, passed to `Buy`, `Sell` and `Update` as `PlaybackSoundQueue.Follow(playback, sounds)` takes it (SH3); the Forge's salvage by `ForgeScene.Salvage` after `TrySalvage` succeeded (SH3);
   - a page step's sound by `EventPresenter` on the update its pages are built (FE2), through a small switch over the page steps (`GiveItemStep` → `Field.ItemGot` unless its receipt added nothing, `RecruitStep` → `.Recruit`, `StartQuestStep` and `AdvanceQuestStep` → `.QuestUpdated`, `CompleteQuestStep` → `.QuestCompleted`, Dialogue and Rest → none); a quest step that changed nothing never shows, so it never sounds;
   - a warp's by `ShowWarp` as it starts the fade from the map, never by `StayBlackIntoAWarp` (a cutscene's black, decision 434); a rest's by `EventPresenter` on the update `RestPlayback.IsFading` turns true, free or paid; the inn's refusal and No are silent (FE3);
   - a level-up by `VictoryScreen` when its bars' level-ups rose in the update, whether by filling or by the skip; the drops' `Field.ItemGot` on the Confirm that shows them; the confirm on the Confirm that leaves; `BattleScene` copies them (V1).
5. **Refactors before growth.** The backlog names the split for each file near 300; SH1 and FE1 move code with no behavior change, guarded by the existing tests passing unchanged. `BattleScene` (288) takes V1's 4 lines without its split (README, "Out of scope").
6. **Skill families** (K1, decision 431). A skill's sound plays as its action starts and its events' sounds at their beats (decision 423), so a family sound is the wind-up or the cast, never the landing.
7. **Briefs are distinct by kind.** Sounds heard close together get different kinds of sound, not the same shimmer at different pitches: a status applied is a sharp sting, a ward a low metallic hum, a status ending an airy release, a regen tick a water droplet, a heal a warm rising chime, an item got a bright pluck; a miss is a thin whiff, unlike the slash's blade swish and the blunt's heavy whoosh. The jingles are single hits (decision 432).

### Extension points
- A later placeholder sound (an effect, a voice, an ambient loop): a manifest entry and a rerun; the rule in `.claude/rules/content.md` (amended in P1) says so.
- Real audio: an entry leaves the manifest when its file is replaced by a recorded or bought one; P2 then needs the file in `tunes` or a new "owned" list, decided when the first one lands.
- A door's or chest's sound: Core reports the swaps an update made, the presenter asks a `Field` sound (decision 436).

### Replacement mapping
| Existing | Becomes | Slice |
|---|---|---|
| The 28 generated tones (`new-placeholder-tone.ps1`) | freesound previews converted by `fetch-placeholder-sounds.ps1` from `sound-sources.json`, same file names | P1, B1, B2, K1 |
| `new-placeholder-tone.ps1` and the backlog line about the tones | deleted once no file comes from it | K1 |
| `PlaceholderSkillStrike.wav`, `PlaceholderSkillCharm.wav` | eight new family files; `PlaceholderSkillMend.wav` kept with a new source | K1 |
| `ShopScene`'s preview fields and `SwitchMember`/`RefreshPreview` | `Trade/GearPreview` | SH1 |
| `ForgeSceneSoundTests.Update_YesInTheSalvagePrompt_AsksNothing` | `Update_YesInTheSalvagePrompt_AsksTheDealSound` (decision 435 changes the rule) | SH3 |
| `EventPresenter`'s `_reading`, `_messageBox`, `_typing`, `ShowPages`' turning | `Exploration/MessageReading`; the presenter keeps `StartReading` | FE1 |
| `EventPresenter.Sounds => _typing.Sounds` | the presenter's own `AskedSounds`, the reading's ticks copied in | FE1 |

## Risks
- **Licensing:** a CC-BY sound must be credited wherever the game is distributed; `CREDITS.md` is in `content/`, which the publish copies, and P2 keeps it current. A misread license is the slice's to catch on the sound's page; the script refuses anything outside the three SPDX ids. Sampling+ and NC are easy to pick by mistake from a search without the CC0 filter. A rip (a sound lifted from a game or film and uploaded as CC0) is refused on its description (decision 427), but one that doesn't say so can slip through: the developer's ear at each phase is the last check.
- **Network and third parties:** a slice can't finish offline; a sound removed from freesound or a preview URL changed stops only its regeneration, since the WAV is committed.
- **WAV compatibility:** ffmpeg adds a `LIST` chunk without `-fflags +bitexact`; the hand check of P1 is the game playing the converted files and the startup check passing. Stereo previews are mixed to mono, which can thin a wide stereo sound: listen.
- **Timing:** battle sounds land on beats (`PlaybackSoundQueue`), so a lead-in makes them late; the trim at the peak minus 40 dB keeps a soft attack while cutting room noise. A sound whose attack is quieter than that keeps a sliver of lead: set `startSeconds`.
- **Loudness:** a matched peak isn't a matched loudness (a short click and a long swell at the same peak differ); `gainDb` per entry, and L1 evens them by ear. Frequent sounds (move, typing, resource gain, ticks) must stay short and quiet: their briefs cap them.
- **Reproducibility:** two ffmpeg builds can produce different bytes for the same entry; `-Only` keeps an unchanged file untouched.
- **Repository size:** at 44 100 Hz every effect together is about 2 MB, each version kept by git; a swap adds its file's size again.
- **Files near 300 lines:** `ShopScene` 296 and `EventPresenter` 296 split first (SH1, FE1). `BattleScene` goes 288 → about 292 in V1. `UiSoundFiles` (83) grows to about 110. `EventPresenterTests` (305) gets no test; new test files sit beside it. `ShopScene` after SH1 about 255, after SH2 and SH3 about 280: SH2 says where the asks go if it would pass 285.
- **Test churn:** K1 changes two rows of `ContentCatalogLoaderSkillSoundTests` (fire_blast, taunt) and its `Replace` string; `EnemyTacticsValidatorTests` names `PlaceholderSkillStrike.wav` in inline JSON that no existence check reads, and K1 moves it to `PlaceholderSkillBlunt.wav` so no deleted name lingers. FE2 takes `UiSoundFilesTests.EveryGroup_IsCheckedAbove` from 3 to 4. SH3 rewrites `ForgeSceneSoundTests.Update_YesInTheSalvagePrompt_AsksNothing`, since decision 435 changes what it pins. SH3 changes `ShopCounter.Buy`, `Sell` and `Update`'s signatures: no test calls them (the tests drive the counter through `ShopScene`), so only the scene's three calls change.
- **Determinism and saves:** none; no sound reads a random or the clock, and nothing is saved.
