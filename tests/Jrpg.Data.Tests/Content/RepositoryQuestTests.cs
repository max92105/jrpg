using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Quests;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The placeholder errand in the repository's content (decision 397): the start map's quest giver starts it, and the
// innkeeper advances it only while it is at its first stage.
public sealed class RepositoryQuestTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void StartMap_QuestGiverStartsThePlaceholderErrand()
    {
        var giver = _catalog.Maps["milestone"].Layout.Objects.Single(placed => placed.Uid == "quest_giver");

        var starting = Assert.Single(giver.Events, mapEvent => mapEvent.Steps.Any(step => step is StartQuestStep { QuestId: "placeholder_errand" }));
        Assert.Equal(new QuestCondition("placeholder_errand", QuestConditionKind.NotStarted), starting.OnlyWhenQuest);
        var completing = Assert.Single(giver.Events, mapEvent => mapEvent.Steps.Any(step => step is CompleteQuestStep { QuestId: "placeholder_errand" }));
        Assert.Equal(new QuestCondition("placeholder_errand", QuestConditionKind.ReachedStage, "return_to_giver"), completing.OnlyWhenQuest);
    }

    // The first event that may start wins: the reminder, gated on "active", must come after the completing event, or
    // the errand could never finish.
    [Fact]
    public void StartMap_QuestGiverCompletesBeforeReminding()
    {
        var events = _catalog.Maps["milestone"].Layout.Objects.Single(placed => placed.Uid == "quest_giver").Events;

        var completing = events.ToList().FindIndex(mapEvent => mapEvent.OnlyWhenQuest is { Kind: QuestConditionKind.ReachedStage });
        var reminding = events.ToList().FindIndex(mapEvent => mapEvent.OnlyWhenQuest is { Kind: QuestConditionKind.Active });
        Assert.True(completing >= 0 && reminding > completing, $"completing at {completing}, reminding at {reminding}");
    }

    [Fact]
    public void StartMap_InnkeeperAdvancesTheErrandOnlyAtAskInnkeeper()
    {
        var innkeeper = _catalog.Maps["milestone"].Layout.Objects.Single(placed => placed.Uid == "innkeeper");

        var advancing = Assert.Single(innkeeper.Events, mapEvent => mapEvent.Steps.Any(step => step is AdvanceQuestStep));
        Assert.Equal(new QuestCondition("placeholder_errand", QuestConditionKind.AtStage, "ask_innkeeper"), advancing.OnlyWhenQuest);
        Assert.Contains(advancing.Steps, step => step is AdvanceQuestStep { QuestId: "placeholder_errand", StageId: "return_to_giver" });
    }
}
