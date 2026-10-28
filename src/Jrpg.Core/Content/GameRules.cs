using Jrpg.Core.Loot;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Content;

/// <summary>
/// Global design rules that aren't tied to one entity: the stat conversions among them (ADR 0026), and the rarity table
/// dropped gear rolls on (ADR 0030).
/// </summary>
public sealed record GameRules(PartyRules Party, ExperienceTable Experience, ExplorationRules Exploration, StatRules Stats, LootRules Loot);
