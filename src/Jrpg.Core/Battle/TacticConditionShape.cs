namespace Jrpg.Core.Battle;

/// <summary>What a trigger kind names as "that one", which number it reads and whether it is a reaction.</summary>
/// <param name="Subject">Whom it names.</param>
/// <param name="Number">The number it reads, or none.</param>
/// <param name="IsReaction">
/// Whether it answers what foes did to the enemy since its last turn, read from its <see cref="TacticMemory"/>
/// (decision 844), instead of a state read at its turn.
/// </param>
public sealed record TacticConditionShape(TacticSubject Subject, TacticConditionNumber Number, bool IsReaction = false);
