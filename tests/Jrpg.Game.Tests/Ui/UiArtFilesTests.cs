using System.Reflection;
using Jrpg.Data.Art;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class UiArtFilesTests
{
    private static readonly string _artRoot = ArtFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content"));

    // Compared with the folder's list, since File.Exists ignores case on Windows and the release may not.
    [Theory]
    [InlineData(typeof(UiArtFiles.Backgrounds), ArtFolders.Backgrounds)]
    [InlineData(typeof(UiArtFiles.Icons), ArtFolders.Icons)]
    public void EveryConstant_ExistsInItsFolder(Type group, string folder)
    {
        var files = group.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral)
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToList();
        var present = Directory.GetFiles(Path.Combine(_artRoot, folder)).Select(Path.GetFileName).ToHashSet(StringComparer.Ordinal);

        Assert.NotEmpty(files);
        Assert.All(files, file => Assert.True(present.Contains(file), $"content/art/{folder} has no '{file}'."));
    }

    [Fact]
    public void EveryGroup_IsCheckedAbove()
    {
        Assert.Equal(2, typeof(UiArtFiles).GetNestedTypes().Length);
    }
}
