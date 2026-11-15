# IU5 (optional, only on the developer's word). A potion or a bomb names its tiered effect and acts with its user's Power

**Not run** unless the developer answers "tiered" to the README's open question (decision 874). Items keep a flat power
until then (decision 873, row 6's decision 785). If it runs, it goes after IU4 and before IU12, and IU12's consumable
form then picks a record instead of typing a power (its file says what changes).

Core + Data + content + Game, about 250 lines (about 160 of them tests and re-pins), plus the five item edits and one
`effects.json` record. Needs IU1, row 6's T1, T2 and T4, row 7's FX1 (`CatalogReferences`, its decision 975). Design:
design.md, section 2.5; decision 874.

## Goal
An item's damage or heal names a record of `effects.json` (`"tier": "heal_1"`, power 0), as a skill's does; its amount
is its user's Power times the record's `perPower`, nothing else raising it. The user is the battle's actor, and from
the menu `FieldItemUse.Use`'s existing `user` (the target for a single-target item, the leader for an item that takes
everyone, as `UseOn` passes it); the menu's HP preview reads the same user. Data requires the record on an item's
damage or heal (replacing T2's refusal); revive and rest keep a flat power. The five repository items convert.

## New types
- `Core/Effects/ItemEffectScaling` (public static): an item's effect as its user lands it, a tiered amount from the
  user's Power alone (decisions 321, 874).
  - `public static Effect ScaledFor(Effect effect, int userPower)`: a tiered effect `with { Power =
    DamageFormula.TieredAmount(userPower, tier.PerPower, 0) }`; any other effect returned as it is.

## Changes
- `Core/Battle/EffectLanding.LandFromItem`: lands `ItemEffectScaling.ScaledFor(effect, actor.CurrentStats.Power)`.
- `Core/Party/FieldItemUse`: `Use` lands through `ItemEffectScaling` with its `user`'s sheet Power; `HpAfterUse(item,
  user, target)` takes the same user (`Game/Menus/ItemUse` passes the target for a single-target item, the leader for
  an "all" item, as `UseOn` does), so preview and use agree.
- `Data/Items/ItemCatalogValidator`: T2's "effect.tier is for skills" rule becomes "effect.tier is required for an
  item's damage or restoreHp: name a record of effects.json (decision 874)."; row 6's `EffectValidator` rules for a
  tier apply as they are.
- Row 7's `CatalogProblems.OfTieredEffects`: also runs the items' check over the rebound catalog (row 7's decision 975:
  `CatalogReferences.Rebound` already rebinds items' effects).
- `docs/decisions/0056-...md`: an amendment line (decision 874).
- `content/effects.json`: `heal_3` (`restoreHp`, `perPower` 9, the sheet's High) in id order.
- `content/items.json`: Potion `heal_1`, Hi-Potion `heal_2`, Elixir `heal_3`, Bomb `fire_1`, Bomb All `fire_2`, each
  `"power": 0`; the Elixir's description "Restores a large amount of HP."; every other line, the bomb's `effectFile`
  included, unchanged.
- Re-pins: grep `Core.Tests` and `Game.Tests` for tests reading a repository potion's or bomb's amount (on 2026-11-15
  among `Party/FieldItemUseTests`, `Party/FieldItemUseHpAfterTests`, `Battle/BattleSkillAndItemTests`,
  `Game.Tests/Menus/InventoryPageAimTests`, `InventoryPageUseTests`); each builds its own item or asserts the direction,
  never the new number. About 40 lines.

## Tests
- `Core.Tests/Effects/ItemEffectScalingTests` (new): `ScaledFor_TieredHeal_IsUserPowerTimesPerPower`,
  `ScaledFor_TieredDamage_IgnoresEveryModifier`, `ScaledFor_FlatRevive_KeepsItsPower`.
- `Core.Tests/Battle/BattleItemTierTests` (new; combatants and items built in the test, certain hits):
  `Execute_ItemNamingAHealRecord_HealsTheActorsPowerTimesPerPower`,
  `Execute_ItemNamingADamageRecord_IgnoresTheActorsDamageModifier`, `Execute_SameItemByTwoActors_HealsBySeparatePowers`.
- `Core.Tests/Party/FieldItemUseTierTests` (new: `FieldItemUseTests` is at 276):
  `Use_TieredPotionOnOneMember_HealsByThatMembersPower`, `Use_TieredAllItem_HealsByTheLeadersPower`,
  `HpAfterUse_TieredPotion_EqualsWhatUseGives`.
- `Data.Tests/Items/ItemCatalogValidatorTierTests` (T2's): `Validate_ItemHealWithoutATier_RefusesNamingTheItem`,
  `Validate_ItemDamageWithoutATier_Refuses`, `Validate_ItemHealNamingAHealRecord_Accepted`,
  `Validate_ItemReviveWithAFlatPower_Accepted`.
- `Data.Tests/Content/ContentCatalogLoaderTieredEffectTests` (T2's): `Load_RepositoryPotion_NamesAHealRecord` (kind
  only, no id or number pinned).
- Row 7's `CatalogProblems` tests: `OfTieredEffects_HealRecordAnItemNamesTurnedDamage_NamesItemsJson`.

## Verify by hand
- Title > New game, take damage, Menu > Inventory > Potion on a member: preview and heal equal that member's Power
  (Menu > Status) times 2. In a fight, a Bomb deals about the actor's Power times 2 and still plays
  `FireExplosion.png`.
