using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

public sealed class LevelUpPopTests
{
    [Fact]
    public void ScaleAt_LevelUp_IsOneAndAHalf()
    {
        Assert.Equal(1.5f, LevelUpPop.ScaleAt(TimeSpan.Zero));
    }

    [Fact]
    public void ScaleAt_PastPop_IsOwnSize()
    {
        Assert.Equal(1f, LevelUpPop.ScaleAt(TimeSpan.FromSeconds(0.5)));
    }
}
