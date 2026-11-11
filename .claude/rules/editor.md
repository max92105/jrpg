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
- A dialog's OK greys through `CanApply` with a problem line for every input its apply step refuses (the top for a new folder, the unchanged path for a rename); an apply returning false on an input the dialog accepted leaves a lit button that does nothing (MF5 review).
- A note or label a view model sets at an action (a pick's count, the lock's note) is cleared or recomputed when any input it reads changes (the selection, the layer, the lock); its test changes the input with the note shown (UO1, UO2 reviews).
