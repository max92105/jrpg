# Workspaces: the mode bar (the editor push's row 11)
Status: approved, in progress

Drafted on 2026-11-15 from the editor push's row 11 ([roadmap](../2026-11-15-editor-push/README.md), decisions 683, 693,
694; [design 3.1](../2026-11-15-editor-push/design.md)). The developer: "the editor became more than just a map editor.
I want multiple modes of edition now I think" and "mode bar to use more of screen space would be very good". He was
away while this was planned: the defaults taken for him are marked and listed under "Defaults to confirm".

Approved on 2026-11-15 by the coordinator, the developer having delegated approval, with the devil's advocate's review
applied: the plan restaged in three (B1, decision 936): the dialog and the Database menu's move before row 7, the six
windows as pages inside that dialog right after, the mode bar at row 11's rank; a throwing Apply refused, never
thrown (B2, decision 939); the World parts off in Database mode completed (C1-C3, decision 925); focus, the smoke
script's order, recounts per conversion, Redo's wording (C4-C7).

- Numbers: decisions 916-940, ADR 0060. Slice ids WD1-WD8 (dialog pages, stages 1 and 2) and WS1-WS6 (the mode bar,
  stage 3); MB is an older plan's.
- Editor only, plus `.claude/scripts/open-editor-windows.ps1`. No Core, Data, content or save change.
- **The page recipe** every Database editor follows from stage 2 on, rows 7-10 and 12 included:
  [page-recipe.md](page-recipe.md).

## Goal
1. **Stage 1, screen space at once:** every Database editor opens at the main window's size in one dialog,
   `Workspaces/CatalogDialog`, hosting a page; the Database menu leaves `MainWindow.xaml` for `Shell/DatabaseMenu.xaml`
   so rows 7-10 and 12 add their entries there.
2. **Stage 2, pages:** the six other windows become pages inside that dialog, one small slice each; rows 7-10 and 12
   build their editors as pages from birth on [page-recipe.md](page-recipe.md).
3. **Stage 3, the mode bar** at row 11's rank: World (today's map editor, whole) and Database, the Database mode a page
   list and the chosen page filling the window, with the undo rule of a live page (ADR 0060).

## The cost, honestly
| Stage | What he gets | Slices | Undo rule |
|---|---|---|---|
| 1 | Every Database editor at the main window's size; the menu out of the main window's XAML | WD1-WD2 (about 300 lines) | Unchanged (modal, one step per OK) |
| 2 | Pages, so later editors are written once for both hosts | WD3-WD8 (about 80-150 lines each) | Unchanged |
| 3 | The mode bar, the whole window, no window to open or close | WS1-WS6 (about 1250 lines with tests) | Apply on leave (ADR 0060) |

Set aside: per-field undo steps, about 8 more slices (decision 931). The roadmap's "about 9 slices" was low: stage 3
alone is six, since the undo rule touches every exit (page, mode, undo, redo, save, close) and each needs its tests
before a page can be typed into in Database mode.

## Order, and why
1. **Stage 1 (WD1, WD2) runs right after row 6, before row 7's first window slice** (decision 936): rows 7, 9, 10 and
   12 add Database entries and editors; with the menu moved (row 9's decision 890, done here) and the dialog in place,
   they add a menu line in `DatabaseMenu.xaml` and a page, never a window to convert later. WD1 is the pure move, WD2
   the dialog with the Shops page as its first.
2. **Stage 2 (WD3-WD8) right after stage 1**, mechanical: Quests; Drop tables before Enemies (the Enemies window's
   Edit... then opens the Drop tables page in a dialog over it); Effect strips, Cinematics, Character sprites. Each
   starts by re-reading its window and recounting, since rows 3-5 change some of them first.
3. **Stage 3 at row 11's rank, after rows 7-10** (decision 693; he may move it earlier): WS1 the mode bar with an
   empty Database mode; ADR 0060; WS2-WS4 the rules headless (session; leaving a page and the mode; Undo, Redo, Save,
   close), so no commit has a page whose typed work an undo could drop silently; WS5 the Database view hosting every
   page; WS6 Edit... buttons showing a page on a record, and the dialog retired.

## Guiding rules
- **Never lose typed work, never ask in flow:** in Database mode a page's typed changes are applied before anything
  that could change the draft under them; a problem refuses the move and names itself; Revert is the only way to drop
  them (the developer dislikes prompts in flow, decision 497).
- **One session at a time:** only the shown page holds unapplied edits; World mode holds none. So a page never
  re-checks the draft under its session: it starts again from the draft on every change (`.claude/rules/editor.md`'s
  "re-checks on every `CatalogDraft.Current` change").
- **Pages, written once:** a page is a user control over its catalog view model, hosted by `CatalogDialog` (stages 1-2)
  or the Database view (stage 3); the view model gains the three `IEditedCatalog` members and nothing else.
- **Headless first:** the workspace and its leave rule are tested through `DatabaseWorkspaceViewModel` and
  `MainViewModel`; the views only bind them.
- **Sizes:** Editor slices about 200 lines, tests at about 10 lines each; WS2 and WS5 are larger and name their split.
- **Files near 300** (checked 2026-11-15; each slice recounts): `Shell/MainWindow.xaml` 294 (WD1 takes the Database
  menu out, about -6; rows 7-10 then add nothing there; WS1 recounts before adding its lines); `Shell/MainViewModel.cs`
  286 (+2 in WS3); `Shell/MainWindow.xaml.cs` 268 (+6 in WS1); `Shell/EditorTemplates.xaml` 297 (untouched);
  `Shell/CombinedHistory.cs` 221 (+10 in WS4); `Shell/MainViewModel.Saving.cs` 197 (+25 in WS4).
- **Test files, globbed on 2026-11-15:** no `tests/Jrpg.Editor.Tests/Workspaces/` folder; no
  `MainViewModelWorkspaceTests`, `MainViewModelPageUndoTests`, `MainViewModelPageSavingTests`. Each slice names the file
  it fills; `DropTableCatalogViewModelTests` (254) only renames two tests in place; `FieldSpriteCatalogViewModelTests`
  (242) gets the sibling `FieldSpriteCatalogViewModelChoiceTests` in WD8.

## Out of scope
- Per-field undo steps (decision 931).
- New layouts for the big screen: pages keep their windows' layouts (decision 929); a relayout is per page, later.
- An Art mode, or more modes (decision 916).
- The modal dialogs that are not catalog editors: Map settings, the event dialogs, the composer, New NPC, Edit shapes,
  the warp picker, the GIF import's options. They stay modal in World mode.
- Remembering the mode or the shown page between runs (decision 922).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when
a slice is done and write its deviations in the slice file. Tunables, design and risks: [design.md](design.md); the
page recipe: [page-recipe.md](page-recipe.md).

### Stage 1: the dialog and the menu (right after row 6, before row 7's first window slice)
- [x] **WD1. The Database menu in its own file, `Shell/DatabaseMenu.xaml`, its clicks caught on the menu** (Editor,
  about 80 lines, a move) → [slices/WD1.md](slices/WD1.md)
- [ ] **WD2. `CatalogDialog` and the Shops page: one dialog at the main window's size hosting a page; the six other
  windows open at that size too** (Editor, about 230 lines) → [slices/WD2.md](slices/WD2.md)

### Stage 2: the six windows as pages (right after stage 1)
- [ ] **WD3. The Quests page** (Editor, about 130 lines) → [slices/WD3.md](slices/WD3.md)
- [ ] **WD4. The Drop tables page; the Enemies window's Edit... opens it in a dialog** (Editor, about 130 lines) →
  [slices/WD4.md](slices/WD4.md)
- [ ] **WD5. The Enemies page** (Editor, about 150 lines) → [slices/WD5.md](slices/WD5.md)
- [ ] **WD6. The Effect strips page** (Editor, about 130 lines) → [slices/WD6.md](slices/WD6.md)
- [ ] **WD7. The Cinematics page** (Editor, about 140 lines) → [slices/WD7.md](slices/WD7.md)
- [ ] **WD8. The Character sprites page** (Editor, about 150 lines) → [slices/WD8.md](slices/WD8.md)

### Stage 3: the mode bar (row 11's rank, after rows 7-10)
- [ ] **WS1. The mode bar: World and Database, the Database mode an empty page; World's menus, tool bar, map keys,
  Delete selected objects and the map's undo off in Database mode** (Editor, about 230 lines) →
  [slices/WS1.md](slices/WS1.md)
- [ ] ADR 0060, the Database mode's pages and the undo rule of a live page. Write it from decisions 918-924, 928, 931
  and 939; status "accepted as a default; the developer's to overturn". Amends ADR 0045 (its amendment B, the nested
  window, retired by WS6).
- [ ] **WS2. A page's session: the catalog pages, the shown one started from the draft, Apply as one undo step
  starting it again on the same record, Revert** (Editor, about 230 lines) → [slices/WS2.md](slices/WS2.md)
- [ ] **WS3. Leaving a page applies it first: another page and World mode, refused with the page's problem or its
  apply's failure; a draft change starts the shown page again** (Editor, about 240 lines) → [slices/WS3.md](slices/WS3.md)
- [ ] **WS4. Undo, Redo, Save, Save all, the close question and the dialogs still opened in Database mode apply the
  page first** (Editor, about 250 lines) → [slices/WS4.md](slices/WS4.md)
- [ ] **WS5. The Database view hosts every page: the grouped list, the bar, focus kept; the Database menu shows pages;
  the smoke script reaches them** (Editor + script, about 260 lines) → [slices/WS5.md](slices/WS5.md)
- [ ] **WS6. Edit... buttons show a page on a record (Enemies' drop table, Skills' strip); the nested refresh and
  `CatalogDialog` go** (Editor, about 150 lines, about 200 deleted) → [slices/WS6.md](slices/WS6.md)

The rows' own editors (statuses, skills, items, affixes, encounters, backgrounds, classes...) are not slices here:
they are pages from birth (decision 934). Files `slices/WS7.md` to `slices/WS12.md` are superseded stubs to delete.

## Decisions

Every decision below was taken while the developer was away; the ones marked "(default ...)" are tool design picks he
may overturn; the others follow from them, from the code or from the review.

- **916.** Two modes, **World** and **Database**, as two buttons at the left of the tool bar row (no extra height in
  World mode). One mode per catalog is refused: about twenty buttons by row 12, and the Database list already shows
  them. An Art mode waits for art pages that need it. *(default taken while the developer was away; his to overturn)*
- **917.** The Database mode: the pages on the left under group headings (Story: Quests, Cinematics; Battle: Enemies,
  Drop tables, Effect strips and rows 7, 8 and 10's; Items: row 9's; Trade: Shops; Characters: Character sprites and
  row 12's), the shown page filling the rest under a bar along its top: its title, Apply, Revert, its problem line and
  the note "Applied when you leave this page, save, undo or close." *(default taken while the developer was away; his
  to overturn)*
- **918.** A page is a user control over its catalog view model (`IEditedCatalog`), hosted by `CatalogDialog` (stages
  1-2, and outside Database mode) or the Database view (stage 3). In Database mode it starts from the draft when shown;
  Apply records its whole session as one undo step (ADR 0045's one step per session kept) and the page starts again
  from the draft on the same record, so an added record's id is fixed at its first save, undo or leave (decision 582)
  and only records added since can be removed (decision 581).
- **919.** In Database mode, leaving a page applies it first: showing another page, World mode, Undo, Redo, Save, Save
  all, closing the editor, and any dialog opened over the Database mode that changes the draft. A page whose problem
  stops Apply refuses the move, naming the page and its problem ("The Shops page has a problem: ... Fix it or press
  Revert."); Revert drops what was typed. No question is asked. *(default taken while the developer was away; his to
  overturn)*
- **920.** So Ctrl+Z on a page with typed changes applies them and takes that step back at once; Ctrl+Y with typed
  changes only applies them, which records a new step and so drops the redo (as any edit does). Inside a focused text
  box Ctrl+Z stays the box's own (WPF's). With nothing in the history, Ctrl+Z is greyed and the typed changes stay.
- **921.** Any change of the draft while a page is shown (its own Apply, an undo, a redo, a save) starts its session
  again from the draft, the same record chosen (the first when it is gone). By decision 919 nothing typed is pending
  at that moment.
- **922.** One page session at a time: World mode ends it; Database mode reopens the last page shown on its last chosen
  record (the list's first page on the first visit). The editor starts in World mode; neither is remembered between
  runs.
- **923.** Save and Save all apply the shown page first; a page that can't apply stops them with nothing written, the
  save line naming the page and why.
- **924.** The close question and the unexpected-error box (ADR 0050) list a page that can't apply ("the Shops page,
  not applied: ..."); its Save all then refuses as in decision 923 and the editor stays open; Don't save closes without
  running Apply again. A page with valid typed changes is applied before the question, so it counts as the catalogs'
  unsaved change.
- **925.** In Database mode the map is off: the World menus (View, Tiles, Objects, NPCs, Map) and the tool bar's bands
  hidden; the window's key handler skipping the tool, object, pan and zoom keys; Copy, Cut, Paste and Delete selected
  objects greyed; the undo history no longer reaching the shown map's steps (`History.OpenMap` null, the map's steps
  waiting as another map's do, and back on World). File, Edit, Database and Help stay. Ctrl+N, Ctrl+M (Go to map) and
  Alt+Left (Back to previous map) show World first. *(default taken while the developer was away; his to overturn)*
- **926.** The Database menu stays as a shortcut, in `Shell/DatabaseMenu.xaml` from stage 1: before stage 3 each entry
  opens its page in `CatalogDialog`; from WS5 each shows its page (and the Database mode), its "..." dropped; NPCs >
  Character sprites does the same.
- **927.** Ctrl+Shift+W shows World, Ctrl+Shift+D shows Database; the F1 list names them. *(default taken while the
  developer was away; his to overturn)*
- **928.** Edit... buttons that reach another catalog: until WS6 the Enemies page's Edit... beside the Drop table box
  opens the Drop tables page in a `CatalogDialog` over it, its OK applying its own step at once (decision 602 kept);
  from WS6 it shows the Drop tables page on the enemy's table, the Enemies page applied first (or the move refused),
  and the nested refresh goes, since the Enemies page starts again from the draft when shown. A Skills page's strip
  Edit... (row 7) takes the same path. *(default taken while the developer was away; his to overturn)*
- **929.** Pages keep their windows' layouts: what filled the window (lists, previews) fills the page; fixed-width boxes
  stay fixed. A relayout for the big screen is per page, when he asks. *(default taken while the developer was away;
  his to overturn)*
- **930.** `CatalogDialog` keeps OK as the default button and Cancel as the cancel button (Enter and Esc as today);
  pages in the Database mode have no keys for Apply and Revert; Ctrl+S applies and saves. *(default taken while the
  developer was away; his to overturn)*
- **931.** ADR 0060's options set aside: per-edit undo steps (every view model rewritten to apply typed boxes live, a
  typed id invalid between keys, about 8 more slices); a question when leaving with unapplied changes (a prompt in
  flow); each page keeping its session across switches as open maps do (an undo under a hidden session makes it
  stale: drop it silently or ask); staying at stage 1's dialogs (the screen space without the mode bar he asked for).
- **932.** Stage 2's order: Quests, Drop tables, Enemies, Effect strips, Cinematics, Character sprites (Shops is WD2's);
  each conversion deletes its window and leaves the others working.
- **933.** `open-editor-windows.ps1`: stages 1-2 find the dialog by its title, as today. From WS5 an entry that opens no
  window but shows a page reports "shown as a page" with the main window's screenshot, found by the "{Title} page"
  automation name on the hosted page; its default list runs every World-menu entry (Map > Settings..., NPCs >
  Character sprites...) before the first page entry, the Map and NPCs menus being hidden in Database mode.
- **934.** Rows 7-10 and 12 build their editors as pages from birth on [page-recipe.md](page-recipe.md), opened in
  `CatalogDialog` from `DatabaseMenu.xaml` until WS5 and shown in the Database mode after; no conversion slice for them.
- **935.** Names: `Workspaces/` holds `CatalogDialog`, `DialogBounds`, `IEditedCatalog`, `EditorWorkspace`, `ModeBar`,
  `DatabaseWorkspaceViewModel`, `DatabaseWorkspaceView`, `CatalogPage`, `PageSession`, `ICatalogEditing<TEdited>`,
  `CatalogGroup`, `DatabasePageViews`; a converted window `<Catalog>Window` becomes `<Catalog>Page` in its feature
  folder (`ShopCatalogWindow` → `ShopCatalogPage`, `FieldSpriteWindow` → `FieldSpritePage`). `MainViewModel.Mode` keeps
  meaning the side panel's tab (`EditingMode`).
- **936.** (Review B1, the coordinator's.) Three stages: stage 1 (the dialog and the menu's move) right after row 6 and
  before row 7's first window slice; stage 2 (the six windows as pages in the dialog) right after; stage 3 (the mode
  bar) at row 11's rank, after rows 7-10. Stages 1 and 2 change no undo rule.
- **937.** `CatalogDialog`: one modal window for any page: the page's title, the page, its problem line, OK greyed while
  the page has a problem, Cancel; it opens over the main window at its size (`DialogBounds`, which also sizes the six
  windows until each converts); OK applies the page's session as one undo step, Cancel drops it.
- **938.** The Database menu moves to `Shell/DatabaseMenu.xaml` in WD1 (row 9's decision 890, done here for every row):
  a `MenuItem`-rooted file; its entries' clicks are caught as a routed `MenuItem.Click` on the menu element in
  `MainWindow.xaml` and dispatched by the entry's `Tag` (the page title) to `MainWindow.Database`'s openers. From WS5
  the entries bind `ShowDatabasePageCommand`, and the routed handler skips any entry so bound.
- **939.** (Review B2.) A page's Apply that throws (a `To...` on a state its problem line missed, Data's check) is turned
  into a refusal by `TryApplyShown`, "The Shops page can't apply: {message}", catching the two exceptions an apply
  raises (`InvalidOperationException`, `ContentValidationException`), so no move, save or close depends on it
  succeeding; the error is still a bug to fix.
- **940.** (Review nitpick 3, C4.) After `TryApplyShown` the page that called it is detached (the session started
  again): a page command reads the values it needs first, and any dialog it opens is owned by the main window. When the
  shown page is rebuilt and the old one held the keyboard focus, the new one takes it (`MoveFocus` to its first
  element).

## Defaults to confirm
Taken while the developer was away; each is his to overturn, and the slices that change if he does are named.
- **The undo rule of a live page (ADR 0060; 919, 920, 918):** leaving a Database page applies it, a problem refuses the
  move, Ctrl+Z on typed changes applies then undoes; an id is fixed at the first save, undo or leave. The alternative
  he is most likely to prefer is a question on leave (decision 931). WS3, WS4.
- **916:** two modes on two buttons beside the tool bar (WS1).
- **917:** the group headings and the bar along the page's top (WS5).
- **925:** World menus hidden, not greyed, in Database mode (WS1).
- **927:** Ctrl+Shift+W and Ctrl+Shift+D (WS1).
- **928:** Edit... beside the Drop table box shows the Drop tables page from WS6, instead of a dialog over the enemies.
- **929, 930:** layouts kept as they are; Enter and Esc only in the dialog (WD2-WD8, WS5).

## Open design questions (developer decides)
- None open: every question above was taken as a default to confirm.

## ADRs needed
- **0060, the Database mode's pages and the undo rule of a live page** (before WS2): written from decisions 918-924,
  928, 931 and 939, "accepted as a default; the developer's to overturn". Amends ADR 0045. Stages 1 and 2 need no ADR:
  the dialog keeps ADR 0045's OK and Cancel.
