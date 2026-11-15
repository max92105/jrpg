---
name: migration-roadmap
description: Where to find plan status and numbers; the game's direction
metadata:
  type: project
---

Plan status lives in `docs/plans/README.md`, each plan's decisions and ADRs in its own README, and leftovers in
`docs/plans/backlog.md`; the history of finished plans is `docs/plans/done/`, not this note.

Direction (developer, 2026-11-03, still true on 2026-11-13): every system is in; the work is content and art in the
Mana Seed family (16 px art drawn 3x at the 48 px cell), the packs kept outside the repository, and editor tools that
let the developer write the introduction and the first quest line without opening JSON (characters and NPC animation,
shops, quest NPCs, enemies are done). Balancing stays in JSON until the schema settles; class and skill editors, the
tactics forms and an Encounters window are backlog or Later.

Since 2026-11-15 the direction is an editor program: `docs/plans/2026-11-15-editor-push/README.md` holds the ranked
roadmap (approved order: safety net, a skill's own strip, event window, cinematic focus, sprite cells/mirrors, skills
and statuses model, their editors, enemy behavior, items/uniques/affixes, encounters/battle backgrounds, mode bar,
classes and progression last); the developer wants both story and battle sides before building again. New editors are
modal windows until the mode bar row converts them. Plan the next row from its line and design.md section.

**Why:** dated plan-by-plan history went stale here while `docs/plans/README.md` stayed right.

**How to apply:** read the plans index for what is next; ask the developer for direction beyond the above. See
[[plan-lessons]].
