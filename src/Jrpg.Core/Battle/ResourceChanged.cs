namespace Jrpg.Core.Battle;

/// <summary>A resource of <c>Holder</c> changed by <c>Change</c>, as applied after clamping; never 0.</summary>
public sealed record ResourceChanged(Combatant Holder, string ResourceId, int Change) : BattleEvent;
