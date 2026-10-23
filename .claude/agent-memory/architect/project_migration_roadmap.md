---
name: migration-roadmap
description: The WPF demo migration is finished (tag wpf-demo-final); the code-health plan was approved 2026-10-23; next decision/ADR numbers; what the next plans are
metadata:
  type: project
---

The migration from the WPF demo is finished (2026-10-23).

The code-health plan `docs/plans/2026-10-23-code-health/` was approved on 2026-10-23 with 17 slices: F1, F2, F4, F5a, A1, A2, P1, P2, N1, X1, E1a, E2, E3, U1, U2, U3, U6. The rest went to `backlog.md`, "When next touched".

Developer answers recorded there:
- 145/146: the sidebar lists active and reserve members and marks the edited member.
- 148: the max-level victory card shows "Max" and a full bar.
- 149: the official party size is 5.

Numbers: next question/decision 156, next ADR 0016. Open questions: 147, 150-155 (content party size, enemy count, rail controls, sidebar placement, reserve look, switch buttons), 92, 112.

Next plans are new systems: cutscenes (needs an ADR), map banner (shares the banner band), autosave (shares "load latest"), event battles, gold/drops, animated battlers, NPC movement, audio, loading screen.

**Why:** the developer wants the code healthy, and their menu feedback addressed, before new features.

**How to apply:** check this plan's status before planning the next feature, and reuse the vocabulary it adds (CombinedHistory, FieldSpriteLayout, CategoryRail, MenuPointer, IMemberPage, NumberText, ColumnTitles, FooterBar, OutcomeFrame). Verify that they exist in code first. See [[content-quirks]], [[plan-lessons]].
