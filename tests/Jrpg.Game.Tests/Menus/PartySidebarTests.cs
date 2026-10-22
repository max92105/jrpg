using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class PartySidebarTests
{
    private readonly PartyState _party = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1)).Party;

    [Fact]
    public void Cards_ActiveMembers_InPartyOrder()
    {
        var joining = _party.ReserveMembers[0];
        _party.TryAddToActive(joining);

        var sidebar = NewSidebar();

        Assert.Equal([_party.Leader, joining], sidebar.Cards.Select(card => card.Member));
    }

    [Fact]
    public void Cards_ReserveMember_IsNotListed()
    {
        var sidebar = NewSidebar();

        Assert.DoesNotContain(sidebar.Cards, card => _party.ReserveMembers.Contains(card.Member));
    }

    [Fact]
    public void Cards_Leader_ShowsLevelAndVitals()
    {
        var leader = _party.Leader;

        var card = NewSidebar().Cards[0];

        Assert.Equal(leader.Definition.Name, card.Name);
        Assert.Equal($"Lv {leader.Level}", card.Level);
        Assert.Equal($"HP {leader.Vitals.Hp} / {leader.Vitals.MaxHp}", card.Hp);
        Assert.Equal($"MP {leader.Vitals.Mp} / {leader.Vitals.MaxMp}", card.Mp);
    }

    [Fact]
    public void Refresh_NothingChanged_KeepsTheSameCards()
    {
        var sidebar = NewSidebar();
        var before = sidebar.Cards[0];

        sidebar.Refresh();

        Assert.Same(before, sidebar.Cards[0]);
    }

    [Fact]
    public void Refresh_MemberHealed_RebuildsOnlyThatCard()
    {
        var joining = _party.ReserveMembers[0];
        _party.TryAddToActive(joining);
        joining.Vitals.TakeDamage(5);
        var sidebar = NewSidebar();
        var leaderCard = sidebar.Cards[0];

        joining.Vitals.RestoreHp(5);
        sidebar.Refresh();

        Assert.Same(leaderCard, sidebar.Cards[0]);
        Assert.Equal($"HP {joining.Vitals.Hp} / {joining.Vitals.MaxHp}", sidebar.Cards[1].Hp);
    }

    [Fact]
    public void Refresh_ActiveOrderChanged_ReordersTheCards()
    {
        var first = _party.ReserveMembers[0];
        var second = _party.ReserveMembers[1];
        _party.TryAddToActive(first);
        _party.TryAddToActive(second);
        var sidebar = NewSidebar();

        _party.Reorder([_party.Leader, second, first]);
        sidebar.Refresh();

        Assert.Equal([_party.Leader, second, first], sidebar.Cards.Select(card => card.Member));
    }

    private PartySidebar NewSidebar() => new(_party, RepositoryScreenText.Text.Messages);
}
