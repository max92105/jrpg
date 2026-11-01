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

// A quest step applies when it becomes current and doesn't wait (ADR 0006; G4 makes a changed one wait for its line),
// and skipping a cutscene applies it too (ADR 0016).
public sealed class EventRunQuestTests
{
    private static readonly ObjectDefinition _sign = new() { Id = "sign", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
    private static readonly QuestDefinition _errand = TestQuests.Errand;
    private static readonly DialogueStep _thanks = new(new Speech(["Thank you."]));

    private readonly ActiveMap _map = new ActiveMapBuilder().WithRows("11").WithObject(_sign, new Point2D(0, 0)).Build();
    private readonly WorldState _world = new();

    [Fact]
    public void Start_StartQuestStep_QuestActiveAtFirstStage()
    {
        var run = Start(new StartQuestStep("errand"));

        Assert.True(run.IsFinished);
        Assert.Equal("take", _world.Quests.StageOf("errand"));
        Assert.False(_world.Quests.IsCompleted("errand"));
    }

    [Fact]
    public void Start_AdvanceQuestStep_QuestAtThatStage()
    {
        var run = Start(new AdvanceQuestStep("errand", "deliver"));

        Assert.True(run.IsFinished);
        Assert.Equal("deliver", _world.Quests.StageOf("errand"));
    }

    [Fact]
    public void Start_CompleteQuestStep_QuestCompleted()
    {
        var run = Start(new CompleteQuestStep("errand"));

        Assert.True(run.IsFinished);
        Assert.True(_world.Quests.IsCompleted("errand"));
        Assert.Equal("deliver", _world.Quests.StageOf("errand"));
    }

    [Fact]
    public void Start_QuestStepBeforeDialogue_AppliedAndWaitsOnTheDialogue()
    {
        var run = Start(new StartQuestStep("errand"), _thanks);

        Assert.Equal(_thanks, run.Current);
        Assert.Equal("take", _world.Quests.StageOf("errand"));
    }

    [Fact]
    public void SkipPresentation_QuestStepBetweenStills_Applied()
    {
        var run = Start(new StillStep("One.png", 4, Caption: null), new CompleteQuestStep("errand"), new StillStep("Two.png", 4, Caption: null));
        Assert.False(_world.Quests.IsCompleted("errand"));

        run.SkipPresentation();

        Assert.True(run.IsFinished);
        Assert.True(_world.Quests.IsCompleted("errand"));
    }

    private EventRun Start(params EventStep[] steps)
    {
        var catalog = new ContentCatalogBuilder().WithQuests(_errand).Build();
        var context = new ExplorationContext(catalog, new Inventory(), _world, new FixedSequenceRandom());
        return new EventRun(new TriggeredEvent(new MapEventBuilder().WithSteps(steps).Build(), _map.Definition.Layout.Objects[0]), _map, context, TestParties.LedBy("sarxian"));
    }
}
