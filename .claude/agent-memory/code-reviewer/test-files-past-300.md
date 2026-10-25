---
name: test-files-past-300
description: Slices keep appending cases to big test files until they pass ~300 lines (ActionPlaybackTests in DR1, ClassCatalogValidatorTests in DR2)
metadata:
  type: feedback
---

Test files grow past ~300 lines because each slice appends to the existing class's tests instead of opening a file per concern.
Seen: ActionPlaybackTests (DR1, 281 -> 318), ClassCatalogValidatorTests (DR2, 288 -> 320, resource cases).

**Why:** the Growth checklist item covers test files too; the developer has had this Warning twice in a row.
**How to apply:** run `wc -l` on every modified test file and compare with `git show HEAD:<file> | wc -l`; the
Warning proposes the split (for example, resource-definition validator cases in their own file).
