using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Core.Session;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Exploration;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Session;

// The quest log travels through the save (format 7): every started quest's stage and completion, in start order. The
// quest is started the way play starts it, by a map event, since the session shows the log only through its saves.
public sealed class GameSessionQuestSaveTests
{
    private static readonly DateTimeOffset _savedAt = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly QuestDefinition _heist = new("heist", "A heist", "Case the vault.", QuestKind.Main, [new QuestStage("case", "Case the vault.")]);

    [Fact]
    public void Capture_StartedQuest_KeepsItsStage()
    {
        var session = StartNew();
        session.Exploration.Walk(MoveIntent.FromAxes(1, 0), TestExplorationMap.ThirdOfASecond);
        Assert.IsType<AdvanceQuestStep>(session.Exploration.CurrentEventStep);
        session.Exploration.ContinueEvent();
        Assert.IsType<DialogueStep>(session.Exploration.CurrentEventStep);
        session.Exploration.ContinueEvent();

        var save = session.Capture(_savedAt);

        Assert.Equal([new QuestProgress("errand", "deliver", IsCompleted: false)], save.Quests);
    }

    [Fact]
    public void Capture_NewGame_HasNoQuest()
    {
        Assert.Empty(StartNew().Capture(_savedAt).Quests);
    }

    [Fact]
    public void Restore_SavedQuests_CapturedAgainInTheSameOrder()
    {
        IReadOnlyList<QuestProgress> quests = [new QuestProgress("heist", "case", IsCompleted: true), new QuestProgress("errand", "take", IsCompleted: false)];
        var save = StartNew().Capture(_savedAt) with { Quests = quests };

        var restored = GameSession.Restore(save, Catalog(), new FixedSequenceRandom());

        Assert.Equal(quests, restored.Capture(_savedAt).Quests);
    }

    private static GameSession StartNew() => GameSession.StartNew(Catalog(), "sarxian", new FixedSequenceRandom());

    // A four-cell field whose second cell advances the errand to its last stage when walked onto; the line after the
    // step keeps the event waiting, so the test sees it ran before continuing.
    private static ContentCatalog Catalog()
    {
        var advance = new MapEventBuilder().WithId("cell_1_0").AsTouch().WithSteps(new AdvanceQuestStep("errand", "deliver"), new DialogueStep(new Core.Dialogue.Speech(["Onward."]))).Build();
        var field = new MapLayoutBuilder().WithLayer("1111").Build() with { Id = "field", CellEvents = [new CellEvent(new GridCell(1, 0), advance)] };
        return new ContentCatalogBuilder()
            .WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition())
            .WithMaps(field)
            .WithQuests(TestQuests.Errand, _heist)
            .WithNewGame(new NewGameDefinition
            {
                LeaderChoices = [new LeaderChoice { MemberId = "sarxian" }],
                StartingItems = [],
                StartLocation = new MapLocation("field", new Point2D(0.5, 0.5), Facing.Right),
                FieldSpriteFile = "player.png",
            })
            .Build();
    }
}
