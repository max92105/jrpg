using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Input;

namespace Jrpg.Game.Tests.Battle;

// The notice shows for 1.5 s.
public sealed class EscapedNoticeTests
{
    private readonly EscapedNotice _notice = new();

    [Fact]
    public void Update_BeforeDisplayTime_StaysUp()
    {
        Assert.Null(_notice.Update(TimeSpan.FromSeconds(1.4), default));
    }

    [Fact]
    public void Update_PastDisplayTime_IsDone()
    {
        _notice.Update(TimeSpan.FromSeconds(1.4), default);

        var request = _notice.Update(TimeSpan.FromSeconds(0.1), default);

        Assert.IsType<BattleFinished>(request);
    }

    [Fact]
    public void Update_Confirm_IsDone()
    {
        var request = _notice.Update(TimeSpan.FromSeconds(0.1), InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<BattleFinished>(request);
    }

    [Fact]
    public void Update_AfterDone_ReturnsNull()
    {
        _notice.Update(TimeSpan.FromSeconds(2), default);

        Assert.Null(_notice.Update(TimeSpan.FromSeconds(1), default));
    }
}
