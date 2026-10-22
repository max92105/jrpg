---
paths:
  - "src/Jrpg.Editor/**/*"
---

# Rules for Jrpg.Editor (WPF tools)

- MVVM: views (XAML) have no logic in code-behind beyond view-only concerns. Logic lives in ViewModels.
- Use `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`, `[RelayCommand]`) if already referenced; don't hand-roll `INotifyPropertyChanged` boilerplate.
- The editor reads and writes `content/` JSON through Jrpg.Data, using the same schemas the game uses. Never invent a second format.
- Validate on save: an editor must not be able to write content the game would reject.
- The editor never references Jrpg.Game.
- A local or keyed style on a control the theme styles is `BasedOn="{StaticResource {x:Type X}}"`; otherwise it replaces the dark theme (`Shell/EditorTheme.xaml`, `Shell/EditorTemplates.xaml`) with the light Windows look.
- A feature reached by a modifier gesture (Shift + drag) also gets a visible control where it is used (a button beside the panel, a menu entry); the slice's hand check says how the developer finds it, not only that it works.
