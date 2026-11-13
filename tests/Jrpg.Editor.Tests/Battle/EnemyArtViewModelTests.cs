using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// An enemy's three pictures and its flip as typed (decision 589's pattern), carried into the record and the duplicate.
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
}
