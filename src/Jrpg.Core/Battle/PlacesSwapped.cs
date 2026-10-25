namespace Jrpg.Core.Battle;

/// <summary><c>Mover</c> and <c>Other</c> traded places in the party's line (ADR 0023).</summary>
public sealed record PlacesSwapped(Combatant Mover, Combatant Other) : BattleEvent;
