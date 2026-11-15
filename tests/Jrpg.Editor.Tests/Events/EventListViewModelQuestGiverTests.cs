using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// The Quest giver preset (decision 588): its quest comes from the chosen entry of the Preset drop-down (decision 731);
// applying it gives the NPC the offer and the reminder, and it can't run without a quest.
public sealed class EventListViewModelQuestGiverTests
{
    private static readonly PlacedObject _elder = new()
    {
        Uid = "elder_1",
        ObjectId = "villager",
        Position = new Point2D(4, 4),
        Z = MapLayers.PlayerZ,
        Events = [],
    };

    private static readonly StepChoices _choices =
        new([], ["villager"], ["start"], ["sarxian"]) { QuestIds = ["heist", "placeholder_errand"] };

    [Fact]
    public void ApplyQuestGiverPreset_ChosenQuest_AddsBothEvents()
    {
        var list = new EventListViewModel(_elder, _choices, "villager");

        list.ApplyQuestGiverPresetCommand.Execute("placeholder_errand");

        Assert.Equal(["elder_1_0", "elder_1_1"], list.Events.Select(item => item.Id));
        var events = list.Events.Select(item => item.ToEvent()).ToList();
        Assert.Equal(new QuestCondition("placeholder_errand", QuestConditionKind.NotStarted), events[0].OnlyWhenQuest);
        Assert.Equal(new QuestCondition("placeholder_errand", QuestConditionKind.Active), events[1].OnlyWhenQuest);
        Assert.Contains(events[0].Steps, step => step is StartQuestStep { QuestId: "placeholder_errand" });
        Assert.Equal("Event elder_1_0, step 1: Write at least one line.", list.Problem);
    }

    [Fact]
    public void ApplyQuestGiverPresetCommand_EmptyQuest_CannotExecute()
    {
        var list = new EventListViewModel(_elder, _choices, "villager");

        Assert.False(list.ApplyQuestGiverPresetCommand.CanExecute(""));
    }

    [Fact]
    public void ApplyQuestGiverPresetCommand_AQuest_CanExecute()
    {
        var list = new EventListViewModel(_elder, _choices, "villager");

        Assert.True(list.ApplyQuestGiverPresetCommand.CanExecute("heist"));
    }
}
