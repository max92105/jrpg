using Jrpg.Core.Battle;
using Jrpg.Core.Maps;
using Jrpg.Game.Battle;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Battle;

// The animated sheet is four 960 x 540 frames in a 2 x 2 grid (1920 x 1080), 200 ms a frame, as the placeholder's.
public sealed class BattleBackdropTests
{
    private static readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(200);
    private static readonly Point _gridSheet = new(1920, 1080);

    private static readonly AnimatedBackgroundDefinition _pulse = new()
    {
        File = "Pulse.png",
        FrameWidth = 960,
        FrameHeight = 540,
        Animation = new TileAnimation { FrameCount = 4, FrameMilliseconds = 200 },
    };

    [Fact]
    public void SourceIn_StillImage_IsTheCoverFitOfTheWholeImage()
    {
        var backdrop = new BattleBackdrop("Wide.png", animation: null);

        var source = backdrop.SourceIn(new Point(2000, 1000));

        Assert.Equal(new Rectangle(111, 0, 1778, 1000), source);
    }

    [Fact]
    public void SourceIn_AnimatedAtTheStart_IsTheFirstCell()
    {
        var backdrop = new BattleBackdrop(_pulse.File, _pulse);

        var source = backdrop.SourceIn(_gridSheet);

        Assert.Equal(new Rectangle(0, 0, 960, 540), source);
    }

    [Fact]
    public void SourceIn_AnimatedAfterOneFrameTime_IsTheSecondCell()
    {
        var backdrop = new BattleBackdrop(_pulse.File, _pulse);
        backdrop.Advance(_frameTime);

        var source = backdrop.SourceIn(_gridSheet);

        Assert.Equal(new Rectangle(960, 0, 960, 540), source);
    }

    [Fact]
    public void SourceIn_AnimatedFrameOnTheSecondRow_IsOneRowDown()
    {
        var backdrop = new BattleBackdrop(_pulse.File, _pulse);
        backdrop.Advance(_frameTime * 2);

        var source = backdrop.SourceIn(_gridSheet);

        Assert.Equal(new Rectangle(0, 540, 960, 540), source);
    }

    [Fact]
    public void SourceIn_AnimatedAfterTheWholeLoop_IsTheFirstCellAgain()
    {
        var backdrop = new BattleBackdrop(_pulse.File, _pulse);
        backdrop.Advance(_frameTime * 4);

        var source = backdrop.SourceIn(_gridSheet);

        Assert.Equal(new Rectangle(0, 0, 960, 540), source);
    }

    [Fact]
    public void SourceIn_FrameNarrowerThanTheScreen_CropsTopAndBottomWithinTheCell()
    {
        var fourByThree = _pulse with { FrameWidth = 720, FrameHeight = 540 };
        var backdrop = new BattleBackdrop(fourByThree.File, fourByThree);
        backdrop.Advance(_frameTime * 2);

        var source = backdrop.SourceIn(new Point(1440, 1080));

        // 720 wide covers the screen at 405 tall: the cell at (0, 540) loses 67 px at its top.
        Assert.Equal(new Rectangle(0, 607, 720, 405), source);
    }

    [Fact]
    public void Advance_StillImage_SourceUnchanged()
    {
        var backdrop = new BattleBackdrop("Wide.png", animation: null);

        backdrop.Advance(_frameTime * 3);

        Assert.Equal(new Rectangle(111, 0, 1778, 1000), backdrop.SourceIn(new Point(2000, 1000)));
    }
}
