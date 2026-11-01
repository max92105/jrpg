using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Data.Maps;
using Jrpg.Data.Tests.Quests;

namespace Jrpg.Data.Tests.Maps;

// A map is refused when a quest step names a quest or a stage the content doesn't have (ADR 0032).
public sealed class MapEventValidatorQuestTests
{
    private static readonly ObjectDefinition _sign = new() { Id = "sign", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
    private static readonly QuestDefinition _errand = TestQuests.Errand;

    private static readonly MapReferences _references = new(
        new MapContentReferences(
            TileIds: new HashSet<int>(),
            Objects: new Dictionary<string, ObjectDefinition> { ["sign"] = _sign },
            ItemIds: new HashSet<string>(),
            EncounterIds: new HashSet<string>(),
            MemberIds: new HashSet<string>())
        {
            Quests = new Dictionary<string, QuestDefinition> { ["errand"] = _errand },
        },
        Layouts: new Dictionary<string, MapLayout>());

    [Fact]
    public void FindProblem_KnownQuestAndStage_ReturnsNull()
    {
        var layout = WithSignEvent(new StartQuestStep("errand"), new AdvanceQuestStep("errand", "deliver"), new CompleteQuestStep("errand"));

        Assert.Null(MapEventValidator.FindProblem(layout, _references));
    }

    [Fact]
    public void FindProblem_StartQuestUnknownQuest_NamesIt()
    {
        var problem = MapEventValidator.FindProblem(WithSignEvent(new StartQuestStep("heist")), _references);

        Assert.Equal("event 'sign@0,0_0': step 0: startQuest has unknown quest id 'heist'.", problem);
    }

    [Fact]
    public void FindProblem_AdvanceQuestUnknownStage_NamesQuestAndStage()
    {
        var problem = MapEventValidator.FindProblem(WithSignEvent(new AdvanceQuestStep("errand", "burn")), _references);

        Assert.Equal("event 'sign@0,0_0': step 0: advanceQuest names stage 'burn', which quest 'errand' doesn't have.", problem);
    }

    [Fact]
    public void FindProblem_AdvanceQuestUnknownQuest_NamesIt()
    {
        var problem = MapEventValidator.FindProblem(WithSignEvent(new AdvanceQuestStep("heist", "take")), _references);

        Assert.Equal("event 'sign@0,0_0': step 0: advanceQuest has unknown quest id 'heist'.", problem);
    }

    [Fact]
    public void FindProblem_CompleteQuestUnknownQuest_NamesIt()
    {
        var problem = MapEventValidator.FindProblem(WithSignEvent(new CompleteQuestStep("heist")), _references);

        Assert.Equal("event 'sign@0,0_0': step 0: completeQuest has unknown quest id 'heist'.", problem);
    }

    private static MapLayout WithSignEvent(params EventStep[] steps) => new()
    {
        Id = "square",
        Width = 1,
        Height = 1,
        Layers = [],
        Objects = [new PlacedObject
        {
            Uid = "sign@0,0",
            ObjectId = "sign",
            Position = new Point2D(0, 0),
            Z = MapLayers.PlayerZ,
            Events = [new MapEvent { Id = "sign@0,0_0", Activation = EventActivation.Interact, Prompt = "Read", Steps = steps }],
        }],
    };
}
