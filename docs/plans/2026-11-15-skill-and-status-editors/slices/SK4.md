# SK4. Database > Skills...: the page

Editor + script, about 170 lines (XAML about 120, wiring about 40; no new tests: SK2 and SK3 hold the rules).
Decisions 976, 977, 978, 982, 996. Design: design.md, sections 2.4 and 2.7 ("Skills"); the mode bar's
[page recipe](../../2026-11-15-mode-bar/page-recipe.md), steps 3, 4 and 6. Builds on SK3 and the mode bar's WD1-WD2.

## Goal
Database > Skills... opens the Skills page in `CatalogDialog` at the main window's size: the skills left with Add,
Duplicate and Remove; the chosen skill's Id, Name, Description, Icon, Sound, Strip (with its pace line) and Quick
right, with its Learned by and Used by lines; the dialog's problem line and OK (greyed by a problem) apply the session
as one undo step, Cancel drops it. From here a skill's texts and files are edited without JSON; its costs, effect and
statuses keep their file values until SK5-SK7.

## New types
- `Editor/Skills/SkillCatalogPage` (`UserControl`, XAML + code-behind): the page by the recipe: no OK, Cancel or
  problem line; every typed box `UpdateSourceTrigger=PropertyChanged`; bindings to its DataContext name
  `AncestorType=skills:SkillCatalogPage`; no per-frame work. Layout: a left column (220 px) with the list (id, then the
  name in the muted brush) and Add, Duplicate, Remove; a right `ScrollViewer` with Id (read-only once saved), Name,
  Description (two lines, wraps), Icon and Sound (combo boxes over `Choices`, Icon with an empty entry), Strip (a
  combo over `Choices.EffectImages` with "(the actor's attack strip)" for none) with the `StripPaceText` line under it
  in the muted brush, Quick (tooltip "Keeps the turn; needs a cost"), the Learned by and Used by lines. Local styles
  `BasedOn` the theme's; the chosen-record panel uses `EnabledWhenChosen` (`EditorTheme.xaml`).

## Changes
- `Editor/Shell/MainViewModel.Database.cs` (88): `SkillEditing` and a private `ReadSkillChoices()` (the icons and
  effects folders' images through `ArtFolders.ImagesIn`, the audio effects through `AudioFolders.FilesIn`, the
  draft's effect strip records, read when the page opens, as `ReadCinematicMedia`).
- `Editor/Shell/MainViewModel.cs` (286): `SkillEditing = new SkillEditingViewModel(Content, ReadSkillChoices);` after
  `EffectStripEditing` (+1).
- `Editor/Shell/MainWindow.Database.cs`: one opener registered under "Skills": `OpenInDialog("Skills",
  skillEditing.Start, skillEditing.Apply, edited => new SkillCatalogPage(edited))`, returning when `SkillEditing` is
  null.
- `Editor/Shell/DatabaseMenu.xaml`: "Skills..." (`Tag="Skills"`) after "Effect strips...", tooltip "Each skill's name,
  texts, icon, sound, own strip, costs, effect and statuses; classes and enemies still list theirs in their files."
- `.claude/scripts/open-editor-windows.ps1`: `'Database>Skills...'` in the default `-Windows` list (decision 996).
- `docs/map.md`: an Editor `Skills` row.

## Tests
None new (draw and wiring): `SkillCatalogViewModelTests` holds the rules (page recipe, step 7).

## Verify by hand
1. `dotnet run --project src/Jrpg.Editor`, Database > Skills...: the page at the main window's size, the skills in id
   order; choose `fireball`: Name Fireball, Strip `FireExplosion.png` with its pace line, "Learned by: Mage".
2. Rename it "Fire Ball", OK, File > Save all: `git diff content/skills.json` shows the name only. Ctrl+Z, Save all.
3. Add: the problem line asks for an id; type `test_skill`, a name, pick a sound: the line clears and OK lights.
   Remove it; Esc.
4. Choose `heal` (it needs faith but spends nothing) and tick Quick: the problem line shows Data's message ("isQuick
   needs a resource cost in resourceChanges..."), OK greys; untick it and the line clears.
5. Run `.claude/scripts/open-editor-windows.ps1`: "Database>Skills...: opened", no `ERROR` line and no binding-error
   line.
