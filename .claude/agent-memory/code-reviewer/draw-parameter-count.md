---
name: draw-parameter-count
description: Game views keep growing Draw(batch, textures, place, area, opacity) to 5 parameters, past the max-4 rule; count parameters on every changed signature
metadata:
  type: feedback
---

Draw signatures with 5 parameters slipped through B3 (`ShopListView.Draw`) and the forge plan (`ForgeListView.Draw`); L2b added two more (`ShopDetailView.Draw` moved Area out of its Place, `ShopView.Draw` took list + detail).

**Why:** `.claude/rules/code-quality.md` caps parameters at 4; verify.ps1 doesn't enforce it, so only the reviewer catches it.

**How to apply:** count parameters on every new or changed method signature in the diff. The usual fix: a fixed area goes to the view's constructor (the host's areas are static), or the per-frame values join one `Place` record. Older 5-parameter Draws are follow-ups.
