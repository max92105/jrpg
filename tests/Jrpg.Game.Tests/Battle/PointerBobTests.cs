using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

public sealed class PointerBobTests
{
    [Fact]
    public void OffsetAt_HalfPeriod_IsFullBob()
    {
        var offset = PointerBob.OffsetAt(TimeSpan.FromSeconds(0.45));

        Assert.Equal(8, offset, 3);
    }

    [Fact]
    public void OffsetAt_FullPeriod_IsBackAtRest()
    {
        var offset = PointerBob.OffsetAt(TimeSpan.FromSeconds(0.9));

        Assert.Equal(0, offset, 3);
    }

    [Fact]
    public void OffsetAt_EighthPeriod_EasesInSlowerThanLinear()
    {
        var offset = PointerBob.OffsetAt(TimeSpan.FromSeconds(0.1125));

        // 8 x (1 - cos 45°) / 2; a straight ramp would already be at 2.
        Assert.Equal(1.172f, offset, 3);
    }
}
