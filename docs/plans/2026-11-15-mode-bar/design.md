# Workspaces: the mode bar: design

The README holds the decisions, the slices and the defaults to confirm; [page-recipe.md](page-recipe.md) the steps a
page follows. This file holds the tunables, what exists, the structure per stage, the undo rule in detail, the
replacement mapping and the risks.

## 1. Tunables and placeholders

| Name | Where | Placeholder value | Meaning |
|---|---|---|---|
| Dialog size | `Workspaces/DialogBounds` | the main window's bounds (maximized when it is) | Where `CatalogDialog` and the windows not converted open (decision 937). |
| Mode keys | `Shell/MainWindow.xaml` input bindings | Ctrl+Shift+W, Ctrl+Shift+D | Show World, show Database (decision 927). |
| Mode button labels | `Workspaces/ModeBar.xaml` | "World", "Database" | The two buttons (decision 916). |
| Page list width | `Workspaces/DatabaseWorkspaceView.xaml` | 200 px | The Database mode's left column, as the map list's `MapListWidth`. |
| Groups | `Workspaces/CatalogGroup` | Story, Battle, Items, Trade, Characters | The list's headings, in this order (decision 917). |
| Bar note | `Workspaces/DatabaseWorkspaceView.xaml` | "Applied when you leave this page, save, undo or close." | Says the leave rule where Apply is (decision 919). |
| Refusal texts | `Workspaces/DatabaseWorkspaceViewModel` | "The {title} page has a problem: {problem} Fix it or press Revert."; "The {title} page can't apply: {message}" | Why a move was refused (decisions 919, 939); Undo and Save show the same text in their lines. |
| Unsaved line | `Workspaces/DatabaseWorkspaceViewModel.UnappliedPage` | "the {title} page, not applied: {problem or message}" | Listed by the close question and the error box (decision 924). |
| Page name for the script | the hosted page's `AutomationProperties.Name`, set by `DatabaseWorkspaceView` | "{title} page" | What `open-editor-windows.ps1` finds from WS5 (decision 933). |

## 2. What exists (2026-11-15)

- **One main window, map first** (`Shell/MainWindow.xaml`, 294 lines): menu (its Database menu 8 lines), `StatusBand`,
  a `ToolBarTray` (main band, then the Tiles and Objects bands by `MainViewModel.Mode`), a `StatusBar`, then a
  five-column `Grid`: map list, splitter, map viewport, splitter, side panel.
- **Seven modal catalog windows**, each `XEditingViewModel.Start()` → a catalog view model → `new XWindow(vm,
  lookups) { Owner = this }.ShowDialog()` → `Apply(vm)` on OK, one `CatalogDraft.SetX` step (ADR 0045, decision 591).
  Rows 3-10 change several before this plan's stages reach them (each conversion re-reads and recounts):

| Window | Opened by | XAML / code-behind lines | Chosen record | Per-frame work | Changed first by |
|---|---|---|---|---|---|
| `Trade/ShopCatalogWindow` | `MainWindow.Database.OnEditShopsClicked` | 58 / 18 | `SelectedShop` | none | |
| `Quests/QuestCatalogWindow` | `MainWindow.QuestEditing` | 76 / 27 | `SelectedQuest` | none (problem shown on OK only) | |
| `Loot/DropTableCatalogWindow` | `MainWindow.Database.EditDropTables` (menu, and nested from Enemies) | 96 / 24 | `SelectedTable`, `ChooseTable(id)` | none | |
| `Battle/EnemyCatalogWindow` | `MainWindow.Database.OnEditEnemiesClicked` | 205 / 205 | `SelectedEnemy` | `RenderTick` (idle loop) | row 8 (Tactics tab, width 1000), after stage 2 |
| `Battle/EffectStripCatalogWindow` | `MainWindow.Database.OnEditEffectStripsClicked` | 79 / 112 | `SelectedStrip` (by file) | `RenderTick` | row 7 (the shared preview), after stage 2 |
| `Cinematics/CinematicCatalogWindow` | `MainWindow.Database.OnEditCinematicsClicked` | 156 / 78 | `SelectedCinematic` | `RenderTick` (Play) | row 4 (the focus point), before stage 1 |
| `Sprites/FieldSpriteWindow` | `MainWindow.FieldSprites` (NPCs menu) | 146 / 189 | `Chosen` (by file) | `RenderTick` (walk) | row 5 (cells and mirrors), before stage 1 |

- **Every catalog view model** exposes `Problem` and raises its change; five have `CanApply => Problem is null`.
- **Undo:** `Shell/CombinedHistory` (one journal over the open maps and `CatalogDraft`; `OpenMap` the map whose steps it
  reaches), bound to Ctrl+Z, Ctrl+Y, the Edit menu and the tool bar; a catalog undo removing a record in use is refused
  into `Refusal`.
- **Saving:** `MainViewModel.Saving`: `Save`, `SaveAll`, `ConfirmClose`, `UnsavedChanges` (also read by the
  unexpected-error box, ADR 0050).
- **Keys:** `MainWindow.OnPreviewKeyDown` sends every key to the zoom, tool, object and viewport handlers (B, R, F,
  G, arrows, Delete, Esc, Ctrl+0, Ctrl+1) whatever has focus, except text and combo boxes for the tool keys.
- **The smoke script** opens each `Menu>Entry`, finds the dialog the editor opened, screenshots and closes it.

### The pages to come (rows 7-12, built from birth on the recipe; names from the roadmap and the rows' plans)
Effect records, Statuses, Skills (its strip's Edit... on WS6's show-page-on-record path), the skills, classes and
enemies table (read-only), Items, Affixes, Encounters, Battle backgrounds, Classes, Party members, Experience, New game.
A read-only page (the table) has nothing to apply: its view model's `Problem` is always null and its editing's `Apply`
records nothing; the slice that builds it hides Apply and Revert for it (`CatalogPage.IsReadOnly`, its own tests).

## 3. Design overview

### Structure

New folder `src/Jrpg.Editor/Workspaces/` (namespace `Jrpg.Editor.Workspaces`):

| Type | Responsibility | Slice |
|---|---|---|
| `IEditedCatalog` | A page's edited catalog as its hosts see it: its problem and its chosen record's key. | WD2 |
| `CatalogDialog` (window) | Hosts one page modally: its title, the page, its problem line, OK greyed by it, Cancel. | WD2 (deleted WS6) |
| `DialogBounds` | Places a Database dialog over its owner at the owner's size. | WD2 (deleted WS6) |
| `EditorWorkspace` | Which half of the editor the main window shows: World or Database. | WS1 |
| `ModeBar` (user control) | The World and Database buttons, the shown one lit. | WS1 |
| `DatabaseWorkspaceView` (user control) | Draws the Database mode: the grouped page list, the bar, the shown page. | WS1 (placeholder), WS5 |
| `ICatalogEditing<TEdited>` | Starts a page's edited catalog from the draft and applies it as one undo step. | WS2 |
| `CatalogPage` | One entry of the Database list: its title (and group, WS5), and how its session starts. | WS2 |
| `PageSession` | One shown page's edited catalog with its apply. | WS2 |
| `DatabaseWorkspaceViewModel` | The Database mode's state: the pages, the shown one and its session, Apply, Revert and the leave rule. | WS2, WS3 |
| `CatalogGroup` | The list's headings, in order. | WS5 |
| `DatabasePageViews` | Builds the view of a shown page from its edited catalog, by the catalog's type. | WS5 |

`Shell/DatabaseMenu.xaml(.cs)` (WD1): the Database menu, a `MenuItem`-rooted file. `Shell/MainViewModel.Workspaces.cs`
(WS1): `Workspace`, `IsWorldShown`, `IsDatabaseShown`, `ShowWorldCommand`, `ShowDatabaseCommand`,
`GoBackToMapCommand`, then `DatabaseWorkspace` and the page list (WS3), `ShowDatabasePageCommand(string title)` (WS5)
and `ShowDatabasePageOnRecord` (WS6).

`IEditedCatalog` carries all three members from WD2: `Problem` is read by `CatalogDialog`; `Choose` by the openers that
open a page on a record (the Enemies Edit...'s drop table, WD4); `ChosenKey`'s first reader is WS2. Written from birth
so that about twenty page view models are never reopened for stage 3.

### Stages 1 and 2: the dialog

`MainWindow.Database.cs` gets one generic opener (WD2):

```csharp
// Starts the page's session, shows it in the dialog over the main window, applies it on OK.
private void OpenInDialog<TEdited>(string title, Func<TEdited> start, Action<TEdited> apply,
    Func<TEdited, FrameworkElement> buildPage) where TEdited : IEditedCatalog
```

Each catalog's entry in `DatabaseMenu.xaml` carries its title as `Tag`; the routed `MenuItem.Click` handler on the
menu element looks the title up in a dictionary of openers (`Dictionary<string, Action>`), so no `switch` grows with
the pages (CA1502). The `buildPage` lambdas are exactly what WS5 registers in `DatabasePageViews`.

### Stage 3: signatures (WS2, WS3)

```csharp
internal interface IEditedCatalog : INotifyPropertyChanged          // WD2
{
    string? Problem { get; }
    string? ChosenKey { get; }          // the chosen record's id (its file for strips and sprites); null with none
    void Choose(string key);            // an unknown key keeps the choice
}

internal interface ICatalogEditing<TEdited> where TEdited : IEditedCatalog
{
    TEdited Start();
    void Apply(TEdited edited);
}

internal sealed class CatalogPage
{
    public static CatalogPage Of<TEdited>(string title, ICatalogEditing<TEdited> editing)
        where TEdited : IEditedCatalog;                       // WS5 adds CatalogGroup group
    public string Title { get; }
    public PageSession Start(string? chosenKey);
}

internal sealed class PageSession
{
    public IEditedCatalog Edited { get; }
    public void Apply();
}

internal sealed partial class DatabaseWorkspaceViewModel : ObservableObject
{
    public DatabaseWorkspaceViewModel(CatalogDraft draft, IReadOnlyList<CatalogPage> pages);
    public IReadOnlyList<CatalogPage> Pages { get; }
    public CatalogPage? ShownPage { get; }                    // observable
    public PageSession? Session { get; }                      // observable
    public string? Refusal { get; }                           // observable, WS3
    public string? UnappliedPage { get; }                     // WS4
    public void Open();
    public bool TryClose();                                   // WS3 (WS2: Close)
    public bool TryApplyShown();                              // WS3; a throwing Apply becomes a Refusal (decision 939)
    public CatalogPage PageTitled(string title);              // WS5
    // commands: Apply (greyed while Session.Edited.Problem is set), Revert, ShowPage(CatalogPage) (WS3),
    // ShowPageOnRecord(CatalogPage, string key) (WS6)
}
```

The remembered chosen key per page is a dictionary inside the workspace, not state on `CatalogPage`.

### Folder dependencies (Editor)

| From | To | Why |
|---|---|---|
| Workspaces | Catalogs, Data `Content` | `CatalogDraft` (the restart), `ContentValidationException` (decision 939) |
| Trade, Quests, Loot, Battle, Cinematics, Sprites (and the rows' new folders) | Workspaces | `IEditedCatalog`, `ICatalogEditing<T>` |
| Shell | Workspaces, the feature folders | builds the pages, registers their views, binds the mode bar, opens the dialog |
| Workspaces | nothing else | never Shell nor a feature folder: `DatabasePageViews` takes registrations, `ModeBar` binds through its DataContext, `CatalogDialog` takes a built page |

No cycle: Catalogs references none of these; feature folders never reference Shell.

### The undo rule (ADR 0060, stage 3)

The invariant: **in Database mode only the shown page holds unapplied edits, and nothing changes the draft while it
holds them.** Every entry point that could change the draft or leave the page calls
`DatabaseWorkspaceViewModel.TryApplyShown()` first; it returns false with `Refusal` set when the page's problem stops
Apply or when Apply throws (decision 939):

| Entry point | Slice | Page that applies | Page that can't apply |
|---|---|---|---|
| Another page in the list or the Database menu | WS3, WS5 | applied, the other shown | refused, `Refusal` in the bar, the page stays |
| World (button, Ctrl+Shift+W, Ctrl+N, Ctrl+M, Alt+Left) | WS3 (WS1 wires the keys) | applied, session ended | refused, Database stays |
| Undo | WS4 | applied, then that step taken back | refused, `History.Refusal` |
| Redo | WS4 | applied: a new step, which drops the redo (decision 920); with nothing typed, the redo | refused, `History.Refusal` |
| Save, Save all | WS4 | applied, then written | refused, `SaveError` "... Nothing was written." |
| Closing | WS4 | applied before the question | listed in the question; Save all refuses, Don't save closes (no Apply run) |
| A dialog opened from Database mode that changes the draft (a `CatalogDialog` page before WS5) | WS4 (removed WS6) | applied, the dialog opens | refused, the dialog doesn't open |
| An Edit... showing another page on a record | WS6 | applied, the other page on the record | refused |

The restart rule (decision 921): the workspace listens to `CatalogDraft.Current`; on any change while a session is
shown, it starts the session again from the draft and chooses the old `ChosenKey`. Its own Apply is such a change, so
Apply needs no restart of its own; an Apply that changes nothing (the `Set*` methods record no step when the records
are the same) leaves the session as it is, which is right since nothing was added.

Off in Database mode, so they can't change the draft or the hidden map under a page (decision 925): the map list, the
map, the side panel, the World menus, the tool bar, the map keys, Copy, Cut, Paste and Delete selected objects, and the
history's reach into the shown map (`History.OpenMap` null). The sheet purge and a map rename (`CatalogDraft.Rewriting`)
start only from those.

### Key decisions
- **Restaged (decision 936):** the dialog gives the screen space at once and makes every later editor a page; the
  mode bar, whose undo rule is the costly part, comes at the rank the developer gave it.
- **Apply on leave, not a question** (decision 919): the developer keeps work open and is asked once on close
  (decision 497); refusing a move with a named problem is what Save already does.
- **The view is rebuilt on each session restart:** `DatabasePageViews.ViewOf(session.Edited)` builds a fresh page; a
  page's subscriptions and caches die with it, so no stale handler survives an undo; focus moves to the new page when
  the old one held it (decision 940).
- **Registrations, not a switch:** about twenty page types in one `switch` would break CA1502; each page registers one
  line in `MainWindow.Database.cs`, where its lookups (art loaders, the animation clock) already live.
- **Theme resources only from the application:** a page's `StaticResource` is resolved while its XAML loads, before it
  joins its host, so `MainWindowStyles.xaml` keys (`ErrorLine`) are not found there; pages use `EditorTheme.xaml` keys
  (`ProblemTextBrush`, `EnabledWhenChosen`), as the windows did.

### Extension points
A new catalog page: [page-recipe.md](page-recipe.md). In short: its view model implements `IEditedCatalog`, its editing
view model `ICatalogEditing<T>` (from WS2), one `<Catalog>Page` user control, one `DatabaseMenu.xaml` entry, one opener
(stages 1-2) or one `CatalogPage.Of` and one `Register` line (stage 3), one smoke-script entry.

### Replacement mapping

| Today | Becomes | Slice |
|---|---|---|
| The Database menu in `MainWindow.xaml`, one `Click` handler per entry | `Shell/DatabaseMenu.xaml`, one routed handler dispatching by `Tag`; from WS5 `ShowDatabasePageCommand` | WD1, WS5 |
| `ShopCatalogWindow`, `QuestCatalogWindow`, `DropTableCatalogWindow`, `EnemyCatalogWindow`, `EffectStripCatalogWindow`, `CinematicCatalogWindow`, `FieldSpriteWindow` | `<Catalog>Page` user controls of the same layout, without OK, Cancel and the problem line | WD2-WD8 |
| Each window's OK, Cancel and problem line | `CatalogDialog`'s (stages 1-2); the Database bar's Apply and Revert and the leave rule (stage 3) | WD2-WD8, WS5 |
| `Closed += ... Stop()` | `Unloaded` (and `Loaded` starts) | WD5-WD8 |
| `MainWindow.Database.On*Clicked`, `MainWindow.QuestEditing`, `MainWindow.FieldSprites` | `OpenInDialog` calls (stages 1-2); page view registrations (WS5) | WD2-WD8, WS5 |
| `MainViewModel.Has*Editing` (menu `IsEnabled`) | the opener's own check (stages 1-2); `ShowDatabasePageCommand`'s CanExecute (WS5) | WD2-WD8, WS5 |
| `MainWindow.Database.EditDropTables` (the nested window) | a nested `CatalogDialog` on the table (WD4); `ShowPageOnRecord` (WS6) | WD4, WS6 |
| `EnemyCatalogViewModel.TakeDropTables`, `EnemyEditingViewModel.RefreshDropTables`, `EnemyDropTableRefreshTests` | the session restart on a draft change | WS6 |
| `DropTableCatalogViewModel.ChooseTable` | `Choose` (`IEditedCatalog`) | WD4 |
| `CatalogDialog`, `DialogBounds` | deleted once nothing opens a dialog page | WS6 |

## 4. Risks

- **Silent loss of typed work** if an entry point that changes the draft in Database mode skips `TryApplyShown`: the
  restart would drop the page's typing without a word. Mitigation: the entry-point table above, a test per row, the
  guard on every dialog opened from Database mode until WS6, and the recipe's rule for a page command that changes the
  draft outside its session.
- **A throwing Apply** would otherwise block a save or a close; decision 939 turns it into a refusal and Don't save
  never runs it (tests with a throwing test catalog).
- **A detached page** (decision 940): code after `TryApplyShown` in a page's handler runs on a page no longer shown.
- **`Shell/MainWindow.xaml`:** WD1 frees about 6 lines; rows 7-10 add nothing to it; WS1 recounts and adds about 8.
- **The view rebuilt on each restart:** after Apply, an undo or a save, the page loses scroll, and the Enemies and
  Effect strips pages decode their images again (small PNGs; battlers are at most 1024 px).
- **The map's render tick keeps running in Database mode:** it advances the animation clock the previews play on, and
  redraws a hidden map's animated tiles when their frame changes. If the developer notices a slow page, skip the hidden
  map's redraw (backlog).
- **Pages sized for 760 x 520 windows** look sparse at full screen (decision 929).
- **Bindings that only push on lost focus:** a typed box bound with the default `LostFocus` trigger would not reach the
  view model before Ctrl+Z or a mode switch applies the page; the recipe requires `PropertyChanged` on every typed box.
- **Rows changing windows first:** rows 4 and 5 change Cinematics and Character sprites before stage 1; row 7 the
  Effect strips preview and row 8 the Enemies window after stage 2, on the pages. Each conversion re-reads and
  recounts; the rows' plans were written for windows and must read "page" where they edit one (the coordinator's
  note to those plans).
- **No save format, content or Core change.** Determinism unaffected.
