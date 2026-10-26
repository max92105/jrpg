# B1b. The frozen frame breaks into shards

Part of [the UI second pass plan](../README.md). Decision 258. Design: [design.md](../design.md), "Tunables and placeholders" (`ShatterLayout`), "Key decisions" (shards are triangles; the shatter's randomness is the Game's). After B1a.

Developer: "a fade, ideally a screen break, like glass shattering."

- **B1b. The frozen frame breaks into shards (`ShatterLayout`)** (Game, about 240 lines with the 1.6x for a new view)
  - **Goal:** at the flash's peak the frozen frame cracks: it is drawn as about 24 triangular shards around an impact point at the screen's center, each slightly offset from its neighbors with a bright hairline along its edges, so the field reads as broken glass. The shards still hold their places (B1c flies them).
  - **New types:**
    - Game `Flow/Shard` (internal readonly record struct): one shard of the broken frame: its three corners in the frame (which are also its texture coordinates), its center.
    - Game `Flow/ShatterLayout` (internal sealed): the frame broken into shards around an impact point, from a seed.
      - `static ShatterLayout Break(Point impact, int seed)`: rays from the impact at jittered angles, crossed by jittered rings, cut into triangles that tile the screen exactly; `new Random(seed)` inside (Game code, never the battle's `IRandom`). `IReadOnlyList<Shard> Shards`.
    - Game `Flow/ShatterView` (internal sealed): draws the shards as textured triangles from the frozen frame, with `GraphicsDevice.DrawUserIndexedPrimitives` and a `BasicEffect` (point-sampled, at the virtual screen's scale); vertices built once per layout. The sprite batch ends before and restarts after.
  - **Changes (Game):** `Flow/GameFlow` builds the layout when the frame freezes (seed: a placeholder constant plus a count of encounters the flow has started) and draws the view instead of the plain frozen frame once the flash peaks.
  - **Tests** (`ShatterLayoutTests`):
    - `Break_SameSeed_GivesTheSameShards`
    - `Break_DifferentSeeds_GiveDifferentShards`
    - `Break_Shards_CoverTheScreenExactly` (the triangles' areas sum to 1920 x 1080)
    - `Break_Shards_AllHaveACornerAtOrAroundTheImpactRing` (shards radiate from the impact)
    - `Break_ShardCount_IsAboutTheTarget`
  - **Verify by hand:** an encounter: the flash, then the field visibly cracked into glass shards from the center, then the fade. F12 during the crack.
