using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// An enemy's resistances as one row per damage type (design key decision 3), whole percents typed, permille written.
public sealed class EnemyResistanceTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    // The tests' own weakness to fire over the test world's goblin, whose numbers are the developer's to change.
    private static EnemyDefinition FireWeakGoblin =>
        _catalog.Enemies["goblin"] with { Resistances = Resistances.None with { Fire = -250 } };

    [Fact]
    public void Resistances_FireWeakGoblin_OneRowPerTypeFireMinus25()
    {
        var enemy = EnemyViewModel.From(FireWeakGoblin);

        Assert.Equal(Enum.GetValues<DamageType>(), enemy.Resistances.Select(row => row.Type));
        Assert.Equal("-25", enemy.Resistances.Single(row => row.Type == DamageType.Fire).PercentText);
        Assert.All(enemy.Resistances.Where(row => row.Type != DamageType.Fire), row => Assert.Equal("0", row.PercentText));
        Assert.Equal("Fire", enemy.Resistances.Single(row => row.Type == DamageType.Fire).Name);
    }

    [Fact]
    public void ToEnemy_RowEdited_WritesPermille()
    {
        var enemy = EnemyViewModel.From(FireWeakGoblin);

        enemy.Resistances.Single(row => row.Type == DamageType.Blunt).PercentText = "25";

        Assert.Equal(Resistances.None with { Fire = -250, Blunt = 250 }, enemy.ToEnemy().Resistances);
    }

    [Fact]
    public void ToEnemy_Unchanged_KeepsTheResistances()
    {
        Assert.Equal(FireWeakGoblin.Resistances, EnemyViewModel.From(FireWeakGoblin).ToEnemy().Resistances);
    }

    [Fact]
    public void Problem_RowNotANumber_NamesTheRow()
    {
        var enemy = EnemyViewModel.From(FireWeakGoblin);

        enemy.Resistances.Single(row => row.Type == DamageType.Ice).PercentText = "";

        Assert.Equal("Ice resistance must be a whole number (was '').", enemy.Problem);
    }

    // A row's change reaches the list's problem line as the enemy's own change (not through the row).
    [Fact]
    public void Problem_ResistancePastCap_IsDataMessageAndNotifies()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        var goblin = catalog.Enemies.Single(enemy => enemy.Id == "goblin");
        goblin.Resistances.Single(row => row.Type == DamageType.Fire).PercentText = "80";

        Assert.Equal("enemies.json: enemy 'goblin': resistances.fire must be within -100..75 (was 80).", catalog.Problem);
        Assert.Contains(nameof(EnemyCatalogViewModel.Problem), notified);
    }

    [Fact]
    public void DuplicateOf_EditedRow_CopiesTheRowsAsTyped()
    {
        var enemy = EnemyViewModel.From(FireWeakGoblin);
        enemy.Resistances.Single(row => row.Type == DamageType.Dark).PercentText = "abc";

        var copy = EnemyViewModel.DuplicateOf(enemy);

        Assert.Equal("abc", copy.Resistances.Single(row => row.Type == DamageType.Dark).PercentText);
        Assert.Equal("-25", copy.Resistances.Single(row => row.Type == DamageType.Fire).PercentText);
    }
}
