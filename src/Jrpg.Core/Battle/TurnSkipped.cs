using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary><c>Combatant</c> lost its turn to <c>Cause</c>; the lost turn still counts down (ADR 0021).</summary>
public sealed record TurnSkipped(Combatant Combatant, StatusKind Cause) : BattleEvent;
