namespace Jrpg.Core.Battle;

/// <summary><c>Caster</c>'s spell turned on them through <c>ResourceId</c>, costing <c>HpLost</c>.</summary>
public sealed record Backfired(Combatant Caster, string ResourceId, int HpLost) : BattleEvent;
