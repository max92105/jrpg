using System.Text.Json.Nodes;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// enemies.json is saved with the other catalogs (ADR 0045): resistances in whole percents (decision 587), every
// default-valued field left out so an enemy the editor writes reads like a hand-written one (decision 586), checked
// against the drop tables and skills as the loader checks it.
public sealed class ContentWriterEnemyTests : IDisposable
{
    private static readonly string[] _defaultValuedFields =
    [
        "\"skillId\": null",
        "\"when\": null",
        "\"percent\": 0",
        "\"isBoss\": false",
        "\"tactics\": []",
        "\"phases\": []",
        "\"dropTableId\": null",
        "\"gold\": null",
    ];

    private readonly CopiedContentRoot _content = new("writer-enemies", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterEnemyTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMapCatalogs_Enemies_LoadTheSameAfterRelay()
    {
        _writer.WriteMapCatalogs(_catalog);

        var reloaded = ContentCatalogLoader.Load(_content.Path).Enemies;
        Assert.Equal(_catalog.Enemies.Keys.Order(StringComparer.Ordinal), reloaded.Keys.Order(StringComparer.Ordinal));
        Assert.All(_catalog.Enemies.Values, enemy => AssertSameEnemy(enemy, reloaded[enemy.Id]));
    }

    [Fact]
    public void WriteMapCatalogs_RelaidEnemies_HoldNoDefaultValuedField()
    {
        _writer.WriteMapCatalogs(_catalog);

        var text = File.ReadAllText(_content.PathOf("enemies.json"));
        Assert.All(_defaultValuedFields, field => Assert.DoesNotContain(field, text, StringComparison.Ordinal));
    }

    [Fact]
    public void WriteMapCatalogs_BossEnemy_WritesIsBossTrueAndLeavesItOutOtherwise()
    {
        var goblin = _catalog.Enemies["goblin"];
        var boss = goblin with { Id = "goblin_king", IsBoss = true };

        _writer.WriteMapCatalogs(WithEnemies(goblin with { IsBoss = false }, boss));

        var written = WrittenEnemies();
        Assert.True((bool?)written["goblin_king"]["isBoss"]);
        Assert.False(written["goblin"].AsObject().ContainsKey("isBoss"));
    }

    [Fact]
    public void WriteMapCatalogs_EnemyWithoutLoop_LeavesIdleAnimationOut()
    {
        _writer.WriteMapCatalogs(WithEnemies(_catalog.Enemies["goblin"] with { IdleAnimation = null }));

        Assert.False(WrittenEnemies()["goblin"].AsObject().ContainsKey("idleAnimation"));
    }

    [Fact]
    public void WriteMapCatalogs_EnemyWithLoop_WritesIt()
    {
        var loop = new TileAnimation { FrameCount = 8, FrameMilliseconds = 120 };
        var gnoll = _catalog.Enemies["goblin"] with { Id = "gnoll", IdleAnimation = loop };

        _writer.WriteMapCatalogs(WithEnemies(gnoll));

        Assert.Equal(loop, ContentCatalogLoader.Load(_content.Path).Enemies["gnoll"].IdleAnimation);
    }

    [Fact]
    public void WriteMapCatalogs_SkillActionWithCondition_WritesSkillWhenAndPercent()
    {
        var cleaveWhenAllyHurt = new EnemyAction
        {
            SkillId = "cleave",
            Weight = 2,
            When = new TacticCondition(TacticConditionKind.AllyHpBelow, 50),
        };
        var healer = _catalog.Enemies["goblin"] with
        {
            Id = "goblin_healer",
            Tactics = [new EnemyAction { Weight = 4 }, cleaveWhenAllyHurt],
        };

        _writer.WriteMapCatalogs(WithEnemies(healer));

        var tactics = WrittenEnemies()["goblin_healer"]["tactics"]!.AsArray();
        Assert.False(tactics[0]!.AsObject().ContainsKey("skillId"));
        var cleave = tactics[1]!;
        Assert.Equal(("cleave", 2), ((string?)cleave["skillId"], (int?)cleave["weight"]));
        Assert.Equal(("allyHpBelow", 50), ((string?)cleave["when"]!["kind"], (int?)cleave["when"]!["percent"]));
        AssertSameEnemy(healer, ContentCatalogLoader.Load(_content.Path).Enemies["goblin_healer"]);
    }

    [Fact]
    public void WriteMapCatalogs_UnknownTacticSkill_WritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf("enemies.json"));
        var unknown = new EnemyAction { SkillId = "no_such_skill", Weight = 1 };
        var broken = _catalog.Enemies["goblin"] with { Id = "goblin_mage", Tactics = [unknown] };

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(WithEnemies(broken)));

        var expected = "enemies.json: enemy 'goblin_mage': tactics[0].skillId has unknown skill id 'no_such_skill'.";
        Assert.Equal(expected, exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf("enemies.json")));
    }

    private ContentCatalog WithEnemies(params EnemyDefinition[] enemies)
    {
        var all = new Dictionary<string, EnemyDefinition>(_catalog.Enemies);
        foreach (var enemy in enemies)
        {
            all[enemy.Id] = enemy;
        }

        return _catalog with { Enemies = all };
    }

    private Dictionary<string, JsonNode> WrittenEnemies() =>
        JsonNode.Parse(File.ReadAllText(_content.PathOf("enemies.json")))!
            .AsArray()
            .ToDictionary(enemy => (string)enemy!["id"]!, enemy => enemy!);

    // Records compare their lists by reference, so the tactics and each phase's are compared by content.
    private static void AssertSameEnemy(EnemyDefinition expected, EnemyDefinition actual)
    {
        Assert.Equal(expected with { Tactics = actual.Tactics, Phases = actual.Phases }, actual);
        Assert.Equal(expected.Tactics, actual.Tactics);
        Assert.Equal(expected.Phases.Select(phase => phase.BelowHpPercent), actual.Phases.Select(phase => phase.BelowHpPercent));
        Assert.All(expected.Phases.Zip(actual.Phases), pair => Assert.Equal(pair.First.Tactics, pair.Second.Tactics));
    }
}
