using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Game.Exploration;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Exploration;

public sealed class MapCameraTests
{
    private readonly MapCamera _camera = new();

    [Fact]
    public void SnapTo_MapSmallerThanView_CentersMap()
    {
        // The forest is 30x17 cells, 1440x816 pixels: 480 and 264 pixels narrower and shorter than the screen.
        _camera.SnapTo(new Point2D(3, 3), Layout(30, 17));

        Assert.Equal(new Vector2(-240, -132), _camera.Origin);
    }

    [Fact]
    public void SnapTo_NearMapEdge_ClampsToEdge()
    {
        _camera.SnapTo(new Point2D(10, 3), Layout(45, 45));

        Assert.Equal(Vector2.Zero, _camera.Origin);
    }

    [Fact]
    public void SnapTo_NearFarEdge_ClampsToFarEdge()
    {
        // 45 cells are 2160 pixels; the screen's 1920x1080 then ends exactly at the map's right and bottom edges.
        _camera.SnapTo(new Point2D(44, 44), Layout(45, 45));

        Assert.Equal(new Vector2(2160 - 1920, 2160 - 1080), _camera.Origin);
    }

    [Fact]
    public void SnapTo_MiddleOfLargeMap_CentersOnTheTarget()
    {
        _camera.SnapTo(new Point2D(22.5, 22.5), Layout(45, 45));

        Assert.Equal(new Vector2(1080 - 960, 1080 - 540), _camera.Origin);
    }

    private static MapLayout Layout(int width, int height) => new()
    {
        Id = "test_map",
        Width = width,
        Height = height,
        Layers = [],
        Objects = [],
    };
}
