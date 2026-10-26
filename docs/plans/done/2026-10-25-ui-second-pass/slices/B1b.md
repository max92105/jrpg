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
- **Done, with deviations:**
  - **Layout:** 8 rays (the 4 corners and a jittered point on each edge, 25-75 % along) crossed by one ring at a jittered 35-55 % of each ray. The fan around the impact gives 8 shards and the ring-to-border quads 16 more, 24 in all. With the corners included, the outer border is the screen itself, so the shards tile it exactly; the area test checks that. `Shard` is a record of its three corners and its center; the flights come in B1c.
  - **View:** `ShatterView` uses `DrawUserPrimitives` (a plain triangle list, so no index buffer, not the slice's `DrawUserIndexedPrimitives`) through a `BasicEffect` made on the first draw and kept for the game's life. Each shard is pushed 3 px out from the impact, so dark cracks open, and white hairlines at 55 % are drawn by the sprite batch, which it begins again the same way the host does. Vertices and hairlines are built once per layout.
  - **Flow:** `FreezeFrame` breaks the layout (seed 258 plus the encounters so far), and the shards replace the plain frozen frame from `EncounterFlash.IsPastPeak` on. 227 lines.
  - **Review:** the host's scene batch and the view's restart share `Rendering/VirtualScreen.BeginBatch`. The radiating test now requires each shard to have a corner off the screen's border, meaning the impact or a ring point. The seed's comment says 258 is arbitrary.
  - **Tests:** 5 in `ShatterLayoutTests` and 2 in `EncounterFlashTests` (just before and at the peak). 2187 in the solution.
