using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

// The banner: 0.15 s in, 1.2 s shown, 0.3 s out.
public sealed class BattleBannerTests
{
    private readonly BattleBanner _banner = new();

    [Fact]
    public void Opacity_BeforeAnyMessage_IsHidden()
    {
        Assert.Equal(0, _banner.Opacity);
    }

    [Fact]
    public void Update_DuringHold_IsFullyShown()
    {
        _banner.Show("Fireball");

        _banner.Update(TimeSpan.FromSeconds(1));

        Assert.Equal(1, _banner.Opacity);
    }

    [Fact]
    public void Update_PastHold_FadesOut()
    {
        _banner.Show("Fireball");

        _banner.Update(TimeSpan.FromSeconds(1.5));

        Assert.Equal(0.5f, _banner.Opacity, 3);
    }
}
