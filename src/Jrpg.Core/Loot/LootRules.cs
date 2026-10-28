using Jrpg.Core.Items;

namespace Jrpg.Core.Loot;

/// <summary>
/// The rarity table dropped gear rolls on (ADR 0030); Unique has no row, as it never rolls (decision 339).
/// </summary>
public sealed record LootRules(IReadOnlyDictionary<Rarity, RarityRule> Rarities);
