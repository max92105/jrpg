---
name: detached-comments
description: Inserting or removing a member next to a `//` or `///` comment leaves the comment on the wrong member
metadata:
  type: feedback
---

When a diff adds a member or test just above an existing helper, check the line above the insertion: a `//` comment or
`///` block that described the helper now sits above the new code (seen in MO2b, InventoryPageTests; the user also
keeps a memory rule "insert above doc comments"). The reverse also happens: a removed or moved member leaves its comment
behind on the next member (seen in D1: BattleState's AttackTargeting const moved to AttackEffect, its comment stayed
above `_turnOrder`; and in A4a: a new test inserted between a comment and Roll_NotEquipment_Throws merged the old
comment into the new test's).

**Why:** the comment then misdescribes the new member; a misleading comment counts as Critical (cheap fix).
**How to apply:** in `git diff`, look at the context line right before each `+    [Fact]` / new member, and right after
each removed `-` member: a context `//` line with no `-` below it is orphaned.
