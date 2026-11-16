---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-enemy-behavior, slice EB3b (second commit of EB3: Editor/Battle/TacticRuleRow, TacticRuleRowTests,
map.md Battle row, EB3.md Done note). 2026-11-16. Round 1.
Verdict: Ready to commit (no Critical, no Warning).

- EB3a's open item (`Of` without caller or test): fixed, TacticRuleRow.From calls both and the From test pins them
  with Assert.Same.
- Suggestion (open, optional): the reflection theory Problem_NumberBoxTyped_IsRaisedWithTheSentence could be two
  plain facts; judged acceptable (public property, nameof-guarded).
- Suggestion (open, optional): Sentence prints NumberText raw, so an empty or space-padded box reads "below  %";
  trimming in WhenWords would tidy it (the Problem line already names the empty box).
- Checked, no finding: Problem order weight then percent; hidden box never written (percent 0); From's
  `Percent: > 0` pinned by From_KindWithoutNumber; fixtures differ from New's defaults; every relay tested; CopyOf
  never calls ToAction (editor.md); NumberUnit's throwing arm is the enum switch's required default.
