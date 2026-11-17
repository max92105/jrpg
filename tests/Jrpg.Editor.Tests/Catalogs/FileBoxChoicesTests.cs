using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// What a box choosing one file of a folder offers (decisions 419, 906, 909): the entry for none, then the folder's
// files, a named file the folder lacks kept after the first entry.
public sealed class FileBoxChoicesTests
{
    private static readonly string[] _inFolder = ["Cave.png", "Forest.png"];

    [Fact]
    public void Of_NoFileNamed_NoneThenTheFolders()
    {
        Assert.Equal(["(none)", "Cave.png", "Forest.png"], FileBoxChoices.Of(null, "(none)", _inFolder));
    }

    [Fact]
    public void Of_FileInTheFolder_ListedOnce()
    {
        Assert.Equal(["(none)", "Cave.png", "Forest.png"], FileBoxChoices.Of("Forest.png", "(none)", _inFolder));
    }

    [Fact]
    public void Of_FileNoLongerInTheFolder_KeptAfterNone()
    {
        Assert.Equal(["(none)", "Gone.png", "Cave.png", "Forest.png"], FileBoxChoices.Of("Gone.png", "(none)", _inFolder));
    }
}
