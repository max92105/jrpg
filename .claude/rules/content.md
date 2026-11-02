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
- A new or edited content file is laid out as `ContentWriter` writes it (one field per line in nested objects, sorted ids, CRLF, trailing newline), so the identical-bytes test passes the day the writer takes it over (quests QE1 rewrote `quests.json`).
- Placeholder music comes from `.claude/scripts/new-placeholder-tune.pl`, never plain held tones: the developer has to live with it until the real music.
