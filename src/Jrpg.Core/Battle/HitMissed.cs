namespace Jrpg.Core.Battle;

/// <summary>
/// One hit of a damage effect that missed <paramref name="Target"/>: nothing landed, and it is no hit for the gains a
/// hit sets off (decision 326).
/// </summary>
public sealed record HitMissed(Combatant Target) : BattleEvent;
