---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-13-drop-table-editor, slice DT0 (decision 604: TypedNumber to Catalogs, EnabledWhenChosen to
EditorTheme.xaml, CatalogDraft.HasSameRecords). 2026-11-13. Round 1. Verdict: ready to commit. No Critical/Warning.

- Suggestion (open): EditorTheme.xaml:146 comment and map.md Shell row say "Database window's", but the
  Character sprites window (FieldSpriteWindow) using the style sits in the NPCs menu -> "a catalog window's".
- Suggestion (open): `(current, edited) => current == edited` written twice (CatalogDraft.Database.cs:18,
  CatalogDraft.cs:157) -> EqualityComparer<T>.Default.Equals.
- Suggestion (open): DT0.md Done note "Window.Resources with them, left empty" reads as kept-but-empty; they're removed.
- Carried from the editor bug fixes (decisions 594/595): all fixed in commit b4c8df6c before it landed: an import with a bad guess no longer counts (it never greys OK); FieldSpriteWindow keeps itself on the desktop once loaded (KeepOnDesktop over the virtual screen); map.md Sprites row wording fixed. Don't carry them.
