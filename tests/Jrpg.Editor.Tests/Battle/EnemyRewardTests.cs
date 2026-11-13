using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// An enemy's gold (none, or a range), drop table (none or one of the draft's) and boss box; the ranges are Data's.
public sealed class EnemyRewardTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    // The tests' own rewards over the test world's goblin, whose numbers are the developer's to change.
    private static EnemyDefinition Pauper => _catalog.Enemies["goblin"] with { Gold = null, DropTableId = null, IsBoss = false };

    private static EnemyDefinition Rich => Pauper with { Gold = new GoldRange(6, 12), DropTableId = "goblin", IsBoss = true };

    [Fact]
    public void HasGold_EnemyWithoutGold_IsFalseWithZeroBoxes()
    {
        var enemy = EnemyViewModel.From(Pauper);

        Assert.False(enemy.HasGold);
        Assert.Equal(("0", "0"), (enemy.GoldMinText, enemy.GoldMaxText));
        Assert.Equal("", enemy.DropTableId);
        Assert.False(enemy.IsBoss);
    }

    [Fact]
    public void From_RichEnemy_ReadsGoldDropTableAndBoss()
    {
        var enemy = EnemyViewModel.From(Rich);

        Assert.True(enemy.HasGold);
        Assert.Equal(("6", "12"), (enemy.GoldMinText, enemy.GoldMaxText));
        Assert.Equal("goblin", enemy.DropTableId);
        Assert.True(enemy.IsBoss);
    }

    [Fact]
    public void ToEnemy_GoldTurnedOff_HasNone()
    {
        var enemy = EnemyViewModel.From(Rich);

        enemy.HasGold = false;

        Assert.Null(enemy.ToEnemy().Gold);
    }

    [Fact]
    public void ToEnemy_GoldTurnedOn_WritesTheTypedRange()
    {
        var enemy = EnemyViewModel.From(Pauper);

        enemy.HasGold = true;
        enemy.GoldMinText = "3";
        enemy.GoldMaxText = "9";

        Assert.Equal(new GoldRange(3, 9), enemy.ToEnemy().Gold);
    }

    [Fact]
    public void ToEnemy_DropTableNone_IsNull()
    {
        var enemy = EnemyViewModel.From(Rich);

        enemy.DropTableId = "";

        Assert.Null(enemy.ToEnemy().DropTableId);
    }

    [Fact]
    public void ToEnemy_DropTablePickedAndBossTicked_WritesThem()
    {
        var enemy = EnemyViewModel.From(Pauper);

        enemy.DropTableId = "goblin";
        enemy.IsBoss = true;

        var built = enemy.ToEnemy();
        Assert.Equal(("goblin", true), (built.DropTableId, built.IsBoss));
    }

    // Gold boxes that don't parse only count while the enemy drops gold.
    [Fact]
    public void Problem_GoldMinNotANumberWhileGoldOff_IsNull()
    {
        var enemy = EnemyViewModel.From(Pauper);

        enemy.GoldMinText = "x";

        Assert.Null(enemy.Problem);
    }

    [Fact]
    public void Problem_GoldMinNotANumberWhileGoldOn_NamesTheBox()
    {
        var enemy = EnemyViewModel.From(Rich);

        enemy.GoldMinText = "x";

        Assert.Equal("Gold min must be a whole number (was 'x').", enemy.Problem);
    }

    [Fact]
    public void Problem_GoldMaxBelowMin_IsDataMessage()
    {
        var onlyRich = _catalog with { Enemies = new Dictionary<string, EnemyDefinition> { ["goblin"] = Rich } };
        var catalog = new EnemyCatalogViewModel(onlyRich);

        catalog.Enemies[0].GoldMaxText = "4";

        Assert.Equal("enemies.json: enemy 'goblin': gold.max must be within 6..9999999 (was 4).", catalog.Problem);
    }

    [Fact]
    public void DropTableChoices_Draft_StartsWithNoneThenTablesById()
    {
        var choices = new EnemyCatalogViewModel(_catalog).DropTableChoices;

        Assert.Equal("", choices[0]);
        Assert.Contains("goblin", choices);
        Assert.Equal(choices.Skip(1).Order(StringComparer.Ordinal), choices.Skip(1));
    }

    [Fact]
    public void DuplicateOf_RichEnemy_CopiesGoldDropTableAndBoss()
    {
        var copy = EnemyViewModel.DuplicateOf(EnemyViewModel.From(Rich));

        Assert.Equal((true, "6", "12"), (copy.HasGold, copy.GoldMinText, copy.GoldMaxText));
        Assert.Equal(("goblin", true), (copy.DropTableId, copy.IsBoss));
    }
}
