namespace Jrpg.Core.Loot;

/// <summary>
/// One rolled rarity's row in the rarity table (ADR 0030): how likely it is, and how many affixes it rolls.
/// </summary>
/// <param name="Weight">Relative to the other rows' weights; 0 never rolls.</param>
/// <param name="MinAffixes">The fewest affixes a piece of this rarity rolls.</param>
/// <param name="MaxAffixes">The most; Rare rolls 2 or 3 (decision 340).</param>
public sealed record RarityRule(int Weight, int MinAffixes, int MaxAffixes);
