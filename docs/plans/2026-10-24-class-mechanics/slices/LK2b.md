# LK2b. Tick tints and the rogue's step

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "When things happen in a turn" (events table). Split from LK2 on 2026-10-25; the status marks are LK2a.

- **LK2b. Tick numbers tinted by their status; the rogue stays forward through a quick skill** (Game, about 130 lines)
  - **Goal:**
    - Tick and backfire numbers rise like impact numbers: Poison purple-tinted, Regen green.
    - The rogue stays forward through a quick skill instead of stepping back and forward.
  - **Changes (Game):**
    - `UiTheme` gets the tick tints;
    - `BattleTurnSequence`: after a quick skill, the same actor goes straight to `AwaitingCommand`. It reads that from Core, with no rule of its own: the same `CurrentActor` and a `PartyTurn` phase.
  - **Tests:**
    - `BattleTurnSequenceTests`: `Update_AfterQuickSkill_StaysForwardAndAwaitsCommand`
    - `FloatingNumberTests`: `ColorOf_PoisonTick_IsPoisonTint`
  - **Verify by hand:**
    - Poison ticks rise purple.
    - Lazarus stays forward after Ready.
