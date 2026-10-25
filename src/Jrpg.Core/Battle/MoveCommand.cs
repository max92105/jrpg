namespace Jrpg.Core.Battle;

/// <summary>The acting party member trades places with <c>SwapWith</c>, an ally right beside them (ADR 0023).</summary>
public sealed record MoveCommand(Combatant SwapWith) : BattleCommand;
