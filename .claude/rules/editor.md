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
