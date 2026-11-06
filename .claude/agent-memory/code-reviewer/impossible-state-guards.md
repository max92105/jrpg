---
name: impossible-state-guards
description: Editor view-model code guards states a modal window or menu enabling rules out (null map, missing layer), skipping silently
metadata:
  type: feedback
---

The developer's Editor code often adds `is { } x` / `FirstOrDefault` + null-filter guards for states that can't occur
(the map can't change while a modal composer is open; a menu entry is disabled without content), and the guard makes a
bug silently do nothing instead of failing.

Seen: OM3 `StartObjectFromMap` `CurrentMap is not { } shown`; OM4 `ErasedCells` layer `FirstOrDefault` + `is not null`.

**Why:** checklist's AI-slop item (defensive checks for impossible states); a silent skip hides the bug a test would
catch.
**How to apply:** for each new null guard in Shell/Objects view models, ask what makes the state reachable; if nothing,
propose `Single`/`!` and a Warning (Suggestion if it's only the `?.`/`!` mix).
