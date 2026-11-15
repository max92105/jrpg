# Cinematics in the editor
Status: done

Drafted on 2026-11-14 from the developer's request (cleaned up by the caller):

> Set up cinematics in the editor: arrange the stills of a cinematic in order, set how long each still is visible,
> put audio fitting each still (a voice line per still, probably music for the whole cinematic), set a small zoom per
> still (a slow Ken Burns zoom in or out), and preview it in the editor if reasonable. The stills of Ragnir's intro are
> in `content/art/cinematics/ragnir_intro/` (`1_happy_life.png` to `6_i_will_get_him_back.png`).

Approved on 2026-11-14 with the developer's answers: the full version (a record played by one step, ADR 0046); skipping
by holding Confirm for 2 s with a filling ring (decisions 623-626); a picture-only preview, each still's voice length
shown beside its seconds instead of audio in the editor (decision 620); only the `still` step retired, `voice` kept
(decisions 621, 622); every still drawn with linear sampling (decision 627); stills loaded one per frame before the
lead-in counts (decision 628); the timeline moved into CG1 with the fade.

A second request came with this one (animated monsters in battle from GIFs). It is its own plan,
[`2026-11-14-animated-monsters`](../2026-11-14-animated-monsters/README.md), which runs after this one and depends on CA0.

Builds on ADR 0016 (intros are map cutscenes run by `EventRun`), ADR 0006 (event steps), ADR 0009 (art folders), ADR
0018 and 0035 (voices, music chosen by the flow), ADR 0011 (content the only source of truth), ADR 0012 and 0014
(editor, undo), ADR 0045 (Database windows), ADR 0038 (art sources).

- Numbers: decisions 609-630, ADR 0046.
- Core: a new leaf folder `Cinematics/` (the record, its stills, the zoom, the fade, the timeline) and one event step,
  `CinematicStep`. Data: `cinematics.json`, its validator, the startup checks, the writer, a WAV's length. No save format
  change: saves never name a cinematic, and an event in progress is never saved.

## Goal
1. **A cinematic is a record the editor edits.** Database > **Cinematics...**: each cinematic's id, an optional music
   track for the whole of it, and its stills in order; each still's picture, its seconds (its voice line's length shown
   beside them), an optional subtitle, an optional voice line and an optional slow zoom.
2. **The game plays it from one event step:** black before, each still faded in and out for its seconds with its
   subtitle, its voice started with it, the track under the whole cinematic, the zoom drifting over its time; holding
   Confirm for 2 s skips it, a ring filling around the key's name; then the event goes on (Ragnir's intro: the warp to
   the milestone).
3. **The editor previews it**: the chosen still's picture as soon as the window opens; **Play** from the chosen still
   shows the stills with their fades, zoom and subtitle on the preview's own clock, silent (decision 620).
4. **One way to show stills**: the eight placeholder intros become cinematics and the `still` step leaves; the `voice`
   step stays (decision 621).

## Order, and why
1. **CA0 first, content only:** the working tree fails `ArtSourcesTests` today (six unlisted stills, six GIFs loose in
   `content/art/`); every slice must end green, and the monsters plan needs the GIFs in their source folder.
2. **CM1-CM3, the model:** the record loads and is checked, then the event step that names it, with the event dialog's
   support in the same slice (an editor that can't open a step throws on it).
3. **CE0-CE7, the editor tool**, before the game plays anything: the developer asked to set up Ragnir's intro first. CE6
   is the first hand check of the whole tool; CE7 shows each voice's length beside its still's seconds.
4. **CG0-CG6, the game:** two moves first (`EventPresenter`'s cutscene part out of its 294 lines; the fade and the
   timeline into Core, which the preview shares), then the stills playing, then voices and music with Ragnir's intro
   switched to the cinematic, then the hold to skip (behavior, then the ring: game screens split behavior and look),
   then the zoom and the smooth drawing.
5. **CP1, the preview's Play**, after the game, since it reuses the Core timeline, fade and zoom.
6. **CR1-CR3, one way to show stills**, last: content first (the placeholder intros), then the test world, then the
   `still` step removed end to end.

## Guiding rules
- **One rule source for time and look:** which still shows at a time, its opacity, its zoom scale and whose voice
  speaks are Core (`Cinematics/`), read by the game and the editor's preview alike (the editor never references Game).
  The black around a cinematic, the subtitle band, the hold to skip and its ring stay Game.
- **One rule source for content** (ADR 0045): the window's problem line is its own checks (a typed id's shape, a box
  that isn't a number) and then `CatalogProblems.OfCinematics`; no editor code copies `CinematicCatalogValidator`.
- **The editor never locks out on missing art or sound** (decision 442): a still or voice the folders lack is marked
  and saved; the game's startup refuses it.
- **Determinism:** every time is the update's delta (the game) or the render tick's (the preview); Core never reads a
  clock; no randomness anywhere in this plan.
- **No allocation per frame:** the timeline answers with a struct; subtitles and the hint are measured when they
  change; the stills' textures are loaded one per frame before the lead-in counts (decision 628).
- **Undo covers every edit** (ADR 0014): the window's OK is one `CatalogDraft` step; an undo that would remove a
  cinematic a map's event names is refused (decision 630).
- **Sizes:** Editor slices about 200 lines with tests at about 10 lines each; a Game slice that adds a view type is
  estimated at 1.6x (CG5's ring).
- **Files near 300** (checked 2026-11-14): `Game/Exploration/EventPresenter.cs` 294 (CG0 moves the cutscene part out
  first); `Data/Content/ContentWriter.cs` 294 (CE0 moves the catalog list into `ContentWriter.Catalogs.cs` first, as the
  backlog line asks); `Editor/Events/EventStepViewModel.cs` 272 (about 284 in CM3; CR3 takes the still's fields out);
  `Core/Exploration/ExplorationState.cs` 271 (+3 in CM3); `Data/Art/ArtReferenceCheck.cs` 261 (+6 in CM2; the
  monsters plan's MB0 splits it before its own growth); `Editor/Shell/MainViewModel.cs` 282 and `Shell/MainWindow.xaml`
  291 (+1 each in CE6).
- **Test files past 250 get siblings, named here:** `EventRunTests` (269) → `EventRunCinematicTests`;
  `EventStepViewModelTests` (266) → `EventStepViewModelCinematicTests`; `ContentCatalogLoaderTests` (272) →
  `ContentCatalogLoaderCinematicTests`; `ContentWriterTests` (254) → `ContentWriterCinematicTests`;
  `ArtReferenceCheckTests` (239) → `ArtReferenceCheckCinematicTests`. Globbed on 2026-11-14: no file named
  `*Cinematic*`, `*Cutscene*`, `*SkipHold*`, `*Ring*`, `*WavDuration*` in `src/` or `tests/`.

## Out of scope (follow-ups, added to `docs/plans/backlog.md` by CA0)
- A pan toward a point (the other half of Ken Burns, decision 618).
- Unloading a cinematic's stills once it ends (the existing `TextureLibrary` backlog line, decision 619).
- Audio in the editor's preview (decision 620).
- Crossfades between stills: each still fades from and to black, as today (decision 159).
- Video files (ADR 0016 option C); cinematics started other than by an event step.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when a
slice is done and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 0: green again
- [x] **CA0. One content commit: the Ragnir stills listed, the French-named copies deleted, the GIFs moved to
  `art-src/gifs/`** (content, about 40 lines) → [slices/CA0.md](slices/CA0.md)

### Phase 1: the model
- [x] ADR 0046, cinematics as content records. Write it from the entry under "ADRs" below (decision 609).
- [x] **CM1. `cinematics.json` loads into `Cinematics/` records, checked by `CinematicCatalogValidator`** (Core + Data,
  about 250 lines) → [slices/CM1.md](slices/CM1.md)
- [x] **CM2. The startup checks read the cinematics: every still's picture, voice and track by exact name** (Data, about
  150 lines) → [slices/CM2.md](slices/CM2.md)
- [x] **CM3. A `cinematic` event step: Core waits on it, Data checks its id, the event dialog edits it** (Core + Data +
  Editor, about 250 lines) → [slices/CM3.md](slices/CM3.md)

### Phase 2: the editor tool
- [x] **CE0. `ContentWriter`'s catalog list into `ContentWriter.Catalogs.cs`** (Data, about 60 lines moved) →
  [slices/CE0.md](slices/CE0.md)
- [x] **CE1. Cinematics save from the editor: equal by content, written sorted, `CatalogProblems.OfCinematics`,
  `CatalogDraft.SetCinematics`** (Core + Data + Editor, about 200 lines) → [slices/CE1.md](slices/CE1.md)
- [x] **CE2. An undo that would remove a cinematic a map's event plays is refused** (Editor, about 150 lines) →
  [slices/CE2.md](slices/CE2.md)
- [x] **CE3. One still as typed: picture, seconds, subtitle, voice, zoom boxes, its problem** (Editor, about 190 lines)
  → [slices/CE3.md](slices/CE3.md)
- [x] **CE4. One cinematic as edited: its id, its track, its stills added, removed and moved** (Editor, about 200
  lines) → [slices/CE4.md](slices/CE4.md)
- [x] **CE5. The Cinematics window's rules: the list, Add, Remove while new, the pictures, voices and tracks offered,
  the problem line, one undo step** (Editor + Data, about 220 lines) → [slices/CE5.md](slices/CE5.md)
- [x] **CE6. The Cinematics window and Database > Cinematics..., the chosen still's picture shown** (Editor, about 200
  lines) → [slices/CE6.md](slices/CE6.md)
- [x] **CE7. Each still's voice length beside its Seconds box** (Data + Editor, about 150 lines) →
  [slices/CE7.md](slices/CE7.md)

### Phase 3: in the game
- [x] **CG0. `EventPresenter`'s cutscene part into `CutscenePresenter`** (Game, about 110 lines moved) →
  [slices/CG0.md](slices/CG0.md)
- [x] **CG1. The fade and the timeline in Core: `StillFade`, `CinematicDefinition.Duration` and `MomentAt`** (Core +
  Game, about 160 lines) → [slices/CG1.md](slices/CG1.md)
- [x] **CG2. The game plays a cinematic step: its stills loaded one per frame, then in order with their fades and
  subtitles, black around** (Game, about 250 lines) → [slices/CG2.md](slices/CG2.md)
- [x] **CG3. A cinematic's voices and track, a voice already playing cut by them; Ragnir's intro plays its cinematic**
  (Core + Game + content, about 260 lines) → [slices/CG3.md](slices/CG3.md)
- [x] **CG4. Hold Confirm 2 s to skip a cinematic: `SkipHold`, the "Hold E to skip" hint** (Game, about 220 lines) →
  [slices/CG4.md](slices/CG4.md)
- [x] **CG5. The ring filling around the key's name** (Game, about 150 lines raw, about 240 at the 1.6x) →
  [slices/CG5.md](slices/CG5.md)
- [x] **CG6. The slow zoom, and every still drawn smoothly in its own batch** (Core + Game, about 140 lines) →
  [slices/CG6.md](slices/CG6.md)

### Phase 4: the preview
- [x] **CP1. Play in the Cinematics window: the stills from the chosen one, faded, zoomed and subtitled on the
  preview's clock, silent** (Editor, about 230 lines) → [slices/CP1.md](slices/CP1.md)

### Phase 5: one way to show stills
- [x] **CR1. The eight placeholder intros become cinematics, their voice step kept** (content + tests, about 240 lines,
  mostly content) → [slices/CR1.md](slices/CR1.md)
- [x] **CR2. The test world's intros play cinematics from its own frozen `cinematics.json`** (tests, about 150 lines) →
  [slices/CR2.md](slices/CR2.md)
- [x] **CR3. The `still` step leaves, end to end** (Core + Data + Game + Editor, about 60 lines added, about 400
  deleted) → [slices/CR3.md](slices/CR3.md)

## Decisions
- 609: a cinematic is a content record in `content/cinematics.json` (id, an optional track, its stills in order: each
  a picture, seconds, an optional subtitle, voice line and zoom), played by one event step `{ "type": "cinematic",
  "cinematicId": "..." }`; the event keeps the scripting around it (the leader gate, the warp after). Chosen over
  today's steps with zoom and music added and a better step editor, for a window independent of any map and a
  reusable, previewable record (ADR 0046).
- 610: a cinematic's stills sit in a folder of its own under `content/art/cinematics/`, and content names a picture
  with that folder (`ragnir_intro/1_happy_life.png`), as music names `town/loop.wav` (decision 496); amends ADR 0009's
  "file name only" for stills.
- 611: the six Ragnir stills and the six GIFs are listed under the existing `generated` source in
  `content/art/art-sources.json` (placeholder art made with an image generator). **Assumed** by the plan; the developer
  corrects the source if they are the developer's own final art.
- 612: CA0 is one content commit (the "art" commit is pushed and can't be amended): the stills listed, the six
  French-named copies at the root of `content/art/` deleted, and the six GIFs moved to `art-src/gifs/` at the
  repository's root, renamed per decision 633. Why there: the GIFs are generated art that can't be fetched again (unlike
  the bought packs kept outside the repository, decision 439), so they stay versioned; outside `content/`, nothing reads
  them (not the game, not the test outputs' `content/**` copy, not `ArtSourcesTests`); they stay after the monsters
  plan's import as the source a re-import at another height reads.
- 613: a still's voice line starts when its still starts (after the lead-in for the first), plays on over later stills
  that have none, is cut by the next still that has one, and stops when the cinematic ends or is skipped; the rule is
  Core's `CinematicDefinition.VoicedStillAt`, read by the game. The voice and the stills run on two clocks (ADR 0016
  accepts the drift).
- 614: a cinematic has at most one track, any of `content/audio/music/`, asked by the flow from the step's first update
  to its last still (the map's track fades out over one second first, decision 416); the map's track comes back after;
  no track keeps the map's.
- 615: a still may have a subtitle and a voice line together (subtitles, decision 172).
- 616: a still added in the editor lasts 4 s and has no zoom.
- 617: a still's zoom is a start and an end percent, each 100 to 150 (below 100 would show black edges around a covering
  picture), linear over the still's whole time (fades included), about the picture's center; none when left out (both
  100 is written as none).
- 618: a pan toward a point is a backlog line.
- 619: unloading a cinematic's stills after it plays stays a backlog line; six stills stay in video memory after the
  intro.
- 620: the editor's preview is picture only (Play, CP1): no audio in the editor. Instead each still shows its voice
  line's length beside its Seconds box (CE7: Data reads a WAV's length from its header), so stills can be timed to the
  voice.
- 621: the `still` step is retired once every intro is a cinematic (CR1-CR3); the `voice` step stays, the voice-over of
  any story scene. The eight placeholder intros become eight placeholder cinematics, each event keeping its `voice`
  step before the `cinematic` step.
- 622: a voice already playing when a cinematic starts (an event's `voice` step before it) is cut by the cinematic's
  track at the step's first update, or else by the first of its stills that has a voice; with neither, it plays on over
  the cinematic. Once cut, it doesn't come back in that event (only another `voice` step starts one).
- 623: a cinematic is skipped by holding Confirm for 2 s: while held, a ring fills around the key's name in the hint
  ("Hold E to skip"); letting go before 2 s empties the ring and the cinematic carries on; a single press does nothing;
  Cancel does nothing. A hold counts only from a press made while the cinematic step is current, so the press that
  started the event can't skip it by being held. The progress is `Game/Exploration/SkipHold`, pure and tested: it grows
  by the update's delta while held, returns to 0 on release, is done at 2 s.
- 624: the 2 s is a named Game constant, `SkipHold.HoldTime`, not content: `rules.json` holds game rules only and the
  game has no content place for UI timings; every presentation timing is a named Game constant today (`BattlePacing`,
  the cutscene's lead-in and lead-out, the still fade).
- 625: the hold applies to the cinematic step only; the `still` step keeps today's press of Confirm or Cancel until CR3
  deletes it.
- 626: the ring is drawn as 48 baked segments: one small segment texture made once by `UiTextures` (`Ui/RingSegmentShape`),
  drawn rotated about the ring's center by `Ui/ProgressRingView`, lit up to the progress (the segment at the edge lit by
  its share), dim after; no shader and no arc primitive (MonoGame has none), no allocation per frame.
- 627: every still is drawn with linear sampling, zoomed or not, in its own `SpriteBatch.Begin` (the stills are
  paintings; the map's point sampling would shimmer under a zoom); the subtitle, hint and ring draw after it as today.
- 628: a cinematic's stills are loaded one per frame, in order, while the screen is black and before the lead-in starts
  counting (`StillPreload`, driven by the draw); a 2.7 MB PNG decoded mid-fade would stall a frame.
- 629: which still shows at a time (`CinematicDefinition.MomentAt`, `Duration`), its opacity (`StillFade.OpacityAt`, the
  0.8 s moved from the game) and its zoom (`CinematicStill.ScaleAt`) are Core, read by the game and the editor's
  preview.
- 630: an undo that would remove a cinematic a map's event plays is refused, as for shops (decision 581): maps keep
  their own histories, so an undo on another map isn't protected by order alone.

## Open design questions (developer decides)
None left: every question was answered at approval.

## ADRs
- [x] **ADR 0046: cinematics as content records** (decision 609; amends ADR 0016's decision B for the presentation
  part, and ADR 0009's "file name only" for stills, decision 610). Write it from this entry; the developer chose A.
  - **A. A record played by one event step (chosen):** `cinematics.json`, edited in a Database window (ADR 0045); the
    event keeps the scripting. Pros: a window independent of any map; a record that can be reused and previewed; the
    voice and zoom sit on the still they belong to; the event stays two steps. Cons: a new content file with its
    loader, validator, writer, undo refusal and startup checks (about 21 slices with the game, the preview and the
    retirement); two ways to show stills until CR3.
  - **B. Today's steps, improved (about 6 slices):** the `still` step gains a zoom (start and end percent) and an
    optional voice, a new `music` step sets the track for the rest of the event, and the event dialog's step editor
    gets a picture box, Up and Down, and a still's preview. Pros: no new file or catalog plumbing; everything stays in
    the intro map's event; a third of the work. Cons: a cinematic is edited inside one map's cell event (found by
    opening the intro map in Events mode); it can't be reused or previewed as a whole outside its event; the event
    dialog's switches, at their complexity limit, grow with each field.
  - **C. A record played by its own scene, outside events** (ADR 0016's option A). Pros: the smallest player. Cons:
    loses the event's gate and warp; a second way to script scenes, rejected once already.
