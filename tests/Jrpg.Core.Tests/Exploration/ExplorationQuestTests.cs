using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;
using Jrpg.Core.World;

namespace Jrpg.Core.Tests.Exploration;

// An event gated on a quest starts only while the quest meets its condition (ADR 0033), so a villager's lines follow
// the errand: one event per situation, the first that may start wins. The same gate holds the cell and arrival events.
public sealed class ExplorationQuestTests
{
    private static readonly QuestDefinition _errand = TestQuests.Errand;
    private static readonly DialogueStep _offer = new(new Speech(["Take this letter to the mill."]));
    private static readonly DialogueStep _nudge = new(new Speech(["The mill is north."]));
    private static readonly DialogueStep _thanks = new(new Speech(["You delivered it. Thank you."]));
    private static readonly DialogueStep _again = new(new Speech(["Thanks again."]));
    private static readonly DialogueStep _creak = new(new Speech(["The floor creaks."]));
    private static readonly DialogueStep _welcome = new(new Speech(["Welcome, stranger."]));

    private static readonly MapEvent _offerEvent = new MapEventBuilder().WithId("villager_0").WithSteps(_offer, new StartQuestStep("errand"))
        .OnlyWhenQuest(new QuestCondition("errand", QuestConditionKind.NotStarted)).Build();
    private static readonly MapEvent _nudgeEvent = new MapEventBuilder().WithId("villager_1").WithSteps(_nudge, new AdvanceQuestStep("errand", "deliver"))
        .OnlyWhenQuest(new QuestCondition("errand", QuestConditionKind.AtStage, "take")).Build();
    private static readonly MapEvent _thanksEvent = new MapEventBuilder().WithId("villager_2").WithSteps(_thanks, new CompleteQuestStep("errand"))
        .OnlyWhenQuest(new QuestCondition("errand", QuestConditionKind.AtStage, "deliver")).Build();
    private static readonly MapEvent _againEvent = new MapEventBuilder().WithId("villager_3").WithSteps(_again)
        .OnlyWhenQuest(new QuestCondition("errand", QuestConditionKind.Completed)).Build();
    private static readonly MapEvent _creakEvent = new MapEventBuilder().WithId("cell_0_1").AsTouch().WithSteps(_creak)
        .OnlyWhenQuest(new QuestCondition("errand", QuestConditionKind.ReachedStage, "deliver")).Build();
    private static readonly MapEvent _welcomeEvent = new MapEventBuilder().WithId("cell_0_0").AsArrive().WithSteps(_welcome)
        .OnlyWhenQuest(new QuestCondition("errand", QuestConditionKind.NotStarted)).Build();

    private readonly WorldState _world = new();

    [Fact]
    public void TryInteract_TwoEventsGatedOnStages_RunsTheCurrentStagesEvent()
    {
        _world.Quests.AdvanceTo(_errand, "deliver");
        var exploration = StartAtTheInn();

        exploration.TryInteract();

        Assert.Equal(_thanks, exploration.CurrentEventStep);
    }

    [Fact]
    public void TryInteract_AfterEventStartedTheQuest_RunsTheActiveEvent()
    {
        var exploration = StartAtTheInn();
        exploration.TryInteract();
        Assert.Equal(_offer, exploration.CurrentEventStep);
        exploration.ContinueEvent();
        Assert.False(exploration.IsEventRunning);

        exploration.TryInteract();

        Assert.Equal(_nudge, exploration.CurrentEventStep);
        Assert.Equal("take", _world.Quests.StageOf("errand"));
    }

    [Fact]
    public void InteractionInFront_AfterEventCompletedTheQuest_FollowsTheCompletedEvent()
    {
        _world.Quests.AdvanceTo(_errand, "deliver");
        var exploration = StartAtTheInn();
        exploration.TryInteract();

        exploration.ContinueEvent();

        Assert.True(_world.Quests.IsCompleted("errand"));
        Assert.Same(_againEvent, exploration.InteractionInFront?.Event);
    }

    [Fact]
    public void Walk_OntoCellGatedOff_StartsNoEvent()
    {
        var exploration = StartAtTheInn();

        exploration.Walk(MoveIntent.FromAxes(-1, 0), TestExplorationMap.ThirdOfASecond);

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Walk_OntoCellGatedOn_StartsItsEvent()
    {
        _world.Quests.AdvanceTo(_errand, "deliver");
        var exploration = StartAtTheInn();

        exploration.Walk(MoveIntent.FromAxes(-1, 0), TestExplorationMap.ThirdOfASecond);

        Assert.Equal(_creak, exploration.CurrentEventStep);
    }

    [Fact]
    public void New_ArrivalEventForQuestNotStarted_Runs()
    {
        var exploration = Start("gate", new Point2D(0.5, 0.5));

        Assert.Equal(_welcome, exploration.CurrentEventStep);
    }

    [Fact]
    public void New_ArrivalEventForQuestStarted_DoesNotRun()
    {
        _world.Quests.Start(_errand);

        var exploration = Start("gate", new Point2D(0.5, 0.5));

        Assert.False(exploration.IsEventRunning);
    }

    // The inn: a villager at (1, 0) the player faces from (1.5, 1.5), a creaking cell at (0, 1) one step left. The
    // gate: one cell with an arrival event.
    private ExplorationState StartAtTheInn() => Start("inn", new Point2D(1.5, 1.5));

    private ExplorationState Start(string mapId, Point2D position)
    {
        var inn = new MapLayoutBuilder().WithLayer("111", "111").Build() with
        {
            Id = "inn",
            Objects = [new PlacedObject { Uid = "villager@1,0", ObjectId = "notice_board", Position = new Point2D(1, 0), Z = MapLayers.PlayerZ, Events = [_offerEvent, _nudgeEvent, _thanksEvent, _againEvent] }],
            CellEvents = [new CellEvent(new GridCell(0, 1), _creakEvent)],
        };
        var gate = new MapLayoutBuilder().WithLayer("1").Build() with { Id = "gate", CellEvents = [new CellEvent(new GridCell(0, 0), _welcomeEvent)] };
        var catalog = new ContentCatalogBuilder()
            .WithTiles(TestTiles.Walkable(1))
            .WithObjects(TestExplorationMap.NoticeBoard)
            .WithMaps(inn, gate)
            .WithQuests(_errand)
            .Build();
        var context = new ExplorationContext(catalog, new Inventory(), _world, new FixedSequenceRandom());
        return new ExplorationState(context, TestParties.LedBy("sarxian"), new MapLocation(mapId, position, Facing.Up));
    }
}
