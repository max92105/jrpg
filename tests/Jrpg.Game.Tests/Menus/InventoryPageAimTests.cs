using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// Using an item aims at the party sidebar's cards (decision 267). The list starts on the potion, which anyone standing
// can take, and the whole-party start has more recruits than cards in view.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class InventoryPageAimTests
{
    [Fact]
    public void Update_ConfirmPotion_PointsTheSidebarAtTheFirstCandidate()
    {
        var session = NewSession();
        var page = NewOpenPage(session);
        Assert.Null(page.Sidebar.PointedIndex);

        page.Update(Pressing(GameButton.Confirm));

        Assert.True(page.IsChoosingTarget);
        Assert.Same(MemberOrder.ActiveThenReserve(session.Party)[0], PointedMember(page));
    }

    [Fact]
    public void Update_DownWhileAiming_PointsTheSidebarAtTheNextCandidate()
    {
        var session = NewSession();
        var page = NewOpenPage(session);
        page.Update(Pressing(GameButton.Confirm));

        page.Update(Pressing(GameButton.Down));

        Assert.Same(MemberOrder.ActiveThenReserve(session.Party)[1], PointedMember(page));
    }

    [Fact]
    public void Update_DownToTheLastReserveMember_ScrollsTheSidebarToIt()
    {
        var page = NewOpenPage(NewSession());
        var cardCount = page.Sidebar.Cards.Count;
        Assert.True(cardCount > PartySidebar.VisibleCards, "The test needs more recruits than cards in view.");
        page.Update(Pressing(GameButton.Confirm));
        Assert.Equal(0, page.Sidebar.FirstVisible);

        for (var press = 1; press < cardCount; press++)
        {
            page.Update(Pressing(GameButton.Down));
        }

        Assert.Equal(cardCount - 1, page.Sidebar.PointedIndex);
        Assert.Equal(cardCount - PartySidebar.VisibleCards, page.Sidebar.FirstVisible);
    }

    [Fact]
    public void Hint_WhileAiming_NamesTheItem()
    {
        var page = NewOpenPage(NewSession());

        page.Update(Pressing(GameButton.Confirm));

        var potion = StartMapScenarios.BelowClosedDoor.Items["potion"].Name;
        Assert.Equal(RepositoryScreenText.Text.Messages.Format(MessageIds.MenuHintInventoryTarget, new MessageArgument("item", potion)), page.Hint);
    }

    [Fact]
    public void Hint_AfterCancel_IsTheListHint()
    {
        var page = NewOpenPage(NewSession());
        page.Update(Pressing(GameButton.Confirm));
        Assert.True(page.IsChoosingTarget);

        page.Update(Pressing(GameButton.Cancel));

        Assert.Equal(RepositoryScreenText.Text.Messages.Text(MessageIds.MenuHintInventory), page.Hint);
        Assert.Null(page.Sidebar.PointedIndex);
    }

    private static PartyMember? PointedMember(InventoryPage page) =>
        page.Sidebar.PointedIndex is { } index ? page.Sidebar.Cards[index].Member : null;

    private static GameSession NewSession() => StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    private static InventoryPage NewOpenPage(GameSession session)
    {
        var page = new InventoryPage(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
        page.Open();
        return page;
    }
}
