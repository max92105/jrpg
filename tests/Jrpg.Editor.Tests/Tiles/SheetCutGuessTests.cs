using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class SheetCutGuessTests
{
    // A Mana Seed sheet: both sides multiples of 16, neither of 48.
    [Fact]
    public void TileSizeFor_ManaSeedSheet_Is16()
    {
        Assert.Equal(16, SheetCutGuess.TileSizeFor(256, 400));
    }

    [Fact]
    public void TileSizeFor_DivisibleBy48And16_Is16()
    {
        Assert.Equal(16, SheetCutGuess.TileSizeFor(96, 48));
    }

    [Fact]
    public void TileSizeFor_DivisibleBy24Only_Is24()
    {
        Assert.Equal(24, SheetCutGuess.TileSizeFor(72, 24));
    }

    [Fact]
    public void TileSizeFor_NoCommonSizeDivides_Is48()
    {
        Assert.Equal(48, SheetCutGuess.TileSizeFor(1000, 7));
    }
}
