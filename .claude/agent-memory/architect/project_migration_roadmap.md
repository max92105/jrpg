---
name: migration-roadmap
description: Plan status after the migration, code-health and new-game flow plans; next decision/ADR numbers; open questions; next plans
metadata:
  type: project
---

Migration, code-health and new-game flow plans are done (the new-game flow on 2026-10-24, moved to `docs/plans/done/`); leftovers wait in `docs/plans/backlog.md`.

The new-game flow built: a title showing only usable entries; nine leader choices with a choice screen; the leader starting alone on an `intro` map whose arrival event (gated on that leader) plays stills, a voice and a warp, as a map cutscene (ADR 0016, not a slideshow content type); a RecruitStep map event (placeholder recruit NPCs on the start map); class stats by level (ADR 0017, `ClassGrowth`); voices as WAV through MonoGame `SoundEffect` (ADR 0018, `VoiceChannel` written by scenes, played by the host). Next free decision 188, next ADR 0019. Open questions from it: 179-183.

Still-open older questions: 147, 150-155, 92, 112. Later plans: more cutscene steps, map banner, autosave (shares "load latest"), event battles, gold/drops, animated battlers, NPC movement, music/SFX and volume, loading screen, a map sprite per leader (question 58), the story itself.

**Why:** the developer asked for real features after the cleanup; game design decisions stay theirs.

**How to apply:** continue numbering from 188/0019; reuse the vocabulary in `docs/map.md`. See [[content-quirks]], [[plan-lessons]].
