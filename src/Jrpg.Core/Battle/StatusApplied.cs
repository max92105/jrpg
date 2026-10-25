using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>A status was granted to <c>Target</c>, or refreshed, for <c>Turns</c> of its turns (ADR 0021).</summary>
public sealed record StatusApplied(Combatant Target, StatusKind Status, int Turns) : BattleEvent;
