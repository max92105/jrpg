# 0047. Animated battle sprites

Status: accepted
Date: 2026-11-14

## Context
The developer added animated GIFs for five monsters (a gnoll archer, a gnoll axe warrior, a gnoll warrior, a gnoll
shaman and a hyena warlord boss) and a fire explosion, and wants each monster's idle loop to play in battle. The game
reads art as PNGs at runtime with no GIF decoder (ADR 0009, which already says "the GIF attack effects become PNG strips
of square frames"); Core can't touch images and Data has no imaging; no NuGet package is added without asking; there is
no Python on this machine. An animation already has a shape in content: a frame count and one frame time (ADR 0039),
reused by field sprites' walks (ADR 0043). Enemies are edited in the Enemies window (ADR 0045). Plan
`2026-11-14-animated-monsters`, decisions 631-642.

## Options
### A. The editor converts a GIF into a PNG strip at import; the enemy record declares the loop (chosen)
- Pros: the game stays PNG-only (ADR 0009); WPF decodes GIFs with no package; the scale, the background and the timing
  are chosen once, in the tool the enemy is set up in; the loop is set in the Enemies window's own session (one undo
  step); the field reuses ADR 0039's shape and `FrameAt`.
- Cons: GIF composing is ours to write and test; two enemies sharing a strip each set the loop (Duplicate copies it); a
  party member's loop would be the same field again.

### B. A per-image record, `battle-sprites.json`, as field sprites (ADR 0043)
- Pros: the layout belongs to the image, shared by enemies and, later, party members.
- Cons: a new catalog with its loader, writer, draft setter and a nested window (or a combined undo step) in the
  Enemies window: about three slices more for one field.

### C. GIFs decoded at runtime
- Pros: no conversion step.
- Cons: needs a package or our own LZW decoder and frame composer in the game (Core can't, Data has no imaging); the
  editor must decode them too; GIF quirks at every start.

### D. Convert outside the editor (a script or an image tool), then today's PNG import
- Pros: nothing new in the editor's import; any tool the developer likes.
- Cons: a manual step per GIF and per change of size; the frame count and time typed by hand from what the tool said;
  no repository script can decode GIFs without a package or Python; the editor still needs the loop's boxes.

## Decision
A: on import, the editor composes a GIF's frames whole into one PNG strip (frames side by side, one row) in `battlers/`
or `effects/`, scaled to a battle height in screen pixels (default 128, editable per import), and the enemy record
declares its loop as `"idleAnimation": { "frameCount": 8, "frameMilliseconds": 100 }`, absent for a still sprite
(decisions 631, 637). One frame time per loop: the GIF's most common delay, a delay under 20 ms read as 100 ms, shown
in a box the developer can change, the import saying when the GIF's delays differed (decision 632).

## Consequences
- The game and Data stay PNG-only: a GIF never ships, and the source GIFs live in `art-src/gifs/`; the strips
  imported from them are listed as `generated` art (ADR 0038).
- An animated battler draws at its frame's own size; a battler without a loop keeps decision 100's scaling, so every
  PNG already imported draws as before.
- The startup check gains a strip check: an enemy's strip must hold its declared frames side by side.
- Per-frame times are out of scope (one frame time per loop, as ADR 0039); a GIF whose delays vary plays evenly.
- An image wider than 4096 px or a GIF of one frame is refused at import, the message naming why.
- A party member's loop, when it comes, is the same field on `PartyMemberDefinition` (decision 641).
