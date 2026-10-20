---
name: plan-feature
description: Plan a feature as small vertical slices, then have the plan challenged before any code is written. Run only when the user explicitly asks for /plan-feature.
argument-hint: "<feature description>"
---

Plan this feature: $ARGUMENTS

1. Delegate to the **architect** subagent with the feature description and any decisions already taken in this session. It writes a plan folder in `docs/plans/` (`README.md`, `design.md`, `slices/`; see `docs/plans/README.md`).
2. Delegate to the **devils-advocate** subagent, giving it the plan folder path. Ask for its verdict, findings as Blockers / Concerns / Nitpicks.
3. Show me, in this order:
   - the slice list (one line each),
   - the devil's advocate Blockers and Concerns (one line each),
   - the open game-design questions I need to answer.
4. Propose the specific edits you'd make to the plan to address the Blockers/Concerns, but **do not apply them and do not write any code**. Wait for my answers and approval.
5. Once I approve, have the **architect** (continue the same agent, it has the plan in context) apply the edits and my decisions: decisions in the README's "Decisions", answered questions removed from "Open design questions", slices moved or split as needed, `Status: approved`, and the row in `docs/plans/README.md` updated.
