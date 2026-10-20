using Jrpg.Core.Randomness;
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
    private readonly GameSession _session = GameSession.StartNew(StartMapScenarios.BelowClosedDoor, new SeededRandom(1));

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
        AddSelectedReserveMembers(page, count: 3);

        page.Update(Pressing(GameButton.Confirm));

        Assert.Equal(4, _session.Party.ActiveMembers.Count);
        Assert.Equal("The party is full.", page.Notice);
    }

    [Fact]
    public void Update_ConfirmActiveMember_MovesToReserve()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Right));
        AddSelectedReserveMembers(page, count: 1);
        page.Update(Pressing(GameButton.Left));
        page.Update(Pressing(GameButton.Down));
        var leaving = page.Selected;

        page.Update(Pressing(GameButton.Confirm));

        Assert.DoesNotContain(leaving, _session.Party.ActiveMembers);
        Assert.Single(_session.Party.ActiveMembers);
    }

    [Fact]
    public void Update_LeaderSelected_OffersNoMove()
    {
        var page = NewPage();

        page.Update(Pressing(GameButton.Confirm));

        Assert.Equal([_session.Party.Leader], _session.Party.ActiveMembers);
        Assert.Equal($"{_session.Party.Leader.Definition.Name} leads the party and always stays in it.", page.Notice);
    }

    [Fact]
    public void Update_MovingAfterRefusal_ClearsTheNotice()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Right));

        Assert.Null(page.Notice);
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
        var reserve = _session.Party.Members.Except(_session.Party.ActiveMembers).ToList();

        page.Update(Pressing(GameButton.Right));

        Assert.Same(reserve[2], page.Selected);
    }

    [Fact]
    public void Update_RightFromLowerThanReserve_SelectsLastReserveCard()
    {
        var page = NewPage();
        page.Update(Pressing(GameButton.Right));
        AddSelectedReserveMembers(page, count: 3);
        var lastReserve = _session.Party.Members.Except(_session.Party.ActiveMembers).Last();
        page.Update(Pressing(GameButton.Left));
        page.Update(Pressing(GameButton.Up));

        page.Update(Pressing(GameButton.Right));

        Assert.Same(lastReserve, page.Selected);
    }

    // The selected reserve card joins, and the next one slides into its place.
    private static void AddSelectedReserveMembers(PartyPage page, int count)
    {
        for (var added = 0; added < count; added++)
        {
            page.Update(Pressing(GameButton.Confirm));
        }
    }

    private PartyPage NewPage() => new(RepositoryScreenText.Text, new UiTextures(), _session, StartMapScenarios.BelowClosedDoor);
}
