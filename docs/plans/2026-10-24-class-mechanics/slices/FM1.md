# FM1. The party's formation in battle; Move

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Key design decisions" 4, "Structure". ADR 0023; decision 201; question 211.

- **FM1. The party's formation in battle; Move swaps places with a neighbor** (Core, about 220 lines)
  - **Goal:**
    - A battle keeps the party's line, starting from the active order.
    - On a member's turn, Move swaps them with the ally just before or after them, fallen or not. It costs the turn (question 211) and is reported as `PlacesSwapped`.
    - Ranges (PL2b) measure along this line.
    - **Known gap:** the screen offers Move and shows it from FM2. Enemies never move, so nothing throws before then.
  - **New types:**
    - Core `Battle/Formation` (public sealed class): the party's line in one battle.
      - `Line` (`IReadOnlyList<Combatant>`), `PositionOf(Combatant)`;
      - `Neighbors(Combatant)`;
      - `WithinRange(Combatant center, int range)`: the center included, in line order;
      - `internal void Swap(Combatant a, Combatant b)`.
    - Core `Battle/MoveCommand(Combatant SwapWith)` (BattleCommand): the actor trades places with a neighbor.
    - Core `Battle/PlacesSwapped(Combatant Mover, Combatant Other)` (BattleEvent).
  - **Changes:**
    - `Battle/BattleState`:
      - `Formation` (public), built in the constructor from the party side of `Combatants`, in order;
      - `TargetChoiceForMove()`: the actor's neighbors;
      - `Execute` handles `MoveCommand`: an illegal one throws `ArgumentException`, and an enemy actor throws `InvalidOperationException`.
  - **Tests:**
    - `FormationTests`:
      - `Constructor_Party_LineIsActiveOrder`
      - `Neighbors_Middle_ReturnsBothSides`
      - `Neighbors_End_ReturnsOne`
      - `WithinRange_RangeOneInMiddle_ReturnsThree`
      - `WithinRange_RangeTwoNearEnd_StopsAtEnd`
      - `Swap_Neighbors_ExchangesPositions`
    - `BattleFormationTests` (new file):
      - `Execute_MoveToNeighbor_SwapsAndReports`
      - `Execute_MoveToNonNeighbor_Throws`
      - `Execute_MoveToFallenNeighbor_Swaps`
      - `Execute_Move_EndsTurn`
  - **Verify by hand:** none (FM2 shows it).
