using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

public sealed class EventPresetsTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    // A closed door that already used ids door_1_0 and door_1_1.
    private static readonly EventIdAllocation _door = new("door_1", "door_close", ["door_1_0", "door_1_1"]);

    [Fact]
    public void Chest_GivesItemThenSwapsToOpenedObject()
    {
        var chest = Assert.Single(EventPresets.Chest(new EventIdAllocation("chest_1", "chest", []), "chest_opened"));

        Assert.Equal(("chest_1_0", EventActivation.Interact, "Open", true), (chest.Id, chest.Activation, chest.Prompt, chest.IsOnce));
        Assert.Equal([new GiveItemStep("", 1), new SwapObjectStep("chest_opened")], chest.Steps);
    }

    [Fact]
    public void Talk_AnyObject_IsInteractDialogueWithTalkPrompt()
    {
        var talk = Assert.Single(EventPresets.Talk(new EventIdAllocation("villager_1", "villager", [])));

        Assert.Equal(("villager_1_0", EventActivation.Interact, "Talk", false), (talk.Id, talk.Activation, talk.Prompt, talk.IsOnce));
        Assert.IsType<DialogueStep>(Assert.Single(talk.Steps));
    }

    [Fact]
    public void Talk_AnyObject_LeavesSpeakerAndLinesEmpty()
    {
        var talk = Assert.Single(EventPresets.Talk(new EventIdAllocation("villager_1", "villager", [])));

        var speech = Assert.IsType<DialogueStep>(Assert.Single(talk.Steps)).Speech;
        Assert.Null(speech.Speaker);
        Assert.Empty(speech.Lines);
    }

    // Decision 353: the forge master's line is left for the developer, then the Forge opens.
    [Fact]
    public void ForgeMaster_TalksThenOpensTheForge()
    {
        var forgeMaster = Assert.Single(EventPresets.ForgeMaster(new EventIdAllocation("smith_1", "villager", [])));

        Assert.Equal(("smith_1_0", EventActivation.Interact, "Talk"), (forgeMaster.Id, forgeMaster.Activation, forgeMaster.Prompt));
        Assert.Collection(
            forgeMaster.Steps,
            step => Assert.Empty(Assert.IsType<DialogueStep>(step).Speech.Lines),
            step => Assert.IsType<OpenForgeStep>(step));
    }

    [Fact]
    public void DoorYouOpen_OpensWhileClosed_WarpsOnlyWhenOpen()
    {
        var events = EventPresets.DoorYouOpen(_door, "door_open");

        Assert.Equal(("door_1_2", EventActivation.Interact, "door_close"), (events[0].Id, events[0].Activation, events[0].OnlyWhenObjectId));
        Assert.Equal(("Open", "Enter"), (events[0].Prompt, events[1].Prompt));
        Assert.Equal([new SwapObjectStep("door_open")], events[0].Steps);
        Assert.Equal(("door_1_3", EventActivation.Touch, "door_open"), (events[1].Id, events[1].Activation, events[1].OnlyWhenObjectId));
        Assert.Equal([new WarpStep("", new GridCell(0, 0))], events[1].Steps);
    }

    [Fact]
    public void Door_WalkedInto_Warps()
    {
        var door = Assert.Single(EventPresets.Door(_door));

        Assert.Equal((EventActivation.Touch, "Enter", null), (door.Activation, door.Prompt, door.OnlyWhenObjectId));
        Assert.IsType<WarpStep>(Assert.Single(door.Steps));
    }

    [Fact]
    public void Sign_PressedForADialogueStillToWrite()
    {
        var sign = Assert.Single(EventPresets.Sign(_door));

        Assert.Equal("Read", sign.Prompt);
        Assert.Empty(Assert.IsType<DialogueStep>(Assert.Single(sign.Steps)).Speech.Lines);
    }

    [Theory]
    [InlineData("door_close", "door_open")]
    public void GuessOpenedObject_RepositoryObjects_FindsTheOpenedOne(string objectId, string expected)
    {
        Assert.Equal(expected, EventPresets.GuessOpenedObject(objectId, _catalog.Objects));
    }

    [Fact]
    public void GuessOpenedObject_ChestFindsChestOpened()
    {
        Assert.Equal("chest_opened", EventPresets.GuessOpenedObject("chest", _catalog.Objects));
    }

    [Fact]
    public void GuessOpenedObject_NoObjectWithOpen_KeepsTheObject()
    {
        var onlyTrees = _catalog.Objects.Where(pair => pair.Key.StartsWith("tree", StringComparison.Ordinal)).ToDictionary();

        Assert.Equal("tree", EventPresets.GuessOpenedObject("tree", onlyTrees));
    }

    [Fact]
    public void GuessOpenedObject_OpenedObjectsInSeveralCategories_PrefersTheSameCategory()
    {
        // door_open sorts before vault_opened, but only the vault's own category counts first.
        var definitions = new Dictionary<string, ObjectDefinition>
        {
            ["vault"] = Definition("vault", "Vaults"),
            ["door_open"] = Definition("door_open", "General"),
            ["vault_opened"] = Definition("vault_opened", "Vaults"),
        };

        Assert.Equal("vault_opened", EventPresets.GuessOpenedObject("vault", definitions));
    }

    private static ObjectDefinition Definition(string id, string category) => new()
    {
        Id = id,
        Width = 1,
        Height = 1,
        CollisionPolygon = [],
        Category = category,
        Tiles = [],
        AbovePlayerZones = [],
    };
}
