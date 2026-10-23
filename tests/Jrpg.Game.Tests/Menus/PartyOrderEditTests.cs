using Jrpg.Core.Party;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;

namespace Jrpg.Game.Tests.Menus;

public sealed class PartyOrderEditTests
{
    private readonly List<PartyMember> _members =
        StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor).Party.Members.Take(4).ToList();

    [Fact]
    public void Swap_TwoMembers_SwapsTheirPlaces()
    {
        var order = PartyOrderEdit.Swap(_members, _members[1], _members[3]);

        Assert.Equal([_members[0], _members[3], _members[2], _members[1]], order);
    }

    [Fact]
    public void Swap_SameMember_KeepsOrder()
    {
        var order = PartyOrderEdit.Swap(_members, _members[2], _members[2]);

        Assert.Equal(_members, order);
    }

    [Fact]
    public void Swap_LeavesTheGivenOrderUnchanged()
    {
        var before = _members.ToList();

        PartyOrderEdit.Swap(_members, _members[1], _members[2]);

        Assert.Equal(before, _members);
    }
}
