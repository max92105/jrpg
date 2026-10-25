using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// At the end of <c>Holder</c>'s turn a status took or restored HP: <c>Amount</c> is what Poison took or Regen
/// restored, as applied (ADR 0021).
/// </summary>
public sealed record StatusTicked(Combatant Holder, StatusKind Status, int Amount) : BattleEvent;
