using Jrpg.Core.Events;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>
/// The placeholder errand on the start map (decision 397): the giver's talk starts it, so the next talk runs the active
/// event instead; the innkeeper still rests a party with no quest.
/// </summary>
public sealed class StartMapQuestTests
{
    [Fact]
    public void Interact_QuestGiver_StartsThePlaceholderErrand()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowQuestGiver);
        Assert.True(exploration.TryInteract());
        var offer = Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
        exploration.ContinueEvent();
        Assert.False(exploration.IsEventRunning);

        Assert.True(exploration.TryInteract());

        var reminder = Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
        Assert.Equal("Quest giver", offer.Speech.Speaker);
        Assert.NotEqual(offer.Speech.Lines[0], reminder.Speech.Lines[0]);
    }

    [Fact]
    public void Interact_InnkeeperWithNoQuest_StillRunsTheRestEvent()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowInnkeeper);
        Assert.True(exploration.TryInteract());
        var line = Assert.IsType<DialogueStep>(exploration.CurrentEventStep);

        exploration.ContinueEvent();

        Assert.DoesNotContain("road north", line.Speech.Lines[0], StringComparison.Ordinal);
        Assert.IsType<RestStep>(exploration.CurrentEventStep);
    }
}
