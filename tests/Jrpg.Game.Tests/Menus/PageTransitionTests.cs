using Jrpg.Game.Menus;

namespace Jrpg.Game.Tests.Menus;

public sealed class PageTransitionTests
{
    private readonly PageTransition _transition = new();

    [Fact]
    public void Update_AtStart_IsOffsetAndTransparent()
    {
        _transition.Start();

        _transition.Update(TimeSpan.Zero);

        Assert.Equal(PageTransition.SlideDistance, _transition.Offset);
        Assert.Equal(0f, _transition.Opacity);
    }

    [Fact]
    public void Update_PastTheDuration_IsInPlaceAndOpaque()
    {
        _transition.Start();

        _transition.Update(PageTransition.Duration * 2);

        Assert.Equal(0, _transition.Offset);
        Assert.Equal(1f, _transition.Opacity);
    }

    [Fact]
    public void Update_Halfway_IsEasedOut()
    {
        _transition.Start();

        _transition.Update(PageTransition.Duration / 2);

        Assert.Equal(3, _transition.Offset);
        Assert.Equal(0.875f, _transition.Opacity, 3);
    }

    [Fact]
    public void Start_AgainMidway_RestartsFromTheBeginning()
    {
        _transition.Start();
        _transition.Update(PageTransition.Duration / 2);

        _transition.Start();

        Assert.Equal(PageTransition.SlideDistance, _transition.Offset);
        Assert.Equal(0f, _transition.Opacity);
    }
}
