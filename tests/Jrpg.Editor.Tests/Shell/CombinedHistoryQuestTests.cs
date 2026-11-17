using Jrpg.Core.Events;
using Jrpg.Core.Quests;

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
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var errand = viewModel.Catalog!.Quests["placeholder_errand"];
        viewModel.Content!.SetQuests([errand with { Stages = [.. errand.Stages, new QuestStage("extra", "One more.")] }]);
        var mapId = EditorTestContent.SaveWithCellEvent(viewModel, new AdvanceQuestStep("placeholder_errand", "extra"));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: stage extra of quest placeholder_errand is used on map {mapId}.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog.Quests["placeholder_errand"].HasStage("extra"));
    }

    [Fact]
    public void Undo_RemovesAQuestAnEventNames_RefusedNamingIt()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var heist = new QuestDefinition("heist", "The heist", "Case the vault.", QuestKind.Main, [new QuestStage("case", "Case the vault.")]);
        viewModel.Content!.SetQuests([.. viewModel.Catalog!.Quests.Values, heist]);
        var mapId = EditorTestContent.SaveWithCellEvent(viewModel, new StartQuestStep("heist"));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: quest heist is used on map {mapId}.", viewModel.History.Refusal);
        Assert.True(viewModel.Catalog.Quests.ContainsKey("heist"));
    }
}
