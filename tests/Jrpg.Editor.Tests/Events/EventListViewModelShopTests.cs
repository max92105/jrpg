using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// The Shopkeeper preset and the Open shop button, on a villager who already talks.
public sealed class EventListViewModelShopTests
{
    private static readonly MapEvent _talk = new()
    {
        Id = "villager_1_0",
        Activation = EventActivation.Interact,
        Prompt = "Talk",
        Steps = [new DialogueStep(new Speech(["Hello."]))],
    };

    private static readonly PlacedObject _villager = new()
    {
        Uid = "villager_1",
        ObjectId = "villager",
        Position = new Point2D(4, 4),
        Z = 1,
        Events = [_talk],
    };

    private static readonly StepChoices _choices = new(["potion"], ["villager"], ["start"], ["sarxian"]) { ShopIds = ["general_store"] };

    private readonly EventListViewModel _list = new(_villager, _choices, "villager");

    // The preset's line is the developer's to write and its shop theirs to pick: the empty line is selected first.
    [Fact]
    public void ApplyShopkeeperPresetCommand_SelectsTheEmptyDialogueStep()
    {
        _list.ApplyShopkeeperPresetCommand.Execute(null);

        var shopkeeper = Assert.Single(_list.Events);
        Assert.Same(shopkeeper.Steps[0], shopkeeper.SelectedStep);
        Assert.Equal("Open shop: ", shopkeeper.Steps[1].Label);
        Assert.Equal("Event villager_1_1, step 1: Write at least one line.", _list.Problem);
    }

    [Fact]
    public void AddOpenShopCommand_NoStepChosen_AddsOpenShopLast()
    {
        var talk = _list.Events[0];
        talk.SelectedStep = null;

        talk.AddOpenShopCommand.Execute(null);
        talk.Steps[^1].ShopId = "general_store";

        Assert.Equal(new OpenShopStep("general_store"), talk.ToEvent().Steps[^1]);
    }
}
