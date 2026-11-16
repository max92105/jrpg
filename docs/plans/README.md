# Plans

Feature plans from `/plan-feature`, worked through one slice at a time with `/slice`. Status goes `approved` → `approved, in progress` → `retro pending` (every slice done) → `done` (after `/retro`, or when the developer skips it), and the plan then moves to `done/`.

Code-health work found along the way (big files, copies, open follow-ups) waits in [backlog.md](backlog.md) for the next cleanup plan.

**Next free numbers:** decision 1002, ADR 0063 (706-1000 and ADRs 0051-0062 are reserved for the editor push's sub-plans of 2026-11-15). A new decision or ADR takes its number from this line and bumps it in the same change, so two plans never share a number.

**Layout of a plan** (from the battle plan on): a folder `YYYY-MM-DD-name/` with
- `README.md`: status, goal, order, guiding rules, out of scope, the slice index (one line and a checkbox per slice), the developer's decisions, open questions, ADRs needed. Read for every slice.
- `design.md`: tunables, design overview (what exists, Core changes, structure, key decisions, replacement mapping) and risks. Read when a slice points to it.
- `slices/<id>.md`: one slice's goal, types, tests, hand checks, and once done its "Done" or "Done, with deviations" note.

Older plans are single files with the same sections.

| Plan | Status |
|---|---|
| [2026-11-15-editor-push](2026-11-15-editor-push/README.md) | approved (the roadmap of twelve sub-plans, and sub-plan 1: the editor's safety net, NET1; NET2 optional) |
| [2026-11-15-skill-and-status-editors](2026-11-15-skill-and-status-editors/README.md) | approved, in progress (the editor push's row 7; its balance table runs after row 12) |
| [2026-11-15-items-uniques-affixes](2026-11-15-items-uniques-affixes/README.md) | approved (the editor push's row 9) |
| [2026-11-15-classes-and-progression](2026-11-15-classes-and-progression/README.md) | approved (the editor push's row 12) |
| [2026-11-15-enemy-behavior](2026-11-15-enemy-behavior/README.md) | approved, in progress (the editor push's row 8) |
| [2026-11-15-encounters-and-backgrounds](2026-11-15-encounters-and-backgrounds/README.md) | approved (the editor push's row 10) |
| [2026-11-15-mode-bar](2026-11-15-mode-bar/README.md) | approved, in progress (the editor push's row 11: the dialog pages WD1-WD8 run before row 7, the mode bar WS1-WS6 at its rank) |
| [2026-11-15-skills-and-statuses-model](2026-11-15-skills-and-statuses-model/README.md) | retro pending (the editor push's row 6) |
| [2026-11-15-event-window-cleanup](2026-11-15-event-window-cleanup/README.md) | retro pending (the editor push's row 3) |
| [2026-11-15-sprite-cells-and-mirrors](2026-11-15-sprite-cells-and-mirrors/README.md) | retro pending (the editor push's row 5) |
| [2026-11-15-cinematic-focus-point](2026-11-15-cinematic-focus-point/README.md) | retro pending (the editor push's row 4) |
| [2026-11-15-skill-effect-strips](2026-11-15-skill-effect-strips/README.md) | retro pending (the editor push's row 2; SE1-SE3 done) |
| [2026-11-14-battle-turn-presentation](done/2026-11-14-battle-turn-presentation/README.md) | done |
| [2026-11-14-animated-monsters](done/2026-11-14-animated-monsters/README.md) | done |
| [2026-11-14-cinematics](done/2026-11-14-cinematics/README.md) | done |
| [2026-11-13-drop-table-editor](done/2026-11-13-drop-table-editor/README.md) | done |
| [2026-11-12-character-sprites-and-npcs](done/2026-11-12-character-sprites-and-npcs/README.md) | done |
| [2026-11-12-shops-and-enemy-database](done/2026-11-12-shops-and-enemy-database/README.md) | done |
| [2026-11-11-code-health-cleanup](done/2026-11-11-code-health-cleanup/README.md) | done |
| [2026-11-11-game-code-health-cleanup](done/2026-11-11-game-code-health-cleanup/README.md) | done |
| [2026-11-10-map-fixes-banner-and-folders](done/2026-11-10-map-fixes-banner-and-folders/README.md) | done |
| [2026-11-05-map-editing-flows](done/2026-11-05-map-editing-flows/README.md) | done |
| [2026-11-05-free-levels-and-picker-width](done/2026-11-05-free-levels-and-picker-width/README.md) | done |
| [done/2026-11-05-object-layers-and-sheet-zoom](done/2026-11-05-object-layers-and-sheet-zoom/README.md) | done |
| [2026-11-03-object-composer-and-animated-tiles](done/2026-11-03-object-composer-and-animated-tiles/README.md) | done |
| [done/2026-11-03-mana-seed-reset](done/2026-11-03-mana-seed-reset/README.md) | done (the maps painted after it, as content work) |
| [done/2026-11-02-sound-pass](done/2026-11-02-sound-pass/README.md) | done |
| [done/2026-10-29-music-and-sfx](done/2026-10-29-music-and-sfx/README.md) | done |
| [done/2026-10-29-enemy-skills-and-bosses](done/2026-10-29-enemy-skills-and-bosses/README.md) | done |
| [done/2026-10-29-quests](done/2026-10-29-quests/README.md) | done |
| [done/2026-10-29-gold-and-shops](done/2026-10-29-gold-and-shops/README.md) | done |
| [done/2026-10-27-forge](done/2026-10-27-forge/README.md) | done |
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
