# 0048. Battle effect strips as records

Status: accepted
Date: 2026-11-14

## Context
An attack plays an effect strip of square frames (decision 57): today every strip plays at a fixed 0.08 s a frame
(`EffectTiming.FrameTime`), is drawn at a fixed size and starts at the impact. The developer's reference clip shows a
slash that lands its hit on a chosen frame, an impact strip with its own pace, and a swing played before the impact. The
same strip is shared by many attackers: `AttackEnemy.png` is named by 13 records today. Plan
`2026-11-14-battle-turn-presentation`, decisions 654-659.

## Options
### A. A record per strip image, `effect-strips.json` (frame time, hit frame, swing file) (chosen)
- Pros: facts about the art live with the art, once for a strip 13 records name today; another strip (the explosion)
  lines up its own hit; the swing belongs with its impact; edited in one Database window.
- Cons: a catalog with its loader, check, writer and window.

### B. Fields on each attacker, ADR 0047's shape (the idle loop on the enemy record)
ADR 0047 put the idle loop on the enemy because a loop belongs to that enemy's sprite, one owner; an effect strip
differs: one strip is shared by many enemies and party members.
- Pros: no new catalog.
- Cons: a strip's pace copied on each attacker, 13 times today, and drifting apart.

### C. Game constants per slot (swing pace, impact pace, impact lead)
- Pros: no data.
- Cons: art facts in code; every impact strip shares one hit frame; an art change is a code change.

## Decision
A: `content/effect-strips.json` gives a strip, by its file name in `effects/`, its frame time, the frame its hit lands
on and the swing strip it plays first (decision 654); a strip it doesn't list plays at 80 ms a frame (decision 57's
value) with its hit on frame 0 and no swing. The swing is named by the attacker's impact strip's record, not per
attacker (decision 657).

## Consequences
- `EffectTiming.FrameTime` becomes the unlisted pace of a strip without a record (`EffectStripDefinition`), read by the
  game and the editor's GIF import alike.
- The startup check covers each listed strip and swing: the image is in `effects/`, its hit frame within its frames.
- The records get an editor (Database > Effect strips..., ADR 0045's shape); nothing refers to a record by id, so its
  undo is never refused.
- A strip's look on screen (its size, its hit lining up with the impact, the swing) is the game's, from the record.
