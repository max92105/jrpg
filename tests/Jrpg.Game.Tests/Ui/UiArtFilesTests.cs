using System.Reflection;
using Jrpg.Data.Art;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class UiArtFilesTests
{
    [Fact]
    public void EveryConstant_ExistsInRepositoryContent()
    {
        var folder = Path.Combine(ArtFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content")), ArtFolders.Backgrounds);
        var files = typeof(UiArtFiles).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral)
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToList();

        Assert.NotEmpty(files);
        Assert.All(files, file => Assert.True(File.Exists(Path.Combine(folder, file)), $"content/art/{ArtFolders.Backgrounds} has no '{file}'."));
    }
}
