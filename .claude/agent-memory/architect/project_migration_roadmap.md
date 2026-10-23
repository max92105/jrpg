---
name: migration-roadmap
description: Plan status after the migration and code-health plans; next decision/ADR numbers; open questions; next plans
metadata:
  type: project
---

Migration and code-health plans are done (2026-10-23); leftovers wait in `docs/plans/backlog.md`, "When next touched".

Current plan: `docs/plans/2026-10-23-new-game-flow/` (approved 2026-10-23, revised by the main session after the architect hit a usage limit): title hides unusable entries; 9 leader choices (Teelle Paladin, Thaali Druid, Kamilia removed); leader starts alone, others recruited via a RecruitStep map event; D&D-like classes give stats by level (ADR 0017, `ClassGrowth`); items have no class restriction; intro slideshow with subtitles and one placeholder voice. Decisions 156-178, open 179-184, next free 185; ADRs 0016 cinematics, 0017 class stats, 0018 audio; next ADR 0019.

Design choices made there (check the ADRs once written): the intro is a slideshow content type (`cinematics.json`), not the map cutscene system; audio is WAV via MonoGame `SoundEffect`, scenes write a plain `VoiceChannel` and the host plays it (update path stays device-free); `SceneFactory` owns the channel because GameFlow/SceneFactory ctors are at 4 params.

Still-open older questions: 147, 150-155, 92, 112. Later plans: map cutscenes (own ADR), map banner, autosave (shares "load latest"), event battles, gold/drops, animated battlers, NPC movement, music/SFX, loading screen.

**Why:** the developer asked for real features after the cleanup; game design decisions stay theirs.

**How to apply:** continue numbering from 185/0019 unless the plan's numbers were renumbered; reuse the vocabulary in `docs/map.md`. See [[content-quirks]], [[plan-lessons]].
