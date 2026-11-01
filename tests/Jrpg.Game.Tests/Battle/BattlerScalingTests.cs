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

    // A boss looks bigger only through its art (decision 408): art past 512 px would draw as a painting and fail here.
    [Fact]
    public void Of_RepositoryChiefArt_IsTwiceTheGoblinsDrawnSize()
    {
        var chief = BattlerScaling.Of(SizeOfBattlerOf("goblin_chief"));
        var goblin = BattlerScaling.Of(SizeOfBattlerOf("goblin"));

        Assert.False(chief.IsShrunkSmoothly);
        Assert.Equal(new Point(goblin.DrawnSize.X * 2, goblin.DrawnSize.Y * 2), chief.DrawnSize);
    }

    private static Point SizeOfBattlerOf(string enemyId)
    {
        var file = BattleScenarios.Catalog.Enemies[enemyId].BattleSpriteFile;
        var (width, height) = PngSize.Read(Path.Combine(AppContext.BaseDirectory, "content", "art", "battlers", file));
        return new Point(width, height);
    }
}
