namespace Jrpg.Core.Battle;

/// <summary><c>Actor</c> will take one extra turn right after <c>After</c>'s next turn (decision 202).</summary>
public sealed record ExtraTurnReadied(Combatant Actor, Combatant After) : BattleEvent;
