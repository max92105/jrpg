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
