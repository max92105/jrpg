using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// <c>Combatant</c> lost its turn to <c>Cause</c>, a skipping status (ADR 0021); the lost turn still counts down.
/// </summary>
public sealed record TurnSkipped(Combatant Combatant, StatusDefinition Cause) : BattleEvent;
