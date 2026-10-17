namespace Jrpg.Core.Battle;

public sealed record ActionStarted(Combatant Actor, BattleCommand Command) : BattleEvent;
