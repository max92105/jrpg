---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-shops-and-enemy-database, slice EN6 (Editor/Battle: EnemyArtViewModel, BattleArtChoices on the catalog
with one folder lookup, BattleArtImport with the folder given; choices on the catalog and the lookup as a constructor
parameter are developer decisions, don't re-raise). 2026-11-13. Round 1. Verdict: ready to commit.

- EN3 Critical (Rewards partial covering resistances/IsBoss): fixed (.Resistances partial split out).
- EN3 Suggestions (DropTableChoices per get, ResistanceRow.Problem notify, Resistances get-only): fixed.
- Warning (open): BattleArtImport is the third copy of the import loop (SheetImport, FieldSpriteImport); backlog
  line records folding FieldSpriteImport onto it; developer's call.
- Suggestion (open): EnemyArtViewModel.Apply(enemy) reads ambiguously; ApplyTo(enemy) or WrittenOver(enemy).
- Suggestion (open): MissingOf on a new enemy (all names "") yields "battle sprite " lines with a trailing blank;
  say "battle sprite (none)" or let EN7 show empty boxes differently.
- Suggestion (open): BattleArtChoices tests live in EnemyArtViewModelTests; move to BattleArtChoicesTests.
