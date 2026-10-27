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
  - **The flight:** each `Shard` carries its direction (from the impact through its center), speed (300-900 px/s) and spin (up to 1.5 turns/s either way), drawn by the layout from its seed; `OffsetAt` and `RotationAt` give where it is after so many seconds.
  - **`EncounterShatter`** eases its flight out (`Share`, `FlightSeconds`) over a length the flow gives it, the fade-out's (after review, so the two can't drift); its clock runs only once started, and the flow makes a new one for each encounter, so the shards hold their places between the crack and the fade (a test pins it).
  - **The view** pushes each shard 4 px along its own direction (after review: no second copy of the impact), and moves, turns and shrinks (to 70 %) each shard about its center into the vertex arrays built once, and fades the shards and their edges with the effects' alpha; the flow draws black under them, so the field flies away into the dark, and the fade's black covers all at the end.
  - **Tests:** the four planned, plus `EncounterShatterTests.Share_BeforeStart_IsZero` and `Share_Halfway_IsPastHalf`, and after review `ShatterLayoutTests.Break_EachShard_FliesAtTheSpeedAndSpinRange`: 2182 in all.
