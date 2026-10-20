using Jrpg.Core.Party;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

public sealed class PartyStateTests
{
    private readonly PartyMember _leader = new PartyMemberBuilder().WithId("sarxian").Build();
    private readonly PartyMember _jazog = new PartyMemberBuilder().WithId("jazog").Build();
    private readonly PartyMember _lyra = new PartyMemberBuilder().WithId("lyra").Build();
    private readonly PartyMember _mira = new PartyMemberBuilder().WithId("mira").Build();

    [Fact]
    public void Constructor_FirstActiveMember_IsLeader()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        Assert.Same(_leader, party.Leader);
    }

    [Fact]
    public void MemberById_ReserveMember_ReturnsIt()
    {
        var party = new PartyState([_leader, _jazog, _lyra], [_leader], new PartyRules(MaxActiveMembers: 3));

        Assert.Same(_lyra, party.MemberById("lyra"));
    }

    [Fact]
    public void MemberById_UnknownId_Throws()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        Assert.Throws<ArgumentException>(() => party.MemberById("stranger"));
    }

    [Fact]
    public void Constructor_MoreActiveMembersThanAllowed_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreateParty(maxActiveMembers: 1, _leader, _jazog));
    }

    [Fact]
    public void Constructor_ActiveMemberNotRecruited_Throws()
    {
        var stranger = new PartyMemberBuilder().WithId("stranger").Build();

        Assert.Throws<ArgumentException>(() => CreateParty(maxActiveMembers: 4, _leader, stranger));
    }

    [Fact]
    public void Constructor_EmptyActiveOrder_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreateParty(maxActiveMembers: 4));
    }

    [Fact]
    public void Constructor_DuplicateActiveMember_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreateParty(maxActiveMembers: 4, _leader, _jazog, _jazog));
    }

    [Fact]
    public void TryAddToActive_NotRecruited_Throws()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader);
        var stranger = new PartyMemberBuilder().WithId("stranger").Build();

        Assert.Throws<ArgumentException>(() => party.TryAddToActive(stranger));
    }

    [Fact]
    public void TryAddToActive_RoomLeft_AppendsMember()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader);

        var added = party.TryAddToActive(_jazog);

        Assert.True(added);
        Assert.Equal([_leader, _jazog], party.ActiveMembers);
    }

    [Fact]
    public void TryAddToActive_PartyFull_ReturnsFalse()
    {
        var party = CreateParty(maxActiveMembers: 2, _leader, _jazog);

        var added = party.TryAddToActive(_lyra);

        Assert.False(added);
        Assert.Equal([_leader, _jazog], party.ActiveMembers);
    }

    [Fact]
    public void TryAddToActive_AlreadyActive_ReturnsFalse()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        var added = party.TryAddToActive(_jazog);

        Assert.False(added);
    }

    [Fact]
    public void TryMoveToReserve_ActiveMember_RemovesFromActive()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        var moved = party.TryMoveToReserve(_jazog);

        Assert.True(moved);
        Assert.Equal([_leader], party.ActiveMembers);
    }

    [Fact]
    public void TryMoveToReserve_Leader_ReturnsFalse()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        var moved = party.TryMoveToReserve(_leader);

        Assert.False(moved);
        Assert.Equal([_leader, _jazog], party.ActiveMembers);
    }

    [Fact]
    public void TryMoveToReserve_LastStanding_ReturnsFalseAndKeepsThemActive()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);
        KnockOut(_leader);

        var moved = party.TryMoveToReserve(_jazog);

        Assert.False(moved);
        Assert.Equal([_leader, _jazog], party.ActiveMembers);
    }

    [Fact]
    public void CanMoveToReserve_Leader_ReturnsFalse()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        Assert.False(party.CanMoveToReserve(_leader));
    }

    [Fact]
    public void CanMoveToReserve_ActiveMemberWithAnotherStanding_ReturnsTrue()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        Assert.True(party.CanMoveToReserve(_jazog));
    }

    [Fact]
    public void CanMoveToReserve_ReserveMember_ReturnsFalse()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        Assert.False(party.CanMoveToReserve(_lyra));
    }

    [Fact]
    public void CanMoveToReserve_LastStandingWithFallenLeader_ReturnsFalse()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog, _lyra);
        KnockOut(_leader);
        KnockOut(_lyra);

        Assert.False(party.CanMoveToReserve(_jazog));
    }

    [Fact]
    public void CanMoveToReserve_FallenMemberWithLeaderStanding_ReturnsTrue()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);
        KnockOut(_jazog);

        Assert.True(party.CanMoveToReserve(_jazog));
    }

    [Fact]
    public void CanChangePlace_Leader_ReturnsFalse()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        Assert.False(party.CanChangePlace(_leader));
    }

    [Fact]
    public void CanChangePlace_ActiveMember_ReturnsTrue()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        Assert.True(party.CanChangePlace(_jazog));
    }

    [Fact]
    public void CanChangePlace_ReserveMember_ReturnsFalse()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        Assert.False(party.CanChangePlace(_lyra));
    }

    [Fact]
    public void CanChangePlace_NotRecruited_Throws()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader);
        var stranger = new PartyMemberBuilder().WithId("stranger").Build();

        Assert.Throws<ArgumentException>(() => party.CanChangePlace(stranger));
    }

    [Fact]
    public void ReserveMembers_NewParty_AreTheInactiveRecruitsInRecruitOrder()
    {
        var party = new PartyState([_leader, _jazog, _lyra, _mira], [_leader, _lyra], new PartyRules(MaxActiveMembers: 3));

        Assert.Equal([_jazog, _mira], party.ReserveMembers);
    }

    [Fact]
    public void ReserveMembers_AfterTryAddToActive_LeavesOutTheJoiner()
    {
        var party = new PartyState([_leader, _jazog, _lyra, _mira], [_leader], new PartyRules(MaxActiveMembers: 3));

        party.TryAddToActive(_lyra);

        Assert.Equal([_jazog, _mira], party.ReserveMembers);
    }

    [Fact]
    public void ReserveMembers_AfterTryMoveToReserve_PutsTheMemberBackInRecruitOrder()
    {
        var party = new PartyState([_leader, _jazog, _lyra, _mira], [_leader, _lyra], new PartyRules(MaxActiveMembers: 3));

        party.TryMoveToReserve(_lyra);

        Assert.Equal([_jazog, _lyra, _mira], party.ReserveMembers);
    }

    [Fact]
    public void ReserveMembers_AfterRefusedTryAddToActive_Unchanged()
    {
        var party = new PartyState([_leader, _jazog, _lyra], [_leader, _jazog], new PartyRules(MaxActiveMembers: 2));

        party.TryAddToActive(_lyra);

        Assert.Equal([_lyra], party.ReserveMembers);
    }

    [Fact]
    public void Reorder_SameMembersLeaderFirst_ChangesOrder()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog, _lyra);

        party.Reorder([_leader, _lyra, _jazog]);

        Assert.Equal([_leader, _lyra, _jazog], party.ActiveMembers);
    }

    [Fact]
    public void Reorder_DifferentMembers_Throws()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        Assert.Throws<ArgumentException>(() => party.Reorder([_leader, _lyra]));
    }

    [Fact]
    public void Reorder_DuplicateMember_Throws()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog, _lyra);

        Assert.Throws<ArgumentException>(() => party.Reorder([_leader, _jazog, _jazog]));
    }

    [Fact]
    public void Reorder_LeaderNotFirst_Throws()
    {
        var party = CreateParty(maxActiveMembers: 3, _leader, _jazog);

        Assert.Throws<ArgumentException>(() => party.Reorder([_jazog, _leader]));
    }

    private static void KnockOut(PartyMember member) => member.Vitals.TakeDamage(member.Vitals.MaxHp);

    private PartyState CreateParty(int maxActiveMembers, params PartyMember[] activeOrder) =>
        new([_leader, _jazog, _lyra], activeOrder, new PartyRules(maxActiveMembers));
}
