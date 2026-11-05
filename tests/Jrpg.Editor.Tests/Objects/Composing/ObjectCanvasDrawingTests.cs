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
    private static readonly SheetCell _wall = new("house", 0, 0);
    private static readonly SheetCell _window = new("house", 3, 2);

    [Fact]
    public void Draw_TwoByThreeCanvas_IsTheCanvasSizeInMapPixels()
    {
        var image = ObjectCanvasDrawing.Draw(ObjectCanvas.Empty(2, 3), 0, _ => MissingArtMark.Picture);

        Assert.Equal((2 * MapDrawing.CellPixels, 3 * MapDrawing.CellPixels), (image.Width, image.Height));
    }

    [Fact]
    public void Draw_PaintedCell_AsksTheImageOfItsSheetCell()
    {
        var canvas = ObjectCanvas.Empty(3, 3)
            .Stamp(0, new GridCell(0, 0), new SheetCellRectangle("forest", 4, 1, 1, 1))
            .Stamp(0, new GridCell(2, 1), new SheetCellRectangle("house", 0, 2, 1, 1));
        var asked = new List<SheetCell>();

        ObjectCanvasDrawing.Draw(canvas, 0, cell =>
        {
            asked.Add(cell);
            return MissingArtMark.Picture;
        });

        Assert.Equal(new HashSet<SheetCell> { new("forest", 4, 1), new("house", 0, 2) }, asked.ToHashSet());
    }

    // The dictionary keeps the order cells were painted in: the overlay first here.
    [Fact]
    public void Draw_LevelOnePaintedBeforeTheBase_DrawsTheBaseFirst()
    {
        var canvas = ObjectCanvas.Empty(3, 3)
            .Stamp(1, new GridCell(1, 1), new SheetCellRectangle("house", 3, 2, 1, 1))
            .Stamp(0, new GridCell(1, 1), new SheetCellRectangle("house", 0, 0, 1, 1));
        var asked = new List<SheetCell>();

        ObjectCanvasDrawing.Draw(canvas, 0, cell =>
        {
            asked.Add(cell);
            return MissingArtMark.Picture;
        });

        Assert.Equal([new SheetCell("house", 0, 0), new SheetCell("house", 3, 2)], asked);
    }

    [Fact]
    public void Draw_CellAtTwoOne_IsDrawnTwoCellsRightAndOneDown()
    {
        var canvas = ObjectCanvas.Empty(3, 3).Stamp(0, new GridCell(2, 1), new SheetCellRectangle("forest", 4, 1, 1, 1));

        var drawing = (DrawingGroup)ObjectCanvasDrawing.Draw(canvas, 0, _ => MissingArtMark.Picture).Drawing;

        var cell = Assert.Single(drawing.Children.OfType<ImageDrawing>());
        Assert.Equal(new Rect(2 * MapDrawing.CellPixels, MapDrawing.CellPixels, MapDrawing.CellPixels, MapDrawing.CellPixels), cell.Rect);
    }

    [Fact]
    public void Draw_LevelAboveTheChosenOne_IsFaded()
    {
        var images = new Dictionary<SheetCell, ImageSource>();

        var drawing = (DrawingGroup)ObjectCanvasDrawing.Draw(WallWithWindow(), 0, cell => ImageOf(images, cell)).Drawing;

        var faded = Assert.Single(drawing.Children.OfType<DrawingGroup>());
        Assert.Equal(0.4, faded.Opacity);
        Assert.Same(images[_window], Assert.Single(faded.Children.OfType<ImageDrawing>()).ImageSource);
    }

    [Fact]
    public void Draw_ChosenAndLowerLevels_AreDrawnWhole()
    {
        var images = new Dictionary<SheetCell, ImageSource>();

        var drawing = (DrawingGroup)ObjectCanvasDrawing.Draw(WallWithWindow(), 1, cell => ImageOf(images, cell)).Drawing;

        Assert.Empty(drawing.Children.OfType<DrawingGroup>());
        Assert.Equal([images[_wall], images[_window]], drawing.Children.OfType<ImageDrawing>().Select(drawn => drawn.ImageSource));
    }

    // A wall on the base and a window over it on level 1, on one cell.
    private static ObjectCanvas WallWithWindow() =>
        ObjectCanvas.Empty(3, 3)
            .Stamp(0, new GridCell(1, 1), SheetCellRectangle.Between(_wall, _wall))
            .Stamp(1, new GridCell(1, 1), SheetCellRectangle.Between(_window, _window));

    // Each sheet cell gets its own image, so a test can tell which cell was drawn where.
    private static ImageSource ImageOf(Dictionary<SheetCell, ImageSource> images, SheetCell cell)
    {
        if (!images.TryGetValue(cell, out var image))
        {
            image = new DrawingImage(new GeometryDrawing(Brushes.Red, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            images.Add(cell, image);
        }

        return image;
    }
}
