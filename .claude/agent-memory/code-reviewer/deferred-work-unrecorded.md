---
name: deferred-work-unrecorded
description: a Done note that moves work to/from another slice often leaves that slice's file unchanged; grep the target slice
metadata:
  type: feedback
---

When a slice's Done note defers a planned piece to a later slice (or pulls one in early), check the other slice's
file says so. Seen twice in the loot plan: S1a pulled the unknown-affix drop from S1b without updating S1b.md; G1c
deferred `InventoryRow.Piece` to P1a, whose file doesn't mention it.

**Why:** the later slice gets planned/implemented from its own file; unrecorded work is silently dropped or done twice.
**How to apply:** grep the named slice file for the deferred member; if absent, raise a one-line Follow-up.
