using System.Text.Json.Nodes;
using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// A quest condition is written as content and read back whole; a kind that names no stage writes none (ADR 0033). The
// quests are saved with the other catalogs, checked against every map (decision 394).
public sealed class ContentWriterQuestTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-quests");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterQuestTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMap_GatedEvent_ReadsBackWithItsCondition()
    {
        var condition = new QuestCondition("placeholder_errand", QuestConditionKind.AtStage, "ask_innkeeper");

        _writer.WriteMap(WithGatedCell(condition), _catalog);

        var loaded = ContentCatalogLoader.Load(_content.Path).Maps["start"].Layout.CellEvents;
        Assert.Contains(loaded, cell => cell.Event.Id == "gated_cell" && condition.Equals(cell.Event.OnlyWhenQuest));
    }

    [Fact]
    public void WriteMap_ConditionWithoutStage_LeavesStageIdOut()
    {
        _writer.WriteMap(WithGatedCell(new QuestCondition("placeholder_errand", QuestConditionKind.Active)), _catalog);

        // The map holds other gated events (the placeholder errand's), so only the written cell event's condition is
        // read.
        var layout = JsonNode.Parse(File.ReadAllText(_content.PathOf(Path.Combine("map-layouts", "start.json"))))!;
        var gated = layout["cellEvents"]!.AsArray().Single(cell => (string?)cell!["event"]!["id"] == "gated_cell")!["event"]!["onlyWhenQuest"]!.AsObject();
        Assert.Equal("active", (string?)gated["kind"]);
        Assert.False(gated.ContainsKey("stageId"));
    }

    [Fact]
    public void WriteMap_ConditionNamingUnknownQuest_ThrowsNamingIt()
    {
        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMap(WithGatedCell(new QuestCondition("heist", QuestConditionKind.Active)), _catalog));

        Assert.Contains("onlyWhenQuest has unknown quest id 'heist'.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WriteMapCatalogs_WritesQuestsSortedById()
    {
        var zed = new QuestDefinition("zed", "Zed", "Last.", QuestKind.Side, [new QuestStage("go", "Go.")]);
        var abc = new QuestDefinition("abc", "Abc", "First.", QuestKind.Main, [new QuestStage("go", "Go.")]);

        _writer.WriteMapCatalogs(_catalog with { Quests = new Dictionary<string, QuestDefinition>(_catalog.Quests) { ["zed"] = zed, ["abc"] = abc } });

        var written = JsonNode.Parse(File.ReadAllText(_content.PathOf("quests.json")))!.AsArray();
        Assert.Equal(["abc", "placeholder_errand", "zed"], written.Select(quest => (string?)quest!["id"]));
        Assert.Equal(abc, ContentCatalogLoader.Load(_content.Path).Quests["abc"]);
    }

    [Fact]
    public void WriteMapCatalogs_StageAnEventNamesMissing_RefusesAndWritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf("quests.json"));
        var errand = _catalog.Quests["placeholder_errand"];
        var withoutFirstStage = _catalog with { Quests = new Dictionary<string, QuestDefinition> { ["placeholder_errand"] = errand with { Stages = [errand.Stages[1]] } } };

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(withoutFirstStage));

        Assert.Contains("'ask_innkeeper'", exception.Message, StringComparison.Ordinal);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf("quests.json")));
    }

    private MapDefinition WithGatedCell(QuestCondition condition)
    {
        var start = _catalog.Maps["start"];
        var gated = new MapEvent { Id = "gated_cell", Activation = EventActivation.Touch, OnlyWhenQuest = condition, Steps = [new StartQuestStep("placeholder_errand")] };
        return new MapDefinition(start.Layout with { CellEvents = [.. start.Layout.CellEvents, new CellEvent(new GridCell(0, 0), gated)] }, start.Settings);
    }
}
