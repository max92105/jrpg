---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice CL1 (classes.json written by the writer: CatalogProblems.Party
OfClasses/CheckClasses, SortedClasses, resource fields in the four AbsentWhen filters, decision 1011 empty gains;
content/classes.json relaid, semantically identical: only explicit "startsAt": 0 dropped). 2026-11-17.
Round 1. Verdict: Ready to commit.

- Suggestion: AbsentWhenEmptyFilter.cs cites decision 944 for the `gains` entry (summary and inline comment); the
  rule is decision 1011 — open
- Suggestion: the same 12-line "Note from CL1" (page recipe) copied into CL4, PM4, NG3, XP3; one copy in README — open
- WS6 Warning (drop-tables test on the only table) — fixed (targets "vault")
- WS6 Suggestion (ADR 0060 "strip Edit... same path") — fixed
