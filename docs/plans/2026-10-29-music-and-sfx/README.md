# Music and sound effects
Status: draft

Drafted on 2026-10-29 from the developer's request. Builds on ADR 0018 (voices: WAV through MonoGame's `SoundEffect`, scenes write a `VoiceChannel`, the host plays it after each update, an exact-name startup check, silent without a device), which left music and effects to "their own plan". Numbers: none yet; questions are Q1... and decisions are listed unnumbered until approval.

## Goal
1. **Volumes**: Music, Effects and Voices in the Options page and `PlayerSettings`, applied at once; the voice-over already follows its volume.
2. **Music**: a track per map (a field in the map's settings, chosen in the editor's map settings), a battle theme, a per-encounter theme for bosses, a title theme; tracks loop and change smoothly on scene changes; after a battle the map's track comes back.
3. **Sound effects**: cues for menu moves and confirms, dialogue typing, battle hits, misses, critical hits, a status applied, and victory; their files named in content, checked at startup by exact name as voices are; silent without a device.
4. **Core stays free of audio**: the Game maps what Core reports (battle events, the session's state) to music and cues, as `ActionPlayback` maps battle events to visuals. Determinism is unaffected.

## Order, and why
1. **Phase 1: volumes (V1).** Small and independent, and checkable by ear at once on the intro's voice-over.
2. **Phase 2: music (C1, C2, P1, P2, P3, E1).** Content first (map and encounter tracks, then the game-wide audio file with the themes and the effect files, both checked at startup), then music chosen headless from the flow's state, then played on the device, then crossfaded and resumed. The editor's track choice comes once the field exists and the track can be heard.
3. **Phase 3: sound effects (S1, S2, S3, S4, B1, B2, D1, F1).** The effect path with its first caller (the title screen), then the small screens, the game menu (two slices), the battle (impacts and victory, then commands), dialogue typing, and the Forge.
4. **Phase 4: options look and keeping them (V2, V3).** The volume rows drawn as pips once every volume has something to act on; V3 only if Q1 says settings are saved between runs.

## Guiding rules
- **No audio in Core.** Core gains only content names (`MapSettings.MusicFile`, `EncounterDefinition.MusicFile`, `Content/AudioDefinition`), as it holds `VoiceStep.File` and `BackgroundFile` today. Playback, cue ids and channels are the Game's; the closed list of effect ids is Data's (design.md, "Key decisions").
- **Headless first.** Every rule (which track plays, which cue a turn's impact makes, when typing ticks, volume steps, fade gains) is a plain class tested in `tests/Jrpg.Game.Tests` by the channel values it writes; only `MusicPlayer`, `SoundEffectPlayer`, `VoicePlayer` and `SoundLibrary` touch MonoGame audio, and they get a hand check.
- **Silent, never stopped.** No device, a file that won't load, or a cue content doesn't name: silent, reported once (ADR 0018).
- **Placeholders, not composing.** Every audio file this plan adds is a generated tone or chord named `Placeholder...wav` (design.md, "Tunables and placeholders"); no music is composed here.
- **No NuGet package.** MonoGame's `SoundEffect` and `SoundEffectInstance` only (design.md, "Which MonoGame types carry music").
- **Slice sizes:** under about 250 lines, tests at about 13 lines each plus the existing tests a change breaks. A Game slice that adds a view type gives its raw estimate and the 1.6x. Editor slices aim at about 200 lines.
- **Files near 300 lines:** new tests go in new files beside `EventPresenterTests` (305), `GameFlowTests` (321), `CommandMenuTests` (393), `ActionPlaybackTests` (287), `ContentCatalogLoaderTests` (272). Watched: `PartyPage` 275, `EventPresenter` 258, `GameFlow` 254, `BattleScene` 254, `ActionPlayback` 252, `InventoryPage` 242, `ForgeScene` 233, `GameMenuScene` 223, `CommandMenu` 220. `Editor/Shell/MainViewModel` is already 490 (design.md, "Risks").
- **No save format change.** Settings aren't in saves; content gains optional fields only.

## Out of scope (follow-ups)
- Composed music, recorded effects, per-cue volumes, pitch variation, positional (panned) sound.
- Streaming or compressed music (`Song`/`.ogg`): the ADR records the way to switch if real tracks make WAV too big.
- Sounds for battle happenings beyond the listed ones (blocked, resisted, heals, ticks, backfires, falls, escape), field sounds (footsteps, doors, chests), ambient loops, a defeat jingle: extension points in design.md, pending Q9.
- An editor for encounters' themes or the game-wide audio file: they are edited in JSON.
- Pausing audio when the window loses focus.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: volumes
- [ ] **V1. Music, Effects and Voices volumes in the Options page; the voice-over follows its volume** (Game + text, about 190 lines) → [slices/V1.md](slices/V1.md)

### Phase 2: music
- [ ] **C1. A map's track and an encounter's theme in content, checked at startup; placeholder tracks** (Core + Data + content, about 190 lines) → [slices/C1.md](slices/C1.md)
- [ ] ADR: game-wide audio names (content file, Data's closed effect list), written from the README's entry once the developer has chosen.
- [ ] **C2. The game-wide audio file: title and battle themes, every effect's file, checked at load and startup** (Core + Data + content, about 235 lines) → [slices/C2.md](slices/C2.md)
- [ ] ADR: how the game asks for music and effects (music from the flow's state, effects reported by scenes as values).
- [ ] ADR: music playback and file format (looped `SoundEffectInstance` WAV, or `Song`/`MediaPlayer` OGG), extending ADR 0018.
- [ ] **P1. The music the flow calls for, headless: title, map, battle theme from the encounter's flash until the battle screen closes** (Game, about 210 lines) → [slices/P1.md](slices/P1.md)
- [ ] **P2. The music plays on the device: looped, at the music volume, changing tracks with a cut** (Game, about 110 lines) → [slices/P2.md](slices/P2.md)
- [ ] **P3. Tracks crossfade; the map's track resumes after a battle** (Game, about 200 lines) → [slices/P3.md](slices/P3.md)
- [ ] **E1. The editor's map settings choose the map's track** (Editor + Data, about 170 lines) → [slices/E1.md](slices/E1.md)

### Phase 3: sound effects
- [ ] **S1. The effect path, and the title screen's move, confirm and cancel cues** (Game, about 230 lines) → [slices/S1.md](slices/S1.md)
- [ ] **S2. Cues on the leader choice and the defeat menu** (Game, about 120 lines) → [slices/S2.md](slices/S2.md)
- [ ] **S3. Cues on the game menu's tabs, the Options page and the System page** (Game, about 190 lines) → [slices/S3.md](slices/S3.md)
- [ ] **S4. Cues on the Inventory, Equipment, Party and Status pages** (Game, about 210 lines) → [slices/S4.md](slices/S4.md)
- [ ] **B1. A turn's impact sounds its hits, critical hits, misses and statuses; victory sounds** (Game, about 180 lines) → [slices/B1.md](slices/B1.md)
- [ ] **B2. Cues on the battle's commands, skill and item lists and target cursor** (Game, about 140 lines) → [slices/B2.md](slices/B2.md)
- [ ] **D1. Dialogue typing ticks** (Game, about 140 lines) → [slices/D1.md](slices/D1.md)
- [ ] **F1. Cues at the Forge** (Game, about 110 lines) → [slices/F1.md](slices/F1.md)

### Phase 4: options look and keeping them
- [ ] **V2. The volume rows as pip meters** (Game, raw 70, about 110 lines at 1.6x) → [slices/V2.md](slices/V2.md)
- [ ] ADR: a settings file (only if Q1 = saved between runs; question 89 said a settings file needs one).
- [ ] **V3. Settings kept between runs** (Game, about 170 lines; only if Q1 = yes) → [slices/V3.md](slices/V3.md)

## Decisions (proposed, numbered at approval)
Planner's picks, each open to the developer's change; the questions below may override some.
- **Music follows the flow's state, not scenes' writes**: no session → the title theme (title and leader choice); a pending encounter or a battle screen on show → its encounter's `musicFile`, else the battle theme; otherwise the current map's `musicFile`. The battle theme starts at the encounter's flash.
- **The same track across a change plays on**: a warp between two maps with one `musicFile` doesn't restart it.
- **A map without `musicFile` is silent** (pending Q5).
- **The battle theme plays through the victory and defeat screens** until the battle screen closes (pending Q4).
- **One crossfade length for every change** (placeholder 1.0 s, pending Q2), and **the track left for a battle resumes where it paused** (pending Q3).
- **Scenes report effect ids as values; the flow writes them into the effect channel by file** (ADR C's pick): no scene or page gains a constructor parameter.
- **One sound per cue per update**: the same id asked twice in an update plays once (three hits landing together make one hit sound).
- **A turn's impact**: a critical hit sound if any hit was critical, else a hit sound if damage landed; a miss sound if any hit missed; a status sound if any status was applied. Blocked and resisted are silent (pending Q9).
- **A move cue only when the selection changed** (a one-entry list or a disabled neighbour makes none).
- **Volumes are eleven steps, 0 (silent) to 10 (full), a linear gain**; defaults Music 7, Effects 8, Voices 10 (placeholders).
- **Each player sets its own instances' volume; `SoundEffect.MasterVolume` is never touched** (it is one global for all three).
- **Typing ticks every second character typed, never on a space, and not at Instant speed** (placeholders, pending Q8).

## Open design questions (developer decides)
- **Q1. Are settings saved between runs?** You asked for volumes "saved as the other settings are", but today none are: `PlayerSettings` is "for this run only (question 89)". Either the volumes are kept for the run too (this plan without V3), or every setting is saved to a file (V3 and its ADR).
- **Q2. How does music change at each kind of scene change?** The plan crossfades every change over 1.0 s. Alternatives: the battle theme cuts in at the encounter's flash (classic), the title and loads fade out with the screen's fade to black and the new track starts at black, warps crossfade.
- **Q3. After a battle, does the map's track resume where it paused, or restart?** (The plan resumes.)
- **Q4. Victory: is it a one-shot fanfare over silence (the battle theme fading out at the victory screen), a fanfare over the battle theme, or a looping victory theme?** The plan plays the `victory` effect over the battle theme.
- **Q5. A map without a track: silence, or the previous track plays on?** (The plan: silence. The intro map has none; it has its voice-over.)
- **Q6. The boss hook: any encounter may name its own theme (`musicFile`, the plan), or an `isBoss` flag picks one boss theme from the game-wide file?**
- **Q7. Which UI cues and screens?** The plan has move, confirm and cancel on the title, leader choice, defeat menu, game menu pages, battle commands and the Forge. Is a cancel cue wanted (you listed moves and confirms)? A "refused" buzzer for greyed actions? Prompts?
- **Q8. Typing: how often does it tick, and is it silent while a voice-over plays?** Some games tick per character, some per word or not at all with voice.
- **Q9. Which other battle happenings get a sound in this plan** (blocked, resisted, heals, Poison and Regen ticks, backfires, falls, escape)? The plan: none.
- **Q10. Do the volumes show as numbers (V1) then pips (V2), or as a bar?** The reference UI shows a slider; the plan reuses `Ui/PipRow`'s diamonds.

## ADRs needed
- **ADR needed: game-wide audio names.** Options: a content file `content/audio.json` (themes and effect files) with Data's closed list of effect ids, checked at load and startup (the plan's pick); code constants as `Ui/UiArtFiles` (decision 84), checked by a test; or a file-name convention (`effects/<id>.wav`). Before C2.
- **ADR needed: how the game asks for music and effects.** Options: the flow chooses the music from its state and scenes report effect ids as values the flow writes into a channel (the plan's pick); or channels injected into every presenter that sounds (most scenes and pages are already at four constructor parameters). Before P1.
- **ADR needed: music playback and file format** (ADR 0018 deferred it to this plan). Options: looped `SoundEffectInstance` from WAV (crossfade, resume, per-instance volume; whole file in memory, big files in git; the plan's pick for placeholders) or `Song`/`MediaPlayer` from OGG (streams, small files; one global song, no crossfade, static state). Before P1.
- **ADR needed (only if Q1 = yes): a settings file** (format, place, when it is written, what a broken one does). Before V3.
