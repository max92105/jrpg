using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// One enemy as the Enemies window edits it (ADR 0045): numbers as typed, the id fixed once saved, and everything the
// form doesn't edit carried through from the record, a duplicate included (decision 585).
public sealed class EnemyViewModelTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static EnemyDefinition Goblin => _catalog.Enemies["goblin"];

    private static EnemyDefinition Chief => _catalog.Enemies["goblin_chief"];

    [Fact]
    public void From_Goblin_ReadsItsNumbers()
    {
        var enemy = EnemyViewModel.From(Goblin);

        Assert.False(enemy.IsNew);
        Assert.Equal(("goblin", Goblin.Name), (enemy.Id, enemy.Name));
        Assert.Equal(TypedNumber.Text(Goblin.MaxHp), enemy.MaxHpText);
        Assert.Equal(TypedNumber.Text(Goblin.Accuracy), enemy.AccuracyText);
        Assert.Equal(Goblin.DamageType, enemy.DamageType);
        Assert.Equal($"goblin: {Goblin.Name}", enemy.Label);
        Assert.Null(enemy.Problem);
    }

    [Fact]
    public void ToEnemy_Unchanged_EqualsTheRecord()
    {
        var enemy = EnemyViewModel.From(Chief).ToEnemy();

        Assert.Equal(Chief with { Tactics = enemy.Tactics, Phases = enemy.Phases }, enemy);
        Assert.Same(Chief.Tactics, enemy.Tactics);
        Assert.Same(Chief.Phases, enemy.Phases);
    }

    [Fact]
    public void ToEnemy_MaxHpEdited_ChangesOnlyIt()
    {
        var enemy = EnemyViewModel.From(Goblin);

        enemy.MaxHpText = " 45 ";

        Assert.Equal(Goblin with { MaxHp = 45 }, enemy.ToEnemy());
    }

    [Fact]
    public void ToEnemy_NameEdited_TrimsIt()
    {
        var enemy = EnemyViewModel.From(Goblin);

        enemy.Name = " Hobgoblin ";

        Assert.Equal("Hobgoblin", enemy.ToEnemy().Name);
    }

    [Fact]
    public void New_Defaults_PassDataChecks()
    {
        var enemy = EnemyViewModel.New();
        enemy.Id = "slime";
        enemy.Name = "Slime";

        var built = enemy.ToEnemy();

        Assert.Null(CatalogProblems.OfEnemies([built], _catalog));
        Assert.Equal((1, 0, DamageType.Blunt, 75), (built.MaxHp, built.Damage, built.DamageType, built.Accuracy));
        Assert.Empty(built.Tactics);
    }

    [Fact]
    public void DuplicateOf_Goblin_CopiesNumbersWithEmptyId()
    {
        var copy = EnemyViewModel.DuplicateOf(EnemyViewModel.From(Goblin));

        Assert.True(copy.IsNew);
        Assert.Equal("", copy.Id);
        Assert.Equal((Goblin.Name, TypedNumber.Text(Goblin.MaxHp)), (copy.Name, copy.MaxHpText));
        Assert.Equal(Goblin.DamageType, copy.DamageType);
        Assert.Equal("(new enemy)", copy.Label);
    }

    [Fact]
    public void DuplicateOf_GoblinChief_CopiesTacticsAndPhases()
    {
        var copy = EnemyViewModel.DuplicateOf(EnemyViewModel.From(Chief));
        copy.Id = "wolf_chief";

        var built = copy.ToEnemy();

        Assert.Equal(Chief.Tactics, built.Tactics);
        Assert.Equal(Chief.Phases.Select(phase => phase.BelowHpPercent), built.Phases.Select(phase => phase.BelowHpPercent));
        Assert.Equal((Chief.IsBoss, Chief.BattleSpriteFile), (built.IsBoss, built.BattleSpriteFile));
        Assert.Equal(Chief.DropTableId, built.DropTableId);
    }

    // Duplicate copies the boxes as typed: a new enemy with no id, or a box that isn't a number, duplicates too.
    [Fact]
    public void DuplicateOf_NewWithoutId_CopiesTheTypedBoxes()
    {
        var typed = EnemyViewModel.New();
        typed.Name = "Slime";
        typed.MaxHpText = "abc";

        var copy = EnemyViewModel.DuplicateOf(typed);

        Assert.Equal(("", "Slime", "abc"), (copy.Id, copy.Name, copy.MaxHpText));
        Assert.Equal("The id must not be empty.", copy.Problem);
    }

    [Fact]
    public void Problem_NewWithoutId_AsksForIt()
    {
        var enemy = EnemyViewModel.New();

        Assert.Equal("The id must not be empty.", enemy.Problem);
        Assert.Throws<InvalidOperationException>(enemy.ToEnemy);
    }

    [Fact]
    public void Problem_MaxHpNotANumber_NamesTheBox()
    {
        var enemy = EnemyViewModel.From(Goblin);

        enemy.MaxHpText = "ten";

        Assert.Equal("Max HP must be a whole number (was 'ten').", enemy.Problem);
    }

    // A negative number parses; whether it is allowed is Data's rule, shown by the list.
    [Fact]
    public void Problem_NegativeDamage_IsNotTheFormsToRefuse()
    {
        var enemy = EnemyViewModel.From(Goblin);

        enemy.DamageText = "-3";

        Assert.Null(enemy.Problem);
        Assert.Equal(-3, enemy.ToEnemy().Damage);
    }
}
