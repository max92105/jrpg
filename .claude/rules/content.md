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
- Every file under `content/art/` has an entry in `content/art/art-sources.json` (a source with its author and license, and a `files` list; ADR 0038), and every source a section in `content/art/CREDITS.md`, written by hand; `ArtSourcesTests` reads the repository's folder and fails on a file without an entry. Placeholder sources (`isPlaceholder`) list what must be replaced before release.
- Bought art packs (Mana Seed, ELV Games) stay outside the repository (`C:\git_perso\art-packs\`, decision 439): only the sheets a map uses are imported, through the editor's Import, each with its manifest line.
- Agents never open a Mana Seed image, nor a capture or screenshot that shows one: the Mana Seed license refuses AI-related use, the developer accepted the risk (decision 438) and this is the free mitigation. Work from file names, sizes and the developer's descriptions.
- Mana Seed sheets are cut at 16 x 16 px (the Set up guess tries 16 first); tiles are square, since the map draws each into a square cell, and a bigger prop (the 80 x 112 trees, the 16 x 32 and 48 x 32 props) is an object built from cells.
