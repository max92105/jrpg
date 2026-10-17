---
name: lesson
description: Turn a mistake or correction from this session into a permanent rule in the right place.
argument-hint: "<what went wrong or what to remember>"
disable-model-invocation: true
---

Something should be remembered permanently: $ARGUMENTS

Decide where it belongs, then propose the exact text (one or two lines) and wait for my OK:

- Applies to the whole project, every session → `CLAUDE.md` (keep it under ~150 lines; if near the limit, propose what to remove).
- Applies only to one area (Core, Game, Editor, tests, content) → the matching file in `.claude/rules/`.
- It's a multi-step procedure → a new or existing skill in `.claude/skills/`.
- It must happen **every time without exception** (e.g., "never edit X") → propose a hook instead, since instructions can be ignored but hooks can't.
- It's a review pattern → add it to the checklist in `.claude/agents/code-reviewer.md`.

Write rules as concrete, checkable statements ("Use `IRandom`, never `new Random()` in Core"), not vague advice ("be careful with randomness").
