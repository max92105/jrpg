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

- **Look at `docs/ui-reference/` (its README says what each shot shows) before designing or drawing any screen** (title, menus, message box, battle HUD, victory). They are the developer's favorite UI/UX (Octopath Traveler II): beautiful, modern, the target style. Take inspiration from them, don't copy them pixel for pixel.
- What they do:
  - full-screen art with dark scrims and vignettes; the UI floats over it in translucent dark layers, not opaque boxed windows;
  - a thin dark header bar with the page title at top left, and a footer bar with a one-line hint;
  - vertical arrow-shaped tabs with an icon and a label; the selected one is deep navy fading to black, with a soft white glow near the pointer and faint hairlines (decision 270); sub-entries open beside it; a pointing-hand cursor, where ours is a diamond (decision 253: take the vibe, not a one-for-one copy);
  - panels and rows fade out horizontally instead of having hard borders; hairline separators;
  - small labels (`HP`, `SP`, `LV`, `NEXT TURN` in spaced caps) next to large numbers; thin colored bars (HP green, SP blue, EXP orange);
  - an elegant serif for big titles (logo, "Victory!"), a clean sans for the rest; white or pale text;
  - diamond-framed portraits (turn order); lots of empty space, so the art shows through.
- `Ui/UiTheme` holds the starting values and layouts. Where they're plainer than the examples, lean toward the examples. Ask the developer before departing from both.
- One selection look everywhere (decision 252): every selected row is `UiTextures.DrawSelectedRow` with `MenuPointer`'s diamond at its left; no screen draws its own.
- Every list's pointer glides and bobs through `SelectionMotion` (decisions 262, 336): no list draws `MenuPointer` at a row worked out from its cursor index, and a page that hosts another page or list passes it `Advance`.
- Other games' assets are reference only (decision 264): the extracted UI export is never committed, and nothing from another game (the screenshots in `docs/ui-reference/` included) is loaded or shipped. The game draws its own versions.
