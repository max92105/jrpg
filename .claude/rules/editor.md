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
