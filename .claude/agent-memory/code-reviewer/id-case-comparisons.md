---
name: id-case-comparisons
description: Map/content ids are file names (case-insensitive on Windows); every id comparison in a rename/create path must agree on case
metadata:
  type: feedback
---

Ids that are also file names must be compared without case at every check of one path, not just some.
Seen twice: RN1 (MapIdRename's taken check was case-sensitive: "forest" -> "Forest" overwrote then deleted files) and
RN2 (MapRenamingViewModel excluded its own id without case but checked "already this map's" with case, so a older
upper-case id passed the dialog and Data's case-insensitive backstop threw an ArgumentException TryWrite doesn't catch).

**Why:** a case-only mismatch between the editor's check and Data's backstop is a crash or data loss on Windows.

**How to apply:** for each new id check, list every comparison of that id in the editor VM and in Data, and confirm
they use the same comparer; try a case-only rename of an upper-case older id (loaders don't force lower case).
