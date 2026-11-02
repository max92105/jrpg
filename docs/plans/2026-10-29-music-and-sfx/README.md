# Music and sound effects
Status: approved, in progress

Drafted on 2026-10-29 from the developer's request. Builds on ADR 0018 (voices: WAV through MonoGame's `SoundEffect`, scenes write a `VoiceChannel`, the host plays it after each update, an exact-name startup check, silent without a device), which left music and effects to "their own plan".

- Numbers: decisions 415-425, ADR 0035.
- Approved on 2026-10-29 with the developer's answers (415-424) and the devil's advocate's edits: the effect channel is cleared first in `GameFlow.Update` and filled from the top scene right after `_scenes.Update`, before a request is applied; the sound files the code plays are constants (decision 425), so the game-wide audio file and its ADR are gone; one ADR (0035) extends 0018; no crossfade (a cut at the flash, the map's track resumed with a short fade-in); the Forge's cues joined the small screens' slice; every skill and every battle event kind sounds (decision 423), which added battle slices; settings saved between runs left for a separate plan (decision 415).

## Goal
1. **Volumes**: Music, Effects and Voices in the Options page and `PlayerSettings`, 0 to 10, shown as diamond pips (decision 424), applied at once; kept for the run until the settings plan (decision 415).
2. **Music**: a track per map (a field in the map's settings, chosen in the editor's map settings; none is silence, decision 419), a battle theme, each encounter's own theme (the Goblin Chief's boss theme, decision 420), a title theme. At a battle the music cuts to the battle theme at the flash; afterwards the map's track resumes where it paused with a short fade-in (decisions 416, 417); a victory's fanfare plays over silence, then the map's track returns (decision 418).
3. **Sound effects**: move, confirm and cancel on the menus and screens, a battle-start sound at the flash (decision 421), dialogue typing (decision 422), a sound for every skill (named in `skills.json`) and every battle event kind (decision 423).
4. **Core stays free of audio**: the Game maps what Core reports (battle events, the session's state) to music and sounds, as `ActionPlayback` maps battle events to visuals. Determinism is unaffected.

## Order, and why
1. **Phase 1: volumes (V1).** Small and independent, and checkable by ear at once on the intro's voice-over.
2. **Phase 2: music (C1, P1, P2, P3, E1, C2).** Map tracks in content first, then the music chosen headless from the flow's state, played on the device, then the pause and resume around a battle; the editor's choice once a track can be heard; the encounter's own theme once the enemy plan's Goblin Chief encounter exists (its first user, enemy plan decision 414).
3. **Phase 3: sound effects on the screens (S1, S2, S3, S4, D1).** The effect path with its first caller (the title), the small screens and the Forge, the game menu in two slices, dialogue typing.
4. **Phase 4: battle sounds (B1, B2, B3, B4, B5).** The impact's events with the timing every battle sound uses, every other event kind, the skills' own sounds, the battle start and the outcomes with their music, then the commands.
5. **Phase 5: the look (V2).** The volume rows as pips.

## Guiding rules
- **No audio in Core.** Core gains only content names (`MapSettings.MusicFile`, `EncounterDefinition.MusicFile`, `SkillDefinition.SoundFile`), as it holds `VoiceStep.File` and `BackgroundFile` today. Playback, channels and the files the code plays are the Game's (design.md, "Key decisions").
- **Headless first.** Every rule (which track plays, which sound an event makes and when, when typing ticks, volume gains, the pause and resume) is a plain class tested in `tests/Jrpg.Game.Tests` by the values it gives or the channel it writes; only `MusicPlayer`, `SoundEffectPlayer`, `VoicePlayer` and `SoundLibrary` touch MonoGame audio, and they get a hand check.
- **Silent, never stopped.** No device, or a file that won't load: silent, reported once (ADR 0018).
- **Placeholders, not composing.** Every audio file this plan adds is a generated tone or chord named `Placeholder...wav` (design.md, "Tunables and placeholders"); no music is composed here.
- **No NuGet package.** MonoGame's `SoundEffect` and `SoundEffectInstance` only (design.md, "Which MonoGame types carry music").
- **Slice sizes:** under about 250 lines, tests at about 13 lines each plus the existing tests a change breaks. A Game slice that adds a view type gives its raw estimate and the 1.6x. Editor slices aim at about 200 lines.
- **Files near 300 lines:** new tests go in new files beside `EventPresenterTests` (305), `GameFlowTests` (321), `CommandMenuTests` (393), `ActionPlaybackTests` (287), `ContentCatalogLoaderTests` (272). Watched: `PartyPage` 275, `EventPresenter` 258, `GameFlow` 254, `BattleScene` 254, `ActionPlayback` 252 (the enemy plan's B3 adds to it too), `InventoryPage` 242, `ForgeScene` 233, `GameMenuScene` 223, `CommandMenu` 220. `Editor/Shell/MainViewModel` is already 490 (design.md, "Risks").
- **No save format change.** Settings aren't in saves; content gains optional fields and one required skill field.

## Out of scope (follow-ups)
- **Settings kept between runs** (decision 415): a separate small plan after this one, for the volumes, full screen and text speed (question 89; it needs its own ADR on the settings file).
- Composed music, recorded effects, per-sound volumes, pitch variation, positional (panned) sound.
- Crossfades between tracks (decision 416 chose cuts), streaming music (ADR 0035 names the way).
- Field sounds (footsteps, doors, chests), ambient loops, a refused buzzer, prompt sounds (decision 421).
- A `playMusic` event step (ADR 0035 says where it would go: Core state and the save).
- An editor for encounters' themes or skills' sounds: they are edited in JSON.
- Pausing audio when the window loses focus.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: volumes
- [x] **V1. Music, Effects and Voices volumes in the Options page; the voice-over follows its volume** (Game + text, about 200 lines) → [slices/V1.md](slices/V1.md)

### Phase 2: music
- [x] **C1. A map's track in content, checked at startup; placeholder map tracks** (Core + Data + content, about 170 lines) → [slices/C1.md](slices/C1.md)
- [x] ADR 0035: music and sound effects, extending ADR 0018. Write it from the entry under "ADRs" below (the options are chosen).
- [x] **P1. The music the flow calls for, headless: the title theme, the map's track, the battle theme from the flash** (Game, about 220 lines) → [slices/P1.md](slices/P1.md)
- [x] **P2. The music plays on the device: looped, at the music volume, cut on a change; the themes preloaded** (Game, about 110 lines) → [slices/P2.md](slices/P2.md)
- [x] **P3. The map's track pauses for a battle and resumes where it was with a short fade-in; a track left is released** (Game, about 220 lines) → [slices/P3.md](slices/P3.md)
- [x] **E1. The editor's map settings choose the map's track** (Editor + Data, about 170 lines) → [slices/E1.md](slices/E1.md)
- [x] **C2. An encounter's own theme: the Goblin Chief's boss theme** (Core + Data + Game + content, about 110 lines; after the enemy plan's C1) → [slices/C2.md](slices/C2.md)

### Phase 3: sound effects on the screens
- [x] **S1. The effect path, and the title's move, confirm and cancel sounds** (Game, about 240 lines) → [slices/S1.md](slices/S1.md)
- [x] **S2. Sounds on the leader choice, the defeat menu and the Forge** (Game, about 200 lines) → [slices/S2.md](slices/S2.md)
- [x] **S3. Sounds on the game menu's opening and tabs, the Options page and the System page** (Game, about 210 lines) → [slices/S3.md](slices/S3.md)
- [x] **S4. Sounds on the Inventory, Equipment, Party and Status pages** (Game, about 230 lines) → [slices/S4.md](slices/S4.md)
- [x] **D1. Dialogue typing ticks, spaced and muted under a voice** (Game, about 170 lines) → [slices/D1.md](slices/D1.md)

### Phase 4: battle sounds
- [ ] **B1. A turn's impact sounds its hits, critical hits, misses, blocks, resists, statuses and heals, each at its beat** (Game, about 245 lines) → [slices/B1.md](slices/B1.md)
- [ ] **B2. Every other battle event kind sounds at its beat: falls, ticks, backfires, stuns, gains, moves, items, Defend** (Game, about 200 lines) → [slices/B2.md](slices/B2.md)
- [ ] **B3. Every skill names its sound, checked at load and startup, played as its action starts** (Core + Data + Game + content, about 190 lines) → [slices/B3.md](slices/B3.md)
- [ ] **B4. The battle-start sound at the flash; victory's fanfare over silence, then the map's track; defeat and escape** (Game, about 230 lines) → [slices/B4.md](slices/B4.md)
- [ ] **B5. Sounds on the battle's commands, skill and item lists and target cursor** (Game, about 140 lines) → [slices/B5.md](slices/B5.md)

### Phase 5: the look
- [ ] **V2. The volume rows as diamond pips** (Game, raw 70, about 110 lines at 1.6x) → [slices/V2.md](slices/V2.md)

## Decisions
- **415. Settings are saved between runs, in a separate small plan after this one** (volumes, full screen, text speed); until then the volumes work for the run, as the other options.
- **416. At a battle the music cuts to the battle theme at the encounter's flash; afterwards the map's track comes back with a short fade-in. No crossfade** anywhere: every other change is a cut.
- **417. The map's track resumes where it paused** when it comes back after a battle.
- **418. On a victory a fanfare plays over silence, then the map's track returns** during the rewards (once the fanfare's time has passed).
- **419. A map without a track is silent.**
- **420. Each encounter may name its own theme** (`encounters.json` `musicFile`, else the battle theme); the Goblin Chief's encounter carries the boss theme (enemy plan decision 414).
- **421. Interface sounds are move, confirm and cancel**, plus a battle-start sound at the encounter's flash; no refused buzzer and no prompt sounds.
- **422. Typing ticks every 2 characters typed, at least 60 ms apart, muted while a voice plays.**
- **423. Every skill and every battle event kind has a sound**: each skill in `skills.json` names its sound file (validated at load, checked at startup by exact name, placeholders meanwhile), played as the skill's action starts; every battle event kind maps to a sound the code names (hit, critical, miss, block, resisted, status applied, heal, resource gain, Poison and Regen ticks, a fall, a backfire, a stun, a status ending, a move, an extra turn, an item, Defend), and the outcome screens sound victory, defeat and escape.
- **424. Volumes show as diamond pips, 0 to 10.**
- **425. The sound files the code plays are constants** (`Ui/UiSoundFiles`: the title and battle themes, the interface and battle sounds), checked by one existence test as `Ui/UiArtFiles` is (decision 84); content names only map tracks, encounter themes and skill sounds, each checked at startup.

Planner's picks (not numbered; design.md records them): volumes as a squared taper (`(level / 10)²`) with defaults Music 7, Effects 8, Voices 10; a track shared by two maps plays on across a warp; a sound file asked twice in one update plays once; a move sound only when the selection changes; the last track left waits paused (one only) and is resumed if wanted again; a skill's sound and its event sounds both play; any combatant's fall sounds; a resource cost is silent (the skill's sound covers it), a gain sounds; prompts are silent, including the action a prompt's Yes carries out.

## Open design questions (developer decides)
None open. Noted for the developer's review (interpretations, not forks): "every effect has a sound" is read as every battle event kind, so items and the basic attack sound through their events (`itemUse`, hit) rather than a file of their own; typing stays muted for the rest of an event once its voice step has started (the channel can't tell when the line ends).

## ADRs
- **ADR 0035: music and sound effects, extending ADR 0018** (written before P1 from this entry; no options left to ask):
  - **Music is chosen by the flow from its state** (`Flow/MusicSelection`: no session, a pending encounter, the battle screen's music cue, the current map) and written into a `MusicChannel` after every update; no scene writes it. A future `playMusic` event step would set Core state (a track override in `WorldState`, so the save keeps it) that the selection reads; never a scene writing the channel.
  - **Effects are reported by scenes as values** (`IScene.Sounds`, `IMenuPage.Sounds`: the files the last update asks for). `GameFlow.Update` clears the effect channel first, and copies the top scene's right after `_scenes.Update`, before it applies a request. Why not injected like the voice: the voice is one long-lived sound a single presenter starts and stops, which a channel injected into `EventPresenter` suits; effects come from almost every presenter, most already at four constructor parameters, and as values they are asserted on the scene the way its requests are.
  - **Playback stays WAV through `SoundEffect`**: music loops on a `SoundEffectInstance` (`IsLooped`), paused and resumed; volumes are set per instance, never through the global `SoundEffect.MasterVolume`. If real tracks make WAV too big, the way out is streaming through `DynamicSoundEffectInstance`, not `Song`/`MediaPlayer` (one global song: it loses the resume where paused and two tracks at once).
  - **A track is released when it leaves the mix** (its instance and its loaded sound); the title and battle themes stay loaded, preloaded in `JrpgGame.StartAudio` so the flash doesn't stall on a load (as `LoadContent` preloads the menu's background).
