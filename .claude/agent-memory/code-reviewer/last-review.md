---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skills-and-statuses-model, slice SR1 (statuses.json: StatusDefinition/StatusPart/StatusPartKind,
StatusCatalogValidator, ContentCatalog.Statuses, loader reads statuses.json first, write-back test). 2026-11-15.
Round 1. Verdict: Ready to commit.

- Suggestion (open, optional): ContentCatalogLoader.cs:62 "Read first:" above effects.json is no longer first since
  statuses.json is read above it; say "Read before the skills and items:".
- Suggestion (open, optional): no test for one family's records with the same kinds in another order (the validator's
  SequenceEqual is order-sensitive by decision; pin it).
- Checked: content matches design 3.3 (ten records, families, amounts); no Core.Statuses -> Core.Content cycle;
  enum converter refuses integers, so FindAmountProblem's default arm is unreachable from JSON; Amount absent reads 0
  and the validator refuses it for ticks.
- T4: no findings carried (Soft/Flat stun suggestion was optional, dropped).
