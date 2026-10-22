# B2b. Battlers

Part of [the battle plan](../README.md). Design: [design.md](../design.md).

- **B2b. Battlers** (Data + Game, about 220 lines)
  - **Goal:** each living combatant is drawn at its spot. `isSpriteFlipped` flips an enemy, and fallen enemies aren't drawn. Decision 100 placeholders:
    - painted party art is shrunk smoothly to 96 px;
    - the goblin's pixel art is point-scaled to 128 px (half), feet on the spot's bottom.
  - **New types (Game):**
    - `Battle/BattlerArt` (record, pure `Of(Combatant combatant)`): the files that show one combatant: sprite, flip, portrait, attack effect.
    - `Battle/BattlerScaling` (pure): how a battler image of a given size reaches the screen: a smooth shrink to 96 px, or a whole-factor point scale.
    - `UiTextures.Battler(textures, file, size)`: smooth shrinks, made once.
  - **Data:** `ArtFolders.Battlers`; `ArtReferenceCheck` covers party and enemy battle sprites (exact names).
  - **Tests:**
    - Art: `BattlerArt_Of_Enemy_UsesEnemySpriteAndFlip`, `BattlerArt_Of_PartyMember_UsesMemberSprite`.
    - Scaling: `BattlerScaling_Of_1024PxPainting_ShrinksSmoothlyTo96`, `BattlerScaling_Of_256PxPixelArt_PointScalesByHalf`.
    - Data: `ArtReferenceCheck_FindProblems_MissingBattleSprite_NamesTheEnemy`.
  - **Verify by hand:**
    - [ ] The goblin's pixels stay crisp (no smoothing).
    - [ ] Sarxian's painting shrinks cleanly.
    - [ ] Formation as in legacy.
- **Done, with deviations:** drawing lives in a new `Battle/BattlersView`, so `BattleScene` stays a router. Only living combatants are drawn, so fallen party members disappear too (legacy `AnimateDeath` faded any combatant out). `BattlerScaling` is a record struct (`DrawnSize`, `IsShrunkSmoothly`); `UiTextures.Icon` and `Battler` share one `ShrunkOnce` helper and one cache keyed by folder, instead of a third copy. Review added the 512 px boundary test and a missing party battle sprite test.
