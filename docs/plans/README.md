# Plans

Feature plans from `/plan-feature`, worked through one slice at a time with `/slice`. Status goes `approved` → `approved, in progress` → `retro pending` (every slice done) → `done` (after `/retro`, or when the developer skips it), and the plan then moves to `done/`.

Code-health work found along the way (big files, copies, open follow-ups) waits in [backlog.md](backlog.md) for the next cleanup plan.

**Next free numbers:** decision 374, ADR 0031. A new decision or ADR takes its number from this line and bumps it in the same change, so two plans never share a number.

**Layout of a plan** (from the battle plan on): a folder `YYYY-MM-DD-name/` with
- `README.md`: status, goal, order, guiding rules, out of scope, the slice index (one line and a checkbox per slice), the developer's decisions, open questions, ADRs needed. Read for every slice.
- `design.md`: tunables, design overview (what exists, Core changes, structure, key decisions, replacement mapping) and risks. Read when a slice points to it.
- `slices/<id>.md`: one slice's goal, types, tests, hand checks, and once done its "Done" or "Done, with deviations" note.

Older plans are single files with the same sections.

| Plan | Status |
|---|---|
| [2026-10-27-forge](2026-10-27-forge/README.md) | approved (next) |
| [done/2026-10-27-affix-stats](done/2026-10-27-affix-stats/README.md) | done |
| [done/2026-10-27-loot](done/2026-10-27-loot/README.md) | done |
| [done/2026-10-26-stats-and-gear-slots](done/2026-10-26-stats-and-gear-slots/README.md) | done |
| [done/2026-10-26-escape-menu-third-pass](done/2026-10-26-escape-menu-third-pass/README.md) | done |
| [done/2026-10-25-ui-second-pass](done/2026-10-25-ui-second-pass/README.md) | done (retro run on the other machine) |
| [done/2026-10-24-class-mechanics](done/2026-10-24-class-mechanics/README.md) | done |
| [done/2026-10-23-new-game-flow](done/2026-10-23-new-game-flow/README.md) | done |
| [done/2026-10-23-code-health](done/2026-10-23-code-health/README.md) | done |
| [done/2026-10-22-demo-retirement](done/2026-10-22-demo-retirement/README.md) | done |
| [done/2026-10-21-editor](done/2026-10-21-editor/README.md) | done (session check Z skipped) |
| [done/2026-10-20-menu-cleanup](done/2026-10-20-menu-cleanup/README.md) | done |
| [done/2026-10-20-battle](done/2026-10-20-battle/README.md) | done |
| [done/2026-10-18-game-title-menu-movement](done/2026-10-18-game-title-menu-movement.md) | done (check Z folded into 2026-10-22-demo-retirement) |
| [done/2026-10-18-shell-cutover-and-map-editing](done/2026-10-18-shell-cutover-and-map-editing.md) | done |
| [done/2026-10-17-world-exploration-core](done/2026-10-17-world-exploration-core.md) | done |
| [done/2026-10-17-core-migration](done/2026-10-17-core-migration.md) | done |
