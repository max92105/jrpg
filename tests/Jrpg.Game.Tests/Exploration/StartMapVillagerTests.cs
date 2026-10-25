using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Maps;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>The start map's villager (ADR 0015), an NPC placed in the repository's content.</summary>
public sealed class StartMapVillagerTests
{
    [Fact]
    public void Interact_FacingVillagerOnStartMap_StartsItsTalkEvent()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowVillager);

        Assert.True(exploration.TryInteract());

        var dialogue = Assert.IsType<DialogueStep>(exploration.CurrentEventStep);
        Assert.Equal("Villager", dialogue.Speech.Speaker);
    }

    // The placeholder Innkeeper is a villager too: a line, then the rest.
    [Fact]
    public void Interact_FacingInnkeeper_ReadsALineThenRests()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowInnkeeper);
        exploration.TryInteract();
        var first = exploration.CurrentEventStep;

        exploration.ContinueEvent();

        Assert.Equal("Innkeeper", Assert.IsType<DialogueStep>(first).Speech.Speaker);
        Assert.IsType<RestStep>(exploration.CurrentEventStep);
    }

    [Fact]
    public void Move_IntoVillagersFeet_IsBlocked()
    {
        var exploration = WalkUpForOneSecond(StartMapScenarios.BelowVillager);

        // The villager's outline covers the lower half of its cell (4, 5): the player stops below it.
        Assert.True(exploration.PlayerPosition.Y > 5, $"The player walked to y {exploration.PlayerPosition.Y}.");
    }

    // The control for the test above: nothing else blocks that column.
    [Fact]
    public void Move_VillagerRemoved_PassesItsCell()
    {
        var catalog = StartMapScenarios.BelowVillager;
        var start = catalog.Maps["start"];
        var withoutVillager = start.Layout with { Objects = [.. start.Layout.Objects.Where(placed => placed.ObjectId != "villager")] };
        var maps = new Dictionary<string, MapDefinition>(catalog.Maps) { ["start"] = new MapDefinition(withoutVillager, start.Settings) };

        var exploration = WalkUpForOneSecond(catalog with { Maps = maps });

        Assert.True(exploration.PlayerPosition.Y < 5, $"The player stopped at y {exploration.PlayerPosition.Y}.");
    }

    private static ExplorationState WalkUpForOneSecond(ContentCatalog catalog)
    {
        var exploration = StartMapScenarios.StartExploring(catalog);
        for (var update = 0; update < 60; update++)
        {
            exploration.Walk(MoveIntent.FromAxes(0, -1), StartMapScenarios.OneUpdate);
        }

        return exploration;
    }
}
