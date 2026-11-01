namespace Jrpg.Core.Battle;

/// <summary>An enemy entered a phase: the last event of the call whose HP loss reached it (ADR 0034).</summary>
public sealed record EnemyPhaseEntered(Combatant Enemy, EnemyPhase Phase) : BattleEvent;
