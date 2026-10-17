---
paths:
  - "content/**/*.json"
  - "src/Jrpg.Data/**/*.cs"
---

# Rules for game content

- Every entity has a stable string `id` (`"potion_small"`, `"slime_green"`). IDs never change once used; rename the display name instead.
- References between files use IDs, never array indexes.
- Every content type has a loader + validator in Jrpg.Data. The validator checks: unknown IDs, duplicate IDs, out-of-range numbers, missing required fields.
- A content validation test loads all of `content/` and must pass.
- Balance numbers are game-design decisions: propose changes with reasoning, don't silently retune values.
