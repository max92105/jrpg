using Jrpg.Core.Content;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;
using Jrpg.Core.World;

namespace Jrpg.Core.Tests.Exploration;

public sealed class EventRunTests
{
    private static readonly ObjectDefinition _chest = new() { Id = "chest", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
    private static readonly ObjectDefinition _openedChest = _chest with { Id = "chest_opened" };
    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").WithMaxStack(99).Build();
    private static readonly ContentCatalog _catalog = new ContentCatalogBuilder()
        .WithItems(_potion)
        .WithMembers(new PartyMemberBuilder().WithId("omer").BuildDefinition())
        .Build();

    private static readonly DialogueStep _foundSomething = new(new Speech(["Something glints inside."]));
    private static readonly GiveItemStep _givePotions = new("potion", 3);
    private static readonly SwapObjectStep _openChest = new("chest_opened");
    private static readonly StillStep _firstStill = new("Placeholder1.png", 4, "Still one.");
    private static readonly StillStep _secondStill = new("Placeholder2.png", 4, Caption: null);
    private static readonly WarpStep _warpAway = new("elsewhere", new GridCell(0, 0));

    private readonly ActiveMap _map = new ActiveMapBuilder().WithRows("11")
        .WithObject(_chest, new Point2D(0, 0))
        .WithKnownObject(_openedChest)
        .Build();

    private readonly Inventory _inventory = new();
    private readonly PartyState _party = TestParties.LedBy("sarxian");

    private PlacedObject Chest => _map.Definition.Layout.Objects[0];

    [Fact]
    public void Constructor_OnceEvent_MarksDoneBeforeFirstStep()
    {
        var chestEvent = new MapEventBuilder().AsOnce().WithSteps(_foundSomething).Build();

        var run = Start(chestEvent);

        Assert.True(_map.IsDone(chestEvent));
        Assert.Equal(_foundSomething, run.Current);
    }

    [Fact]
    public void Continue_OnStillStep_MovesToTheNextStep()
    {
        var run = Start(new MapEventBuilder().WithSteps(_firstStill, _secondStill).Build());

        run.Continue();

        Assert.Equal(_secondStill, run.Current);
    }

    [Fact]
    public void SkipPresentation_StillsThenWarp_StopsOnTheWarp()
    {
        var run = Start(new MapEventBuilder().WithSteps(_firstStill, _secondStill, _warpAway).Build());

        run.SkipPresentation();

        Assert.Equal(_warpAway, run.Current);
    }

    [Fact]
    public void Start_VoiceThenStill_RunsOnToTheStillWithTheVoiceStarted()
    {
        var voice = new VoiceStep("Placeholder.wav");

        var run = Start(new MapEventBuilder().WithSteps(voice, _firstStill).Build());

        Assert.Equal(_firstStill, run.Current);
        Assert.Equal(voice, run.StartedVoice);
    }

    [Fact]
    public void SkipPresentation_VoiceAndStills_StopsOnTheWarp()
    {
        var run = Start(new MapEventBuilder().WithSteps(new VoiceStep("Placeholder.wav"), _firstStill, _secondStill, _warpAway).Build());

        run.SkipPresentation();

        Assert.Equal(_warpAway, run.Current);
        Assert.Null(run.StartedVoice);
    }

    [Fact]
    public void SkipPresentation_StillThenSwapThenStill_AppliesTheSwapAndFinishes()
    {
        var run = Start(new MapEventBuilder().WithSteps(_firstStill, _openChest, _secondStill).Build());

        run.SkipPresentation();

        Assert.True(run.IsFinished);
        Assert.Equal("chest_opened", _map.CurrentDefinitionOf(Chest).Id);
    }

    [Fact]
    public void SkipPresentation_OnDialogue_ChangesNothing()
    {
        var run = Start(new MapEventBuilder().WithSteps(_foundSomething, _firstStill).Build());

        run.SkipPresentation();

        Assert.Equal(_foundSomething, run.Current);
    }

    [Fact]
    public void Constructor_RepeatableEvent_IsNotMarkedDone()
    {
        var signEvent = new MapEventBuilder().WithSteps(_foundSomething).Build();

        Start(signEvent);

        Assert.False(_map.IsDone(signEvent));
    }

    [Fact]
    public void Constructor_SwapThenDialogue_AppliesSwapAndWaitsOnDialogue()
    {
        var run = Start(new MapEventBuilder().WithSteps(_openChest, _foundSomething).Build());

        Assert.Equal("chest_opened", _map.CurrentDefinitionOf(Chest).Id);
        Assert.Equal(_foundSomething, run.Current);
        Assert.False(run.IsFinished);
    }

    [Fact]
    public void Constructor_OnlySwapSteps_IsFinished()
    {
        var run = Start(new MapEventBuilder().WithSteps(_openChest).Build());

        Assert.True(run.IsFinished);
        Assert.Null(run.Current);
        Assert.Equal("chest_opened", _map.CurrentDefinitionOf(Chest).Id);
    }

    [Fact]
    public void Continue_ReachesGiveItem_AddsItemToInventory()
    {
        var run = Start(new MapEventBuilder().WithSteps(_foundSomething, _givePotions).Build());
        Assert.Equal(0, _inventory.QuantityOf("potion"));

        run.Continue();

        Assert.Equal(_givePotions, run.Current);
        Assert.Equal(3, _inventory.QuantityOf("potion"));
        Assert.Equal(new ItemReceipt(_potion, Added: 3, Lost: 0), run.CurrentReceipt);
    }

    [Fact]
    public void Constructor_GiveItemBeyondMaxStack_ReceiptReportsLostCopies()
    {
        _inventory.Add(_potion, 98);

        var run = Start(new MapEventBuilder().WithSteps(_givePotions).Build());

        Assert.Equal(99, _inventory.QuantityOf("potion"));
        Assert.Equal(new ItemReceipt(_potion, Added: 1, Lost: 2), run.CurrentReceipt);
    }

    [Fact]
    public void Continue_PastGiveItem_ClearsTheReceipt()
    {
        var run = Start(new MapEventBuilder().WithSteps(_givePotions, _foundSomething).Build());

        run.Continue();

        Assert.Equal(_foundSomething, run.Current);
        Assert.Null(run.CurrentReceipt);
    }

    [Fact]
    public void Continue_WarpStep_WaitsOnIt()
    {
        var warp = new WarpStep("forest", new GridCell(1, 1));
        var run = Start(new MapEventBuilder().WithSteps(_foundSomething, warp).Build());

        run.Continue();

        Assert.Equal(warp, run.Current);
    }

    [Fact]
    public void Continue_LastStep_Finishes()
    {
        var run = Start(new MapEventBuilder().WithSteps(_foundSomething).Build());

        run.Continue();

        Assert.True(run.IsFinished);
        Assert.Null(run.Current);
    }

    [Fact]
    public void Continue_AfterFinished_Throws()
    {
        var run = Start(new MapEventBuilder().WithSteps(_foundSomething).Build());
        run.Continue();

        Assert.Throws<InvalidOperationException>(run.Continue);
    }

    [Fact]
    public void Constructor_SwapInCellEvent_Throws()
    {
        var cellEvent = new MapEventBuilder().WithSteps(_openChest).Build();

        Assert.Throws<InvalidOperationException>(() => new EventRun(new TriggeredEvent(cellEvent, Source: null), _map, Context(), _party));
    }

    [Fact]
    public void Run_RecruitStep_AddsMemberAtLevelOne()
    {
        var run = Start(new MapEventBuilder().WithSteps(new RecruitStep("omer")).Build());

        var omer = _party.MemberById("omer");
        Assert.Equal((1, 0), (omer.Level, omer.Experience));
        Assert.Equal((omer.Vitals.MaxHp, omer.Vitals.MaxMp), (omer.Vitals.Hp, omer.Vitals.Mp));
        Assert.Contains(omer, _party.ActiveMembers);
        Assert.Equal(new Recruitment(omer, HasJoined: true), run.CurrentRecruitment);
    }

    [Fact]
    public void Run_RecruitStepForRecruited_ChangesNothingAndSaysSo()
    {
        var recruitOmer = new MapEventBuilder().WithSteps(new RecruitStep("omer")).Build();
        Start(recruitOmer);
        var omer = _party.MemberById("omer");

        var again = Start(recruitOmer);

        Assert.Equal(2, _party.Members.Count);
        Assert.Same(omer, _party.MemberById("omer"));
        Assert.Equal(new Recruitment(omer, HasJoined: false), again.CurrentRecruitment);
    }

    private EventRun Start(MapEvent mapEvent) => new(new TriggeredEvent(mapEvent, Chest), _map, Context(), _party);

    private ExplorationContext Context() => new(_catalog, _inventory, new WorldState(), new FixedSequenceRandom());
}
