using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class PointerBobTests
{
    private const float Distance = 8;

    [Fact]
    public void OffsetAt_HalfPeriod_IsFullBob()
    {
        var offset = PointerBob.OffsetAt(TimeSpan.FromSeconds(0.45), Distance);

        Assert.Equal(8, offset, 3);
    }

    [Fact]
    public void OffsetAt_FullPeriod_IsBackAtRest()
    {
        var offset = PointerBob.OffsetAt(TimeSpan.FromSeconds(0.9), Distance);

        Assert.Equal(0, offset, 3);
    }

    [Fact]
    public void OffsetAt_EighthPeriod_EasesInSlowerThanLinear()
    {
        var offset = PointerBob.OffsetAt(TimeSpan.FromSeconds(0.1125), Distance);

        // 8 x (1 - cos 45°) / 2; a straight ramp would already be at 2.
        Assert.Equal(1.172f, offset, 3);
    }
}
