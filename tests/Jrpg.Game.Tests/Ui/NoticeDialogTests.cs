using Jrpg.Game.Input;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Ui;

public sealed class NoticeDialogTests
{
    [Fact]
    public void IsClosedBy_Confirm_IsTrue()
    {
        Assert.True(NoticeDialog.IsClosedBy(Pressing(GameButton.Confirm)));
    }

    [Fact]
    public void IsClosedBy_Cancel_IsTrue()
    {
        Assert.True(NoticeDialog.IsClosedBy(Pressing(GameButton.Cancel)));
    }

    [Fact]
    public void IsClosedBy_Down_IsFalse()
    {
        Assert.False(NoticeDialog.IsClosedBy(Pressing(GameButton.Down)));
    }
}
