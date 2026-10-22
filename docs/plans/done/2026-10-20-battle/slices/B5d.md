# B5d. Effect strips and placeholder cues

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B5d. Effect strips and placeholder cues** (content + Data + Game, about 230 lines; decisions 57, 101)
  - **Goal:**
    - Damage plays the attacker's attack effect as an animated strip on each target.
    - Any other effect, a heal for example, shows a placeholder cue: the action's name appearing and disappearing over the target. Its trigger and timing can be checked until real animations exist.
  - **Content:**
    - The developer converts `AttackPlayer.gif` and `AttackEnemy.gif` outside the repository into horizontal PNG strips of square frames in `content/art/effects/`.
    - `attackEffectFile` becomes `AttackPlayer.png` and `AttackEnemy.png`.
    - `effects-source/` is deleted, and the CLAUDE.md line mentioning it is updated.
  - **New types (Game):**
    - `Battle/EffectStrip` (pure): which frame shows at a time, and when it's finished; frames = width / height.
    - `Battle/PlaceholderCue` (pure timing).
  - **Data:** `ArtFolders.Effects`; `ArtReferenceCheck` covers attack effects (exact names) and strips that aren't whole square frames.
  - **Tests:**
    - Strip: `EffectStrip_FrameAt_TwoFrameTimes_IsThirdFrame`, `EffectStrip_FrameAt_PastLastFrame_IsFinished`.
    - Cues: `ActionPlayback_Start_HealLanded_ShowsPlaceholderCueNotStrip`, `PlaceholderCue_At_PastLength_IsHidden`.
    - Data: `ArtReferenceCheck_FindProblems_EffectStripWithPartialFrame_NamesTheFile`, `ArtReferenceCheck_FindProblems_MissingAttackEffect_NamesTheMember`.
  - **Verify by hand:**
    - [ ] The slash plays once per damaged target.
    - [ ] A potion shows its cue.
- **Done, with deviations:** at the developer's request I converted the GIFs myself with a throwaway System.Drawing script in the session scratchpad (outside the repository): `AttackPlayer.png` is 10 frames and `AttackEnemy.png` 9 frames, all 128 px, transparent; the GIFs had no frame delays, so 0.08 s per frame stands. `ActionPlayback` now reads events into an empty playback (it had grown past four constructor arguments) and lists `Strips` (`EffectOnTarget`) and `Cues` (`CueOnTarget`, the action's name). A cue shows where a strip would play, 30 px below the target's top. Drawing is a new `ImpactEffectsView`. Test fixtures that named the GIFs now name the PNGs. Added `EffectStrip_Of_TenFrameWideImage_HasTenFrames`, `PlaceholderCue_OpacityAt_WhileShown_IsFullyVisible`, `PlaceholderCue_OpacityAt_HalfwayThroughAFade_IsHalfVisible` and `ActionPlayback_Start_AttackLanded_PlaysAttackersStripOnTarget`; the planned `PlaceholderCue_At_PastLength_IsHidden` is `OpacityAt_PastLength_IsHidden`. An action with no name shows no cue. Follow-up: enemy attack strips are drawn unmirrored over the party; no flip rule is decided.
