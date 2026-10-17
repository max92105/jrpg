---
name: adr
description: Record an architecture decision (ADR) with options, trade-offs and the chosen outcome.
argument-hint: "<decision topic>"
disable-model-invocation: true
---

Write an Architecture Decision Record about: $ARGUMENTS

1. Find the next number in `docs/decisions/` (format `NNNN-short-title.md`). Use `docs/decisions/_template.md` as the structure.
2. Fill in context from this conversation and the code. List at least 2 real options with honest pros/cons for a solo developer.
3. If the decision hasn't been made yet, set `Status: proposed`, give your recommendation, and ask me to decide. Don't mark it accepted on my behalf.
4. If it supersedes an older ADR, update the older one's status to `superseded by NNNN`.
5. Keep it under one page.
