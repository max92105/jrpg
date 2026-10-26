# B1c. The shards fly apart over the fade

Part of [the UI second pass plan](../README.md). Decision 258. Design: [design.md](../design.md), "Tunables and placeholders" (shard flight). After B1b.

- **B1c. The shards fly apart over the fade into battle** (Game, about 180 lines)
  - **Goal:** after the crack, the shards fly outward from the impact, each at its own speed, spinning, shrinking slightly and fading, over the fade-out to black; the battle then fades in as today.
  - **New types:** Game `Flow/EncounterShatter` (internal sealed): how far the shards have flown at a time since the break. `void Start()`, `void Advance(TimeSpan delta)`, `float Share` (0 to 1, eased).
  - **Changes (Game):**
    - `Flow/Shard`: a flight per shard, set by the layout from the same seed: direction (from the impact through its center), speed (300-900 px/s), spin (up to 1.5 turns/s). `Vector2 OffsetAt(float seconds)`, `float RotationAt(float seconds)`.
    - `Flow/ShatterView`: transforms each shard's vertices around its center each frame into the vertex array it already holds (no allocation per frame).
    - `Flow/GameFlow`: starts the shatter when the fade starts; draws shards over black as the fade darkens, so the field flies away into the black.
  - **Tests:**
    - `ShatterLayoutTests`:
      - `Break_EachShard_FliesAwayFromTheImpact`
      - `Break_SameSeed_GivesTheSameFlights`
    - `EncounterShatterTests`:
      - `Share_AtStart_IsZero`
      - `Share_AfterTheFadeOut_IsOne`
  - **Verify by hand:** several encounters: the glass breaks and flies apart, differently each time (the encounter count changes the seed), at a steady frame rate; the battle fades in cleanly; nothing of the shards stays on the battle screen.
- **Done, with deviations:**
  - **`Shard`** carries its flight as a velocity (direction times 300-900 px/s, from the same seed) and a spin (up to 1.5 turns/s either way), with `OffsetAt` and `RotationAt`.
  - **`EncounterShatter`** takes the fade-out's length (the flow's 0.5 s) and stays at 0 until started. `Share` is eased out, so the shards fly fast at first and settle; `FlightSeconds` is that same eased time, which the shards fly by.
  - **Flow:** it makes a fresh shatter per encounter, starts it when the fade starts, and advances it and moves the shards in the update, only while the frozen frame shows. The draw only draws. The fade's black stays over the shards as before, and the shards fade too, so they are gone by the time the screen is black. 245 lines.
  - **View:** the shards shrink to 75 % and fade to clear, through `BasicEffect.Alpha` with alpha blending. The hairlines are drawn from the moved vertices each frame and fade with them; the stored edges are gone. Nothing is allocated per frame.
  - **Review:** the at-start test now advances before `Start`, as the flow does through the flash, so dropping the reset would fail it. A new test bounds every shard's speed (300-900 px/s) and spin (up to 1.5 turns/s). The flow's comment says why each encounter needs a fresh shatter.
  - **Tests:** 3 in `ShatterLayoutTests` and 4 in `EncounterShatterTests` (before start, at start after advancing, halfway, after the fade-out). 2194 in the solution.
