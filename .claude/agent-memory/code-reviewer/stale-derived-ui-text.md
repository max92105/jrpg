---
name: stale-derived-ui-text
description: Editor notes/labels computed at an action (a pick's note) aren't reset when the state they describe changes
metadata:
  type: feedback
---

A note or label computed once at an action (ObjectPicking.Note in UO1 and UO2) keeps describing state that has since
changed: the picked object moved (UO1), the lock unticked or the layer changed (UO2).

**Why:** flagged twice in a row in the map-fixes plan; the stale text tells the developer something false.

**How to apply:** for each new text a view model sets at an action, list the inputs it reads (selection, layer, lock,
map) and check each one's change resets or recomputes it. Missing reset = Warning with a concrete "after X, shows Y".
