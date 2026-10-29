namespace Jrpg.Core.Battle;

/// <summary>A hitter healed by their lifesteal from a hit they landed (decision 346); only reported above 0.</summary>
/// <param name="Actor">The hitter healed.</param>
/// <param name="Amount">The HP actually restored, within max HP.</param>
public sealed record LifeStolen(Combatant Actor, int Amount) : BattleEvent;
