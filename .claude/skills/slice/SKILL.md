---
name: slice
description: Implement the next unchecked slice of an approved plan, with tests, then verify and review it. Run only when the user explicitly asks for /slice.
argument-hint: "[plan path] [slice id]"
---

Implement one slice. Arguments: $ARGUMENTS

Read only what the slice needs: the plan's `README.md`, the slice's own file, and `docs/map.md`. Open `design.md`, ADRs (`docs/decisions/README.md` first) and code only for what those don't answer.

1. **Find the plan**: the path given, otherwise the plan listed as in progress in `docs/plans/README.md`. If none is approved, stop and tell me to run `/plan-feature`. For an older single-file plan, read its header and only the slice's section (find it with Grep), never the whole file.
2. **Pick the slice**: the id given, otherwise the first unchecked `- [ ]` in the plan's slice index. Read its file in `slices/`. Restate its goal in one sentence. If the working tree has uncommitted changes from another slice, say so before starting.
3. **Implement only that slice**, tests in the same change (test first for a pure function or formula). Follow `docs/map.md`'s patterns and reuse what it lists before writing anything new.
4. **Verify** with `powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/verify.ps1 -Fix` until every line passes. Don't run the `dotnet` commands directly unless one failure needs more detail.
5. **Self-review** the diff against `.claude/rules/code-quality.md`: rename anything vague, delete anything unused or speculative, remove narrating comments. Note any file now over ~300 lines and any method copied a third time.
6. **Record before review**: in the slice file add `- **Done, with deviations:** ...` (one or two lines, or "as planned"). If the slice added a folder or a type other code will build on, update its line in `docs/map.md`.
7. **Review**: delegate once to the **code-reviewer** with a short prompt: the slice file path, the plan README path, a 5-10 line summary of the change, decisions the developer took this session, "Build, tests and format pass", and "The plan checkbox is ticked after review; don't report it." Don't paste the plan or the diff. Fix every Critical; fix the Warnings that are cheap and in scope and list the rest with one line each on why not. Re-run the reviewer only if a Critical fix changed the design.
8. **Tick** the slice in the plan's index, and update the deviation note if the review changed anything. If it was the plan's last slice, set the plan's status to `retro pending` in its README and in `docs/plans/README.md` (it moves to `done/` only after `/retro`, or when I explicitly say to skip it).
9. **Stop and report**, in this shape and no longer:
   - one sentence: what the slice does now;
   - **Changed:** files, one line each;
   - **Checks:** the verify script's lines (tests count included);
   - **Review:** Criticals fixed / Warnings fixed / left with why;
   - **Deviations:** or "none";
   - **Commit message** (proposed). **Do not commit**; I commit myself.
   - after the plan's last slice only: "Last slice of the plan: run `/retro`, or tell me to skip it."

If the slice turns out bigger than planned (past ~300 lines with tests) or the plan is wrong, stop at step 2 or 3 and tell me instead of improvising.
