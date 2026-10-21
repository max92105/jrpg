---
paths:
  - "src/Jrpg.Game/**/*.cs"
---

# Rules for Jrpg.Game (MonoGame shell)

- This layer translates: input → Core commands, Core state/events → pixels and sounds. No game rules here.
- If you are about to write an `if` that decides gameplay (damage, who acts next, what an item does), stop: it belongs in Core.
- Use a fixed timestep for simulation updates; rendering may interpolate.
- Pixel art: point sampling (`SamplerState.PointClamp`), integer scaling, no blurry filtering.
- Load assets through the content pipeline or one asset loader class; never load files ad hoc inside draw code.
- A shape or texture that `UiTextures` bakes lives in `Ui/`, never in a feature folder: `Ui` must not depend on `Battle`, `Menus` or `Title`.
- No allocations in `Update`/`Draw` hot paths (no LINQ, no `new` of lists per frame, no string concatenation per frame).

## UI look

- **Look at `docs/ui_ux_exemples/` before designing or drawing any screen** (title, menus, message box, battle HUD, victory). They are the developer's favorite UI/UX (Octopath Traveler II): beautiful, modern, the target style. Take inspiration from them, don't copy them pixel for pixel, and never ship their images or the game's art.
- What they do:
  - full-screen art with dark scrims and vignettes; the UI floats over it in translucent dark layers, not opaque boxed windows;
  - a thin dark header bar with the page title at top left, and a footer bar with a one-line hint;
  - vertical arrow-shaped tabs with an icon and a label; the selected one has a deep blue gradient glowing out to the right and a bright hairline; sub-entries open beside it; a pointing-hand cursor;
  - panels and rows fade out horizontally instead of having hard borders; hairline separators;
  - small labels (`HP`, `SP`, `LV`, `NEXT TURN` in spaced caps) next to large numbers; thin colored bars (HP green, SP blue, EXP orange);
  - an elegant serif for big titles (logo, "Victory!"), a clean sans for the rest; white or pale text;
  - diamond-framed portraits (turn order); lots of empty space, so the art shows through.
- Legacy XAML (`legacy/JRPG/UI/Styles/`) gives starting values and layouts, and was already inspired by this game. Where it's plainer than the examples, lean toward the examples. Ask the developer before departing from both.
