# GT2. `CommandMenuTests` split by menu entry over a `CommandMenuDriver`

Game tests, about 50 new lines, about 170 moved, no production change. Builds on decision 556 and design.md, "Risks" (GT2's fixture).

## Goal
`tests/Jrpg.Game.Tests/Battle/CommandMenuTests.cs` (393 lines) keeps Attack, Defend, Run and Items; the Skills tests and the Move tests get their own classes, and the press helpers all three use live in one static driver, as the Shop tests' `ShopDriver` does.

## New types
- `tests/Jrpg.Game.Tests/Battle/CommandMenuDriver` (static, about 25 lines): presses on a given `CommandMenu`. Holds `Press(menu, button, times)` and `SelectEntry(menu, entry)` (bounded by the entries, so a list without the entry fails the test instead of looping), moved from `CommandMenuTests` with their comments.

## Changes
- New `tests/Jrpg.Game.Tests/Battle/CommandMenuSkillTests.cs` (about 130): Zanyr's battle and bag built in its constructor as `CommandMenuTests` builds them (five lines; decision 556: no shared instance). Its class comment says Zanyr "knows her class's skills, in order: Heal, Recovery, Heal All". Moved: `IsEnabled_ActorWithoutSkills_GreysSkills`, `Update_ConfirmSkills_ListsActorSkills`, `Update_ConfirmMeteorWithThreeMana_SaysNotEnoughMana`, `Update_ConfirmSelfSkill_ProducesCommandOnActor`, `Update_AllAlliesSkill_ProducesCommandOnEveryLivingMember`, `Update_CancelOnSkillList_ReturnsToCommands`, `Update_CancelOnSkillTargets_ReturnsToSkillList`, `Update_ConfirmReady_TargetsOthersButNotSelf`, with `OpenSkills`, `OmersSkillList`, `RecoveryRow`, `HealAllRow`, and `SelectSkill` (private here: only skill tests call it).
- New `tests/Jrpg.Game.Tests/Battle/CommandMenuMoveTests.cs` (about 70): `Update_ConfirmMove_TargetsOnlyNeighbors`, `Update_ConfirmMoveTarget_ProducesMoveCommand`, `Update_CancelOnMoveTargets_ReturnsToCommands`, with `TwoMembersMenu` and its comment.
- `tests/Jrpg.Game.Tests/Battle/CommandMenuTests.cs` (393 → about 200): the rest, with `Own`, `PassTheGoblinsTurns`, `OpenItems`; its class comment keeps "Zanyr acts first", "the bag starts empty" and the seed note; its private `Press` and `SelectEntry` go to the driver.
- `docs/plans/backlog.md`: delete the MO1 line.

Where a test's class is unclear, it goes with the helpers it calls. The mechanical edit: `Press(b, times: n)` → `CommandMenuDriver.Press(_menu, b, times: n)`, `SelectEntry(entry)` → `CommandMenuDriver.SelectEntry(_menu, entry)`, `SelectEntry(menu, entry)` → `CommandMenuDriver.SelectEntry(menu, entry)`; nothing else in a moved body.

## Tests
None new. The runner's totals per project equal before and after; the Done note lists the eleven moved names and the three classes' line counts.

## Verify by hand
None: tests only.
