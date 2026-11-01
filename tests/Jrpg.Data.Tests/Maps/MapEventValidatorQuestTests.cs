using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Data.Maps;
using Jrpg.Data.Tests.Quests;

namespace Jrpg.Data.Tests.Maps;

// A map is refused when a quest step or condition names a quest or a stage the content doesn't have, when a condition's
// stage doesn't fit its kind, or when an ungated event would shadow a later one of its activation on its object (ADRs
// 0032, 0033).
public sealed class MapEventValidatorQuestTests
{
    private static readonly ObjectDefinition _sign = new() { Id = "sign", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
    private static readonly QuestDefinition _errand = TestQuests.Errand;
    private static readonly DialogueStep _line = new(new Core.Dialogue.Speech(["Hello."]));

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
        var layout = WithSignEvents(SignEvent(new StartQuestStep("errand"), new AdvanceQuestStep("errand", "deliver"), new CompleteQuestStep("errand")));

        Assert.Null(MapEventValidator.FindProblem(layout, _references));
    }

    [Fact]
    public void FindProblem_StartQuestUnknownQuest_NamesIt()
    {
        var problem = MapEventValidator.FindProblem(WithSignEvents(SignEvent(new StartQuestStep("heist"))), _references);

        Assert.Equal("event 'sign@0,0_0': step 0: startQuest has unknown quest id 'heist'.", problem);
    }

    [Fact]
    public void FindProblem_AdvanceQuestUnknownStage_NamesQuestAndStage()
    {
        var problem = MapEventValidator.FindProblem(WithSignEvents(SignEvent(new AdvanceQuestStep("errand", "burn"))), _references);

        Assert.Equal("event 'sign@0,0_0': step 0: advanceQuest names stage 'burn', which quest 'errand' doesn't have.", problem);
    }

    [Fact]
    public void FindProblem_AdvanceQuestUnknownQuest_NamesIt()
    {
        var problem = MapEventValidator.FindProblem(WithSignEvents(SignEvent(new AdvanceQuestStep("heist", "take"))), _references);

        Assert.Equal("event 'sign@0,0_0': step 0: advanceQuest has unknown quest id 'heist'.", problem);
    }

    [Fact]
    public void FindProblem_CompleteQuestUnknownQuest_NamesIt()
    {
        var problem = MapEventValidator.FindProblem(WithSignEvents(SignEvent(new CompleteQuestStep("heist"))), _references);

        Assert.Equal("event 'sign@0,0_0': step 0: completeQuest has unknown quest id 'heist'.", problem);
    }

    public static TheoryData<QuestCondition, string?> Conditions => new()
    {
        { new QuestCondition("errand", QuestConditionKind.Active), null },
        { new QuestCondition("heist", QuestConditionKind.Active), "onlyWhenQuest has unknown quest id 'heist'." },
        { new QuestCondition("errand", QuestConditionKind.Active, "take"), "onlyWhenQuest kind 'active' names no stage; remove stageId 'take'." },
        { new QuestCondition("errand", QuestConditionKind.ReachedStage, "deliver"), null },
        { new QuestCondition("errand", QuestConditionKind.ReachedStage), "onlyWhenQuest kind 'reachedStage' needs a stageId." },
        { new QuestCondition("errand", QuestConditionKind.AtStage, "take"), null },
        { new QuestCondition("errand", QuestConditionKind.AtStage, "burn"), "onlyWhenQuest has unknown stage 'burn' of quest 'errand'." },
    };

    [Theory]
    [MemberData(nameof(Conditions))]
    public void FindProblem_QuestCondition_NamesWhatIsWrong(QuestCondition condition, string? expected)
    {
        var layout = WithSignEvents(SignEvent(_line) with { OnlyWhenQuest = condition });

        var problem = MapEventValidator.FindProblem(layout, _references);

        Assert.Equal(expected is null ? null : $"event 'sign@0,0_0': {expected}", problem);
    }

    [Fact]
    public void FindProblem_UngatedEventBeforeAnotherOfItsActivation_NamesBoth()
    {
        var always = SignEvent(_line);
        var gated = SignEvent(_line) with { Id = "sign@0,0_1", OnlyWhenQuest = new QuestCondition("errand", QuestConditionKind.Completed) };

        var problem = MapEventValidator.FindProblem(WithSignEvents(always, gated), _references);

        Assert.Equal("object 'sign@0,0': event 'sign@0,0_0' can always start, so the later interact event 'sign@0,0_1' never would; gate it or make it once.", problem);
    }

    [Fact]
    public void FindProblem_UngatedEventLast_ReturnsNull()
    {
        var gated = SignEvent(_line) with { OnlyWhenQuest = new QuestCondition("errand", QuestConditionKind.Completed) };
        var always = SignEvent(_line) with { Id = "sign@0,0_1" };

        Assert.Null(MapEventValidator.FindProblem(WithSignEvents(gated, always), _references));
    }

    [Fact]
    public void FindProblem_UngatedOnceEventFirst_ReturnsNull()
    {
        var once = SignEvent(_line) with { IsOnce = true };
        var always = SignEvent(_line) with { Id = "sign@0,0_1" };

        Assert.Null(MapEventValidator.FindProblem(WithSignEvents(once, always), _references));
    }

    [Fact]
    public void FindProblem_UngatedEventBeforeAnotherActivation_ReturnsNull()
    {
        var interact = SignEvent(_line);
        var touch = SignEvent(_line) with { Id = "sign@0,0_1", Activation = EventActivation.Touch, Prompt = null };

        Assert.Null(MapEventValidator.FindProblem(WithSignEvents(interact, touch), _references));
    }

    private static MapEvent SignEvent(params EventStep[] steps) =>
        new() { Id = "sign@0,0_0", Activation = EventActivation.Interact, Prompt = "Read", Steps = steps };

    private static MapLayout WithSignEvents(params MapEvent[] events) => new()
    {
        Id = "square",
        Width = 1,
        Height = 1,
        Layers = [],
        Objects = [new PlacedObject { Uid = "sign@0,0", ObjectId = "sign", Position = new Point2D(0, 0), Z = MapLayers.PlayerZ, Events = events }],
    };
}
