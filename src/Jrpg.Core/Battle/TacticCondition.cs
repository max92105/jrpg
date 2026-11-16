namespace Jrpg.Core.Battle;

/// <summary>When an enemy's action may be chosen (decision 402).</summary>
/// <param name="Kind">What it asks of the battle.</param>
/// <param name="Percent">A whole percent of max HP for the HP kinds, 1..99; unused by the other kinds.</param>
/// <param name="Turns">The N of every Nth turn, 2..10; unused by the other kinds.</param>
public sealed record TacticCondition(TacticConditionKind Kind, int Percent = 0, int Turns = 0);
