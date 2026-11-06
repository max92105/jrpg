---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

No plan: the music hand-check fix after 955774dc (decision 496, ADR 0035 amended 2026-11-06): fade out before the next
fades in, loop flag before Play, the title cycles intro/loop/outro. Round 1 (2026-11-06). Verdict: ready to commit.

- Open (Warning): MusicPlayer.Wanted/TrackOf miss the mix's next track, so a fade-out rebuilds the MusicTrack every frame
  (2 File.Exists + 2 concats per update for 1 s); the "read once" comment is false. Fix: expose the mix's next, look it up.
- Open (Warning): a cycling track's part that won't load stops the cycle for good (StartPart leaves no instance,
  FollowParts skips it); the old loop fallback is gone. Also a duplicate _cycling entry on Begin after such a failure.
- Open (Warning): CyclesParts' two sides (intro only, outro only) untested; `&&` passes every test (tests.md rule).
- Open (Suggestion): Parts' spread-of-ternaries expression; README index dates the amendment 2026-11-05.
- Seen and gone: the titlescreen loop.wav -> main.wav rename was in the tree at the start, reverted before the end.
