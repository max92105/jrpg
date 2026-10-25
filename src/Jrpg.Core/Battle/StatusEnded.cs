using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>A status of <c>Holder</c> ran out, or went because its holder fell (ADR 0021).</summary>
public sealed record StatusEnded(Combatant Holder, StatusKind Status) : BattleEvent;
