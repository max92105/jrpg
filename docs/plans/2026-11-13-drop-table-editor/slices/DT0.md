# DT0. Shared pieces first: `TypedNumber`, the chosen-record panel style, `HasSameRecords`

Editor, about 120 lines moved or folded, none new. Design: "Structure and folder dependencies", key decision 6. Decision 604.

## Goal
No behavior changes: `TypedNumber` sits where a second feature folder can read it without reading `Battle`; the Database windows' shared panel style lives in the theme before a fifth window copies it; `CatalogDraft`'s "same by content" check is written once before a fourth setter needs it. DT1's diff is then only behavior.

## New types
None.

## Changes
- `Editor/Battle/TypedNumber.cs` → `Editor/Catalogs/TypedNumber.cs`, namespace `Jrpg.Editor.Catalogs`; its summary unchanged. Usings: `Battle/EnemyViewModel.cs`, `EnemyViewModel.Rewards.cs`, `EnemyViewModel.Resistances.cs`, `ResistanceRow.cs` and `tests/Jrpg.Editor.Tests/Battle/EnemyViewModelTests.cs` gain `using Jrpg.Editor.Catalogs;` where missing. Rebuild until no IDE0005 remains (CLAUDE.md).
- The `EnabledWhenChosen` panel style (greys the chosen record's panel while none is chosen) is declared in four windows' resources (`Trade/ShopCatalogWindow`, `Battle/EnemyCatalogWindow`, `Sprites/FieldSpriteWindow`, `Quests/QuestCatalogWindow`, found by Grep on `x:Key="EnabledWhenChosen"`). It moves once into `Shell/EditorTheme.xaml` (145 lines) with its comment, and the four copies go (the code-quality rule's third copy, overdue).
- `Editor/Catalogs/CatalogDraft.Database.cs`: `private static bool HasSameRecords<T>(IReadOnlyDictionary<string, T> before, IReadOnlyDictionary<string, T> after, Func<T, T, bool> isSame)` (the same count, and every record of `after` found in `before` and the same): `SetShops` (`isSame` record equality), `SetEnemies` (`IsSameEnemy`) and `SetQuests` (in `CatalogDraft.cs`, which shrinks) call it in place of their three written-out checks.
- `docs/plans/backlog.md`: the "six editor boxes parse a whole number their own way" line names `Catalogs/TypedNumber`.
- `docs/map.md`: the Editor `Battle` row stops naming `TypedNumber`; the `Catalogs` row names it ("a whole number as a form keeps it") and `HasSameRecords` stays private (not listed); the `Shell` row's `EditorTheme.xaml` mention adds "the chosen-record panel style".

## Tests
None new. Guard: the test totals of `Jrpg.Editor.Tests` before and after are equal, and every test passes unchanged, `CatalogDraftQuestTests`, `CatalogDraftShopTests` and `CatalogDraftEnemyTests` above all (their "equal by content records nothing" and "changed is one step" tests cover both sides of `HasSameRecords`).

## Verify by hand
1. `git diff --stat` shows the moves, the usings and the three setters shrinking.
2. `dotnet run --project src/Jrpg.Editor`. Open **Database > Quests...**, **Shops...**, **Enemies...** and **NPCs > Character sprites...** in turn: each shows its right-hand panel lit with a record chosen, as before (a missing resource would throw on opening).
3. In **Database > Shops...**, change a shop's name and click **OK**, then **Ctrl+Z**: the name comes back; reopen and click **OK** with nothing changed, then **Ctrl+Y**: the rename comes back (an empty step would have cleared the redo; `HasSameRecords` on both sides).

- **Done:** as planned. The four windows' copies of the panel style go with their comment (the Quests and Shops windows' `Window.Resources`, then empty, removed too); the two setters in `CatalogDraft.Database.cs` and `SetQuests` call `HasSameRecords`; test totals unchanged (4966).
