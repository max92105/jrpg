# 0059. Animated battle backgrounds as grid sheets

Status: Accepted (default taken while the developer was away; his to overturn)
Date: 2026-11-16

## Context
A battle background is one still PNG today, drawn cover-fit over the screen (`Ui/CoverFit`). The developer wants
animated backgrounds he makes from GIFs. Decision 680 keeps the game PNG-only: no GIF or video is read at runtime, so
the editor turns a GIF into a PNG at import, as for an animated enemy (ADR 0047). A full-screen frame is 1920 px wide
and `TextureLimit.MaxSide` is 4096, so a strip of full-size frames holds two. `Rendering/TextureLibrary` decodes on
first `Get` and caches forever, and the 4096 px menu background's decode (over 200 ms) once broke the menu's fade, so
a background's video memory and decode time matter. Constraints: determinism (time from the update's delta, never a
clock); content is the only source of truth (ADR 0011); no save change. The developer delegated the plan's approval,
so this choice was taken while he was away.

## Options
Costs for 16:9 frames, RGBA in video memory; decode time estimated from the one measured point in the code (the 4096 x
4096 menu background, about 12 ms a megapixel). The plan's design.md ("Animated backgrounds") holds the full table.

### A. A grid sheet at a reduced frame size, scaled up by the game (chosen)
Frames left to right then top to bottom in one PNG, at a frame height below the screen's, scaled up by the game.
- Pros: one texture and one decode per background; a 24-frame loop costs 5.5 MB and about 17 ms at 180 px frames
  (3840 x 360, up to 264 frames), 12.4 MB and about 40 ms at 270 px (3840 x 810, up to 120), 22.1 MB and about 70 ms
  at 360 px (3840 x 1440, up to 66), 49.8 MB and about 150 ms at 540 px (3840 x 3240, up to 28); pixel-art GIFs
  (typically 180 to 360 px tall) import at their own height, untouched.
- Cons: painted art loses resolution (a 540 px frame is shown at 2x); a frame count ceiling per frame size.

### B. A folder of full-size frame PNGs, one texture each
- Pros: no frame count limit; full resolution for painted art.
- Cons: about 8.3 MB of video memory and 25 ms of decoding per 1920 x 1080 frame: 24 frames are 199 MB and about
  600 ms of decoding, a visible stall; four to sixteen times the memory of option A; about one more slice.

### C. A strip at a reduced size
- Pros: the enemy strip's shape (ADR 0047), no row rule.
- Cons: at most 8 frames at 480 px wide (270 px frames, 12.4 MB, about 40 ms); can't hold a 24-frame loop.

### D. A grid at full size
- Pros: full resolution, one texture.
- Cons: at most 6 frames (2 x 3, 49.8 MB, about 150 ms); can't hold a 24-frame loop.

### E. A video file
- Cons: refused by decision 680 (the game stays PNG-only); a decoder dependency in the Game.

## Decision
Option A (decision 896): an animated battle background is one grid sheet PNG at a reduced frame size, frames left to
right then top to bottom, scaled up by the game; not a folder of full-size frames, not a strip.
- **The record (897).** In a new `content/animated-backgrounds.json`, by file name: `file`, `frameWidth`,
  `frameHeight` and `animation` (`frameCount` from 2, `frameMilliseconds` from 1: ADR 0039's shape, as an enemy's
  `idleAnimation`). A background without a record is a still picture, drawn as today. The columns are the image's
  width over the frame width, so a hand-laid grid of any column count works; `AnimatedBackgroundDefinition.FrameOrigin`
  is the one rule for where a frame sits.
- **The heights (898).** (default taken while the developer was away; his to overturn) The import offers the GIF's own
  height first (capped at 1080), then each of 1080, 540, 360, 270, 216 and 180 below it (the 1080 px screen's whole
  divisors, so a 16:9 frame is scaled up by a whole factor and point sampling stays even). The default is the GIF's own
  height when its sheet holds every frame, else the largest divisor below it that does; when none does, the problem
  line says so. Nothing is ever scaled up at import.
- **The time (899).** The battle draws the background's frame for the time since the battle screen opened, from the
  update's delta (never a clock), from frame 0 each battle, looping, through the outcome screens; cover-fit over the
  screen, point-sampled, under the vignette and the battlers as today.
- **The decode (900).** On entering a map the game decodes the backgrounds its battles can show (its own and its
  encounters' own), under the warp's black, not on the battle's first frame, and releases the previous map's that the
  new map doesn't list (`TextureLibrary.Release`), never the title's or the menu's background. The GIF dialog shows
  each sheet's video memory before it is written.
- **The shrink (904).** A background GIF is never cleared of a background color (backgrounds are opaque), and is
  always averaged when shrunk (decision 637's averaging, never nearest neighbor: a halved painting keeps its detail as
  color, not dropped pixels); it is never enlarged.

## Consequences
- One decode and one texture per background, 5 to 64 MB of video memory each, only the current map's held.
- **The ceiling: a 24-frame loop is at most 960 x 540 a frame, shown at 2x, and full-size frames fit 6 to a sheet** (up
  to 264 at 180 px).
- Pixel art imported at its own height stays as drawn; painted art is averaged once when shrunk.
- The game stays PNG-only (decision 680); the editor makes the sheet from a GIF and plays it before writing.
- A GIF imported at a height that isn't a divisor of 1080, or a frame narrower than 16:9, is scaled by a fraction when
  the game covers the screen, so point sampling gives uneven pixels; the dialog's divisor heights avoid it.
- A decode on the map's entry that runs long makes the fixed step catch up during the map's fade-in (the menu's
  precedent): about 40 ms for a 270 px sheet, about 200 ms for a full 4096 sheet.
- No save change: a battle is never saved.
- Later work: option B if the developer wants full-resolution painted backgrounds, at its memory and decode cost;
  per-frame times (decision 632) would become a list on the record's `animation`.
