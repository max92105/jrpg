---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice SV2 (refused Ctrl+S points to Save all, decision 519), 2026-11-10.
Round 1. Verdict: ready to commit.

- Open (Warning): MainViewModel.Saving.cs:54 hint appended after any refusal; a read-only/locked file (IO error) or a
  catalog-validator refusal with another map dirty gets "Save all ... makes this valid", Save all fails the same way.
  Fix: hint only when CheckMaps fails on the Ctrl+S content and passes on the open maps.
- Open (Suggestion): MainViewModel.Saving.cs:10/39-40 file and Save summaries still cite decisions 499(-501), 518 only;
  docs/map.md Shell line too (519 missing).
- Open (Suggestion): ContentWriterMapsTests CheckMaps_ValidContent_WritesNothing cannot fail on its bytes asserts
  (static, no root); its real claim is the deadlock content passes: rename MapsValidOnlyTogether_DoesNotThrow-ish.
- Open (Suggestion): Save_RefusedWithNoOtherMapChanged also fails PassTogether, so it doesn't pin the others.Count==0
  guard; the IO-refusal case would.
- SV1 items all fixed (SaveTogether rename, test comment, duplicate test removed, map.md 518, backlog WriteMaps).
