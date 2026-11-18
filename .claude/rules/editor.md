---
paths:
  - "src/Jrpg.Editor/**/*"
---

# Rules for Jrpg.Editor (WPF tools)

- MVVM: views (XAML) have no logic in code-behind beyond view-only concerns. Logic lives in ViewModels.
- Use `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`, `[RelayCommand]`) if already referenced; don't hand-roll `INotifyPropertyChanged` boilerplate.
- The editor reads and writes `content/` JSON through Jrpg.Data, using the same schemas the game uses. Never invent a second format.
- Validate on save: an editor must not be able to write content the game would reject, except missing art: the editor saves with an image missing and warns (decision 442); the game's startup is where the art check refuses.
- The editor never references Jrpg.Game.
- A local or keyed style on a control the theme styles is `BasedOn="{StaticResource {x:Type X}}"`; otherwise it replaces the dark theme (`Shell/EditorTheme.xaml`, `Shell/EditorTemplates.xaml`) with the light Windows look.
- A feature reached by a modifier gesture (Shift + drag) also gets a visible control where it is used (a button beside the panel, a menu entry); the slice's hand check says how the developer finds it, not only that it works.
- A view model that replaces a whole catalog or map the window draws (the sheet purge's `CatalogDraft.Rewriting`) raises its event with the new value before the replacement, so listeners swap what they hold first; raised after, the window drew the old map against the new catalogs and crashed with the files already written (the reset's E3c).
- A panel or command holding a cell, tile or object of the catalog re-checks it on every `CatalogDraft.Current` change (undo, redo, a removal): it closes, re-raises `CanExecute`, or picks the stored record; a test makes that change with the panel open (the composer plan's CO4a, AE1, AE2).
- Code-behind that runs a toolkit `RelayCommand` (a wheel or key handler) checks `CanExecute` first: the toolkit's `Execute` doesn't, so a zoom at its bound throws (the levels plan's SZ1 review, applied in SZ2 and SZ3).
- A view model never guards a state a modal window or a greyed entry rules out (the shown map changing while the composer is open): `Single` or the value, so a bug fails instead of skipping (the map editing flows' OM3, OM4).
- A typed `double` field checks `double.IsFinite`: `double.TryParse` accepts "NaN" and "Infinity" whatever the styles, and the save then throws (the map editing flows' PO1, twice).
- A window-level tool key (B, R, F, G) goes through the one guard in `MainWindow.xaml.cs` that skips a focused text or combo box, never its own check (raised in the SN1, PO2 and PO3 reviews).
- An id that is also a file name (maps, objects) is compared without case at every check of one path, in the editor's dialog and in Data alike; a case-only rename of a hand-made upper-case id is refused by the dialog, never thrown by Data (the map fixes plan's RN1 and RN2 reviews).
- A list or tree row's context menu goes on the item container (`ItemContainerStyle`), the view model reached through the container's `Tag` (`PlacementTarget.Tag`); on the template's text it misses a click beside the name and Shift+F10, and a `Tag` on any element but the menu's owner binds to null (MF3, MF5 reviews).
- A command or a redraw never calls what throws on the user's own input: copy the typed boxes rather than calling a `ToX()` that throws on a problem, and decode an image only after `PngSize.IsReadable`. A throw stops the command halfway and opens the unexpected-error box (ADR 0050) (the shops and enemies plan's EN2 Duplicate and EN7 preview).
- A list view model relaying its items' `PropertyChanged` to its `Problem` reacts to their typed fields only, never their own `Label` or `Problem` notices: each read runs Data's check over the whole list (SH3, EN2).
- A dialog's OK greys through `CanApply` with a problem line for every input its apply step refuses (the top for a new
  folder, the unchanged path for a rename); an apply returning false on an input the dialog accepted leaves a lit button
  that does nothing (MF5 review). A Database page has no `CanApply`: `DatabaseWorkspaceViewModel` greys Apply on the
  page's `IEditedCatalog.Problem`, so a plan writes `Problem_*` tests for each refusal (mode bar WD2-WD7; classes CL3,
  PM2, NG2, XP2).
- A note or label a view model sets at an action (a pick's count, the lock's note) is cleared or recomputed when any
  input it reads changes (the selection, the layer, the lock), and so is every copy of it shown elsewhere (the page
  refusal in `History.Refusal` and `SaveError`, WS4 review); its test changes the input with the note shown (UO1, UO2
  reviews).
- A command greyed on a problem (Play, Apply, Add) reads the same problem as the window's line, Data's included: a value the boxes read but Data refuses still throws downstream (the cinematics plan's CP1 Critical).
- A file or record combo box lists the value the chosen record names even when its folder or catalog lacks it, or WPF
  keeps showing the previous record's (SK4). An editable ComboBox's choices sit on the same DataContext as its `Text`
  binding and always hold the typed or missing value, or WPF empties the `Text` when the matched item leaves the list
  (CL9d, decision 1017, `ResourceIdChoices.Of`). How to probe a binding at runtime: the code reviewer's memory note
  `.claude/agent-memory/code-reviewer/scratch-builds-blocked.md`.
- A row or line over a Core enum shows the file's spelling (`ContentJsonOptions`'s enum naming, as `ContentSpelling`
  does), never the C# name (FX2b, FX6).
- A file the developer picked is read under one catch naming every refusal the read can raise (`InvalidDataException`, `IOException`, `UnauthorizedAccessException`, and the decoder's own), its decoding included, not only the open: a throw stops the command halfway and opens the unexpected-error box (ADR 0050) (the animated monsters plan's MB5, MB6).
- A XAML size (`Width`, `Height`, `FontSize`) bound with `x:Static` reads a `double` constant: WPF refuses an `int`
  there and the window throws as it opens, crashing the editor. Run `.claude/scripts/open-editor-windows.ps1` after a
  XAML change: editor tests never open a window; it shows each Database page (reported "shown as a page" since the mode
  bar's WS5) and an `ERROR` line names a window whose opening threw (the cinematics window's `PreviewWidth`, found at
  the battle presentation plan's hand check). The event windows open from a map click, so after a change to `Events/`
  XAML run it with `-EventWindows -Map <id>` and read its trace for binding errors (a Debug editor writes them to its
  error output).
- A window's work on each drawn frame goes through `Hosting/RenderTick` (`Start`, `Stop`, or a page's
  `RunWhileLoaded`, WD8), never a handler added to `CompositionTarget.Rendering`: a handler that throws there throws on
  every frame and WPF paints nothing, so the editor stays open but blank; `RenderTick` stops the work and hands the
  error to the box once (ADR 0050's hand check).
