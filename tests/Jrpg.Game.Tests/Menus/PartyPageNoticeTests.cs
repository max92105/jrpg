using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// The Party page's refusals open a notice dialog with one OK (decisions 268, 282). Confirm on the leader opens it: the
// leader always stays and can't change place.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class PartyPageNoticeTests
{
    private readonly GameSession _session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    [Fact]
    public void Update_ConfirmOnNotice_ClosesIt()
    {
        var page = NewPageWithNotice();

        page.Update(Pressing(GameButton.Confirm));

        Assert.Null(page.Notice);
    }

    [Fact]
    public void Update_CancelOnNotice_ClosesIt()
    {
        var page = NewPageWithNotice();

        page.Update(Pressing(GameButton.Cancel));

        Assert.Null(page.Notice);
        Assert.True(page.LeavesOnCancel);
    }

    [Fact]
    public void LeavesOnCancel_NoticeOpen_IsFalse()
    {
        var page = NewPageWithNotice();

        Assert.False(page.LeavesOnCancel);
    }

    [Fact]
    public void Update_ConfirmOnNotice_DoesNotConfirmTheMemberAgain()
    {
        var page = NewPageWithNotice();

        page.Update(Pressing(GameButton.Confirm));

        Assert.Null(page.Notice);
        Assert.Same(_session.Party.Leader, page.Selected);
        Assert.True(page.LeavesOnCancel);
    }

    private PartyPage NewPageWithNotice()
    {
        var page = new PartyPage(RepositoryScreenText.Text, new UiTextures(), _session, StartMapScenarios.BelowClosedDoor);
        page.Open();
        page.Update(Pressing(GameButton.Confirm));
        Assert.NotNull(page.Notice);
        return page;
    }
}
