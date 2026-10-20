---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B3a.md, B3a (party status: VitalsText, PartyStatusView/Panel, BattleVitalRow, VitalRowStyle, OutlinedFont, StatBar track)
Round: 1 (verdict: Ready to commit; verify all pass, 1172 tests)

- open (Suggestion): BattleVitalRow.Show(string value, string max, int current, int maximum) pairs near-synonym names; rename to valueText/maxText (or value/maximum ints) so a swap is visible.
- open (Suggestion): PartyStatusPanel ctor takes a (Hp, Mp) tuple to dodge the 4-param limit; a panel-style record (row styles + labels) passed once would read straighter.
- open (Suggestion): VitalsText's MP/MaxMp change path untested (only HP); one MP-changed case would cover MaxIfChanged's second use.
- follow-up: UiTheme.Danger's doc still says "a save that failed"; now also a fallen member's HP.
