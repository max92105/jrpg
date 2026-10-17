---
name: qa
description: Hunt for bugs and missing edge cases in the recently changed code by writing tests. Run only when the user explicitly asks for /qa.
argument-hint: "[area to focus on]"
---

Delegate to the **qa-engineer** subagent. Focus area (optional): $ARGUMENTS

When it returns:
- List the tests it added.
- For every bug it found (tests named `..._Bug`), explain the bug in one line and propose the minimal fix. **Don't apply fixes** until I say so.
- If the subagent touched anything outside `tests/`, tell me and revert that part.
