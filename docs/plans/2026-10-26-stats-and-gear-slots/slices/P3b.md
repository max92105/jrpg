# P3b. Resistances and Intelligence on the sheet

Part of [the stats and gear slots plan](../README.md). Decisions 268, 272, 293. Design: "The stat sheet", "Tunables" (resistance and Intelligence rates, limits). After ADR 0027 and P3a. The review moved Intelligence's parts here from P3a.

- **P3b. Eight resistances from Constitution and Will; Intelligence's status percent and extra turns** (Core + Data + content, about 230 lines)
  - **Goal:** a member's sheet has a resistance per damage type (Constitution raises the three physical ones, Will the five magical ones, each within the floor and cap), and Intelligence's two numbers: the status percent and the extra status turns (one per 25 Intelligence, the breakpoint). The battle reads the resistances in D2 and Intelligence's numbers in I1, which never adds turns to Stun (decision 293).
  - **New types:**
    - Core `Stats/DamageType` (public enum): Blunt, Piercing, Slashing, Fire, Ice, Lightning, Light, Dark (ADR 0027); `Stats/DamageTypes.IsPhysical(type)` (a small public static beside it) says which group a type is in.
    - Core `Stats/Resistances` (public sealed record): one value per damage type in permille, read with `Of(type)`; `Plus(Resistances)`, `Raised(int points)` (Guarded in M2b), `None`; `Cap` (75 %) and `Floor` (−100 %) as constants, applied by `Clamped()`.
  - **Changes:**
    - `Stats/StatRules`: `PhysicalResistancePer100Constitution`, `MagicalResistancePer100Will`, `StatusPercentPer100Intelligence`, `IntelligencePerExtraStatusTurn`; `rules.json`; `GameRulesValidator` (rates at least 0, the breakpoint at least 1).
    - `Stats/StatFormula`: `ResistancesFrom(PrimaryStats, StatRules)`, `StatusPercent`, `ExtraStatusTurns`.
    - `Stats/StatSheet`: `Resistances`, `StatusPermille`, `ExtraStatusTurns`.
  - **Tests:**
    - `StatFormulaTests: ResistancesFrom_TwentyConstitution_RaisesPhysicalOnly`
    - `StatFormulaTests: ResistancesFrom_TwentyWill_RaisesMagicalOnly`
    - `StatFormulaTests: ExtraStatusTurns_JustBelowBreakpoint_AddsNone`
    - `StatFormulaTests: ExtraStatusTurns_AtBreakpoint_AddsOne`
    - `StatFormulaTests: StatusPercent_TwentyIntelligence_IsOneHundredTwenty`
    - `ResistancesTests: Of_Fire_ReadsFire`
    - `ResistancesTests: Clamped_AboveCap_IsCap`
    - `ResistancesTests: Clamped_BelowFloor_IsFloor`
    - `ResistancesTests: Raised_TenPoints_RaisesEveryType`
    - `GameRulesValidatorTests: Validate_ZeroIntelligencePerExtraStatusTurn_NamesTheField`
  - **Call sites:** none changed.
  - **Size:** about 90 Core, 15 Data and content, 10 tests (about 130): about 235.
  - **Verify by hand:** none (S1 shows them).
- **Done, with deviations:**
