using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Exploration;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class ExplorationSceneTests
{
    private static readonly InputFrame _confirmPressed =
        new(MoveIntent.None, ButtonSet.Empty.With(GameButton.Confirm), ButtonSet.Empty.With(GameButton.Confirm));

    [Fact]
    public void Update_DuringWarpFadeIn_DoesNotWalk()
    {
        var (exploration, scene) = Start(StartMapScenarios.NextToForestWarp);
        for (var update = 0; update < 120 && exploration.CurrentMap.Id != "forest"; update++)
        {
            scene.Update(StartMapScenarios.OneUpdate, StartMapScenarios.HoldingLeft);
        }

        Assert.Equal("forest", exploration.CurrentMap.Id);
        var arrival = exploration.PlayerPosition;

        // The rest of the hold and most of the 0.25 s fade-in.
        scene.Update(TimeSpan.FromSeconds(0.1), StartMapScenarios.HoldingLeft);
        scene.Update(TimeSpan.FromSeconds(0.1), StartMapScenarios.HoldingLeft);

        Assert.Equal(arrival, exploration.PlayerPosition);
    }

    [Fact]
    public void Update_ConfirmFacingDoor_OpensIt()
    {
        var (exploration, scene) = Start(StartMapScenarios.BelowClosedDoor);
        var door = exploration.CurrentMap.Layout.Objects.Single(placed => placed.ObjectId == "door_close");

        scene.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.Equal("door_open", exploration.CurrentDefinitionOf(door).Id);
        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Update_ConfirmClosingFoundItem_DoesNotInteractAgain()
    {
        var (exploration, scene) = Start(StartMapScenarios.BelowRepeatableChest);
        scene.Update(StartMapScenarios.OneUpdate, _confirmPressed);
        scene.Update(StartMapScenarios.OneUpdate, default);
        Assert.True(exploration.IsEventRunning);

        scene.Update(StartMapScenarios.OneUpdate, _confirmPressed);

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Update_MenuPressedWhileIdle_RequestsMenu()
    {
        var (_, scene) = Start(StartMapScenarios.BelowClosedDoor);

        var request = scene.Update(StartMapScenarios.OneUpdate, InputFrames.PressingEscape);

        Assert.IsType<MenuRequested>(request);
    }

    [Fact]
    public void Update_MenuPressedDuringWarpFade_RequestsNothing()
    {
        var (exploration, scene) = Start(StartMapScenarios.NextToForestWarp);
        for (var update = 0; update < 120 && !exploration.IsEventRunning; update++)
        {
            scene.Update(StartMapScenarios.OneUpdate, StartMapScenarios.HoldingLeft);
        }

        Assert.True(exploration.IsEventRunning);

        var request = scene.Update(StartMapScenarios.OneUpdate, InputFrames.PressingEscape);

        Assert.Null(request);
    }

    private static (ExplorationState Exploration, ExplorationScene Scene) Start(ContentCatalog catalog)
    {
        var exploration = StartMapScenarios.StartExploring(catalog);
        return (exploration, new ExplorationScene(exploration, catalog, new MapArt(catalog, MapView.CellPixels), new EventPresenter(exploration, RepositoryScreenText.Text, new PlayerSettings())));
    }
}
