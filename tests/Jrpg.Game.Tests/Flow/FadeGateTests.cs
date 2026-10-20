using Jrpg.Game.Flow;
using Jrpg.Game.Scenes;

namespace Jrpg.Game.Tests.Flow;

public sealed class FadeGateTests
{
    // 0.2 s out, 0.1 s on black, 0.2 s in: 0.5 s in all.
    private readonly FadeGate _gate = new(new ScreenFade(TimeSpan.FromSeconds(0.2), TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.2)));

    [Fact]
    public void TakeDueAtBlack_FadeReachesBlack_ReturnsRequestOnce()
    {
        var newGame = new NewGameRequested();
        _gate.StartBehindFade(newGame);
        _gate.Update(TimeSpan.FromSeconds(0.25));

        var due = _gate.TakeDueAtBlack();

        Assert.Same(newGame, due);
        Assert.Null(_gate.TakeDueAtBlack());
    }

    [Fact]
    public void TakeDueAtBlack_StillFadingOut_ReturnsNull()
    {
        _gate.StartBehindFade(new NewGameRequested());
        _gate.Update(TimeSpan.FromSeconds(0.1));

        Assert.Null(_gate.TakeDueAtBlack());
    }

    [Fact]
    public void TakeReleased_RequestHeldDuringFade_ReturnsItOnceFadeEnds()
    {
        var closed = new MenuClosed();
        _gate.StartBehindFade(new NewGameRequested());
        _gate.Update(TimeSpan.FromSeconds(0.25));
        _gate.Hold(closed);
        _gate.Update(TimeSpan.FromSeconds(0.25));

        var released = _gate.TakeReleased();

        Assert.Same(closed, released);
        Assert.Null(_gate.TakeReleased());
    }

    [Fact]
    public void TakeReleased_FadeStillPlaying_ReturnsNull()
    {
        _gate.StartBehindFade(new NewGameRequested());
        _gate.Hold(new MenuClosed());
        _gate.Update(TimeSpan.FromSeconds(0.45));

        Assert.Null(_gate.TakeReleased());
    }

    [Fact]
    public void Hold_SecondRequestWhileOneWaits_Throws()
    {
        _gate.StartBehindFade(new NewGameRequested());
        _gate.Hold(new MenuClosed());

        Assert.Throws<InvalidOperationException>(() => _gate.Hold(new TitleRequested()));
    }
}
