using Jrpg.Core.Party;
using Jrpg.Core.Progression;

namespace Jrpg.Core.Content;

/// <summary>Global design rules that aren't tied to one entity.</summary>
public sealed record GameRules(PartyRules Party, ExperienceTable Experience);
