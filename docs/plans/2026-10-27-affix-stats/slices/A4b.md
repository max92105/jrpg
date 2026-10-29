# A4b. Drop luck from gear; the simulation's luck table

Part of [the affix stats plan](../README.md). Decisions 294, 349, 356, 367. After A4a. Builds on design.md's tunables (`drop_luck`).

- **A4b. Drop luck from gear; the simulation's luck table** (Core + content + Game text + Simulations, about 110 lines: about 45 of code, content and simulation, 65 of tests)
  - **Goal:** the active members' drop luck, fallen members included (decision 356), adds up when a victory's loot rolls and goes to A4a's rolls. Gear gets it only as the `drop_luck` affix. The loot simulation shows and asserts its effect.
  - **New types:** none.
  - **Changes:**
    - `Stats/StatKind.DropLuck` (appended), `StatKinds.AffixOnly` gains it; `StatSheet.DropLuckPermille` (init, 0) and its `AffixStatOf` arm; not on `CombatStats` (battles don't read it); `MemberSheet`.
    - `Session/GameSession.SpoilsOf`: replaces A4a's TODO with the sum of `Party.ActiveMembers`' `Sheet.DropLuckPermille`, the fallen included (comment: decision 356, luck is gear and a fallen member still wears it).
    - Simulations: `LootSimulation` rolls the same 2,000 seeds again at +50 % luck (500 permille), prints a second summary table under it (`WriteSummary` takes the drops and a caption), and a new fact asserts more gear per victory with luck than without.
    - Text: `stat_drop_luck` ("Drop luck"), `MessageIds`, `Ui/StatNames`.
    - Content: `affixes.json` `drop_luck` (`dropLuck`, amulet and ring, 5-15, weight 10).
  - **Tests:**
    - `MemberSheetTests: Of_DropLuckWorn_InPermille` (no `CombatStats` copy to test it through)
    - `GameSessionDropTests: FinishBattle_LeaderWithLuck_DropsAboveTheChance` (the sword's 50 % entry with a draw of 500..749 drops only at +50 %)
    - `GameSessionDropTests: FinishBattle_FallenMemberWithLuck_StillCounts` (decision 356: a second active member, knocked out before the battle, wears the luck; the slice pins the draws, the turn order's included)
    - `LootSimulation: GoblinTrio_FiftyPercentLuck_DropsMoreGearPerVictory` (gear per victory at +50 % above gear per victory without: about 0.99 against 0.66 with the five gear entries, far past the noise of 2,000 victories)
    - `StatSheetTests` and `ItemCatalogValidatorTests`: the `DropLuck` cases.
  - **Call sites:** `StatNamesTests` (theory). `GameSessionDropTests` 132 → about 180.
  - **Verify by hand:** the Equipment comparison lists "Drop luck" when a `drop_luck` ring changes it; the simulation's two tables (after a build, `tests/Jrpg.Simulations/bin/Debug/net10.0/Jrpg.Simulations.exe -showLiveOutput`) show more gear and more Magic and Rare pieces under luck. The Status page shows it from ST1b.
