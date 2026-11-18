# 0062. Skills learned at a level

Status: Accepted (default taken while the developer was away; his to overturn)
Date: 2026-11-17

## Context
ADR 0020 gives every member of a class all the class's skills (`classes.json` `skillIds`, in order) from the moment
they join, and its consequences foresee "learning skills by level" joining the same list through `PartyMember.Skills`.
Row 12 of the editor push names "skills learned by level" beside the Classes page, and the classes and progression plan
(`docs/plans/done/2026-11-15-classes-and-progression/`) builds them in its phase LV, before the Classes page's Skills tab
(CL6), so the tab has its Level column from the start.

Constraints:
- Nothing changes in play until the developer types a level: today's nine classes keep every skill from level 1.
- The Classes page edits a class's skills, their menu order and their levels in one place.
- Data catches at load a level that could never be learned, naming the class and the skill, not in play.
- No save format change (decision 943); nothing random, no wall clock.
- Row 7's readers of a class's whole list (Data's checks, the balance table, the Skills page's "Learned by" line) read
  `ClassDefinition.SkillIds` and should not change.

The developer delegated the plan's approval, so this rule was taken while he was away; it is the first of the plan's
"Defaults to confirm".

## Options
The plan's design.md, section 5, holds the shape, the rule and the options set aside.

### A. An optional level on each skill entry (chosen)
`skillIds` becomes `skills`, a list of `{ "skillId", "level" }` with `level` left out at 1.
- Pros: one list holds the menu order and the levels together, so nothing has to be kept in step; the Skills tab is one
  row per entry with a level box; an absent level is level 1, so the relaid file plays as today; `SkillIds` stays as a
  computed reading, so its readers don't change.
- Cons: the file's shape changes once (a relay commit with a layout diff, LV1); the test builders that write `SkillIds`
  (`ClassBuilder`, about ten sites, and row 7's fixtures) move to `Skills`. About 660 lines over three slices (LV1-LV3).

### B. A `skillLevels` map beside `skillIds`
- Pros: `skillIds` and its readers stay exactly as they are; a class with no levels writes nothing new.
- Cons: two structures to keep in step; a level naming a skill the list lacks needs its own refusal; the Skills tab
  reads two places for each row.

### C. A separate level-up table per class
- Pros: the level-ups read apart from the class's kit.
- Cons: a third structure the Classes page would edit beside the class, with its own check against the class's skills;
  the menu order and the levels live in two places.

Not taken either (the plan's "Defaults to confirm", item 1): skills bought or found. ADR 0020's question 228 (a skill
tree per class, skills granted by items) stays open.

## Decision
Option A (decisions 959, 963-966 and 970; options B and C set aside in design.md, section 5).
- **The shape (959).** A class's skill entry is `{ "skillId", "level" }`, the level a whole number from 1, left out of
  the file at 1 (`ClassDefinition.Skills`, a list of `Party/ClassSkill`); `skillIds` goes. `ClassDefinition.SkillIds`
  stays as a computed reading of every entry's id in order, left out of the file. LV1 relays `classes.json` with every
  level 1.
- **The rule (963, default taken while the developer was away; his to overturn).** A member knows each class skill
  whose level is at or below its own, in the class's order; a level up adds the skills of the levels reached; a member
  recruited or loaded at a higher level knows every skill up to it. An entry without a level is known from level 1.
  `PartyMember` resolves its class's entries at recruit (`Party/SkillsByLevel`) and refreshes `Skills` on a level up
  and at `RestoreProgress` (LV2). A battle builds its combatants from `Skills` when it starts, so a skill learned at its
  end is usable from the next battle.
- **The victory line (964, default taken while the developer was away; his to overturn).** The victory screen names
  each skill learned, "Omer learned Meteor.", one line per skill under the experience bars, shown once that member's bar
  has reached the level (`battle_skill_learned` in `ui.json`); `BattleRewards` fills `ExperienceGain.LearnedSkills`
  (`Battle/LearnedSkill`) from the levels the steps reached (LV2, LV3).
- **Data's checks (965).** A level below 1 is the class validator's message. A level past the experience table's max
  level could never be learned: `ClassSkillLevelCheck` refuses it, naming the class and the skill, run by the loader
  once the rules are read and by the writer and the Classes and Experience pages' problem lines.
- **Saves (966).** No save change: the skills a member knows come from its class and level at recruit, on a level up
  and at load.
- **Two readings (970).** The game's readers of a member's skills (the battle's Skills menu, the Status page, the leader
  profile) read the known ones, `PartyMember.Skills`; the readers of a class's whole list (Data's checks, row 7's
  balance table and "Learned by" line) read `ClassDefinition.SkillIds`.

## Consequences
- **Save format:** no change, `formatVersion` stays (decisions 943 and 966). Saves already hold a member's id, level,
  experience, HP, gear and lasting amounts, never skills (ADR 0007, ADR 0020); the known skills are derived at load, so
  a level typed into a class after a save applies to that save's members when it loads.
- **Easier:** a class's kit can unfold over the levels, typed on the Classes page's Skills tab (CL6); the menu order and
  the levels are edited together.
- **The developer will notice:** nothing changes in play until a level is typed; a skill learned at a battle's end is
  usable from the next battle; a member recruited above level 1 knows every skill up to its level, with no victory line
  for them.
- **The two readings to keep apart:** code asking what a member can use goes through `PartyMember.Skills` (ADR 0020's
  rule, kept); code asking what a class teaches reads `ClassDefinition.SkillIds`. Row 7's balance table shows a class's
  skills at every level, learned or not; a "learned at" mark there is a backlog follow-up.
- **Watch for:** lowering the experience table's max level below a class skill's level is refused on the Experience
  page's line (decisions 956 and 965), since that skill could then never be learned.
- If the developer overturns the rule: leaving every level out of the file restores ADR 0020's play exactly; another
  shape (option C) changes LV1 and CL6, while LV2's rule and LV3's line stay.
- Amends ADR 0020: a member's skills are the class's skills at or below its level, not all of them; the class stays
  their source and members still hold none; `classes.json` `skillIds` becomes `skills`.
