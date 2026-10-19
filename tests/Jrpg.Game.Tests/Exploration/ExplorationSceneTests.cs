using Jrpg.Game.Exploration;
using Jrpg.Game.Rendering;

namespace Jrpg.Game.Tests.Exploration;

public sealed class ExplorationSceneTests
{
    [Fact]
    public void Update_DuringWarpFadeIn_DoesNotWalk()
    {
        var exploration = ForestWarpStart.StartExploring();
        var scene = new ExplorationScene(exploration, ForestWarpStart.Catalog, new MapArt(ForestWarpStart.Catalog, MapView.CellPixels));
        for (var update = 0; update < 120 && exploration.CurrentMap.Id != "forest"; update++)
        {
            scene.Update(ForestWarpStart.OneUpdate, ForestWarpStart.HoldingLeft);
        }

        Assert.Equal("forest", exploration.CurrentMap.Id);
        var arrival = exploration.PlayerPosition;

        // The rest of the hold and most of the 0.25 s fade-in.
        scene.Update(TimeSpan.FromSeconds(0.1), ForestWarpStart.HoldingLeft);
        scene.Update(TimeSpan.FromSeconds(0.1), ForestWarpStart.HoldingLeft);

        Assert.Equal(arrival, exploration.PlayerPosition);
    }
}
