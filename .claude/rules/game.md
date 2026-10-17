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
- No allocations in `Update`/`Draw` hot paths (no LINQ, no `new` of lists per frame, no string concatenation per frame).
