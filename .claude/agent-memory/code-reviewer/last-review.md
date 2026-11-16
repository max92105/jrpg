---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-enemy-behavior, slice EB10 (once per battle: TacticMemory.HasUsed/NoteUsed,
TacticMemoryNotes.NoteOwnTurn, EnemyAction.IsOncePerBattle, EnemyTactics.IsUsedUp, Data validator + AbsentWhenFalse
row, editor check box; TacticRuleRowTests split). 2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no
Warning).

- Suggestion (open, optional): EnemyOncePerBattleTests ChooseCommand_OnceRuleInAPhaseAfterUseBeforeThePhase - assert
  the member's Execute returned an EnemyPhaseEntered for the actor; without it the test also passes if the phase is
  never entered (the base list's once cleave is used up too).
- Suggestion (open, optional): TacticMemory.HasUsed/NoteUsed have no summary while NoteReaction/ForgetReactions do;
  one line "by skill id, through any rule (decision 842)" on HasUsed.
- EB9 carried: TacticConditionShape summary now names IsReaction (fixed); early-return suggestion in
  TacticMemoryNotes.Note dropped (declined, NoteOwnTurn now holds the enemy's part).
- Checked, no finding: every SkillCommand that reaches Execute is noted, a miss, a backfire and a ReadyExtraTurn skill
  included (UseSkill throws before any event on an unusable skill, so Execute never notes a refused one); a quick
  skill notes before the same turn's next pick; the split moved all 11 sentence/notice tests + the theory unchanged;
  writer fixture Power 5 is decision 787's rule (EnemyCatalogValidator:33) and touches no other assertion; the
  AbsentWhenFalse row is covered by ContentWriterRepositoryTests' identical-bytes test.
