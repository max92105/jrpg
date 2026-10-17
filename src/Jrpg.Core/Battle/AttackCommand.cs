namespace Jrpg.Core.Battle;

public sealed record AttackCommand(Combatant Target) : BattleCommand;
