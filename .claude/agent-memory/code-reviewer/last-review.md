---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice MS4 (Save all, the close question, SaveChangesWindow,
MainViewModel.Saving partial), round 1 (2026-11-05). Verdict: ready to commit.

- Open (Suggestion): SaveAll_SecondMapRefused test puts the refused map last, so deleting the `break` in SaveAll
  still passes; refuse forest and paint start validly, assert start stays dirty.
- Open (Suggestion): EditorTestContent.Open(root, closeAnswer) has an empty `<param name="contentRoot"></param>`.
- Open (Suggestion): SaveAll_NewMapOpen_WritesIt... never reads the file back; its MapIds assert holds before the save.
