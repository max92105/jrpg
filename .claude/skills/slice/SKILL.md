---
name: slice
description: Implement the next unchecked slice of an approved plan, with tests, then verify and review it.
argument-hint: "[plan file path] [slice number]"
disable-model-invocation: true
---

Implement one slice. Arguments: $ARGUMENTS

1. Find the plan: use the path given, otherwise the most recent file in `docs/plans/` with `Status: approved`. If there is none, stop and tell me to run `/plan-feature` first.
2. Pick the slice: the number given, otherwise the first unchecked `- [ ]` slice. Restate its goal in one sentence.
3. Implement **only that slice**. Write the tests in the same change (test first when the behavior is a pure function or formula).
4. Run `dotnet build Jrpg.slnx --nologo -v q` and `dotnet test Jrpg.slnx --nologo -v q` until both are clean with zero warnings.
5. Re-read your own diff against `.claude/rules/code-quality.md` before handing it to the reviewer: rename anything vague, delete anything unused or speculative, remove comments that narrate.
6. Delegate to the **code-reviewer** subagent. Fix every Critical finding. List Warnings you chose not to fix and why.
7. Tick the slice in the plan file and add a one-line note for any deviation from the plan.
8. Stop. Show me: what changed (files), test results, review outcome, and a proposed commit message. **Do not commit**; I commit myself.

If the slice turns out bigger than planned or the plan is wrong, stop at step 2 or 3 and tell me instead of improvising.
