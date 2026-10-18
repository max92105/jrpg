using Jrpg.Core.Maps;
using Jrpg.Game.Exploration;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Exploration;

public sealed class MapViewTests
{
    private static readonly MapLayout _largeMap = new()
    {
        Id = "test_map",
        Width = 100,
        Height = 100,
        Layers = [],
        Objects = [],
    };

    [Fact]
    public void VisibleCells_CameraAtOrigin_CoversScreenPlusOneCell()
    {
        // 1920x1080 is 40 by 22.5 cells of 48 pixels.
        var cells = MapView.VisibleCells(Vector2.Zero, _largeMap);

        Assert.Equal(new CellRange(0, 0, 41, 23), cells);
    }

    [Fact]
    public void VisibleCells_CameraBetweenCells_StartsAtThePartlyShownCell()
    {
        var cells = MapView.VisibleCells(new Vector2(100, 50), _largeMap);

        Assert.Equal(new CellRange(2, 1, 43, 24), cells);
    }

    [Fact]
    public void VisibleCells_NearMapEdge_ClampsToMap()
    {
        var smallMap = _largeMap with { Width = 30, Height = 17 };

        var cells = MapView.VisibleCells(new Vector2(-240, -132), smallMap);

        Assert.Equal(new CellRange(0, 0, 30, 17), cells);
    }
}
