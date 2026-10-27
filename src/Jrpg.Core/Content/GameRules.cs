using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Content;

/// <summary>Global design rules that aren't tied to one entity, the stat conversions among them (ADR 0026).</summary>
public sealed record GameRules(PartyRules Party, ExperienceTable Experience, ExplorationRules Exploration, StatRules Stats);
