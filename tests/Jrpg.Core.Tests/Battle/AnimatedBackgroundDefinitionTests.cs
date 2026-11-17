using Jrpg.Core.Battle;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Battle;

// Where an animated background's frame sits in its grid sheet and which frame shows when (ADR 0059): 480 x 270 frames,
// left to right then top to bottom, as many columns as whole frames fit the image's width.
public sealed class AnimatedBackgroundDefinitionTests
{
    private const int EightColumnsWide = 3840;

    private static readonly AnimatedBackgroundDefinition _pulse = new()
    {
        File = "ForestPulse.png",
        FrameWidth = 480,
        FrameHeight = 270,
        Animation = new TileAnimation { FrameCount = 24, FrameMilliseconds = 100 },
    };

    [Fact]
    public void FrameOrigin_FirstFrame_IsTheTopLeft()
    {
        Assert.Equal((0, 0), _pulse.FrameOrigin(0, EightColumnsWide));
    }

    [Fact]
    public void FrameOrigin_LastFrameOfTheFirstRow_IsOnRowZero()
    {
        Assert.Equal((3360, 0), _pulse.FrameOrigin(7, EightColumnsWide));
    }

    [Fact]
    public void FrameOrigin_FirstFrameOfTheSecondRow_WrapsToColumnZero()
    {
        Assert.Equal((0, 270), _pulse.FrameOrigin(8, EightColumnsWide));
    }

    [Fact]
    public void FrameOrigin_LastFrame_IsOnTheThirdRow()
    {
        Assert.Equal((3360, 540), _pulse.FrameOrigin(23, EightColumnsWide));
    }

    [Fact]
    public void FrameOrigin_ImageWidthNotAWholeNumberOfFrames_UsesTheWholeColumns()
    {
        Assert.Equal((0, 270), _pulse.FrameOrigin(8, 4000));
    }

    [Fact]
    public void FrameOrigin_ImageExactlyOneFrameWide_StacksTheFrames()
    {
        Assert.Equal((0, 540), _pulse.FrameOrigin(2, 480));
    }

    [Fact]
    public void FrameOrigin_WidestImage_KeepsEveryFrameOnRowZero()
    {
        Assert.Equal((11040, 0), _pulse.FrameOrigin(23, int.MaxValue));
    }

    [Fact]
    public void FrameOrigin_ImageNarrowerThanAFrame_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _pulse.FrameOrigin(0, 479));
    }

    [Fact]
    public void FrameOrigin_FrameAtTheCount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _pulse.FrameOrigin(24, EightColumnsWide));
    }

    [Fact]
    public void FrameOrigin_FrameAtIntMaxValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _pulse.FrameOrigin(int.MaxValue, EightColumnsWide));
    }

    [Fact]
    public void FrameOrigin_RowPastIntMaxValue_ThrowsOverflow()
    {
        var tall = _pulse with { FrameHeight = int.MaxValue };

        Assert.Throws<OverflowException>(() => tall.FrameOrigin(2, tall.FrameWidth));
    }

    [Fact]
    public void FrameOrigin_NegativeFrame_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _pulse.FrameOrigin(-1, EightColumnsWide));
    }

    [Fact]
    public void FrameAt_LastFrameTime_IsTheLastFrame()
    {
        Assert.Equal(23, _pulse.FrameAt(TimeSpan.FromMilliseconds(2399)));
    }

    [Fact]
    public void FrameAt_PastTheLastFrame_LoopsToTheFirst()
    {
        Assert.Equal(0, _pulse.FrameAt(TimeSpan.FromMilliseconds(2400)));
    }
}
