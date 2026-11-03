using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class SheetImportResultTests
{
    [Fact]
    public void Summary_ImportedAndRefused_ListsBoth()
    {
        var result = new SheetImportResult(["A.png", "B.png"], ["C.png: not a PNG image."]);

        Assert.Equal("Imported A.png, B.png. Refused: C.png: not a PNG image.", result.Summary);
    }

    [Fact]
    public void Summary_NothingImported_ListsTheRefusalsOnly()
    {
        var result = new SheetImportResult([], ["C.png: not a PNG image."]);

        Assert.Equal("Refused: C.png: not a PNG image.", result.Summary);
    }
}
