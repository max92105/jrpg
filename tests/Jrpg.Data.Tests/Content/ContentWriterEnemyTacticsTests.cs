using System.Text.Json.Nodes;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// An enemy's rules as the writer saves them: each optional part left out at its default (decision 848), so a rule the
// editor writes reads like a hand-written one.
public sealed class ContentWriterEnemyTacticsTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-enemy-tactics", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterEnemyTacticsTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteEnemies_ActionWithoutOn_LeavesItOut()
    {
        _writer.WriteMapCatalogs(WithGoblinTactics(new EnemyAction { Weight = 1 }));

        Assert.False(WrittenGoblinTactics()[0]!.AsObject().ContainsKey("on"));
    }

    [Fact]
    public void WriteEnemies_ActionWithOn_WritesItAndReadsItBack()
    {
        var onLowestFoe = new EnemyAction { Weight = 1, On = TacticTarget.LowestHpFoe };

        _writer.WriteMapCatalogs(WithGoblinTactics(onLowestFoe));

        Assert.Equal("lowestHpFoe", (string?)WrittenGoblinTactics()[0]!["on"]);
        Assert.Equal([onLowestFoe], ContentCatalogLoader.Load(_content.Path).Enemies["gnoll"].Tactics);
    }

    [Fact]
    public void WriteEnemies_PriorityRule_WritesIsPriority()
    {
        var priority = new EnemyAction { Weight = 1, IsPriority = true };

        _writer.WriteMapCatalogs(WithGoblinTactics(priority));

        Assert.True((bool?)WrittenGoblinTactics()[0]!["isPriority"]);
        Assert.Equal([priority], ContentCatalogLoader.Load(_content.Path).Enemies["gnoll"].Tactics);
    }

    [Fact]
    public void WriteEnemies_RuleWithoutPriority_LeavesItOut()
    {
        _writer.WriteMapCatalogs(WithGoblinTactics(new EnemyAction { Weight = 1 }));

        Assert.False(WrittenGoblinTactics()[0]!.AsObject().ContainsKey("isPriority"));
    }

    [Fact]
    public void WriteEnemies_OnceRule_WritesItAndReadsItBack()
    {
        var once = new EnemyAction { SkillId = "cleave", Weight = 1, IsOncePerBattle = true };

        _writer.WriteMapCatalogs(WithGoblinTactics(once));

        Assert.True((bool?)WrittenGoblinTactics()[0]!["isOncePerBattle"]);
        Assert.Equal([once], ContentCatalogLoader.Load(_content.Path).Enemies["gnoll"].Tactics);
    }

    [Fact]
    public void WriteEnemies_HpCondition_LeavesTurnsOut()
    {
        var onAllyBelowHalf = new EnemyAction { Weight = 1, When = new TacticCondition(TacticConditionKind.AllyHpBelow, 50) };

        _writer.WriteMapCatalogs(WithGoblinTactics(onAllyBelowHalf));

        Assert.False(WrittenGoblinTactics()[0]!["when"]!.AsObject().ContainsKey("turns"));
    }

    [Fact]
    public void WriteEnemies_EveryNthTurn_WritesTurnsAndReadsItBack()
    {
        var everyThirdTurn = new EnemyAction { Weight = 1, When = new(TacticConditionKind.EveryNthTurn, Turns: 3) };

        _writer.WriteMapCatalogs(WithGoblinTactics(everyThirdTurn));

        Assert.Equal(3, (int?)WrittenGoblinTactics()[0]!["when"]!["turns"]);
        Assert.Equal([everyThirdTurn], ContentCatalogLoader.Load(_content.Path).Enemies["gnoll"].Tactics);
    }

    // A power of 5, so a rule may name cleave, whose effect is tiered.
    private ContentCatalog WithGoblinTactics(params EnemyAction[] tactics)
    {
        var enemies = new Dictionary<string, EnemyDefinition>(_catalog.Enemies)
        {
            ["gnoll"] = _catalog.Enemies["gnoll"] with { Power = 5, Tactics = tactics },
        };

        return _catalog with { Enemies = enemies };
    }

    private JsonArray WrittenGoblinTactics() =>
        JsonNode.Parse(File.ReadAllText(_content.PathOf("enemies.json")))!
            .AsArray()
            .Single(enemy => (string?)enemy!["id"] == "gnoll")!["tactics"]!
            .AsArray();
}
