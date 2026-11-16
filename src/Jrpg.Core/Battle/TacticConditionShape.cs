namespace Jrpg.Core.Battle;

/// <summary>What a trigger kind names as "that one" and which number it reads.</summary>
public sealed record TacticConditionShape(TacticSubject Subject, TacticConditionNumber Number);
