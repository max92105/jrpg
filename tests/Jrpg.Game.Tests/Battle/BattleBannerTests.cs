using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

// The plaque (decision 646): it pops in at full opacity, stays 1.5 s and vanishes at once.
public sealed class BattleBannerTests
{
    private readonly BattleBanner _banner = new();

    [Fact]
    public void Opacity_BeforeAnyMessage_IsHidden()
    {
        Assert.Equal(0, _banner.Opacity);
    }

    [Fact]
    public void Show_AtOnce_IsFullyShown()
    {
        _banner.Show("Attack");

        Assert.Equal(1, _banner.Opacity);
    }

    [Fact]
    public void Update_DuringHold_IsFullyShown()
    {
        _banner.Show("Fireball");

        _banner.Update(TimeSpan.FromSeconds(1));

        Assert.Equal(1, _banner.Opacity);
    }

    // Both sides of the end: shown a tick before 1.5 s, hidden at 1.5 s.
    [Fact]
    public void Update_PastItsTime_IsHidden()
    {
        _banner.Show("Fireball");
        _banner.Update(TimeSpan.FromSeconds(1.5) - TimeSpan.FromTicks(1));
        Assert.Equal(1, _banner.Opacity);

        _banner.Update(TimeSpan.FromTicks(1));

        Assert.Equal(0, _banner.Opacity);
    }
}
