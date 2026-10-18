using System.Windows;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Viewport;

namespace Jrpg.Editor.Tests.Viewport;

public sealed class ViewportTransformTests
{
    private readonly ViewportTransform _viewport = new();

    [Fact]
    public void ZoomAt_KeepsPointUnderCursorFixed()
    {
        _viewport.PanBy(new Vector(30, -20));
        var cursor = new Point(400, 300);
        var mapPointBefore = _viewport.ScreenToMap(cursor);

        _viewport.ZoomAt(cursor, 2.5);

        Assert.Equal(2.5, _viewport.Zoom);
        Assert.Equal(mapPointBefore, _viewport.ScreenToMap(cursor));
    }

    [Theory]
    [InlineData(20, ViewportTransform.MaxZoom)]
    [InlineData(0.01, ViewportTransform.MinZoom)]
    public void ZoomAt_BeyondLimit_ClampsToTheLimit(double zoom, double expected)
    {
        _viewport.ZoomAt(new Point(0, 0), zoom);

        Assert.Equal(expected, _viewport.Zoom);
    }

    [Fact]
    public void Fit_WideMap_FitsWidthAndCenters()
    {
        // 2000x500 map pixels in a 1000x800 view: width decides, 0.5 less the 5 % margin.
        _viewport.Fit(new Size(2000, 500), new Size(1000, 800));

        Assert.Equal(0.475, _viewport.Zoom, 9);
        Assert.Equal(new Point(25, 281.25), _viewport.Origin);
    }

    [Fact]
    public void Fit_SmallMap_NeverZoomsPastActualSize()
    {
        _viewport.Fit(new Size(100, 100), new Size(1000, 800));

        Assert.Equal(1, _viewport.Zoom);
        Assert.Equal(new Point(450, 350), _viewport.Origin);
    }

    [Fact]
    public void ScreenToCell_AfterPan_AccountsForOffset()
    {
        _viewport.PanBy(new Vector(100, 50));

        // Map pixel (110, 50) at 48 pixels per cell.
        var cell = _viewport.ScreenToCell(new Point(210, 100), cellPixels: 48);

        Assert.Equal(new GridCell(2, 1), cell);
    }

    [Fact]
    public void ScreenToCell_LeftOfTheMap_IsANegativeCell()
    {
        var cell = _viewport.ScreenToCell(new Point(-1, 5), cellPixels: 48);

        Assert.Equal(new GridCell(-1, 0), cell);
    }
}
