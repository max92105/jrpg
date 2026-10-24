using Jrpg.Core.Party;

namespace Jrpg.Core.Tests.Builders;

/// <summary>Parties for tests that only need someone to lead, for exploration and its leader-gated events.</summary>
internal static class TestParties
{
    /// <summary>The leader alone, in a party that holds 5 (decision 176).</summary>
    public static PartyState LedBy(string leaderId)
    {
        var leader = new PartyMemberBuilder().WithId(leaderId).Build();
        return new PartyState([leader], [leader], new PartyRules(MaxActiveMembers: 5));
    }
}
