---
name: retro
description: Look back over a finished plan (or the last few slices) and propose improvements to rules, skills, agents and docs, and prune agent memory. Run only when the user explicitly asks for /retro.
argument-hint: "[plan folder or file]"
---

Retrospective on: $ARGUMENTS (default: the plan most recently finished or in progress in `docs/plans/README.md`).

Read only these, not the code:
- the plan's slice notes ("Done, with deviations"), decisions and follow-ups;
- the agents' memory: `.claude/agent-memory/*/`;
- `git log --oneline` for the plan's commits, and `git diff --stat` over them for file sizes;
- `docs/map.md`, `CLAUDE.md`, `.claude/rules/`, `.claude/skills/`, `.claude/agents/` (to see what already exists).

Then propose, and wait for my OK before changing anything:
1. **Repeated deviations or review findings** (the same kind 2+ times) → a rule in `.claude/rules/`, a reviewer checklist line, or a hook if it must hold every time. One or two concrete lines each.
2. **Workflow friction** (steps that cost many turns or tokens, re-reads, slices that had to be split, questions asked too late) → a change to a skill or agent prompt, as an exact edit.
3. **Drift in the docs**: folders or key types missing from `docs/map.md`, ADRs missing from `docs/decisions/README.md`, the plan's row in `docs/plans/README.md`. Fix these directly; they need no OK.
4. **Code health**: files over ~300 lines, code copied 3+ times, follow-ups left open → a list for the next cleanup plan, one line each.
5. **Memory pruning**: notes now recorded in code, `docs/map.md`, an ADR or a rule, or about issues since fixed → list them for deletion.
6. **Finished plan** → move it to `docs/plans/done/` and update the index.

Report as one short list per heading; leave empty headings out. Keep each proposal to what would have saved real time or tokens in this plan.
