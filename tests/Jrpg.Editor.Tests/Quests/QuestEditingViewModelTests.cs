using Jrpg.Core.Quests;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Quests;

namespace Jrpg.Editor.Tests.Quests;

// The Quests window starts from the draft and its whole session is one undo step (decision 394).
public sealed class QuestEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")));
    private readonly QuestEditingViewModel _questEditing;

    public QuestEditingViewModelTests() => _questEditing = new QuestEditingViewModel(_draft);

    [Fact]
    public void Start_Draft_ListsItsQuests()
    {
        var edited = _questEditing.Start();

        Assert.Contains(edited.Quests, quest => quest.Id == "placeholder_errand" && !quest.IsNew);
    }

    [Fact]
    public void Apply_ChangedQuests_OneUndoStepInTheDraft()
    {
        var edited = _questEditing.Start();
        edited.Quests.Single(quest => quest.Id == "placeholder_errand").Title = "The errand";
        AddTheHeist(edited);

        _questEditing.Apply(edited);

        Assert.Equal("The errand", _draft.Current.Quests["placeholder_errand"].Title);
        Assert.Equal(QuestKind.Main, _draft.Current.Quests["heist"].Kind);
        _draft.Undo();
        Assert.False(_draft.CanUndo);
        Assert.False(_draft.Current.Quests.ContainsKey("heist"));
    }

    [Fact]
    public void Apply_Unchanged_RecordsNoStep()
    {
        _questEditing.Apply(_questEditing.Start());

        Assert.False(_draft.CanUndo);
    }

    private static void AddTheHeist(QuestCatalogViewModel edited)
    {
        edited.AddQuestCommand.Execute(null);
        var heist = edited.SelectedQuest!;
        heist.Id = "heist";
        heist.Title = "The heist";
        heist.Description = "Case the vault.";
        heist.Kind = QuestKind.Main;
        heist.AddStageCommand.Execute(null);
        heist.SelectedStage!.Id = "case";
        heist.SelectedStage.Text = "Case the vault.";
    }
}
