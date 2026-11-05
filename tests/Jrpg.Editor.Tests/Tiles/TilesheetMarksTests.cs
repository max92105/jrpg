using System.Windows;
using System.Windows.Media;
using Jrpg.Core.Maps;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Viewport;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class TilesheetMarksTests
{
    // A 16 px cut over a 64 x 32 image: four columns, two rows.
    private static readonly TilesheetDefinition _sheet = new() { Id = "autumn", File = "Autumn.png", TileWidth = 16, TileHeight = 16, Margin = 0, Spacing = 0 };
    private static readonly Size _imageSize = new(64, 32);

    [Fact]
    public void ShownScaleOf_SixteenPixelSheetAtSixtyFour_IsFour() => Assert.Equal(4, TilesheetMarks.ShownScaleOf(_sheet, new CellZoom(64)));

    [Fact]
    public void ShownScaleOf_ImageNotSetUpAtTheMapSize_IsOne() => Assert.Equal(1, TilesheetMarks.ShownScaleOf(null, new CellZoom(48)));

    [Fact]
    public void ShownScaleOf_ImageNotSetUpAtNinetySix_IsTwo() => Assert.Equal(2, TilesheetMarks.ShownScaleOf(null, new CellZoom(96)));

    [Fact]
    public void Draw_AtScaleSix_DrawsTheGridOneSixthOfASheetPixelWide() =>
        Assert.Equal(1.0 / 6, GridPenOf(TilesheetMarks.Draw(new ShownSheet(_sheet, _imageSize, 6), [], null)).Thickness, 9);

    // The Cut panel previewing a 16 px cut on an image not set up, shown at 1: the lines are one pixel, not a third.
    // The fix itself is the view taking its scale from the chosen sheet, not the preview, which only the hand check
    // sees.
    [Fact]
    public void Draw_PreviewCutOnAnImageShownAtOne_DrawsTheGridOnePixelWide() =>
        Assert.Equal(1, GridPenOf(TilesheetMarks.Draw(new ShownSheet(_sheet, _imageSize, 1), [], null)).Thickness);

    // The first outlined rectangle is a grid cell: the drawing starts with the unoutlined rectangle pinning the origin.
    private static Pen GridPenOf(DrawingImage image) =>
        ((DrawingGroup)image.Drawing).Children.OfType<GeometryDrawing>().First(drawing => drawing.Pen is not null).Pen;
}
