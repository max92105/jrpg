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
  - **`ResourceStarts.WillRaised`** applies the rule after the base start (fixed, lasting or speed charges) and before the strain: `raiseStart` adds the points to the amount, `raiseMax` to the max and to the amount when the resource starts full; the amount is clamped to 0..max and the max kept at least 1, so Will taken away by gear (negative points) never breaks a resource.
  - **`StatusSheet` already read `ResourceStarts.For`** (an earlier slice removed the readiness duplicate), so the page follows the battle with no change there. The Will caption gained its resource parts instead: `AttributeCaptions.Of` takes the class resources and writes "+3 % magic res., +1 mana" (one part per resource with `willScaling`, joined by a new `status_caption_separator` text; the druid's reads "+1 growth, +1 decay", the longest caption, 36 characters: the hand check looks at it).
  - **`Open_Mage_ShowsManaStartWithWill` became `Open_Mage_WillCaptionNamesMana`:** the page shows a battle resource's max, not its start, and mana's max doesn't move under `raiseStart`; the caption is where the page shows Will's point.
  - **Re-pin:** `BattleTurnSequenceTests.Update_OmerEndsTurn_NoThrowAndNextActorSteps` expects 6 mana (3 + Will's 1, then the turn's 2), not 5.
  - **Tests:** `WillFormulaTests` 2, `ResourceStartsTests` 6 (the three planned, an empty resource under `raiseMax`, no scaling, and the review's "Will first, then strain" order), validator 2, `AttributeCaptionsTests` 1, `StatusPageTests` 1. Count: 2,526 → 2,538.
  - **Simulation:** attack only, unchanged; green.
  - **Hand check (yours):** the mage starts a battle with 4 mana (Will 14: +1); the rogue has one more Ready charge than speed alone gives only once its Will reaches 10 (6 at level 1: none). The Status page's resources match the battle's, and the druid's Will caption fits its column.
