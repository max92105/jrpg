---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 5b (Save files)
Round: 1 (verdict: Fix the Criticals, then commit)

Accepted (don't re-raise): JsonNode pre-parse for formatVersion then strict deserialize; null list entries refused as damaged; .tmp + File.Move(overwrite); SaveJsonOptions duplicates content options on purpose (+WriteIndented); UnreadableSaveException with SlotName/Reason; Reason embeds STJ message (plan: "naming the slot and path").

- open (Critical): FileSaveStore.ParseObject - duplicate key ({"formatVersion":1,"formatVersion":1,...}) throws ArgumentException from JsonObject, escaping the UnreadableSaveException contract (verified with fsi); pass JsonDocumentOptions { AllowDuplicateProperties = false } to JsonNode.Parse + test
- open (Warning): Read_MissingRequiredField_ThrowsNamingTheField renames playTime->playtime, so it hits the unmapped-member path; the missing-required path is untested
- open (Warning): CheckNoNullEntries has 5 clauses, only Inventory tested; theory per list
- open (Suggestion): WriteThenRead_ReturnsEquivalentSave lacks Method_ prefix (plan: FileSaveStore_WriteThenRead_...)
- open (Suggestion): Deserialize's `?? throw "doesn't hold a save"` unreachable (root is a JsonObject)
- open (Suggestion): Reason doc says "words the Game can show the player" but damaged reasons carry raw STJ text
