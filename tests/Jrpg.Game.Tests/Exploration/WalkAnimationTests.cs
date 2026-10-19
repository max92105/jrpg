using Jrpg.Core.Geometry;
using Jrpg.Game.Exploration;

namespace Jrpg.Game.Tests.Exploration;

public sealed class WalkAnimationTests
{
    private static readonly TimeSpan _tenthOfASecond = TimeSpan.FromSeconds(0.1);

    private readonly WalkAnimation _animation = new();

    [Fact]
    public void Stand_ShowsFirstFrame()
    {
        _animation.Advance(TimeSpan.FromSeconds(0.35));

        _animation.Stand();

        Assert.Equal(0, _animation.Frame);
    }

    [Fact]
    public void Advance_TenthOfASecond_AdvancesOneFrame()
    {
        _animation.Advance(_tenthOfASecond);

        Assert.Equal(1, _animation.Frame);
    }

    [Fact]
    public void Advance_LeftoverFromLastFrame_CountsTowardNext()
    {
        _animation.Advance(TimeSpan.FromSeconds(0.06));
        _animation.Advance(TimeSpan.FromSeconds(0.06));

        _animation.Advance(TimeSpan.FromSeconds(0.08));

        Assert.Equal(2, _animation.Frame);
    }

    [Fact]
    public void Advance_PastLastFrame_WrapsToFirst()
    {
        _animation.Advance(TimeSpan.FromSeconds(0.7));
        Assert.Equal(7, _animation.Frame);

        _animation.Advance(_tenthOfASecond);

        Assert.Equal(0, _animation.Frame);
    }

    [Theory]
    [InlineData(Facing.Down, 0)]
    [InlineData(Facing.Left, 1)]
    [InlineData(Facing.Right, 2)]
    [InlineData(Facing.Up, 3)]
    public void RowOf_EveryFacing_FollowsSheetOrder(Facing facing, int row)
    {
        Assert.Equal(row, WalkAnimation.RowOf(facing));
    }
}
