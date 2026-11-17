# Encounters and battle backgrounds (editor push, roadmap row 10)
Status: approved, in progress

Approved on 2026-11-15 by delegation (the developer delegated approval of the editor push's sub-plans), with the devil's
advocate's review applied: the plan builds on row 7's shared preview (`Animation/FramePreview`, AN1) and GIF dialog
(`GifImport/ComposedGif`, `GifImportWindow.AskFor`, AN2), which always land first; the import's heights start from the
GIF's own (decision 898); a background is always averaged when shrunk (decision 904); the placeholder is four 960 x 540
frames in a hand-laid 2 x 2 grid (decision 910); the previous map's backgrounds are released on entering another
(decision 900); 14 slices (the review counted 13; its merges give 14: BG1-BG9 and EC1-EC5); Make still dropped; the
Database menu is `Shell/DatabaseMenu.xaml` by then; the new editors are pages from birth (the mode bar plan's stage 1
lands before row 7); decision 892 kept.

Planned on 2026-11-15 from roadmap row 10 of [the editor push](../2026-11-15-editor-push/README.md) and its design.md
section 3.3, and the developer's words: "one thing that is missing for map is background setup. I also will have
animated background, gif probably, that I want to be able to import", and "when I import gif or work with animation I
always want a preview and be able to tweak them: frame rate etc."; he confirmed "background" means the battle
background a map's battles use (decision 700). The developer was away while this was planned: the decisions marked
"(default taken while the developer was away; his to overturn)" are listed under [Defaults to confirm](#defaults-to-confirm).

- Numbers: decisions 891-910 (911-915 unused). ADR 0059.
- Projects: Core (a map setting, an optional encounter field, the background rule, the animated background record), Data
  (validators, the startup art check, the writer taking over `encounters.json` and the new `animated-backgrounds.json`),
  Game (the battle plays an animated background; backgrounds decoded and released on map entry), Editor (Map settings'
  box, two Database pages, the GIF dialog's background kind), content (encounters and two maps move their background,
  one placeholder animated background). No save change.

## Goal
1. **A map names its battle background** (decision 700): Map > Settings... has a Battle background box; a map with
   random battles must name one. An encounter may still name its own, which wins (a boss's arena).
2. **Animated battle backgrounds**: a GIF imported in the editor becomes a grid sheet PNG and a record (frame size,
   frame count, frame time); the game, still PNG-only (decision 680), plays it under the battlers on the update's delta.
   The GIF dialog plays the GIF at the chosen height and typed frame time before it writes anything; the Battle
   backgrounds page plays every background and keeps an animated one's frame time editable afterwards.
3. **An Encounters page** (Database mode): each encounter's enemies in their screen order, its own background or the
   map's, its music; the editor writes `encounters.json`.

## Order and why
**Needs row 7's AN1 and AN2** (`docs/plans/2026-11-15-skill-and-status-editors/`: the shared `FramePreview` with its
`FramePlayback` and `PreviewFrames`, and the GIF dialog playing a `ComposedGif` at an editable pace) and AN6's
`PreviewFrames.OfCells`; row 7 lands before this row. The mode bar plan's stage 1 (`Workspaces/`, the page recipe,
`Shell/DatabaseMenu.xaml`) lands before row 7, so the two new editors are pages from birth.

1. **Phase 1, a map's battle background** (BG1, BG2): the smallest change with the most use, and the one the developer
   named first ("background setup" for a map). BG1 moves the rule and the content in one go so every battle still
   looks as it does; BG2 is the Map settings box (Map settings stays a modal window).
2. **ADR 0059 before phase 2**: the sheet's shape had real alternatives (design.md, "Animated backgrounds").
3. **Phase 2, animated backgrounds** (BG3-BG9): the record and its checks (BG3, BG4) before the game plays it (BG5),
   so content naming one is refused or accepted before anything draws it; BG4 makes a placeholder sheet with a script,
   so no slice waits on art a hand check would produce. Then the editor: the sheet conversion (BG6), the Battle
   backgrounds page's rules (BG7) and the page (BG8), then AN2's GIF dialog gains a background kind (BG9), which hands
   its record to the page's session.
4. **Phase 3, the Encounters page** (EC1-EC5): last, as the roadmap ranks it after enemy behavior (row 8): the writer
   takes over the file (EC1), one encounter's rules (EC2), the list and its undo step (EC3), the undo refusal (EC4),
   the page (EC5). It needs nothing from phase 2: its background box lists the folder's files.

The roadmap guessed "about 6 slices"; this plan has 14, because the rules and the page of each editor are separate
slices and Editor slices aim at about 200 lines, not because the scope grew.

## Guiding rules
- **The game stays PNG-only** (decision 680, ADR 0047): a GIF is converted in the editor; the game reads a PNG and a
  record.
- **Facts about one image are per-image content** (ADR 0048's precedent): an animated background's frame size, count
  and time live in its `animated-backgrounds.json` record, by file name; maps and encounters name only the file.
- **One rule per question, in Core**: which background a battle uses (`Exploration/EncounterBackground`), which cell a
  frame reads (`Battle/AnimatedBackgroundDefinition.FrameOrigin`); the game, the editor's previews and Data's checks
  all call them.
- **No battle look changes in phase 1**: BG1 moves `Forest.png` from the four encounters to the two maps that roll
  them; the same picture shows in every battle.
- **One preview everywhere** (the developer's "always a preview"): every preview here is row 7's `FramePreview`, its
  frame time box the record's; no slice builds its own clock or render tick for a preview.
- **Pages from birth**: the Battle backgrounds and Encounters editors follow the mode bar plan's page recipe (its
  WS12 file and `Workspaces/CatalogDialog`, as that plan states it when the slice starts): the catalog view model
  implements `Workspaces/IEditedCatalog`, the editing view model `Workspaces/ICatalogEditing<T>`, one page
  registration, a `<Catalog>Page` user control with no OK, Cancel or problem line (the bar shows them), one entry in
  `Shell/DatabaseMenu.xaml`, one smoke-script entry. Map > Settings... stays a modal window.
- **Files near 300** (counted 2026-11-15): `Shell/MainWindow.xaml` is untouched (the Database entries live in
  `Shell/DatabaseMenu.xaml` by then); `Shell/MainViewModel.cs` 286 (untouched: the `.Database` and `.Workspaces`
  partials take the new members); `Core/Exploration/ExplorationState.cs` 276 (+4 in BG1); `Data/Art/ArtReferenceCheck.cs`
  231 (BG1 moves the background lines into a new partial); `Shell/EditorTemplates.xaml` 297 (untouched).
- **Test files** are named per slice below, each globbed on 2026-11-15; a file past 250 lines gets a sibling.

## Out of scope
- A battle started by an event step (none exists): the map setting is where it would read its background (design.md,
  "Extension points").
- Per-frame times in a GIF (backlog, decision 632): one frame time per background.
- Parallax layers, a background that reacts to the battle, an animated title or menu background.
- An enemy count cap or a formation preview in the Encounters page: the loader's rule (at least one enemy) stands.
- Duplicate in the Encounters page, a "fought on these maps" line, a Make still button: not asked; backlog if wanted.

## Slices

### Phase 1: a map's battle background
- [x] **BG1. A map names its battle background; an encounter's own wins** (Core, Data, Game, Editor, content, about
  260 lines) → [slices/BG1.md](slices/BG1.md)
- [x] **BG2. Map settings' Battle background box** (Editor, about 150 lines) → [slices/BG2.md](slices/BG2.md)

### Phase 2: animated backgrounds
- [x] **ADR 0059, animated battle backgrounds as grid sheets.** Write it from the entry under "ADRs needed" below
  (decisions 896-900, 904; a default taken while the developer was away, so it is marked as such in its Status line).
- [x] **BG3. The animated background record, loaded, validated and written** (Core, Data, about 230 lines) →
  [slices/BG3.md](slices/BG3.md)
- [x] **BG4. The startup art check on backgrounds, and a placeholder animated background** (Data, content, script,
  about 200 lines) → [slices/BG4.md](slices/BG4.md)
- [x] **BG5. The battle plays an animated background; a map's backgrounds decoded on entry, the last map's released**
  (Core, Game, about 250 lines, the 1.6x included) → [slices/BG5.md](slices/BG5.md)
- [ ] **BG6. A GIF into a background sheet, headless** (Editor, about 190 lines) → [slices/BG6.md](slices/BG6.md)
- [ ] **BG7. The Battle backgrounds page's rules: every background, still or animated, and its problem line** (Editor,
  about 320 lines, its split point named) → [slices/BG7.md](slices/BG7.md)
- [ ] **BG8. The Battle backgrounds page: the shared preview, cover-fit, PNG import, Apply as one undo step** (Editor,
  about 200 lines) → [slices/BG8.md](slices/BG8.md)
- [ ] **BG9. AN2's GIF dialog gains a background kind: the GIF plays at the chosen height before it is written**
  (Editor, about 250 lines) → [slices/BG9.md](slices/BG9.md)

### Phase 3: the Encounters page
- [ ] **EC1. The editor writes `encounters.json`** (Data, about 150 lines) → [slices/EC1.md](slices/EC1.md)
- [ ] **EC2. One encounter as edited** (Editor, about 210 lines) → [slices/EC2.md](slices/EC2.md)
- [ ] **EC3. Every encounter, and Apply as one undo step** (Editor, about 250 lines) → [slices/EC3.md](slices/EC3.md)
- [ ] **EC4. An undo can't remove an encounter a map's battles list** (Editor, about 130 lines) →
  [slices/EC4.md](slices/EC4.md)
- [ ] **EC5. The Encounters page** (Editor, about 170 lines) → [slices/EC5.md](slices/EC5.md)

## Decisions
- **891.** The plan is three phases and 14 slices in the order above: a map's background, animated backgrounds, the
  Encounters page. It needs row 7's AN1, AN2 and AN6 and the mode bar plan's stage 1.
- **892.** A battle background is named **both per map and per encounter**: `MapSettings.BattleBackgroundFile`
  (`battleBackgroundFile`, a file in `content/art/backgrounds/`, exact case) is required while the map has random
  battles and optional otherwise; `EncounterDefinition.BackgroundFile` becomes optional, and an encounter's own wins
  over the map's. Why both: decision 700 makes the background the map's, and one encounter is rolled on several maps
  (`goblin_trio` on two today), so a per-encounter-only background would force one copy of each encounter per place;
  a per-map-only one would leave a boss no arena of its own. Why the encounter's wins: naming one is deliberate (a
  boss), the map's is the default. Why required on the map and not "map or every encounter": the check stays inside
  one map file, so an edit in the Encounters page never invalidates a map it doesn't write. (default taken while the
  developer was away; his to overturn)
- **893.** Core says which background a battle uses: `Exploration/EncounterBackground.On(encounter, map settings)`,
  read through `ExplorationState.PendingBattleBackground`; the battle screen reads only that.
- **894.** The repository moves without a visible change: the four encounters drop `"backgroundFile": "Forest.png"`,
  the two maps with random battles (`milestone_outside`, `primeval_growth_crossroad`) and the test world's two
  (`forest`, `to_aslas_1`) name `Forest.png`. `encounters.json` is rewritten in the writer's layout at the same time.
- **895.** The startup art check covers every map's battle background and every encounter's own by exact name, and
  (BG4) every background a map, an encounter or a record names is at most `TextureLimit.MaxSide` (4096 px) a side; the
  editor's import refuses a larger background too. The editor never refuses missing art (decision 442).
- **896.** An animated battle background is **one grid sheet PNG** at a reduced frame size, frames left to right then
  top to bottom, scaled up by the game; not a folder of full-size frames, not a strip (ADR 0059). Its ceiling: a
  24-frame loop is at most 960 x 540 a frame (shown at 2x), and full-size frames fit 6 to a sheet. (default taken while
  the developer was away; his to overturn)
- **897.** Its record, in a new `content/animated-backgrounds.json`, by file name: `file`, `frameWidth`, `frameHeight`
  and `animation` (`frameCount` from 2, `frameMilliseconds` from 1: ADR 0039's shape, as an enemy's `idleAnimation`).
  A background without a record is a still picture, drawn as today. The columns are the image's width over the frame
  width, so a hand-laid grid of any column count works; `AnimatedBackgroundDefinition.FrameOrigin` is the one rule for
  where a frame sits.
- **898.** The import's frame heights: the GIF's own height first (capped at 1080), then each of 1080, 540, 360, 270,
  216 and 180 below it (the 1080 px screen's whole divisors, so a 16:9 frame is scaled up by a whole factor and point
  sampling stays even). The default is the GIF's own height when its sheet holds every frame, else the largest divisor
  below it that does; when none does, the problem line says so. Nothing is ever scaled up at import. (default taken
  while the developer was away; his to overturn)
- **899.** The battle draws the background's frame for the time since the battle screen opened, from the update's
  delta (never a clock), from frame 0 each battle, looping, through the outcome screens; cover-fit over the screen,
  point-sampled, under the vignette and the battlers as today.
- **900.** On entering a map the game decodes the backgrounds its battles can show (its own and its encounters' own),
  under the warp's black, not on the battle's first frame, and releases the previous map's that the new map doesn't
  list (`TextureLibrary.Release`), never the title's or the menu's background. The GIF dialog shows each sheet's video
  memory before it is written.
- **901.** The Battle backgrounds page lists every image in the backgrounds folder in name order, then each record
  whose image is missing; the chosen one plays in row 7's `FramePreview`, cover-fit in a 16:9 area, its frame time box
  the record's. Only an animated background's frame time is typed there; its frame size and count come from its import
  (re-import to change them). Apply is one `CatalogDraft.SetAnimatedBackgrounds` undo step; nothing names a record by
  id, so no undo is refused.
- **902.** A record whose grid doesn't fit its image is the page's problem line and stops Apply; a PNG imported over an
  animated background's file makes it still in the page's session; a GIF imported over a still one adds its record.
- **903.** The GIF dialog (AN2's) has a background kind, opened from the Battle backgrounds page: the height box offers
  decision 898's heights, the clearing box is hidden, a line names the frames, the sheet's size and grid and its video
  memory, and its problem line refuses a frame time that isn't a whole number from 1 or a sheet that can't hold every
  frame within 4096 px. OK writes the sheet into `backgrounds/` at once (a file copy, not undone, as every import) and
  hands its record to the page's session. One background GIF per import; replacing a file is asked first (decision 125).
- **904.** A background GIF is never cleared of a background color (backgrounds are opaque), and is always averaged
  when shrunk (decision 637's averaging, never nearest neighbor: a halved painting keeps its detail as color, not
  dropped pixels); it is never enlarged.
- **905.** The Encounters page edits each encounter: its id typed while new (`Objects/ObjectIds`' rule), its enemies in
  slot order (an enemy's place on screen is its position in the list, `EncounterDefinition`) added after the chosen
  slot from the enemies by name, moved and removed, the same enemy any number of times; Add, and Remove for an
  encounter added since the page last applied (decision 581's rule, as the mode bar plan's decision 918 keeps it).
  Apply is one `CatalogDraft.SetEncounters` undo step.
- **906.** The Background box offers "(the map's)" first, then the backgrounds folder's images; the Music box offers
  "(battle theme)" first (decision 420), then the music folder's tracks. A file no longer in its folder stays listed,
  so opening the page never changes an encounter.
- **907.** An undo that would remove an added encounter while a map's random battles list it is refused, as for shops;
  the shop, cinematic and encounter refusals share one helper in `Shell/CombinedHistory`.
- **908.** The editor writes `encounters.json` (EC1): its optional `backgroundFile` and `musicFile` are left out while
  null.
- **909.** A map's battle background box in Map settings offers "(none)" first, then the backgrounds folder's images; a
  file no longer in the folder stays listed (as the track, decision 419). "(none)" with random battles on is the
  dialog's problem line.
- **910.** The placeholder animated background `ForestPulse.png` is four 960 x 540 frames of `Forest.png` in a
  hand-laid 2 x 2 grid (1920 x 1080: not the writer's one row, so the game's column rule is exercised), a slow
  brightness pulse at 200 ms a frame, made by a script in `art-src/backgrounds/` and listed under `generated`; no map
  names it. (default taken while the developer was away; his to overturn)

## Defaults to confirm
- **892**: backgrounds both per map (required with random battles) and per encounter, the encounter's own winning. The
  alternatives: per map only (a boss's arena becomes a map of its own), or per encounter only (today's model, one
  encounter copy per place).
- **896**: animated backgrounds as one grid sheet at a reduced frame size (ADR 0059), a 24-frame loop at most 960 x 540
  a frame. The alternative with its cost: a folder of full-size frame PNGs, one texture each (about 8.3 MB of video
  memory and 25 ms of decoding per 1920 x 1080 frame: 24 frames are 199 MB and 0.6 s of decoding), about one more
  slice.
- **898**: the heights offered (the GIF's own, then the divisors of 1080 below it) and the default pick.
- **910**: the placeholder animated background and its look.

## Open design questions (developer decides)
None beyond the defaults above.

## ADRs needed
- **ADR 0059, animated battle backgrounds as grid sheets** (decisions 896-900, 904). Its options were chosen as a
  default while the developer was away: write it from this entry with Status "Accepted (default taken while the
  developer was away; his to overturn)". Context: decision 680 keeps the game PNG-only; a full-screen frame is 1920 px
  wide and `TextureLimit.MaxSide` is 4096, so a strip holds two. Options, each with its costs from design.md's table:
  (A) a grid sheet at a reduced frame size, scaled up by the game (chosen); (B) a folder of full-size frame PNGs, one
  texture each; (C) a strip at a reduced size (at most 8 frames at 480 px wide); (D) a grid at full size (at most 6
  frames); (E) a video file (refused by decision 680). Decision: (A), its record in `animated-backgrounds.json`
  (decision 897), the heights (898), the time from the battle's delta (899), the decode on map entry and the release of
  the last map's (900), averaging when shrunk (904). Consequences: one decode and one texture per background, 5 to
  64 MB of video memory each, only the current map's held; **the ceiling: a 24-frame loop is at most 960 x 540 a frame,
  shown at 2x, and full-size frames fit 6 to a sheet** (up to 264 at 180 px); pixel art imported at its own height stays
  as drawn, painted art is averaged once when shrunk. The README index line: "An animated battle background is a grid
  sheet PNG of reduced frames scaled up by the game, its frame size, count and time a record in
  `animated-backgrounds.json`; the editor makes it from a GIF and plays it before writing; a 24-frame loop is at most
  960 x 540."
