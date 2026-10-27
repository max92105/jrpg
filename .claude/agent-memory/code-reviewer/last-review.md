---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-26-stats-and-gear-slots, slice W1b (Will raises Faith's max, lowers Instability's gains). Round 1. Verdict: Ready to commit.

- Warning: ResourceStarts.StrainedAmount uses source.Max (definition), not the Will-raised max; validator now allows raiseMax on a strain source (clarity), giving a negative lowering that raises the strained start above its max — open
- Suggestion: PartyMember.Rest / HasAnythingToRest summaries still say "starting amount"/"off its start" — open
- Suggestion: SaveGameRepair `var maxHp = sheet.MaxHp;` local only forwards; inline sheet.MaxHp — open
