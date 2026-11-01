---
name: detached-comments
description: Inserting or removing a member next to a `//` or `///` comment, or a docs/map.md parenthetical, leaves the comment on the wrong member
metadata:
  type: feedback
---

A member inserted just above an existing one takes that one's comment (MO2b, A4a, G3, S4, I3, quests SV: a helper inserted under a helper's comment); a member removed or moved leaves its comment on the next member (D1); a type inserted into a `docs/map.md` entry between a type and its parenthetical takes the parenthetical (I2a). Three times in the gold plan alone, so the `/slice` skill's self-review now names the check; still look.

**Why:** the comment then misdescribes the new member; a misleading comment counts as Critical (cheap fix).

**How to apply:** in `git diff`, look at the context line right before each `+` member or `[Fact]` and right after each removed `-` member: a context `//`, `///` or `(...)` there with no `-` below it is orphaned. In `docs/map.md`, read every changed entry whole.
