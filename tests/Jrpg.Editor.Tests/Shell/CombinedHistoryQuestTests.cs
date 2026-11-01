using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Quests;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

// An undo that would take away a quest, or a stage, that a saved map's event names is refused naming them (decision
// 394, as decision 117 refuses a used tile).
public sealed class CombinedHistoryQuestTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Undo_RemovesAStageAnEventNames_RefusedNamingIt()
    {
        var viewModel = _content.Open(_content.CopyOfRepositoryContent());
        var errand = viewModel.Catalog!.Quests["placeholder_errand"];
        viewModel.Content!.SetQuests([errand with { Stages = [.. errand.Stages, new QuestStage("extra", "One more.")] }]);
        var mapId = SaveWithCellEvent(viewModel, new AdvanceQuestStep("placeholder_errand", "extra"));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: stage extra of quest placeholder_errand is used on map {mapId}.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog.Quests["placeholder_errand"].HasStage("extra"));
    }

    [Fact]
    public void Undo_RemovesAQuestAnEventNames_RefusedNamingIt()
    {
        var viewModel = _content.Open(_content.CopyOfRepositoryContent());
        var heist = new QuestDefinition("heist", "The heist", "Case the vault.", QuestKind.Main, [new QuestStage("case", "Case the vault.")]);
        viewModel.Content!.SetQuests([.. viewModel.Catalog!.Quests.Values, heist]);
        var mapId = SaveWithCellEvent(viewModel, new StartQuestStep("heist"));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: quest heist is used on map {mapId}.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog.Quests.ContainsKey("heist"));
    }

    // Puts an event with the step on the open map's top-left cell, saves, and opens another map so only the catalog
    // step is left to undo.
    private static string SaveWithCellEvent(MainViewModel viewModel, EventStep step)
    {
        var map = viewModel.CurrentMap!;
        var mapEvent = new MapEvent { Id = "quest_check", Activation = EventActivation.Touch, Steps = [step] };
        map.Do(SetCellEventEdit.For(map.Map.Layout.CellEvents, new GridCell(0, 0), mapEvent));
        viewModel.SaveCommand.Execute(null);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != map.Map.Id));
        return map.Map.Id;
    }
}
