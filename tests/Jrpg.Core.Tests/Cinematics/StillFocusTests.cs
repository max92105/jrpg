using Jrpg.Core.Cinematics;
using Jrpg.Core.Geometry;

namespace Jrpg.Core.Tests.Cinematics;

public sealed class StillFocusTests
{
    [Fact]
    public void Share_TwentyFiveAndSeventyFive_IsAQuarterAndThreeQuarters()
    {
        var focus = new StillFocus { XPercent = 25, YPercent = 75 };

        Assert.Equal(new Point2D(0.25, 0.75), focus.Share);
    }

    [Fact]
    public void Center_IsHalfAndHalf()
    {
        Assert.Equal(new Point2D(0.5, 0.5), StillFocus.Center.Share);
    }
}
