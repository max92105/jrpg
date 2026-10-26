using Jrpg.Game.Scenes;

namespace Jrpg.Game.Tests.Scenes;

public sealed class OpeningFadeTests
{
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);
    private static readonly TimeSpan _duration = TimeSpan.FromSeconds(0.15);

    [Fact]
    public void Opacity_UpdatesBeforeFirstDraw_StaysClear()
    {
        var fade = new OpeningFade(_duration);

        for (var update = 0; update < 30; update++)
        {
            fade.Update(_oneUpdate);
        }

        Assert.Equal(0, fade.Opacity);
    }

    [Fact]
    public void Opacity_HalfTheDurationAfterFirstDraw_IsHalf()
    {
        var fade = new OpeningFade(_duration);
        fade.Update(_duration);
        fade.MarkDrawn();

        fade.Update(_duration / 2);

        Assert.Equal(0.5f, fade.Opacity, 3);
    }

    [Fact]
    public void Opacity_PastTheDurationAfterFirstDraw_IsOpaque()
    {
        var fade = new OpeningFade(_duration);
        fade.MarkDrawn();

        fade.Update(_duration * 2);

        Assert.Equal(1, fade.Opacity);
    }
}
