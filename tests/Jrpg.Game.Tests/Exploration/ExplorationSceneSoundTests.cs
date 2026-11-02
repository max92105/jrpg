using Jrpg.Game.Input;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Exploration;

// What the map scene passes up to the flow: its own menu sound and the event presenter's ticks (ADR 0035).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ExplorationSceneSoundTests
{
    [Fact]
    public void Update_DialogueTyping_AsksTheTypingSound()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowTalkingChest);
        var scene = StartMapScenarios.SceneOf(exploration, StartMapScenarios.BelowTalkingChest);
        Assert.True(exploration.TryInteract());
        var asked = new List<string>();

        for (var update = 0; update < 15; update++)
        {
            scene.Update(StartMapScenarios.OneUpdate, default);
            asked.AddRange(scene.Sounds);
        }

        Assert.Contains(UiSoundFiles.Effects.TextTyping, asked);
    }

    // The chest's found item: the presenter's step sound reaches the flow through the scene, as its ticks do.
    [Fact]
    public void Update_ChestEventShows_SceneAsksTheItemSound()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowChest);
        var scene = StartMapScenarios.SceneOf(exploration, StartMapScenarios.BelowChest);
        Assert.True(exploration.TryInteract());

        scene.Update(StartMapScenarios.OneUpdate, default);

        Assert.Contains(UiSoundFiles.Field.ItemGot, scene.Sounds);
    }

    [Fact]
    public void Update_MenuPressed_AsksTheConfirmSound()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.AfterTheIntro);
        var scene = StartMapScenarios.SceneOf(exploration, StartMapScenarios.AfterTheIntro);

        scene.Update(StartMapScenarios.OneUpdate, Pressing(GameButton.Menu));

        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], scene.Sounds);
    }
}
