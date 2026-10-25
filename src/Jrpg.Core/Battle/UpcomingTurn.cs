namespace Jrpg.Core.Battle;

/// <summary>One turn to come: who acts, and whether it's a readied extra turn rather than their cycle turn.</summary>
public sealed record UpcomingTurn(Combatant Actor, bool IsExtra);
