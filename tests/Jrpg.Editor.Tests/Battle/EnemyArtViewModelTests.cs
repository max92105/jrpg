using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// An enemy's three pictures, its flip and its idle loop as typed (decision 589's pattern, ADR 0047), carried into the
// record and the duplicate.
public sealed class EnemyArtViewModelTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    // The tests' own pictures over the test world's goblin.
    private static EnemyDefinition Wolf => _catalog.Enemies["goblin"] with
    {
        BattleSpriteFile = "Wolf.png",
        PortraitFile = "WolfFace.png",
        AttackEffectFile = "Bite.png",
        IsSpriteFlipped = true,
    };

    [Fact]
    public void From_Wolf_ReadsThreeFilesAndFlip()
    {
        var art = EnemyViewModel.From(Wolf).Art;

        Assert.Equal(("Wolf.png", "WolfFace.png", "Bite.png"), (art.BattleSpriteFile, art.PortraitFile, art.AttackEffectFile));
        Assert.True(art.IsSpriteFlipped);
    }

    [Fact]
    public void ToEnemy_FlipUntickedAndSpriteRetyped_WritesThem()
    {
        var enemy = EnemyViewModel.From(Wolf);

        enemy.Art.IsSpriteFlipped = false;
        enemy.Art.BattleSpriteFile = " DireWolf.png ";

        var built = enemy.ToEnemy();
        Assert.Equal(("DireWolf.png", false), (built.BattleSpriteFile, built.IsSpriteFlipped));
        Assert.Equal(("WolfFace.png", "Bite.png"), (built.PortraitFile, built.AttackEffectFile));
    }

    [Fact]
    public void DuplicateOf_Wolf_CopiesTheArt()
    {
        var copy = EnemyViewModel.DuplicateOf(EnemyViewModel.From(Wolf));

        Assert.Equal(("Wolf.png", true), (copy.Art.BattleSpriteFile, copy.Art.IsSpriteFlipped));
    }

    [Fact]
    public void New_EnemyWithLoop_FillsTheBoxes()
    {
        var art = EnemyViewModel.From(Wolf with { IdleAnimation = Loop(6, 120) }).Art;

        Assert.Equal((true, "6", "120"), (art.IsAnimated, art.FrameCountText, art.FrameMillisecondsText));
    }

    [Fact]
    public void New_EnemyWithoutLoop_IsNotAnimatedWithDefaultBoxes()
    {
        var art = EnemyViewModel.From(Wolf with { IdleAnimation = null }).Art;

        Assert.Equal((false, "2", "100"), (art.IsAnimated, art.FrameCountText, art.FrameMillisecondsText));
    }

    [Fact]
    public void ApplyTo_Animated_SetsIdleAnimation()
    {
        var enemy = EnemyViewModel.From(Wolf with { IdleAnimation = null });

        enemy.Art.IsAnimated = true;
        enemy.Art.FrameCountText = "8";
        enemy.Art.FrameMillisecondsText = " 90 ";

        Assert.Equal(Loop(8, 90), enemy.ToEnemy().IdleAnimation);
    }

    [Fact]
    public void ApplyTo_NotAnimated_ClearsIt()
    {
        var enemy = EnemyViewModel.From(Wolf with { IdleAnimation = Loop(6, 120) });

        enemy.Art.IsAnimated = false;

        Assert.Null(enemy.ToEnemy().IdleAnimation);
    }

    [Fact]
    public void Problem_AnimatedFramesNotANumber_NamesTheBox()
    {
        var enemy = EnemyViewModel.From(Wolf with { IdleAnimation = Loop(6, 120) });

        enemy.Art.FrameCountText = "six";

        Assert.Equal("Frames must be a whole number (was 'six').", enemy.Problem);
    }

    [Fact]
    public void Problem_NotAnimatedFramesNotANumber_IsNull()
    {
        var enemy = EnemyViewModel.From(Wolf with { IdleAnimation = null });

        enemy.Art.FrameCountText = "six";

        Assert.Null(enemy.Problem);
    }

    [Fact]
    public void PreviewLoop_BoxNotANumber_IsNull()
    {
        var art = EnemyViewModel.From(Wolf with { IdleAnimation = Loop(6, 120) }).Art;

        art.FrameMillisecondsText = "fast";

        Assert.Null(art.PreviewLoop);
    }

    // Data refuses a loop of no frames, but the preview reads the boxes as they are typed: no frame, nothing to play.
    [Fact]
    public void PreviewLoop_NoFrames_IsNull()
    {
        var art = EnemyViewModel.From(Wolf with { IdleAnimation = Loop(6, 120) }).Art;

        art.FrameCountText = "0";

        Assert.Null(art.PreviewLoop);
    }

    [Fact]
    public void PreviewLoop_Animated_IsTheBoxes()
    {
        var art = EnemyViewModel.From(Wolf with { IdleAnimation = Loop(6, 120) }).Art;

        art.FrameCountText = "1";

        Assert.Equal(Loop(1, 120), art.PreviewLoop);
    }

    [Fact]
    public void CopyFrom_Animated_CopiesTheLoop()
    {
        var copy = EnemyViewModel.DuplicateOf(EnemyViewModel.From(Wolf with { IdleAnimation = Loop(6, 120) }));

        Assert.Equal(Loop(6, 120), copy.Art.PreviewLoop);
    }

    // A picture change is the enemy's change, so the list's problem line and the marks follow it.
    [Fact]
    public void Art_FileChanged_RaisesTheEnemysArt()
    {
        var enemy = EnemyViewModel.From(Wolf);
        var notified = new List<string?>();
        enemy.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        enemy.Art.PortraitFile = "Elder.png";

        Assert.Contains(nameof(EnemyViewModel.Art), notified);
    }

    private static TileAnimation Loop(int frameCount, int frameMilliseconds) =>
        new() { FrameCount = frameCount, FrameMilliseconds = frameMilliseconds };
}
