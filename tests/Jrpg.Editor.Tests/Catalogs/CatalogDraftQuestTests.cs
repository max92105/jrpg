using Jrpg.Core.Quests;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The quests are replaced whole as one undo step (decision 394); what an undo would take away is listed so it can be
// refused while a map's event names it.
public sealed class CatalogDraftQuestTests
{
    private static readonly QuestDefinition _heist = new("heist", "The heist", "Case the vault.", QuestKind.Main, [new QuestStage("case", "Case the vault.")]);

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")));

    [Fact]
    public void SetQuests_AddedQuest_OneUndoStep()
    {
        _draft.SetQuests([.. _draft.Current.Quests.Values, _heist]);

        Assert.Equal(_heist, _draft.Current.Quests["heist"]);
        Assert.True(_draft.IsDirty);
        _draft.Undo();
        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void SetQuests_SameContent_RecordsNoStep()
    {
        var copies = _draft.Current.Quests.Values.Select(quest => quest with { Stages = [.. quest.Stages] }).ToList();

        _draft.SetQuests(copies);

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void Undo_SetQuests_RestoresTheQuests()
    {
        var before = _draft.Current.Quests;
        _draft.SetQuests([_heist]);

        _draft.Undo();

        Assert.Same(before, _draft.Current.Quests);
    }

    [Fact]
    public void QuestsRemovedByUndo_AddedQuest_ListsItAndNoStage()
    {
        _draft.SetQuests([.. _draft.Current.Quests.Values, _heist]);

        Assert.Equal(["heist"], _draft.QuestsRemovedByUndo());
        Assert.Empty(_draft.QuestStagesRemovedByUndo());
    }

    [Fact]
    public void QuestStagesRemovedByUndo_AddedStage_ListsItAndNoQuest()
    {
        var errand = _draft.Current.Quests["placeholder_errand"];
        _draft.SetQuests([errand with { Stages = [.. errand.Stages, new QuestStage("extra", "One more.")] }]);

        Assert.Equal([("placeholder_errand", "extra")], _draft.QuestStagesRemovedByUndo());
        Assert.Empty(_draft.QuestsRemovedByUndo());
    }
}
