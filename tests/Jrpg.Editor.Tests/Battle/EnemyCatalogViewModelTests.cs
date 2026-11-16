using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Maps;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Tiles;

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
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);

        var ids = catalog.Enemies.Select(enemy => enemy.Id).ToList();
        Assert.Equal(ids.Order(StringComparer.Ordinal), ids);
        Assert.Contains(catalog.Enemies, enemy => enemy.Id == "goblin");
        Assert.Same(catalog.Enemies[0], catalog.SelectedEnemy);
        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void DamageTypes_EveryCoreType_IsOffered()
    {
        Assert.Equal(Enum.GetValues<DamageType>(), new EnemyCatalogViewModel(_catalog, _ => []).DamageTypes);
    }

    [Fact]
    public void DuplicateEnemy_Chosen_AddsAfterAndChoosesIt()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
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
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);

        catalog.AddEnemyCommand.Execute(null);

        Assert.Same(catalog.Enemies[^1], catalog.SelectedEnemy);
        Assert.Equal($"Enemy #{catalog.Enemies.Count}: The id must not be empty.", catalog.Problem);
        Assert.False(catalog.CanApply);
    }

    [Fact]
    public void RemoveEnemy_SavedEnemy_IsGreyed()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);

        Assert.False(catalog.RemoveEnemyCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveEnemy_NewEnemy_RemovesIt()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
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
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.Enemies.Single(enemy => enemy.Id == "goblin").DodgeText = "51";

        Assert.Equal("enemies.json: enemy 'goblin': dodge must be within 0..50 (was 51).", catalog.Problem);
        Assert.Contains(nameof(EnemyCatalogViewModel.Problem), notified);
        Assert.Contains(nameof(EnemyCatalogViewModel.CanApply), notified);
    }

    // T3's rule reaches the line through Data: the goblin uses a tiered skill built here with no Power.
    [Fact]
    public void Problem_TieredSkillWithPowerZero_ShowsDataMessage()
    {
        var cleave = new SkillDefinition
        {
            Id = "tiered_cleave",
            Name = "Cleave",
            Description = "",
            IconFile = "",
            Effect = new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy, DamageType: DamageType.Slashing)
            {
                Tier = new TieredEffect("slashing_1", EffectKind.Damage, 2) { DamageType = DamageType.Slashing },
            },
        };
        var cleaves = new EnemyAction { SkillId = cleave.Id, Weight = 1 };
        var goblin = _catalog.Enemies["goblin"] with { Power = 0, Tactics = [cleaves] };
        var draft = _catalog with
        {
            Skills = new Dictionary<string, SkillDefinition>(_catalog.Skills) { [cleave.Id] = cleave },
            Enemies = new Dictionary<string, EnemyDefinition>(_catalog.Enemies) { ["goblin"] = goblin },
        };

        var catalog = new EnemyCatalogViewModel(draft, _ => []);

        Assert.Equal(
            "enemies.json: enemy 'goblin': power must be at least 1: its skill 'tiered_cleave' names a tiered effect (was 0).",
            catalog.Problem);
    }

    [Fact]
    public void Problem_PowerAboveTenThousand_ShowsDataMessage()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);

        catalog.Enemies.Single(enemy => enemy.Id == "goblin").PowerText = "10001";

        Assert.Equal("enemies.json: enemy 'goblin': power must be within 0..10000 (was 10001).", catalog.Problem);
    }

    [Fact]
    public void Problem_FrameCountOne_IsDatasMessage()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
        var goblin = catalog.Enemies.Single(enemy => enemy.Id == "goblin");

        goblin.Art.IsAnimated = true;
        goblin.Art.FrameCountText = "1";

        Assert.Equal("enemies.json: enemy 'goblin': idleAnimation.frameCount must be at least 2 (was 1).", catalog.Problem);
    }

    [Fact]
    public void TakeImport_GifResult_FillsTheIdleBoxes()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
        var loop = new TileAnimation { FrameCount = 4, FrameMilliseconds = 260 };
        var animations = new Dictionary<string, TileAnimation> { ["Gnoll.png"] = loop };
        var result = new SheetImportResult(["Gnoll.png"], []) { Animations = animations };

        catalog.TakeImport(ArtFolders.Battlers, result);

        Assert.Equal(("Gnoll.png", loop), (catalog.SelectedEnemy!.Art.BattleSpriteFile, catalog.SelectedEnemy.Art.PreviewLoop));
    }

    [Fact]
    public void TakeImport_PngResult_LeavesTheBoxes()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
        var loop = new TileAnimation { FrameCount = 6, FrameMilliseconds = 120 };
        catalog.SelectedEnemy!.Art.TakeLoop(loop);

        catalog.TakeImport(ArtFolders.Battlers, new SheetImportResult(["Wolf.png"], []));

        Assert.Equal(("Wolf.png", loop), (catalog.SelectedEnemy.Art.BattleSpriteFile, catalog.SelectedEnemy.Art.PreviewLoop));
    }

    [Fact]
    public void Problem_TwoNewSameId_SaysSo()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
        catalog.AddEnemyCommand.Execute(null);
        catalog.SelectedEnemy!.Id = "slime";
        catalog.AddEnemyCommand.Execute(null);

        catalog.SelectedEnemy!.Id = "slime";

        Assert.Equal("Enemy slime is listed twice.", catalog.Problem);
    }

    [Fact]
    public void ToEnemies_Unchanged_EqualsDraft()
    {
        var enemies = new EnemyCatalogViewModel(_catalog, _ => []).ToEnemies();

        Assert.Equal(_catalog.Enemies.Keys.Order(StringComparer.Ordinal), enemies.Select(enemy => enemy.Id));
        Assert.All(
            enemies,
            enemy => Assert.Equal(_catalog.Enemies[enemy.Id] with { Tactics = enemy.Tactics, Phases = enemy.Phases }, enemy));
    }
}
