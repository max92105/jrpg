using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Maps;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Battle;

// The Enemies page's list (ADR 0045): the draft's enemies in id order, Duplicate after the chosen one (decision 585),
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
        Assert.Contains(catalog.Enemies, enemy => enemy.Id == "gnoll");
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
    }

    [Fact]
    public void Problem_DodgePastCap_IsDataMessageAndNotifies()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.Enemies.Single(enemy => enemy.Id == "gnoll").DodgeText = "51";

        Assert.Equal("enemies.json: enemy 'gnoll': dodge must be within 0..50 (was 51).", catalog.Problem);
        Assert.Contains(nameof(EnemyCatalogViewModel.Problem), notified);
    }

    // A rule reaches the list as the enemy's Tactics notice, which the list doesn't skip; the rule is added first so
    // the test needs no rule from the content.
    [Fact]
    public void Problem_RuleTyped_IsRaised()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
        var rules = catalog.SelectedEnemy!.Tactics.Lists[0];
        rules.AddRuleCommand.Execute(null);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        rules.SelectedRule!.WeightText = "x";

        Assert.Contains(nameof(EnemyCatalogViewModel.Problem), notified);
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
        var goblin = _catalog.Enemies["gnoll"] with { Power = 0, Tactics = [cleaves] };
        var draft = _catalog with
        {
            Skills = new Dictionary<string, SkillDefinition>(_catalog.Skills) { [cleave.Id] = cleave },
            Enemies = new Dictionary<string, EnemyDefinition>(_catalog.Enemies) { ["gnoll"] = goblin },
        };

        var catalog = new EnemyCatalogViewModel(draft, _ => []);

        Assert.Equal(
            "enemies.json: enemy 'gnoll': power must be at least 1: its skill 'tiered_cleave' names a tiered effect (was 0).",
            catalog.Problem);
    }

    [Fact]
    public void Problem_PowerAboveTenThousand_ShowsDataMessage()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);

        catalog.Enemies.Single(enemy => enemy.Id == "gnoll").PowerText = "10001";

        Assert.Equal("enemies.json: enemy 'gnoll': power must be within 0..10000 (was 10001).", catalog.Problem);
    }

    [Fact]
    public void Problem_FrameCountOne_IsDatasMessage()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
        var goblin = catalog.Enemies.Single(enemy => enemy.Id == "gnoll");

        goblin.Art.IsAnimated = true;
        goblin.Art.FrameCountText = "1";

        Assert.Equal("enemies.json: enemy 'gnoll': idleAnimation.frameCount must be at least 2 (was 1).", catalog.Problem);
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

    [Fact]
    public void Choose_KnownId_ChoosesIt()
    {
        var catalog = new EnemyCatalogViewModel(WithEnemies("bat", "wolf"), _ => []);

        catalog.Choose("wolf");

        Assert.Equal("wolf", catalog.SelectedEnemy!.Id);
    }

    [Fact]
    public void Choose_UnknownId_KeepsTheChoice()
    {
        var catalog = new EnemyCatalogViewModel(WithEnemies("bat", "wolf"), _ => []);
        catalog.Choose("wolf");

        catalog.Choose("dragon");

        Assert.Equal("wolf", catalog.SelectedEnemy!.Id);
    }

    [Fact]
    public void ChosenKey_DuplicatedEnemy_IsItsTypedId()
    {
        var catalog = new EnemyCatalogViewModel(WithEnemies("bat", "wolf"), _ => []);
        catalog.DuplicateEnemyCommand.Execute(null);

        catalog.SelectedEnemy!.Id = "bat_alpha";

        Assert.Equal("bat_alpha", catalog.ChosenKey);
    }

    // Ids in the opposite order to the names, added last name first, so neither the ids nor the draft's order passes.
    [Fact]
    public void SkillChoices_AttackFirstThenSkillsByName()
    {
        var draft = WithSkills(SkillNamed("aa_wither", "Wither"), SkillNamed("zz_balm", "Balm"));

        var choices = new EnemyCatalogViewModel(draft, _ => []).SkillChoices;

        var names = choices.Skip(1).Select(choice => choice.Name).ToList();
        Assert.Equal(new TacticSkillChoice("", "Attack"), choices[0]);
        Assert.Equal(["zz_balm", "aa_wither"], choices.Select(choice => choice.Id).Where(id => id is "zz_balm" or "aa_wither"));
        Assert.Equal(names.Order(StringComparer.Ordinal), names);
    }

    [Fact]
    public void SkillChoices_SkillWithAClassResource_IsNotOffered()
    {
        var spending = SkillNamed("rage_strike", "Rage Strike") with { ResourceChanges = [new ResourceChange("rage", -1)] };
        var draft = WithSkills(SkillNamed("strike", "Strike"), spending);

        var ids = new EnemyCatalogViewModel(draft, _ => []).SkillChoices.Select(choice => choice.Id);

        Assert.Contains("strike", ids);
        Assert.DoesNotContain("rage_strike", ids);
    }

    // The test world's goblin under each id, the only enemies, so a choice is told apart by its id alone.
    private static ContentCatalog WithEnemies(params string[] ids) =>
        _catalog with { Enemies = ids.ToDictionary(id => id, id => _catalog.Enemies["gnoll"] with { Id = id }) };

    private static ContentCatalog WithSkills(params SkillDefinition[] added)
    {
        var skills = new Dictionary<string, SkillDefinition>(_catalog.Skills);
        foreach (var skill in added)
        {
            skills[skill.Id] = skill;
        }

        return _catalog with { Skills = skills };
    }

    private static SkillDefinition SkillNamed(string id, string name) => new()
    {
        Id = id,
        Name = name,
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 4, Targeting.SingleEnemy, DamageType: DamageType.Slashing),
    };
}
