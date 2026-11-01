using Jrpg.Core.Quests;
using Jrpg.Editor.Quests;

namespace Jrpg.Editor.Tests.Quests;

// The Quests window's list (decision 394): the draft's quests in id order, new quests chosen as they are added, and a
// problem naming the quest the game would refuse.
public sealed class QuestCatalogViewModelTests
{
    private static readonly QuestDefinition _errand = new("placeholder_errand", "An errand", "Carry a question.", QuestKind.Side, [new QuestStage("ask_innkeeper", "Ask."), new QuestStage("return_to_giver", "Return.")]);
    private static readonly QuestDefinition _zed = new("zed", "Zed", "Last.", QuestKind.Side, [new QuestStage("go", "Go.")]);
    private static readonly QuestDefinition _abc = new("abc", "Abc", "First.", QuestKind.Main, [new QuestStage("go", "Go.")]);

    [Fact]
    public void New_DraftQuests_ListedInIdOrderWithTheFirstChosen()
    {
        var catalog = new QuestCatalogViewModel([_zed, _errand, _abc]);

        Assert.Equal(["abc", "placeholder_errand", "zed"], catalog.Quests.Select(quest => quest.Id));
        Assert.Same(catalog.Quests[0], catalog.SelectedQuest);
        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void AddQuest_SelectsANewQuestWithAnEditableId()
    {
        var catalog = new QuestCatalogViewModel([_errand]);

        catalog.AddQuestCommand.Execute(null);

        Assert.Equal(2, catalog.Quests.Count);
        Assert.Same(catalog.Quests[1], catalog.SelectedQuest);
        Assert.True(catalog.SelectedQuest!.IsNew);
        Assert.Equal("", catalog.SelectedQuest.Id);
    }

    [Fact]
    public void Problem_RepeatedQuestId_NamesIt()
    {
        var catalog = new QuestCatalogViewModel([_errand]);
        catalog.AddQuestCommand.Execute(null);

        catalog.SelectedQuest!.Id = "placeholder_errand";

        Assert.Equal("Quest placeholder_errand is listed twice.", catalog.Problem);
    }

    [Fact]
    public void Problem_QuestWithAProblem_NamesTheQuestAndNotifies()
    {
        var catalog = new QuestCatalogViewModel([_abc, _errand]);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.Quests[1].Title = "";

        Assert.Equal("Quest placeholder_errand: Write the quest's title.", catalog.Problem);
        Assert.Contains(nameof(QuestCatalogViewModel.Problem), notified);
    }

    [Fact]
    public void Problem_NewQuestWithoutId_NamesItByPosition()
    {
        var catalog = new QuestCatalogViewModel([_errand]);

        catalog.AddQuestCommand.Execute(null);

        Assert.Equal("Quest #2: The id must not be empty.", catalog.Problem);
    }

    [Fact]
    public void ToQuests_AddedQuest_IncludedWithTheOthers()
    {
        var catalog = new QuestCatalogViewModel([_errand]);
        catalog.AddQuestCommand.Execute(null);
        var heist = catalog.SelectedQuest!;
        heist.Id = "heist";
        heist.Title = "The heist";
        heist.Description = "Case the vault.";
        heist.Kind = QuestKind.Main;
        heist.AddStageCommand.Execute(null);
        heist.SelectedStage!.Id = "case";
        heist.SelectedStage.Text = "Case the vault.";

        var quests = catalog.ToQuests();

        Assert.Equal([_errand, new QuestDefinition("heist", "The heist", "Case the vault.", QuestKind.Main, [new QuestStage("case", "Case the vault.")])], quests);
    }

    [Fact]
    public void ToQuests_WithAProblem_Throws()
    {
        var catalog = new QuestCatalogViewModel([_errand]);
        catalog.AddQuestCommand.Execute(null);

        Assert.Throws<InvalidOperationException>(catalog.ToQuests);
    }
}
