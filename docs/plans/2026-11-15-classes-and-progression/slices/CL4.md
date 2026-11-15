# CL4. Database > Classes: the page with its Stats tab

Editor, about 50 lines plus about 120 of XAML. Needs CL3 and row 11's mode bar (stage 3). Design: section 3; decisions
942, 945, 962; the mode bar's WS12 recipe, steps 3-7 and 10.

## Goal
Database > Classes shows the Classes page in the Database mode, under the Characters group: the classes left (id: name)
with Add, Duplicate and Remove under the list; the chosen class right in a tab control whose first tab, Stats, holds
the id (editable while new), name, description (wrapping, three lines high), and the base and growth boxes side by
side in one grid (a row per stat, "Base" and "Per level" columns). The bar above shows Apply, Revert and the problem
line; leaving the page applies it (ADR 0060). Ctrl+Z after an Apply puts the classes back.

**Recount first:** the files below on the day; one that would pass 300 gets its move in this slice (decision 962).

## New types
- `Party/ClassCatalogPage` (`.xaml` + `.xaml.cs`, a `UserControl`): the Classes page, the list left and the chosen
  class's tabs right. Resources from `EditorTheme.xaml` only; the chosen-record panel uses `EnabledWhenChosen`; the
  stat grid labels the primaries in `StatKinds`' order after max HP, speed and accuracy; Speed's label carries the
  tooltip "Sets the rogue's readiness charges; turn order ignores speed for now." (`ClassStats`' own summary).

## Changes
- `Shell/MainViewModel.Workspaces.cs`: `CatalogPage.Of("Classes", CatalogGroup.Characters, new
  ClassEditingViewModel(content))` after Character sprites.
- `Shell/MainWindow.Database.cs`: `Register<ClassCatalogViewModel>(classes => new ClassCatalogPage(classes))`.
- `Shell/DatabaseMenu.xaml`: `<MenuItem Header="Classes" Command="{Binding ShowDatabasePageCommand}"
  CommandParameter="Classes" ToolTip="Each class's stats per level, its skills and their levels, and its battle
  resources; party members name a class." />`.
- `.claude/scripts/open-editor-windows.ps1`: `'Database>Classes'` joins the default list.
- `docs/map.md`, Editor `Party` row: `ClassCatalogPage`.

**Fallback (only if the developer moved row 11 after this row):** `Party/ClassCatalogWindow` instead, the same layout
with OK (greyed through `CanApply`), Cancel and the problem line, sized 1000 x 700; a `Database > Classes...` entry in
`MainWindow.xaml` with `OnEditClassesClicked` in a new `Shell/MainWindow.Party.cs`, start, show modal, apply on OK.

## Tests
None new: the page binds CL2's and CL3's tested view models. Guard: the smoke script shows it with no `ERROR` line.

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: "Database>Classes: shown as a page", no `ERROR`, no binding-error
   line.
2. Run the editor, Database > Classes: nine classes in id order; choose `mage`: its numbers match `content/classes.json`;
   the id box is read-only.
3. Type `x` in Per level power: the problem line names "Growth power" and Apply greys; press World: refused, naming the
   page. Type `3`, Apply, File > Save all: `git diff content/classes.json` shows the mage's growth power at 3. Ctrl+Z,
   Save all: the diff is empty.
4. Add: "(new class)" appears last and chosen; the line asks for an id. Type `ranger`, a name, a description, Apply,
   Save all: `classes.json` holds `ranger` in id order. Run the game: it starts. Ctrl+Z, Save all to take it out.
5. Duplicate `fighter`, type id `knight`: the copy's numbers match the fighter's. Revert: nothing changed.
