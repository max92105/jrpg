using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

public sealed class MainViewModelTests : IDisposable
{
    private static readonly string _repositoryContent = Path.Combine(AppContext.BaseDirectory, "content");

    private readonly string _brokenContent = Path.Combine(Path.GetTempPath(), $"jrpg-editor-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_brokenContent))
        {
            Directory.Delete(_brokenContent, recursive: true);
        }
    }

    [Fact]
    public void Constructor_RepositoryContent_ListsMapsAndOpensTheFirst()
    {
        var viewModel = new MainViewModel(_repositoryContent);

        Assert.Null(viewModel.LoadError);
        Assert.Equal(["forest", "interior_1", "start"], viewModel.MapIds);
        Assert.Equal("forest", viewModel.CurrentMap?.Map.Id);
    }

    [Fact]
    public void SelectedMapId_AnotherMap_OpensIt()
    {
        var viewModel = new MainViewModel(_repositoryContent);

        viewModel.SelectedMapId = "start";

        Assert.Equal("start", viewModel.CurrentMap?.Map.Id);
    }

    [Fact]
    public void Constructor_BrokenContentFile_ShowsTheLoaderMessageAndNoMaps()
    {
        Directory.CreateDirectory(_brokenContent);
        File.WriteAllText(Path.Combine(_brokenContent, "classes.json"), "[ {");

        var viewModel = new MainViewModel(_brokenContent);

        Assert.StartsWith("classes.json", viewModel.LoadError, StringComparison.Ordinal);
        Assert.Empty(viewModel.MapIds);
        Assert.Null(viewModel.CurrentMap);
    }
}
