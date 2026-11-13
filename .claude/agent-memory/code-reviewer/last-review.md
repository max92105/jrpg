---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice SI5 (NewNpcViewModel over set-up sheets, ForChange; NpcSpriteChange;
NewNpcWindow walking preview; SpriteImageFile; Change NPC sprite menu and button). 2026-11-12. Round 1.
Verdict: ready to commit.

- Warning (open): MainViewModel.FieldSprites.cs:26-27 SortedFieldSprites and NpcSpriteChange.cs:48-49 SortedSheets are
  the same sort, twice, and both dead: NewNpcViewModel.cs:40-44 re-sorts the files itself; pass the values unsorted.
- Suggestion (open): NewNpcViewModel.cs:71 "has no record" branch untested and unreachable from the editor's flows.
- Suggestion (open): MainWindow.xaml Change sprite... button tooltip lacks the "Objects &gt; ..." form of its row.
- SI4 items: static _lastCells fixed; CreateFieldSpriteEditing rename fixed; row-box -1 fixed (RowIndexConverter);
  per-tick Problem fixed (_previewSheet cached); SI2 spare-pixels follow-up now in backlog (dropped).
