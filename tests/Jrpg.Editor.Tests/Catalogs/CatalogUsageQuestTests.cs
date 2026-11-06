using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The maps whose events name a quest, in a step or a condition, on a cell or on an object, so an undo removing the
// quest or one of its stages can be refused (decision 394). The test world's maps name only the placeholder errand, so
// the tests add a heist.
public sealed class CatalogUsageQuestTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    [Fact]
    public void UsersOfQuest_StepOnASavedMapAndConditionOnTheOpenMap_ListsBoth()
    {
        var saved = WithForestCellEvent(new MapEvent { Id = "heist_start", Activation = EventActivation.Touch, Steps = [new StartQuestStep("heist")] });
        var gate = new MapEvent { Id = "heist_gate", Activation = EventActivation.Touch, OnlyWhenQuest = new QuestCondition("heist", QuestConditionKind.Active), Steps = [] };
        var open = WithCellEvent(_catalog.Maps["start"].Layout, gate);

        Assert.Equal(["map forest", "map start"], CatalogUsage.UsersOfQuest(saved, "heist", null, [open]));
        Assert.Empty(CatalogUsage.UsersOfQuest(saved, "heist", "case", [open]));
    }

    [Fact]
    public void UsersOfQuest_StageNamedByAnAdvanceStep_ListsTheMapForThatStageOnly()
    {
        var saved = WithForestCellEvent(new MapEvent { Id = "heist_case", Activation = EventActivation.Touch, Steps = [new AdvanceQuestStep("heist", "case")] });

        Assert.Equal(["map forest"], CatalogUsage.UsersOfQuest(saved, "heist", "case", []));
        Assert.Empty(CatalogUsage.UsersOfQuest(saved, "heist", "crack", []));
    }

    [Fact]
    public void UsersOfQuest_StageNamedByAnObjectsCondition_ListsTheMap()
    {
        var forest = _catalog.Maps["forest"];
        var gated = new MapEvent { Id = "heist_look", Activation = EventActivation.Interact, OnlyWhenQuest = new QuestCondition("heist", QuestConditionKind.AtStage, "case"), Steps = [] };
        var first = forest.Layout.Objects[0];
        var layout = forest.Layout with { Objects = [first with { Events = [gated] }, .. forest.Layout.Objects.Skip(1)] };
        var saved = _catalog with { Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["forest"] = new MapDefinition(layout, forest.Settings) } };

        Assert.Equal(["map forest"], CatalogUsage.UsersOfQuest(saved, "heist", "case", []));
    }

    private static ContentCatalog WithForestCellEvent(MapEvent mapEvent)
    {
        var forest = _catalog.Maps["forest"];
        var layout = WithCellEvent(forest.Layout, mapEvent);
        return _catalog with { Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["forest"] = new MapDefinition(layout, forest.Settings) } };
    }

    private static MapLayout WithCellEvent(MapLayout layout, MapEvent mapEvent) =>
        layout with { CellEvents = [.. layout.CellEvents, new CellEvent(new GridCell(0, 0), mapEvent)] };
}
