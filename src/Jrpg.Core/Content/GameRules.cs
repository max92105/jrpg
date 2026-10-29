using Jrpg.Core.Crafting;
using Jrpg.Core.Loot;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Content;

/// <summary>
/// Global design rules that aren't tied to one entity: the stat conversions among them (ADR 0026), the rarity table
/// dropped gear rolls on (ADR 0030), and what the Forge gives and costs (decisions 351-354).
/// </summary>
public sealed record GameRules(PartyRules Party, ExperienceTable Experience, ExplorationRules Exploration, StatRules Stats, LootRules Loot)
{
    /// <summary>
    /// What the Forge gives and costs. An init property, not a positional one, so the many rules built in tests need
    /// none; empty, the Forge refuses every action.
    /// </summary>
    public CraftingRules Crafting { get; init; } = new();
}
