using Jrpg.Core.Content;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// The Enemies window's list (ADR 0045): the draft's enemies in id order, Duplicate after the chosen one (decision 585),
// only a new enemy removable (decision 581), and the problem line ending with Data's message (decision 580).
public sealed class EnemyCatalogViewModelTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    [Fact]
    public void Enemies_Draft_InIdOrderWithTheFirstChosen()
    {
        var catalog = new EnemyCatalogViewModel(_catalog);

        var ids = catalog.Enemies.Select(enemy => enemy.Id).ToList();
        Assert.Equal(ids.Order(StringComparer.Ordinal), ids);
        Assert.Contains(catalog.Enemies, enemy => enemy.Id == "goblin");
        Assert.Same(catalog.Enemies[0], catalog.SelectedEnemy);
        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void DamageTypes_EveryCoreType_IsOffered()
    {
        Assert.Equal(Enum.GetValues<DamageType>(), new EnemyCatalogViewModel(_catalog).DamageTypes);
    }

    [Fact]
    public void DuplicateEnemy_Chosen_AddsAfterAndChoosesIt()
    {
        var catalog = new EnemyCatalogViewModel(_catalog);
        var chief = catalog.Enemies.Single(enemy => enemy.Id == "goblin_chief");
        catalog.SelectedEnemy = chief;

        catalog.DuplicateEnemyCommand.Execute(null);

        var copy = catalog.Enemies[catalog.Enemies.IndexOf(chief) + 1];
        Assert.Same(copy, catalog.SelectedEnemy);
        Assert.True(copy.IsNew);
        Assert.Equal(chief.Name, copy.Name);
    }

    [Fact]
    public void AddEnemy_AddsLastAndChoosesIt()
    {
        var catalog = new EnemyCatalogViewModel(_catalog);

        catalog.AddEnemyCommand.Execute(null);

        Assert.Same(catalog.Enemies[^1], catalog.SelectedEnemy);
        Assert.Equal($"Enemy #{catalog.Enemies.Count}: The id must not be empty.", catalog.Problem);
        Assert.False(catalog.CanApply);
    }

    [Fact]
    public void RemoveEnemy_SavedEnemy_IsGreyed()
    {
        var catalog = new EnemyCatalogViewModel(_catalog);

        Assert.False(catalog.RemoveEnemyCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveEnemy_NewEnemy_RemovesIt()
    {
        var catalog = new EnemyCatalogViewModel(_catalog);
        var count = catalog.Enemies.Count;
        catalog.AddEnemyCommand.Execute(null);
        Assert.True(catalog.RemoveEnemyCommand.CanExecute(null));

        catalog.RemoveEnemyCommand.Execute(null);

        Assert.Equal(count, catalog.Enemies.Count);
        Assert.Null(catalog.Problem);
        Assert.True(catalog.CanApply);
    }

    [Fact]
    public void Problem_DodgePastCap_IsDataMessageAndNotifies()
    {
        var catalog = new EnemyCatalogViewModel(_catalog);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.Enemies.Single(enemy => enemy.Id == "goblin").DodgeText = "51";

        Assert.Equal("enemies.json: enemy 'goblin': dodge must be within 0..50 (was 51).", catalog.Problem);
        Assert.Contains(nameof(EnemyCatalogViewModel.Problem), notified);
        Assert.Contains(nameof(EnemyCatalogViewModel.CanApply), notified);
    }

    [Fact]
    public void Problem_TwoNewSameId_SaysSo()
    {
        var catalog = new EnemyCatalogViewModel(_catalog);
        catalog.AddEnemyCommand.Execute(null);
        catalog.SelectedEnemy!.Id = "slime";
        catalog.AddEnemyCommand.Execute(null);

        catalog.SelectedEnemy!.Id = "slime";

        Assert.Equal("Enemy slime is listed twice.", catalog.Problem);
    }

    [Fact]
    public void ToEnemies_Unchanged_EqualsDraft()
    {
        var enemies = new EnemyCatalogViewModel(_catalog).ToEnemies();

        Assert.Equal(_catalog.Enemies.Keys.Order(StringComparer.Ordinal), enemies.Select(enemy => enemy.Id));
        Assert.All(
            enemies,
            enemy => Assert.Equal(_catalog.Enemies[enemy.Id] with { Tactics = enemy.Tactics, Phases = enemy.Phases }, enemy));
    }
}
