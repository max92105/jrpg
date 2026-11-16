# FX6. Database > Statuses...: the page

Editor + script, about 170 lines (XAML about 140, wiring about 15; FX4 and FX5 hold the rules). Decisions 976, 984,
985, 996. Design: design.md, section 2.7 ("Statuses"); the mode bar's
[page recipe](../../2026-11-15-mode-bar/page-recipe.md). Builds on FX5.

## Goal
Database > Statuses... opens the Statuses page in `CatalogDialog`: the statuses left (id, then name) with Add,
Duplicate, New tier and Remove; the chosen status's boxes right (Id, Name, Description, Harmful, Family, Acts at, Mark
letter, Used by) and its parts as rows (Kind, the amount with its label, Damage type, Stat) with Add, Remove, Up, Down;
the dialog's problem line, OK and Cancel; OK applies the session as one undo step, everything granting an edited status
following it. A new status or a stronger tier is made without JSON.

## New types
- `Editor/Statuses/StatusCatalogPage` (`UserControl`): the page by the recipe (no OK, Cancel or problem line; typed
  boxes `PropertyChanged`; bindings name `AncestorType=statuses:StatusCatalogPage`); a part row chosen by its
  `PreviewGotKeyboardFocus` `EventSetter`; the Family box an editable `ComboBox` (`IsEditable="True"`, `Text` bound to
  `FamilyText`, items `Families`), tooltip "Statuses of one family don't stack: a holder keeps one (empty: its own
  id)"; Acts at as two radio buttons ("Start of its holder's turn", "End of its holder's turn"); Mark letter
  `MaxLength="1"`, tooltip "The letter on its battle mark; empty for the name's first"; a part's amount box and label
  shown by `HasAmount`, Damage type enabled by `CanTypeDamageType`, Stat by `CanTypeStat`.

## Changes
- `Editor/Shell/MainViewModel.Database.cs`: `StatusEditing`; `Editor/Shell/MainViewModel.cs`: one constructor line
  (+1).
- `Editor/Shell/MainWindow.Database.cs`: one opener registered under "Statuses" through `OpenInDialog`.
- `Editor/Shell/DatabaseMenu.xaml`: "Statuses..." (`Tag="Statuses"`) beside "Effects...", tooltip "Poison, Stun,
  Guard...: each status's parts, family and timing; skills grant them with turns and a chance."
- `.claude/scripts/open-editor-windows.ps1`: `'Database>Statuses...'` in the default list.
- `docs/map.md`: an Editor `Statuses` row.

## Tests
None new (draw and wiring): `StatusCatalogViewModelTests` and `StatusViewModelTests` hold the rules.

## Verify by hand
1. Database > Statuses...: `poison_1` shows Poison, family `poison`, end of turn, one part "HP lost each turn" 8, "Used
   by: rot".
2. New tier: `poison_2` appears after it, family `poison`; amount 12, OK, Save all: `git diff content/statuses.json`
   shows the new record in the writer's layout. Database > Skills..., `rot`'s grant box offers `poison_2`; Esc.
3. Back in Statuses, on `poison_2` change the part's kind to heal each turn: Data's family message, OK greys; Esc.
4. `burning_1`: Acts at start; switch to end, OK, run the game, Fire Blast a goblin: the burn ticks after its turn.
   Ctrl+Z, Save all.
5. `slowed_1`: two stat change parts, the Stat boxes show accuracy and crit chance; the mark letter L.
6. Run `.claude/scripts/open-editor-windows.ps1`: "Database>Statuses...: opened", no `ERROR` or binding-error line.
- **Note from FX4:** a part's Damage type box binds `DamageTypeChoices` as items and `SelectedDamageTypeChoice` as
  `SelectedItem` ("(none: ignores resistances)" first, `poison_1`'s); a new stat change's Stat box is blank until
  picked. The map's Editor `Statuses` row exists (FX4): the page joins it.
