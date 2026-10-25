using Jrpg.Game.Battle;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Battle;

// A battler slides from x 100 to x 300 over 0.25 s, easing out: past the halfway point at half the time.
public sealed class FormationSlideTests
{
    private static readonly Rectangle _from = new(100, 50, 40, 80);
    private static readonly Rectangle _to = new(300, 50, 40, 80);

    [Fact]
    public void SpotOf_Halfway_IsBetweenSpots()
    {
        var slide = new FormationSlide();
        slide.Restart();

        slide.Update(FormationSlide.Duration / 2);

        var x = slide.SpotOf(_from, _to).X;
        Assert.InRange(x, 201, 299);
    }

    [Fact]
    public void SpotOf_AfterSlide_IsNewSpot()
    {
        var slide = new FormationSlide();
        slide.Restart();

        slide.Update(FormationSlide.Duration);

        Assert.Equal(_to, slide.SpotOf(_from, _to));
    }

    [Fact]
    public void SpotOf_NeverRestarted_IsNewSpot()
    {
        Assert.Equal(_to, new FormationSlide().SpotOf(_from, _to));
    }
}
