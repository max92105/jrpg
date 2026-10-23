using Jrpg.Core.Content;
using Jrpg.Game.Input;
using Jrpg.Game.NewGame;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.NewGame;

public sealed class LeaderChoiceMenuTests
{
    private static readonly LeaderChoice[] _choices = [Choice("ragnir"), Choice("omer"), Choice("zanyr")];

    [Fact]
    public void Selected_New_IsFirstChoice()
    {
        var menu = new LeaderChoiceMenu(_choices);

        Assert.Same(_choices[0], menu.Selected);
    }

    [Fact]
    public void Update_Down_SelectsNextChoice()
    {
        var menu = new LeaderChoiceMenu(_choices);

        menu.Update(Pressing(GameButton.Down));

        Assert.Same(_choices[1], menu.Selected);
    }

    [Fact]
    public void Update_UpOnFirst_WrapsToLast()
    {
        var menu = new LeaderChoiceMenu(_choices);

        menu.Update(Pressing(GameButton.Up));

        Assert.Same(_choices[2], menu.Selected);
    }

    [Fact]
    public void Update_Confirm_ReturnsSelectedChoice()
    {
        var menu = new LeaderChoiceMenu(_choices);
        menu.Update(Pressing(GameButton.Down));

        var chosen = menu.Update(Pressing(GameButton.Confirm));

        Assert.Same(_choices[1], chosen);
    }

    [Fact]
    public void Update_NothingPressed_ReturnsNothing()
    {
        var menu = new LeaderChoiceMenu(_choices);

        Assert.Null(menu.Update(default));
    }

    private static LeaderChoice Choice(string memberId) => new() { MemberId = memberId };
}
