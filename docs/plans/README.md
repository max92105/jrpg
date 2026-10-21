# Plans

Feature plans from `/plan-feature`, worked through one slice at a time with `/slice`. Status goes `approved` → `approved, in progress` → `retro pending` (every slice done) → `done` (after `/retro`, or when the developer skips it), and the plan then moves to `done/`.

Code-health work found along the way (big files, copies, open follow-ups) waits in [backlog.md](backlog.md) for the next cleanup plan.

**Layout of a plan** (from the battle plan on): a folder `YYYY-MM-DD-name/` with
- `README.md`: status, goal, order, guiding rules, out of scope, the slice index (one line and a checkbox per slice), the developer's decisions, open questions, ADRs needed. Read for every slice.
- `design.md`: tunables, design overview (what exists, Core changes, structure, key decisions, legacy mapping) and risks. Read when a slice points to it.
- `slices/<id>.md`: one slice's goal, types, tests, hand checks, and once done its "Done, with deviations" note.

Older plans are single files with the same sections.

| Plan | Status |
|---|---|
| [2026-10-21-editor](2026-10-21-editor/README.md) | approved |
| [done/2026-10-20-menu-cleanup](done/2026-10-20-menu-cleanup/README.md) | done |
| [2026-10-20-battle](2026-10-20-battle/README.md) | approved, in progress |
| [2026-10-18-game-title-menu-movement](2026-10-18-game-title-menu-movement.md) | approved; every slice done, play-through check Z left |
| [done/2026-10-18-shell-cutover-and-map-editing](done/2026-10-18-shell-cutover-and-map-editing.md) | done |
| [done/2026-10-17-world-exploration-core](done/2026-10-17-world-exploration-core.md) | done |
| [done/2026-10-17-legacy-core-migration](done/2026-10-17-legacy-core-migration.md) | done |
