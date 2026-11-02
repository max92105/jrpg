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
- Placeholder music and the victory and defeat jingles come from `.claude/scripts/new-placeholder-tune.pl`, never plain held tones: the developer has to live with them until the real music (decision 429).
- Placeholder sound effects come from freesound.org (the developer's instruction of 2026-11-02; decisions 426, 427, 437; ADR 0036): CC0 first, CC-BY when nothing CC0 fits, never NC, SA or Sampling+, and never an upload that names or imitates a commercial game or film. Each is an entry in `content/audio/sound-sources.json`, converted by `.claude/scripts/fetch-placeholder-sounds.ps1`, which also writes `content/audio/CREDITS.md`. Find candidates with `.claude/scripts/find-freesound.pl` and write an entry from its page with `.claude/scripts/add-sound-source.pl`. Never edit a converted WAV by hand. Any later placeholder sound comes from there too.
