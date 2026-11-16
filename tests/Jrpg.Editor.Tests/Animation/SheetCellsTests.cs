using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Core.Sprites;
using Jrpg.Editor.Animation;

namespace Jrpg.Editor.Tests.Animation;

// A sheet built in memory, each cell filled with a color only it has, so a frame tells which cell it was cut from.
public sealed class SheetCellsTests
{
    private const int Columns = 4;
    private const int Rows = 2;
    private static readonly (int Width, int Height) _cell = (3, 2);

    [Fact]
    public void Cut_TwoPicks_FramesInPickOrder()
    {
        StaThread.Run(() =>
        {
            var frames = SheetCells.Cut(SheetOf(Columns, Rows), _cell, [new(3, 1), new(0, 0)]);

            Assert.Equal([Filled(ColorOf(3, 1)), Filled(ColorOf(0, 0))], frames);
        });
    }

    [Fact]
    public void Cut_RepeatedCell_CopiedTwice()
    {
        StaThread.Run(() =>
        {
            var frames = SheetCells.Cut(SheetOf(Columns, Rows), _cell, [new(1, 0), new(2, 1), new(1, 0)]);

            Assert.Equal([Filled(ColorOf(1, 0)), Filled(ColorOf(2, 1)), Filled(ColorOf(1, 0))], frames);
        });
    }

    // A column and a row just past the sheet, and a column whose left edge would overflow an int.
    [Theory]
    [InlineData(Columns, 0)]
    [InlineData(0, Rows)]
    [InlineData(int.MaxValue, 0)]
    public void Cut_CellOutsideTheSheet_Throws(int column, int row)
    {
        StaThread.Run(() => Assert.Throws<ArgumentOutOfRangeException>(
            () => SheetCells.Cut(SheetOf(Columns, Rows), _cell, [new(0, 0), new SpriteCell(column, row)])));
    }

    // Three cells of 3 px make a 9 px sheet: the last column ends on its odd right edge, and every pixel of the frame
    // is that column's, none of its neighbor's.
    [Fact]
    public void Cut_LastColumnOfAnOddWidth_ReadsItsPixels()
    {
        StaThread.Run(() =>
        {
            var frames = SheetCells.Cut(SheetOf(3, 1), _cell, [new(2, 0)]);

            Assert.Equal([Filled(ColorOf(2, 0))], frames);
        });
    }

    // A sheet of three bytes a pixel, as a PNG without alpha decodes: the frames come out as BGRA, alpha opaque.
    [Fact]
    public void Cut_Bgr24Sheet_FramesInBgra()
    {
        StaThread.Run(() =>
        {
            var sheet = SheetOf(Columns, Rows);
            var bgr = new FormatConvertedBitmap(sheet, PixelFormats.Bgr24, null, 0);

            var frames = SheetCells.Cut(bgr, _cell, [new(3, 1), new(0, 0)]);

            Assert.Equal([Filled(ColorOf(3, 1)), Filled(ColorOf(0, 0))], frames);
        });
    }

    // Opaque, so converting to BGRA, or from BGR, changes no pixel.
    private static uint ColorOf(int column, int row) => 0xFF000000u | (uint)((column * 16) + row + 1);

    private static uint[] Filled(uint color) => [.. Enumerable.Repeat(color, _cell.Width * _cell.Height)];

    private static BitmapSource SheetOf(int columns, int rows)
    {
        var (width, height) = (columns * _cell.Width, rows * _cell.Height);
        var pixels = Enumerable.Range(0, width * height)
            .Select(index => ColorOf(index % width / _cell.Width, index / width / _cell.Height))
            .ToArray();
        return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * sizeof(uint));
    }
}
