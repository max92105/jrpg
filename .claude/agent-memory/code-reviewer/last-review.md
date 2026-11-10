---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice MF1 (MapFolderPath, MapSettings.Folder, validator, settings
window Folder box), 2026-11-10. Round 1. Verdict: ready to commit.

- Open (Warning): MapFolderPath.cs:11 the one Rule message omits the backslash check, so "Act 1\Village" in content
  or the Folder box is refused with a message the path seems to meet. Name it in the message (or a second message).
- Checked fine: ToSettings copies every field (TryApplySettings uses it); trim forgiven only at the box's ends, inner
  names still strict; "/" and "/Act 1" refused through the empty name.
- RN3 items: OnMapListRightButtonDown removed (fixed); ContentWriter.RenameMap delete follow-up is in backlog.md
  (dropped).
- start.json residue: the developer's hand test, excluded by the caller.
