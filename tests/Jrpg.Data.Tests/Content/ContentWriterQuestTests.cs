using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// A quest condition is written as content and read back whole; a kind that names no stage writes none (ADR 0033).
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

        var json = File.ReadAllText(_content.PathOf(Path.Combine("map-layouts", "start.json")));
        Assert.Contains("\"onlyWhenQuest\"", json, StringComparison.Ordinal);
        Assert.Contains("\"kind\": \"active\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("stageId", json, StringComparison.Ordinal);
    }

    [Fact]
    public void WriteMap_ConditionNamingUnknownQuest_ThrowsNamingIt()
    {
        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMap(WithGatedCell(new QuestCondition("heist", QuestConditionKind.Active)), _catalog));

        Assert.Contains("onlyWhenQuest has unknown quest id 'heist'.", exception.Message, StringComparison.Ordinal);
    }

    private MapDefinition WithGatedCell(QuestCondition condition)
    {
        var start = _catalog.Maps["start"];
        var gated = new MapEvent { Id = "gated_cell", Activation = EventActivation.Touch, OnlyWhenQuest = condition, Steps = [new StartQuestStep("placeholder_errand")] };
        return new MapDefinition(start.Layout with { CellEvents = [.. start.Layout.CellEvents, new CellEvent(new GridCell(0, 0), gated)] }, start.Settings);
    }
}
