namespace Jrpg.Editor.Tests.Shell;

// MainViewModelTests is over 300 lines, so the map settings' track is tested here.
public sealed class MainViewModelMusicTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void StartEditingSettings_TestWorld_OffersTheMusicFolder()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.Contains("town/loop.wav", viewModel.StartEditingSettings()!.MusicChoices);
    }

    // The title's intro and outro are the player's to find beside its loop, never a map's track (decision 496).
    [Fact]
    public void StartEditingSettings_TestWorld_LeavesOutTheTitlesIntro()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.DoesNotContain("titlescreen/intro.wav", viewModel.StartEditingSettings()!.MusicChoices);
    }
}
