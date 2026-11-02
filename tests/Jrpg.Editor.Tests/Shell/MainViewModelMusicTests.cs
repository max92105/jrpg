namespace Jrpg.Editor.Tests.Shell;

// MainViewModelTests is over 300 lines, so the map settings' track is tested here.
public sealed class MainViewModelMusicTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void StartEditingSettings_RepositoryContent_OffersTheMusicFolder()
    {
        var viewModel = _content.Open(_content.RepositoryContent);

        Assert.Contains("PlaceholderTown.wav", viewModel.StartEditingSettings()!.MusicChoices);
    }
}
