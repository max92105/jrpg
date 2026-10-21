---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B5c.md, B5c (FloatingNumber/Look, FloatingNumbersView, ActionPlayback numbers + fallen + clock, white silhouette fade)
Round: 1 (verdict: Ready to commit; verify all pass, 1234 tests)

- open (Warning): FloatingNumbersView places numbers at the formation spot (SpotOf/TopOf), not the stepped spot; a self-targeted heal on the stepped-out actor pops 50 px beside its sprite.
- open (Suggestion): B5c.md deviation note is cut off mid-sentence ("drawn over the sprite at 60").
- open (Suggestion): Update_FallenEnemy_FadesAfterTint asserts InRange(0.01, 0.99); pin 0.75 at 1.5 s and 0 at 2.0 s.
- open (Suggestion): fall-opacity expression `IsAlive ? 1 : Playback?.FallOpacityOf(c) ?? 0` copied in BattlersView and EnemyTagView; one BattleTurnSequence.OpacityOf(c) would hold it.
- follow-up: CommandPanelView target pointer also uses the unstepped spot for a self-target (pre-existing).
