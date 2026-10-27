---
name: detached-comments
description: New members/tests inserted between an existing comment and the member it describes, leaving the comment on the wrong one
metadata:
  type: feedback
---

When a diff adds a member or test just above an existing helper, check the line above the insertion: a `//` comment or
`///` block that described the helper now sits above the new code (seen in MO2b, InventoryPageTests; the user also
keeps a memory rule "insert above doc comments").

**Why:** the comment then misdescribes the new member; a misleading comment counts as Critical (cheap fix).
**How to apply:** in `git diff`, look at the context line right before each `+    [Fact]` / new member.
