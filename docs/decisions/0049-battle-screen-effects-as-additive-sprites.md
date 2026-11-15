# 0049. Battle screen effects as additive sprite drawing

Status: accepted
Date: 2026-11-15

## Context
The developer's reference clip lights the impact: the battlefield flashes warm, nearby battlers wash white, a glow
lights the floor around the target and a halo lingers on it, and red and green fringes split the impact's bright parts
for a moment. The game draws with MonoGame's `SpriteBatch` from PNGs loaded at runtime (ADR 0009), into a virtual
screen that loses its contents when it is set again as the render target (`JrpgGame.FreezeFrameIfWanted`). There is no
shader and no content pipeline. Plan `2026-11-14-battle-turn-presentation`, decisions 660 and 661.

## Options
### A. Additive sprite drawing (chosen)
The light (flash, wash, floor glow, halo) is drawn in short additive batches over textures baked once. The fringe is the
bright sprites (the impact strip's frame, the white silhouettes) drawn as three additive single-channel copies shifted
apart, so the HUD and the numbers stay clean.
- Pros: no shader, no package, no content pipeline (ADR 0009 holds); no render target set mid-frame; local to the
  battle.
- Cons: the fringe touches only what is redrawn, not the background's edges; F12 captures show it, as it is in the
  frame.

### B. A post-process at the host
The finished frame is drawn three times, tinted and shifted, when the host copies it to the window; the shift is passed
through the flow.
- Pros: a true whole-frame aberration in a few lines.
- Cons: it fringes the HUD and the numbers; a scene-to-flow-to-host channel for one effect; captures miss it.

### C. An HLSL effect
- Pros: real bloom and aberration.
- Cons: the MonoGame content pipeline (an `.mgcb`, the effect compiler as a dotnet tool package, a build step), against
  ADR 0009's runtime PNGs; desktop GL shader quirks.

## Decision
A: the battle draws its impact light and its color fringes with `SpriteBatch` alone, in additive batches over textures
it bakes once. The fringes are the impact strip's frame and the white silhouettes drawn as a red copy shifted left, a
green copy shifted right and a blue copy in place, before the numbers.

## Consequences
- The host, the flow and the render targets don't change; the battle scene owns every effect, and captures show it.
- The HUD and the damage numbers are never fringed or washed: they draw after the effects.
- A whole-frame effect (a real bloom, a screen-wide aberration) would need option B or C; this ADR would then be
  superseded.
- Each light batch is a `SpriteBatch.Begin` adding premultiplied colors as they are (`VirtualScreen.BeginLightBatch`, a
  One/One blend: `BlendState.Additive` multiplies by alpha again and would square a fade), so the draw calls grow by
  one batch per light while it plays.
