---
name: plan-feature
description: Plan a feature as small vertical slices, then have the plan challenged before any code is written. Run only when the user explicitly asks for /plan-feature.
argument-hint: "<feature description>"
---

Plan this feature: $ARGUMENTS

1. Delegate to the **architect** subagent with the feature description. It writes a plan file in `docs/plans/`.
2. Delegate to the **devils-advocate** subagent, giving it the plan file path. Ask for its verdict.
3. Show me, in this order:
   - the slice list (one line each),
   - the devil's advocate Blockers and Concerns (one line each),
   - the open game-design questions I need to answer.
4. Propose the specific edits you'd make to the plan to address the Blockers/Concerns, but **do not apply them and do not write any code**. Wait for my answers and approval.
5. Once I approve, update the plan file (set `Status: approved`) with my decisions.
