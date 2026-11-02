# Sound pass: freesound placeholders and the places still silent
Status: approved

Drafted on 2026-11-02 from the developer's request: "Make another plan to make another pass on sound, and use https://freesound.org/ as a reference: find there sounds as placeholders. If you need other placeholders in the future, use this platform." Builds on the music and sound effects plan (`docs/plans/done/2026-10-29-music-and-sfx/`), ADR 0018 and ADR 0035, and on the backlog's lines from that plan (`docs/plans/backlog.md`, "From the music and sound effects plan").

- Numbers: decisions 426-437, ADR 0036.
- Approved on 2026-11-02 with every pick taken and the devil's advocate's edits: sounds are chosen by the implementer and heard by the developer at the end of each phase, a vetoed one getting a shortlist of three (decision 437); the developer says "go" per phase; BO dropped (V1 grows `BattleScene` by about 4 lines); P2 moved after K1 and shrunk to three tests, with the script refusing a disallowed license or a repeated (id, start) key; credits carry the license link, the modification note and the fetch date; the rip rule (decision 427); effects at 44 100 Hz; the trim relative to the measured peak; a found item that couldn't be carried is silent; the Forge's salvage sounds the deal too (decision 435); jingles are single hits (decision 432); the briefs made distinct; K1 changes 23 skill lines.

## Goal
1. **Real placeholder effects:** every sound effect that is a generated sine tone today (28 of the 30 files in `content/audio/effects/`: the four interface sounds, 21 battle sounds and the three skill sounds) is replaced by a sound found on freesound.org, converted to the game's WAV format by a script from a manifest that names each sound's source, author and license. The credits file is written from the same manifest. The music and the victory and defeat jingles stay the tune script's.
2. **Sounds where there are none:** the Shop (move, confirm, cancel, a deal's coins), the Forge's salvage, the field's events (an item got, a warp, a rest, a recruit, a quest updated or completed) and the victory screen (a level-up, the drops).
3. **The way for later placeholders:** the content rule says placeholder sound effects come from freesound through the manifest, so the next plan that needs a sound follows the same path.
4. **Core is untouched**: no rule, state, save or content record changes; the Game maps what it already knows to new sound files.

## Order, and why
The developer says "go" per phase, so Phases 1 and 2 (content and the script only) are heard and settled before any code slice starts.
1. **Phase 1: the pipeline (ADR 0036, P1).** The manifest, the fetch script and the credits proven end to end on the four interface sounds, the ones heard most. The phase's listening is P1's hand check.
2. **Phase 2: battle and skill sounds (B1, B2, K1, P2).** Content only: the existing slots get freesound sounds; the skills move from three shared sounds to nine families (decision 431), which retires the tone script. Once no effect is a tone, P2 adds the test that keeps the manifest, the effects folder and the credits in step. The phase's listening is P2's hand check.
3. **Phase 3: the Shop (SH1, SH2, SH3).** `ShopScene` is at 296 lines: its gear preview moves out first (the backlog's line), then the navigation sounds, then the deal's sound, the Forge's salvage with it. The phase's listening is SH3's hand check.
4. **Phase 4: the field's events (FE1, FE2, FE3).** `EventPresenter` is at 296 lines: its page reading moves out first (the backlog's line), then the page steps' sounds, then the warp's and the rest's. The phase's listening is FE3's hand check.
5. **Phase 5: the victory screen (V1).** The level-up, drops and leaving sounds; `BattleScene` grows by about 4 lines (288 → about 292), and the backlog's line on splitting it stays for the next change. Its listening is V1's hand check.
6. **Phase 6: levels (L1).** The levels evened out across every sound, and the sounds still vetoed replaced.

## Guiding rules
- **No Core change.** Every new sound is chosen by the Game from what it already shows (a step, a fade starting, a deal settled, a bar's level-ups), as `BattleSounds` maps battle events.
- **Headless first** (ADR 0035). Each new sound is asked through an `AskedSounds` and tested through what the host reads (`IScene.Sounds`, the presenter's `Sounds`), each branch its own test, a second update to prove the per-update clear (`.claude/rules/tests.md`).
- **Decision 421 holds, with 435:** a sound is asked where the action went through, never at the press; a refusal stays silent. A yes/no question is silent, its No included; what its Yes carries out (a sale, a salvage, an inn's rest) sounds as that deal or rest. A value chooser (the count picker) ticks on a change and its Cancel sounds.
- **Sound files keep their `Placeholder` names.** They are still placeholders: swapping one is a manifest line and a rerun, with no change to `UiSoundFiles` or `skills.json`. A new slot gets a new `Placeholder...wav` constant and its file in the same slice (`UiSoundFilesTests`).
- **Never edit a converted WAV by hand.** Every freesound effect comes out of `.claude/scripts/fetch-placeholder-sounds.ps1` from `content/audio/sound-sources.json`; a change of source, trim or gain is a manifest edit and a rerun (`-Only <file>`), and `content/audio/CREDITS.md` is rewritten by the same run, committed with it.
- **Choosing is the implementer's, hearing is the developer's** (decision 437). Each slice lists the slots it fills with a one-line brief (what it should sound like, the longest it may be), not sound ids; design.md, "Finding a sound", says how one is picked, and each phase's last slice holds the phase's listening. A slice's report flags every CC-BY sound it took.
- **Slice sizes:** under about 250 lines, tests at about 13 lines each plus the existing tests a change breaks; manifest entries count about 15 lines each. No slice adds a view type.
- **Files near 300 lines:** `ShopScene` 296 and `EventPresenter` 296 are split before they grow (SH1, FE1); `BattleScene` (288) takes V1's 4 lines and stays under 300; `ActionPlayback` 292 and `PartyPage` 296 aren't touched. New tests go in new files beside `EventPresenterTests` (305).

## Out of scope (follow-ups)
- Music and jingles: the five tracks and the victory and defeat jingles stay the tune script's (decision 429); composed or licensed music.
- The voice-over placeholder (`voices/Placeholder.wav`): voices are a recording question, not an effect.
- Footsteps, ambient loops, positional (panned) sound, pitch variation, per-sound volume settings, a refused buzzer, prompt sounds (decisions 421, 430).
- A door's or a chest's opening (`SwapObjectStep`): Core applies it inside the event run and the Game never sees it, so a sound needs Core to report the swap (decision 436).
- The dialogue's page turn, the field interaction itself, the inn's question: silent, as every question (decision 435).
- Splitting `BattleScene`'s outcome screens: the backlog's line stays for the next change that grows it past V1.
- Original-quality files through freesound's OAuth2 API, an API key for searching (ADR 0036's rejected options).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: the pipeline
- [x] ADR 0036: third-party placeholder audio. Write it from the entry under "ADRs" below (the options are chosen).
- [x] **P1. The manifest, the fetch script and the credits; the four interface sounds from freesound; the content rule amended** (script + content, about 240 lines) → [slices/P1.md](slices/P1.md)

### Phase 2: battle and skill sounds
- [x] **B1. The battle's start and its impact sounds from freesound: hit, critical, miss, block, resisted, status, heal, fall** (content, about 140 lines) → [slices/B1.md](slices/B1.md)
- [x] **B2. Every other battle sound from freesound: ticks, backfire, stun, gain, status ended, move, extra turn, item, Defend, enraged, escape** (content, about 180 lines) → [slices/B2.md](slices/B2.md)
- [ ] **K1. Skill sounds in nine families from freesound; the tone script retired** (content + Data tests, about 180 lines) → [slices/K1.md](slices/K1.md)
- [ ] **P2. The manifest's check: every effect sourced or a tune, licenses allowed, every entry credited; Phase 2's listening** (Data tests, about 80 lines) → [slices/P2.md](slices/P2.md)

### Phase 3: the Shop
- [ ] **SH1. The Shop's gear preview moves into `Trade/GearPreview`** (Game refactor, about 110 lines) → [slices/SH1.md](slices/SH1.md)
- [ ] **SH2. The Shop's move, confirm and cancel sounds on the tabs and in the lists** (Game, about 190 lines) → [slices/SH2.md](slices/SH2.md)
- [ ] **SH3. A deal's sound as a purchase, a sale or the Forge's salvage goes through; the picker's count ticks** (Game + content, about 210 lines) → [slices/SH3.md](slices/SH3.md)

### Phase 4: the field's events
- [ ] **FE1. The event presenter's page reading moves into `Exploration/MessageReading`** (Game refactor, about 140 lines) → [slices/FE1.md](slices/FE1.md)
- [ ] **FE2. A found item, a recruit and a quest's start, advance or completion sound as their line shows** (Game + content, about 235 lines) → [slices/FE2.md](slices/FE2.md)
- [ ] **FE3. A warp from the map and a rest sound as their fade starts** (Game + content, about 170 lines) → [slices/FE3.md](slices/FE3.md)

### Phase 5: the victory screen
- [ ] **V1. The victory screen sounds a level-up, the drops and leaving** (Game + content, about 170 lines) → [slices/V1.md](slices/V1.md)

### Phase 6: levels
- [ ] **L1. Levels evened out across every sound; the sounds still vetoed replaced** (content, hand check only) → [slices/L1.md](slices/L1.md)

## Decisions
- **426. Placeholder effects come through a manifest and a script.** `content/audio/sound-sources.json` names each placeholder effect's freesound source and the conversion's settings; `.claude/scripts/fetch-placeholder-sounds.ps1` downloads each preview with `curl`, converts it with ffmpeg into the named WAV and writes `content/audio/CREDITS.md` from the manifest. Swapping a sound is one manifest line and a rerun. No API key. The technical choices (format, trim, fades, credits, the unique key) are ADR 0036's.
- **427. License policy:** CC0 preferred; CC-BY 3.0 or 4.0 when nothing CC0 fits (the credits list every sound anyway, and the slice's report flags each CC-BY one); never NC, SA or Sampling+. **Rip rule:** any upload that names or imitates a commercial game or film is refused, whatever its license.
- **428. ffmpeg is installed once per PC** with `winget install Gyan.FFmpeg`; the script stops with that line when it's missing. It's needed only to regenerate a sound: the converted WAVs are committed, so the game, the build and the tests never need it.
- **429. The music and the victory and defeat jingles stay the tune script's**; this pass is effects only. Revisit if the tunes grate.
- **430. New sound slots.** In: the Shop (move, confirm, cancel, a deal's coins); the Forge's salvage (the deal's sound, decision 435); the field's events: an item got, a warp, a rest, a recruit, a quest updated, a quest completed; the victory screen's level-up, drops and leaving. Out: footsteps, ambient loops, positional sound, a refused buzzer, prompt sounds, a door's or chest's opening (decision 436), the dialogue's page turn.
- **431. Skill sounds come in nine families**, named in `skills.json` as today, so a skill moves family by editing its line: Fire (fire_blast, fireball, meteor), Dark (eldritch_bolt, void_rift, chaos_lance), Light (smite), Slash (power_strike, cleave, equilibrium), Blunt (shield_bash, rampage, palm_strike, flurry, crushing_blow), Mend (heal, recovery, heal_all, field_dressing, rejuvenate), Shout (taunt, war_cry), Ward (aura_of_protection, wide_aura, ready), Hex (expose, rot).
- **432. The new moments' jingles (the rest, a recruit, a quest completed, a level-up) are non-melodic single hits of 0.8 s or less** from freesound, never a fanfare over the victory fanfare.
- **433. Quests have two sounds:** one for a quest started or advanced, a brighter one for a quest completed.
- **434. Every warp started from the map sounds the same short "step through"** (a door's and a map edge's alike, since content can't tell them apart); a warp out of a cutscene's black (the intro's nine) is silent.
- **435. What a question's Yes carries out sounds; the question doesn't.** A deal that went through sounds the coins however it was confirmed (a stock piece at once, the picker's Yes, the sale question's Yes), and the Forge's salvage sounds it too; a rest sounds as its fade starts, after the inn's Yes too. A yes/no question is silent, its No included. A value chooser (the count picker) ticks the move sound on a change and its Cancel sounds the cancel. This replaces the music plan's pick that a prompt's Yes was silent.
- **436. A door's or a chest's opening stays silent in this pass:** `SwapObjectStep` runs inside Core and never reaches the presenter. Follow-up: Core reports the swaps an update made, as it reports `StartedVoice`, then the Game sounds them.
- **437. How sounds are chosen:** the implementer picks one sound per slot from freesound's signals (description, duration, rating, downloads) against the slot's brief, converts it, and the developer listens at the end of each phase (the phase's last slice's hand check), not only at the end of the plan. A vetoed sound gets a shortlist of three candidates with their page links, to play in the browser; the developer picks one, or asks for three more.

Planner's picks (not numbered; design.md records them): the manifest's `file` is a path under `content/audio/` and its `tunes` list names the effects the tune script renders; the game never reads the manifest, so it has no Data loader and its check is a Data test (design.md, "Key decisions" 2); previews are the `-hq.mp3` files; a swap reruns one file (`-Only`), so two PCs with different ffmpeg builds don't churn every WAV; the victory's drops reuse the field's item sound; the skip on the victory screen sounds only the level-ups it reveals, leaving sounds the confirm; the Mend skill sound keeps its file name.

## Open design questions (developer decides)
None open. Noted for the developer's review (interpretations, not forks): the menus' notices already sound (an item used, an equip, a swap sound the confirm; a refusal's notice is silent, decision 421), so the menu gets nothing new; the field's item use keeps the confirm rather than the battle's item sound; the victory screen's gold row stays silent (the fanfare covers it).

## ADRs
- **ADR 0036: third-party placeholder audio** (written before P1 from this entry; no options left to ask):
  - **Context:** the generated sine tones (`new-placeholder-tone.ps1`) were painful and uninformative; the developer asked for freesound.org placeholders, and for every future placeholder sound to come from there (decisions 426, 427).
  - **The manifest:** `content/audio/sound-sources.json` names, per placeholder file (a path under `content/audio/`), the freesound id, the sound's name, its page as `https://freesound.org/s/<id>/`, its author, its license (an SPDX id), its preview URL, the date it was fetched, and the conversion's start, length, fade and gain; a `tunes` list names the effects the tune script renders. The key is (id, start): one take can feed several slots from different starts, never the same slot twice. The game never reads it.
  - **The conversion:** `.claude/scripts/fetch-placeholder-sounds.ps1` downloads the public preview (`-hq.mp3`, 128 kbps, no login) and converts it with ffmpeg to 16-bit PCM mono 44 100 Hz (about 2 MB for every effect; the tunes stay 22 050 Hz). ffmpeg already drops the MP3's encoder delay when the header carries it, so the trim is for an uploader's lead-in: silence below the measured peak minus 40 dB (`detection=peak`) is cut from the start; a 1 ms fade in (5 ms when `startSeconds` cuts into the sound) and a short fade out against clicks; the peak set to the manifest's level plus the entry's gain; bitexact output, a plain RIFF with only `fmt` and `data`. It refuses an entry with a disallowed license or a repeated (id, start) key, since it is the only writer of the converted WAVs.
  - **The credits:** `content/audio/CREDITS.md` is rewritten from the whole manifest on every run (UTF-8 without BOM, CRLF): per entry, the file, the sound's name linked to its page, the author, the license linked to its deed, "modified: trimmed, resampled to mono, level-adjusted", and the fetch date. A Data test checks every entry is in it, every effect is sourced or a tune, and every license is allowed.
  - **Options considered:** originals through freesound's OAuth2 API (better quality, but an account, a token flow and a secret on two PCs, for placeholders); downloading and converting by hand (nothing reproducible, credits drift); an API key for search (not needed once the manifest stores the preview URL; searching is done on the public site); a NuGet decoder (NAudio) in a tool project instead of ffmpeg (a package approval, and its MP3 decoding leans on Windows codecs).
  - **Consequences:** preview quality is enough for placeholders, not for release; real audio replaces manifest entries one by one (a recorded or bought file leaves the manifest and its credit with it); CC-BY sounds oblige the credits file to ship, which `content/` publishing already does; a sound removed from freesound only stops its regeneration, the committed WAV stays; a future content-driven need for a placeholder (a voice, an ambient loop) is a manifest entry, not a new pipeline.
