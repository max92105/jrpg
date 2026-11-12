using Jrpg.Game.Exploration;

namespace Jrpg.Game.Tests.Exploration;

public sealed class WalkAnimationTests
{
    private readonly WalkAnimation _animation = new();

    [Fact]
    public void Advance_TwoDeltas_AddsThemUp()
    {
        _animation.Advance(TimeSpan.FromSeconds(0.06));

        _animation.Advance(TimeSpan.FromSeconds(0.06));

        Assert.Equal(TimeSpan.FromSeconds(0.12), _animation.Walked);
    }

    [Fact]
    public void Stand_AfterWalking_ResetsWalked()
    {
        _animation.Advance(TimeSpan.FromSeconds(0.35));

        _animation.Stand();

        Assert.Equal(TimeSpan.Zero, _animation.Walked);
    }
}
