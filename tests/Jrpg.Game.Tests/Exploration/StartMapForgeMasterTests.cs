using Jrpg.Core.Events;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>The start map's forge master (decision 372): a line, then the Forge.</summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class StartMapForgeMasterTests
{
    [Fact]
    public void Interact_FacingForgeMaster_StartsWithTheirLine()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowForgeMaster);

        Assert.True(exploration.TryInteract());

        Assert.Equal("Forge master", Assert.IsType<DialogueStep>(exploration.CurrentEventStep).Speech.Speaker);
    }

    // Confirm on every other update reads the line through, whatever its typing takes; then the Forge is asked for.
    [Fact]
    public void Update_TalkThroughTheLine_RequestsTheForge()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowForgeMaster);
        var scene = StartMapScenarios.SceneOf(exploration, StartMapScenarios.BelowForgeMaster);

        var requested = false;
        for (var update = 0; update < 300 && !requested; update++)
        {
            requested = scene.Update(StartMapScenarios.OneUpdate, update % 2 == 0 ? Pressing(GameButton.Confirm) : default) is ForgeRequested;
        }

        Assert.True(requested);
        Assert.IsType<OpenForgeStep>(exploration.CurrentEventStep);
    }
}
