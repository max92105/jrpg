using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// The member column's highlight glides between members (decision 262) and starts on the first one when a page opens.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameMenuSceneMemberColumnTests
{
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);
    private static readonly TimeSpan _longAfter = TimeSpan.FromSeconds(1);

    [Fact]
    public void Update_LongAfterRightOnStatus_HighlightReachesTheNextMember()
    {
        var menu = NewStatusMenu();
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));
        menu.Update(_oneUpdate, Pressing(GameButton.Right));

        menu.Update(_longAfter, default);

        Assert.Equal(1, menu.MemberColumn.Selection.Position);
    }

    [Fact]
    public void Update_ReenteringStatus_HighlightIsOnTheFirstMemberAtOnce()
    {
        var menu = NewStatusMenu();
        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));
        menu.Update(_oneUpdate, Pressing(GameButton.Right));
        menu.Update(_longAfter, default);
        menu.Update(_oneUpdate, Pressing(GameButton.Cancel));

        menu.Update(_oneUpdate, Pressing(GameButton.Confirm));

        Assert.Equal(0, menu.MemberColumn.Selection.Position);
    }

    private static GameMenuScene NewStatusMenu()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        var status = new StatusPage(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
        return new(RepositoryScreenText.Text, new UiTextures(), new MenuHeader(RepositoryScreenText.Text, session), [status]);
    }
}
