using Jrpg.Core.Content;
using Jrpg.Core.Party;

namespace Jrpg.Simulations.Battle;

/// <summary>A leader of any class for a simulation, since any class can lead (decision 168).</summary>
internal static class StandInLeader
{
    // The member whose art the leader has; only the class's numbers matter when attacking only.
    private const string StandInId = "sarxian";

    public static PartyMember Of(ContentCatalog catalog, ClassDefinition characterClass) =>
        PartyMember.Recruit(catalog.PartyMembers[StandInId], characterClass, catalog.Skills, catalog.Rules.Stats);
}
