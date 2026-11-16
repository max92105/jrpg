using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Battle;

// One enemy as the Enemies page edits it (ADR 0045): numbers and rules as typed, the id fixed once saved, and a
// duplicate copying the boxes as typed (decision 585).
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

    // The rules are rebuilt from their rows, so they equal the record's by content. A phase compares its list by
    // reference, so each phase's threshold and rules are compared apart, phase by phase.
    [Fact]
    public void ToEnemy_Unchanged_EqualsTheRecord()
    {
        var enemy = EnemyViewModel.From(Chief).ToEnemy();

        Assert.Equal(Chief with { Tactics = enemy.Tactics, Phases = enemy.Phases }, enemy);
        Assert.Equal(Chief.Tactics, enemy.Tactics);
        Assert.Equal(Chief.Phases.Select(phase => phase.BelowHpPercent), enemy.Phases.Select(phase => phase.BelowHpPercent));
        Assert.All(Chief.Phases.Zip(enemy.Phases), pair => Assert.Equal(pair.First.Tactics, pair.Second.Tactics));
    }

    // The rules are rebuilt from their rows into new lists, which a record compares by reference, so they are compared
    // by content apart, as in ToEnemy_Unchanged_EqualsTheRecord.
    [Fact]
    public void ToEnemy_MaxHpEdited_ChangesOnlyIt()
    {
        var enemy = EnemyViewModel.From(Goblin);

        enemy.MaxHpText = " 45 ";

        var built = enemy.ToEnemy();
        Assert.Equal(Goblin with { MaxHp = 45, Tactics = built.Tactics, Phases = built.Phases }, built);
        Assert.Equal(Goblin.Tactics, built.Tactics);
        Assert.Equal(Goblin.Phases.SelectMany(phase => phase.Tactics), built.Phases.SelectMany(phase => phase.Tactics));
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

    [Fact]
    public void From_EnemyWithPower_ShowsItsPower()
    {
        Assert.Equal("5", EnemyViewModel.From(Goblin with { Power = 5 }).PowerText);
    }

    [Fact]
    public void ToEnemy_TypedPower_SetsPower()
    {
        var enemy = EnemyViewModel.From(Goblin with { Power = 5 });

        enemy.PowerText = "7";

        Assert.Equal(7, enemy.ToEnemy().Power);
    }

    [Fact]
    public void Problem_PowerNotANumber_NamesPower()
    {
        var enemy = EnemyViewModel.From(Goblin);

        enemy.PowerText = "x";

        Assert.Equal("Power must be a whole number (was 'x').", enemy.Problem);
    }

    // The box as typed, not the record's 5.
    [Fact]
    public void DuplicateOf_TypedPower_CopiesIt()
    {
        var typed = EnemyViewModel.From(Goblin with { Power = 5 });
        typed.PowerText = "9";

        Assert.Equal("9", EnemyViewModel.DuplicateOf(typed).PowerText);
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

    [Fact]
    public void ToEnemy_RuleWeightTyped_BuildsTheNewWeight()
    {
        var enemy = EnemyViewModel.From(WithRules(new EnemyAction { Weight = 4 }));

        enemy.Tactics.Lists[0].Rules[0].WeightText = "7";

        Assert.Equal(7, Assert.Single(enemy.ToEnemy().Tactics).Weight);
    }

    // The record's phase starts below 50 %, so a carried phase would keep it.
    [Fact]
    public void ToEnemy_PhaseThresholdTyped_BuildsTheNewThreshold()
    {
        var enemy = EnemyViewModel.From(WithRules(new EnemyAction { Weight = 4 }) with
        {
            Phases = [new EnemyPhase { BelowHpPercent = 50, Tactics = [new EnemyAction { Weight = 2 }] }],
        });

        enemy.Tactics.Lists[1].ThresholdText = "30";

        Assert.Equal(30, Assert.Single(enemy.ToEnemy().Phases).BelowHpPercent);
    }

    [Fact]
    public void Problem_RuleWeightNotANumber_NamesTheRule()
    {
        var enemy = EnemyViewModel.From(WithRules(new EnemyAction { Weight = 4 }, new EnemyAction { Weight = 2 }));

        enemy.Tactics.Lists[0].Rules[1].WeightText = "x";

        Assert.Equal("tactics[1]: Weight must be a whole number (was 'x').", enemy.Problem);
    }

    // The list reads the enemy, not its rules: one Tactics notice for the change, and the problem line again.
    [Fact]
    public void Tactics_RuleWeightTyped_NotifiesTacticsAndProblem()
    {
        var enemy = EnemyViewModel.From(WithRules(new EnemyAction { Weight = 4 }));
        List<string?> notified = [];
        enemy.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        enemy.Tactics.Lists[0].Rules[0].WeightText = "5";

        Assert.Equal([nameof(EnemyViewModel.Tactics), nameof(EnemyViewModel.Problem)], notified);
    }

    // Copied from the boxes, so a rule that can't be built yet duplicates as typed instead of throwing.
    [Fact]
    public void DuplicateOf_HalfTypedRule_CopiesItAsTyped()
    {
        var typed = EnemyViewModel.From(WithRules(new EnemyAction { Weight = 4 }));
        typed.Tactics.Lists[0].Rules[0].WeightText = "x";

        var copy = EnemyViewModel.DuplicateOf(typed);

        Assert.Equal("x", copy.Tactics.Lists[0].Rules[0].WeightText);
        Assert.NotSame(typed.Tactics, copy.Tactics);
    }

    // The goblin with the test's own rules from the start and no phase.
    private static EnemyDefinition WithRules(params EnemyAction[] rules) => Goblin with { Tactics = rules, Phases = [] };
}
