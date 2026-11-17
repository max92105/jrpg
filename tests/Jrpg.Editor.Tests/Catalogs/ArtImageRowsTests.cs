using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// An art page's list (decision 442): the folder's images in the given order, then the records whose image is missing.
public sealed class ArtImageRowsTests
{
    // The images given out of name order, so a sort of the folder's part would fail.
    [Fact]
    public void Of_ImagesAndRecords_ImagesFirstInTheGivenOrder()
    {
        var rows = ArtImageRows.Of(["Pulse.png", "Forest.png"], ["Gone.png"]);

        Assert.Equal(
            [new ArtImageRow("Pulse.png", false), new ArtImageRow("Forest.png", false), new ArtImageRow("Gone.png", true)],
            rows);
    }

    [Fact]
    public void Of_RecordWithoutImage_ComesAfterMarkedMissing()
    {
        var rows = ArtImageRows.Of(["Forest.png"], ["Zed.png", "Gone.png"]);

        Assert.Equal(
            [new ArtImageRow("Forest.png", false), new ArtImageRow("Gone.png", true), new ArtImageRow("Zed.png", true)],
            rows);
    }

    [Fact]
    public void Of_RecordWithItsImage_ListedOnce()
    {
        var rows = ArtImageRows.Of(["Forest.png", "Pulse.png"], ["Pulse.png"]);

        Assert.Equal([new ArtImageRow("Forest.png", false), new ArtImageRow("Pulse.png", false)], rows);
    }
}
