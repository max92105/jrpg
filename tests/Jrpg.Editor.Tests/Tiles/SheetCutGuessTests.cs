using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class SheetCutGuessTests
{
    [Fact]
    public void TileSizeFor_DivisibleBy48_Is48()
    {
        Assert.Equal(48, SheetCutGuess.TileSizeFor(384, 96));
    }

    [Fact]
    public void TileSizeFor_DivisibleBy32Only_Is32()
    {
        Assert.Equal(32, SheetCutGuess.TileSizeFor(160, 64));
    }

    [Fact]
    public void TileSizeFor_NoCommonSizeDivides_Is48()
    {
        Assert.Equal(48, SheetCutGuess.TileSizeFor(1000, 7));
    }
}
