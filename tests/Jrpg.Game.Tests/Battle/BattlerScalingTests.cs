using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Game.Battle;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Battle;

public sealed class BattlerScalingTests
{
    [Fact]
    public void Of_1024PxPainting_ShrinksSmoothlyTo96()
    {
        var scaling = BattlerScaling.Of(new Point(1024, 1024));

        Assert.Equal(new BattlerScaling(new Point(96, 96), IsShrunkSmoothly: true), scaling);
    }

    [Fact]
    public void Of_512PxImage_IsStillPixelArt()
    {
        var scaling = BattlerScaling.Of(new Point(512, 512));

        Assert.Equal(new BattlerScaling(new Point(256, 256), IsShrunkSmoothly: false), scaling);
    }

    [Fact]
    public void Of_256PxPixelArt_PointScalesByHalf()
    {
        var scaling = BattlerScaling.Of(new Point(256, 256));

        Assert.Equal(new BattlerScaling(new Point(128, 128), IsShrunkSmoothly: false), scaling);
    }

    // An animated strip is already at its battle height (decision 637): a frame past 512 px isn't taken for a painting.
    [Fact]
    public void AtOwnSize_FrameOver512_IsNotShrunk()
    {
        var scaling = BattlerScaling.AtOwnSize(new Point(600, 700));

        Assert.Equal(new BattlerScaling(new Point(600, 700), IsShrunkSmoothly: false), scaling);
    }

    // A boss looks bigger only through its art (decision 408): art past 512 px would draw as a painting and fail here.
    [Fact]
    public void OfImage_RepositoryChiefArt_IsTwiceTheGoblinsDrawnSize()
    {
        var chief = ScalingOfBattlerOf("goblin_chief");
        var goblin = ScalingOfBattlerOf("gnoll");

        Assert.False(chief.IsShrunkSmoothly);
        Assert.Equal(new Point(goblin.DrawnSize.X * 2, goblin.DrawnSize.Y * 2), chief.DrawnSize);
    }

    [Fact]
    public void OfImage_LoopingStrip_IsOneFrameAtItsOwnSize()
    {
        var loop = new TileAnimation { FrameCount = 3, FrameMilliseconds = 220 };

        Assert.Equal(new BattlerScaling(new Point(128, 128), false), BattlerScaling.OfImage(new Point(384, 128), loop));
    }

    [Fact]
    public void OfImage_StillImage_IsTheWholeImagesScaling()
    {
        Assert.Equal(BattlerScaling.Of(new Point(256, 256)), BattlerScaling.OfImage(new Point(256, 256), null));
    }

    // As the battle draws the repository's enemy: an animated one's frame, a still one's whole image.
    private static BattlerScaling ScalingOfBattlerOf(string enemyId)
    {
        var enemy = BattleScenarios.Catalog.Enemies[enemyId];
        var path = Path.Combine(AppContext.BaseDirectory, "content", "art", "battlers", enemy.BattleSpriteFile);
        var (width, height) = PngSize.Read(path);
        return BattlerScaling.OfImage(new Point(width, height), enemy.IdleAnimation);
    }
}
