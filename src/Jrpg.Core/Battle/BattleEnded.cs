namespace Jrpg.Core.Battle;

public sealed record BattleEnded(BattlePhase Outcome) : BattleEvent;
