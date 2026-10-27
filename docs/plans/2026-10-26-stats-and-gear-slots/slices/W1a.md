# W1a. Will raises a battle resource's start or max

Part of [the stats and gear slots plan](../README.md). Decisions 291, 296, 319, ADR 0019. Design: "Formulas" (`WillFormula`), "Tunables" (Will, the per-class table). After I2. W1b does the lasting and harmful resources.

- **W1a. Will raises a battle resource's start or max** (Core + Data + content + Game, about 180 lines)
  - **Goal:** a resource may declare `"willScaling": { "willPerPoint": 10, "effect": "raiseStart" }` (or `"raiseMax"`): each `willPerPoint` of its holder's Will adds a point to its start (within its max) or to its max (and to its start when it starts full, as speed charges do). The battle starts each resource from that, and the Status page shows the same numbers as the battle will. This slice covers the battle-only resources (design.md's table: stamina, mana, readiness, ferocity, conviction, chakra, growth, decay); `lowerGain` and lasting resources come in W1b, and the validator refuses them until then.
  - **New types:**
    - Core `Resources/WillEffect` (public enum): what Will does to a resource: `RaiseStart`, `RaiseMax` (W1b adds `LowerGain`).
    - Core `Resources/WillScaling` (public sealed record): a resource's Will rule: `WillPerPoint`, `Effect`.
    - Core `Resources/WillFormula` (public static): the points a Will gives (`BonusPoints(will, willPerPoint)` = will ÷ willPerPoint, rounded down), with the placeholder's reasoning (decision 291).
  - **Changes:**
    - `Resources/ResourceDefinition`: gains `WillScaling?` (init).
    - `Battle/ResourceStarts`: public (the Status page reads it); `StartOf` adds the Will points per the rule, after speed charges and before strain.
    - `Data/Resources/ResourceDefinitionValidator`: `willPerPoint` at least 1; until W1b, no `willScaling` on a lasting resource.
    - `content/classes.json`: the table's `raiseStart` and `raiseMax` entries.
    - Game `Menus/StatusSheet.AmountText`: reads `ResourceStarts.For(member)` instead of recomputing readiness charges (removes the duplicate of Core's rule); Will's Status caption adds its resource line ("+1 mana at start").
  - **Tests:**
    - `WillFormulaTests: BonusPoints_TwentyFiveWillPerTen_IsTwo`
    - `ResourceStartsTests: For_WillRaisesStart_StartsHigherWithinMax`
    - `ResourceStartsTests: For_WillRaisesMaxOfFullResource_StartsFullAtNewMax`
    - `ResourceStartsTests: For_WillRaisesMaxOfSpeedCharges_AddsCharges`
    - `ResourceDefinitionValidatorTests: FindProblem_ZeroWillPerPoint_NamesWillScaling`
    - `StatusPageTests: Open_Mage_ShowsManaStartWithWill`
  - **Call sites:** `ResourceStarts.For` 2 (`Combatant`, now `StatusSheet`); `StatusSheet`'s readiness formula 1.
  - **Size:** Core raw about 55, Data and content 20 (non-view); Game raw about 20 (presenter, non-view; no drawing change); 6 tests about 80: about 175.
  - **Verify by hand:** the mage starts a battle with more mana than its `startsAt` (Will 14: +1); the rogue has one more Ready charge than speed alone gives only if its Will reaches 10 (6 at level 1: none; at level 5 it does). The Status page's resources match the battle's.
- **Done, with deviations:**
