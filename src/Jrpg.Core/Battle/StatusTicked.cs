using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// At the end of <c>Holder</c>'s turn a status took or restored HP: <c>Amount</c> is what its ticking part took or
/// restored, as applied (ADR 0021).
/// </summary>
public sealed record StatusTicked(Combatant Holder, StatusDefinition Status, int Amount) : BattleEvent;
