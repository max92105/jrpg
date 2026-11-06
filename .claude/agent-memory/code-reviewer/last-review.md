---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-map-editing-flows, slice OM4 (Replace the map cells with the new object), round 1 (2026-11-06); the plan
is done and retro'd (2026-11-06).
Verdict: ready to commit (no Criticals).

- Fixed: a test erases a layer over the base (the ground chosen erases level 1 too); the OM3 "both warnings" test.
- To the backlog (not carried): the ObjectFromMap impossible-state guards, the `Composing?.`/`Composing!` mix, ErasedCells'
  double pass.
- Rules now (not carried): impossible-state guards, typed double IsFinite and the window key guard are in
  `.claude/rules/editor.md`; the blank line before an added test is verify.ps1's check 7c.
