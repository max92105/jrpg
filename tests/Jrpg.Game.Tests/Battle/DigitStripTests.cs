using Jrpg.Game.Battle;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Battle;

// A digit strip's cells cut to their ink (decision 653), on 400 x 2 alpha images built here: ten 40 px cells, every
// column inked unless a test says otherwise.
public sealed class DigitStripTests
{
    private const int CellWidth = 40;
    private static readonly Point _size = new(10 * CellWidth, 2);

    [Fact]
    public void Measure_NarrowDigit_FindsItsColumns()
    {
        var alpha = Inked();
        ClearCell(alpha, 1);
        Ink(alpha, CellWidth + 15, CellWidth + 24);

        var source = DigitStrip.Measure(alpha, _size, "Digits.png").SourceOf(1);

        Assert.Equal(new Rectangle(CellWidth + 15, 0, 10, 2), source);
    }

    [Fact]
    public void Measure_DigitTouchingItsCellsEdges_KeepsTheWholeCell()
    {
        Assert.Equal(new Rectangle(3 * CellWidth, 0, CellWidth, 2), DigitStrip.Measure(Inked(), _size, "Digits.png").SourceOf(3));
    }

    [Fact]
    public void Measure_EmptyCell_ThrowsNamingTheFile()
    {
        var alpha = Inked();
        ClearCell(alpha, 7);

        var thrown = Assert.Throws<InvalidOperationException>(() => DigitStrip.Measure(alpha, _size, "Digits.png"));

        Assert.Equal("Digits.png: the cell of the digit 7 is empty.", thrown.Message);
    }

    [Fact]
    public void Measure_WidthNotTenCells_Throws()
    {
        var size = new Point(405, 2);

        var thrown = Assert.Throws<InvalidOperationException>(() => DigitStrip.Measure(new byte[405 * 2], size, "Digits.png"));

        Assert.Equal("Digits.png: a 405x2 strip isn't ten digit cells.", thrown.Message);
    }

    [Fact]
    public void Advances_AreTheMeasuredWidths()
    {
        var alpha = Inked();
        ClearCell(alpha, 1);
        Ink(alpha, CellWidth + 15, CellWidth + 24);

        var advances = DigitStrip.Measure(alpha, _size, "Digits.png").Advances;

        Assert.Equal((CellWidth, 10f), (advances[0], advances[1]));
    }

    private static byte[] Inked() => [.. Enumerable.Repeat((byte)255, _size.X * _size.Y)];

    private static void ClearCell(byte[] alpha, int digit)
    {
        for (var row = 0; row < _size.Y; row++)
        {
            Array.Clear(alpha, (row * _size.X) + (digit * CellWidth), CellWidth);
        }
    }

    // The bottom row only, so a column inked on one row of the two counts.
    private static void Ink(byte[] alpha, int firstColumn, int lastColumn) =>
        Array.Fill(alpha, (byte)255, ((_size.Y - 1) * _size.X) + firstColumn, lastColumn - firstColumn + 1);
}
