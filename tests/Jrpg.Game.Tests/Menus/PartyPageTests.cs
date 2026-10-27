using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

/// <summary>On the repository's new game: Sarxian leads alone, six recruits wait in the reserve, and four can be active.</summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class PartyPageTests
{
    private readonly GameSession _session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    [Fact]
    public void Update_ConfirmReserveMember_AddsToParty()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Right));
        var joining = page.Selected;

        page.Update(Pressing(GameButton.Confirm));

        Assert.Equal([_session.Party.Leader, joining], _session.Party.ActiveMembers);
    }

    [Fact]
    public void Update_ConfirmReserveMember_PartyFull_AddsNobody()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Right));
        AddSelectedReserveMembers(page, count: 4);

        page.Update(Pressing(GameButton.Confirm));

        Assert.Equal(5, _session.Party.ActiveMembers.Count);
        Assert.Equal("The party is full.", page.Notice);
    }

    [Fact]
    public void Update_ConfirmActiveMember_OffersActionsWithoutMoving()
    {
        var page = NewPage();
        var second = PartyOfThree(page)[1];

        page.Update(Pressing(GameButton.Confirm));

        Assert.False(page.LeavesOnCancel);
        Assert.Contains(second, _session.Party.ActiveMembers);
    }

    [Fact]
    public void Update_ToReserveChosen_MovesToReserve()
    {
        var page = NewPage();
        var leaving = PartyOfThree(page)[1];
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Down));

        page.Update(Pressing(GameButton.Confirm));

        Assert.DoesNotContain(leaving, _session.Party.ActiveMembers);
        Assert.Equal(2, _session.Party.ActiveMembers.Count);
    }

    [Fact]
    public void Update_ToReserveOnLastStanding_RefusesAndSaysWhy()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Right));
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Left));
        page.Update(Pressing(GameButton.Down));
        var leader = _session.Party.Leader;
        leader.Vitals.TakeDamage(leader.Vitals.MaxHp);
        var lastStanding = page.Selected;
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Down));

        page.Update(Pressing(GameButton.Confirm));

        Assert.Equal([leader, lastStanding], _session.Party.ActiveMembers);
        Assert.Equal($"{lastStanding.Definition.Name} is the only one standing and can't leave the party.", page.Notice);
    }

    [Fact]
    public void Update_CancelInActions_ChangesNothing()
    {
        var page = NewPage();
        var before = PartyOfThree(page);
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Cancel));

        Assert.Equal(before, _session.Party.ActiveMembers);
        Assert.True(page.LeavesOnCancel);
    }

    [Fact]
    public void Update_ChangeOrder_SwapsTwoMembers()
    {
        var page = NewPage();
        var (leader, second, third) = Three(PartyOfThree(page));
        ChooseChangeOrder(page);
        page.Update(Pressing(GameButton.Down));

        page.Update(Pressing(GameButton.Confirm));

        Assert.Equal([leader, third, second], _session.Party.ActiveMembers);
        Assert.Null(page.MovingMember);
    }

    [Fact]
    public void Update_ChangeOrderUp_SkipsTheLeadersPlace()
    {
        var page = NewPage();
        var (leader, second, third) = Three(PartyOfThree(page));
        ChooseChangeOrder(page);

        page.Update(Pressing(GameButton.Up));
        page.Update(Pressing(GameButton.Confirm));

        Assert.Equal([leader, third, second], _session.Party.ActiveMembers);
    }

    [Fact]
    public void Update_CancelWhileMoving_KeepsOrderAndReturnsToTheMember()
    {
        var page = NewPage();
        var before = PartyOfThree(page);
        ChooseChangeOrder(page);
        page.Update(Pressing(GameButton.Down));

        page.Update(Pressing(GameButton.Cancel));

        Assert.Equal(before, _session.Party.ActiveMembers);
        Assert.Same(before[1], page.Selected);
        Assert.True(page.LeavesOnCancel);
    }

    [Fact]
    public void Update_LeaderSelected_OffersNoMove()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Confirm));

        Assert.Equal([_session.Party.Leader], _session.Party.ActiveMembers);
        Assert.Equal($"{_session.Party.Leader.Definition.Name} leads the party and always stays in it.", page.Notice);
    }

    // Decision 268: the notice is a dialog, so Down does nothing until it is closed.
    [Fact]
    public void Update_DownWhileNoticeOpen_KeepsTheNoticeAndTheCursor()
    {
        var page = NewPage();
        PartyOfThree(page);
        page.Update(Pressing(GameButton.Up));
        page.Update(Pressing(GameButton.Confirm));
        Assert.NotNull(page.Notice);

        page.Update(Pressing(GameButton.Down));

        Assert.NotNull(page.Notice);
        Assert.Same(_session.Party.Leader, page.Selected);
    }

    [Fact]
    public void Update_RightFromActive_SelectsReserveAtSameRow()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Right));
        AddSelectedReserveMembers(page, count: 2);
        page.Update(Pressing(GameButton.Left));
        page.Update(Pressing(GameButton.Down));
        page.Update(Pressing(GameButton.Down));
        var reserve = _session.Party.ReserveMembers.ToList();

        page.Update(Pressing(GameButton.Right));

        Assert.Same(reserve[2], page.Selected);
    }

    [Fact]
    public void Update_RightFromLowerThanReserve_SelectsLastReserveCard()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Right));
        AddSelectedReserveMembers(page, count: 4);
        var lastReserve = _session.Party.ReserveMembers[^1];
        page.Update(Pressing(GameButton.Left));
        page.Update(Pressing(GameButton.Up));

        page.Update(Pressing(GameButton.Right));

        Assert.Same(lastReserve, page.Selected);
    }

    [Fact]
    public void Open_AfterHpChanged_CardShowsTheNewHp()
    {
        var page = NewPage();
        var leader = _session.Party.Leader;
        leader.Vitals.TakeDamage(10);

        page.Open();

        Assert.Equal((float)leader.Vitals.Hp / leader.Vitals.MaxHp, page.CardOf(leader).Vitals.Hp.Bar.Fraction);
    }

    [Fact]
    public void Open_AfterMovingTheCursor_StartsOnTheLeader()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Right));

        page.Open();

        Assert.Same(_session.Party.Leader, page.Selected);
    }

    // The selected reserve card joins, and the next one slides into its place.
    private static void AddSelectedReserveMembers(PartyPage page, int count)
    {
        for (var added = 0; added < count; added++)
        {
            page.Update(Pressing(GameButton.Confirm));
        }
    }

    // Brings two reserve members in, then puts the cursor on the second active card; returns the active order.
    private IReadOnlyList<PartyMember> PartyOfThree(PartyPage page)
    {
        page.Update(Pressing(GameButton.Right));
        AddSelectedReserveMembers(page, count: 2);
        page.Update(Pressing(GameButton.Left));
        page.Update(Pressing(GameButton.Down));
        return [.. _session.Party.ActiveMembers];
    }

    // On the second active card: open its actions and take the first, "Change order".
    private static void ChooseChangeOrder(PartyPage page)
    {
        page.Update(Pressing(GameButton.Confirm));
        page.Update(Pressing(GameButton.Confirm));
    }

    private static (PartyMember First, PartyMember Second, PartyMember Third) Three(IReadOnlyList<PartyMember> members) =>
        (members[0], members[1], members[2]);

    private PartyPage NewPage() => new(RepositoryScreenText.Text, new UiTextures(), _session, StartMapScenarios.BelowClosedDoor);
}
