using System.Windows;
using System.Windows.Media;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Objects.Composing;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Objects.Composing;

public sealed class ObjectCanvasDrawingTests
{
    [Fact]
    public void Draw_TwoByThreeCanvas_IsTheCanvasSizeInMapPixels()
    {
        var image = ObjectCanvasDrawing.Draw(ObjectCanvas.Empty(2, 3), _ => MissingArtMark.Picture);

        Assert.Equal((2 * MapDrawing.CellPixels, 3 * MapDrawing.CellPixels), (image.Width, image.Height));
    }

    [Fact]
    public void Draw_PaintedCell_AsksTheImageOfItsSheetCell()
    {
        var canvas = ObjectCanvas.Empty(3, 3)
            .Stamp(new GridCell(0, 0), new SheetCellRectangle("forest", 4, 1, 1, 1))
            .Stamp(new GridCell(2, 1), new SheetCellRectangle("house", 0, 2, 1, 1));
        var asked = new List<SheetCell>();

        ObjectCanvasDrawing.Draw(canvas, cell =>
        {
            asked.Add(cell);
            return MissingArtMark.Picture;
        });

        Assert.Equal(new HashSet<SheetCell> { new("forest", 4, 1), new("house", 0, 2) }, asked.ToHashSet());
    }

    [Fact]
    public void Draw_CellAtTwoOne_IsDrawnTwoCellsRightAndOneDown()
    {
        var canvas = ObjectCanvas.Empty(3, 3).Stamp(new GridCell(2, 1), new SheetCellRectangle("forest", 4, 1, 1, 1));

        var drawing = (DrawingGroup)ObjectCanvasDrawing.Draw(canvas, _ => MissingArtMark.Picture).Drawing;

        var cell = Assert.Single(drawing.Children.OfType<ImageDrawing>());
        Assert.Equal(new Rect(2 * MapDrawing.CellPixels, MapDrawing.CellPixels, MapDrawing.CellPixels, MapDrawing.CellPixels), cell.Rect);
    }
}
