using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// The Preset drop-down (decision 731): the fixed presets under their groups, then a quest giver entry per quest. On a
// villager without events and two quests, built here.
public sealed class EventListViewModelPresetTests
{
    private static readonly PlacedObject _villager = new()
    {
        Uid = "villager_1",
        ObjectId = "villager",
        Position = new Point2D(4, 4),
        Z = MapLayers.PlayerZ,
        Events = [],
    };

    private static readonly StepChoices _choices =
        new(["potion"], ["villager"], ["start"], ["sarxian"]) { QuestIds = ["heist", "placeholder_errand"] };

    private readonly EventListViewModel _list = new(_villager, _choices, "villager_open");

    [Fact]
    public void PresetChoices_TwoQuests_ListsTheFixedPresetsThenOneQuestGiverPerQuest()
    {
        (string, bool)[] expected =
        [
            ("OBJECTS", true), ("Chest", false), ("Door", false), ("Door you open", false), ("Sign", false),
            ("PEOPLE", true), ("Talk", false), ("Forge master", false), ("Shopkeeper", false),
            ("QUEST GIVER", true), ("Quest giver: heist", false), ("Quest giver: placeholder_errand", false),
        ];

        Assert.Equal(expected, _list.PresetChoices.Select(choice => (choice.Label, choice.IsTitle)));
    }

    [Fact]
    public void PresetChoices_NoQuest_QuestGiverEntryCannotRun()
    {
        var list = new EventListViewModel(_villager, _choices with { QuestIds = [] }, "villager_open");

        var entry = list.PresetChoices[^1];

        Assert.Equal("Quest giver (no quest yet)", entry.Label);
        Assert.False(entry.Command!.CanExecute(entry.Parameter));
    }

    [Fact]
    public void PresetChoices_SecondQuestGiverEntry_AppliesThePresetForItsQuest()
    {
        Run("Quest giver: placeholder_errand");

        var offer = _list.Events[0].ToEvent();
        Assert.Equal(new QuestCondition("placeholder_errand", QuestConditionKind.NotStarted), offer.OnlyWhenQuest);
    }

    [Fact]
    public void PresetChoices_ChestEntry_RunsTheChestPreset()
    {
        Run("Chest");

        var chest = Assert.Single(_list.Events);
        Assert.Equal([EventStepKind.GiveItem, EventStepKind.SwapObject], chest.Steps.Select(step => step.Kind));
    }

    // The last of its group: an entry wired to its neighbour's command fails here.
    [Fact]
    public void PresetChoices_ShopkeeperEntry_RunsTheShopkeeperPreset()
    {
        Run("Shopkeeper");

        var shopkeeper = Assert.Single(_list.Events);
        Assert.Equal(EventStepKind.OpenShop, shopkeeper.Steps[^1].Kind);
    }

    private void Run(string label)
    {
        var entry = _list.PresetChoices.Single(choice => choice.Label == label);
        entry.Command!.Execute(entry.Parameter);
    }
}
