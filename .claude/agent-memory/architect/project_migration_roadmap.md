---
name: migration-roadmap
description: Migration and code-health plan both done (2026-10-23); next decision/ADR numbers; open questions; what the next plans are
metadata:
  type: project
---

The migration from the WPF demo is finished (tag wpf-demo-final). The code-health plan is done too (2026-10-23, moved to `docs/plans/done/2026-10-23-code-health/`); what it left waits in `docs/plans/backlog.md`, "When next touched".

Numbers: next question/decision 156, next ADR 0016. Open questions: 147, 150-155 (content party size, enemy count, rail controls, sidebar placement, reserve look, switch buttons), 92, 112.

Next plans are new systems: cutscenes (needs an ADR), map banner (shares the banner band), autosave (shares "load latest"), event battles, gold/drops, animated battlers, NPC movement, audio, loading screen.

**Why:** the developer wanted the code healthy, and their menu feedback addressed, before new features; that is now done.

**How to apply:** reuse the vocabulary the code-health plan added (CombinedHistory, FieldSpriteLayout, CategoryRail, MenuPointer, IMemberPage, NumberText, ColumnTitles, MeasuredText, FooterBar, OutcomeFrame); check in `docs/map.md` that each still exists. See [[content-quirks]], [[plan-lessons]].
