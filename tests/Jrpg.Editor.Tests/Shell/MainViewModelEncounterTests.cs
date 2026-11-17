using Jrpg.Data.Art;
using Jrpg.Editor.Tests.Tiles;

namespace Jrpg.Editor.Tests.Shell;

// The Encounters page's boxes as the shell reads them when the page starts (decision 906): the draft's enemies, the
// backgrounds folder's images and the music folder's tracks.
public sealed class MainViewModelEncounterTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    private string BackgroundsFolder => Path.Combine(ArtFolders.RootIn(_content.ContentCopy), ArtFolders.Backgrounds);

    public void Dispose() => _content.Dispose();

    [Fact]
    public void ReadEncounterChoices_Enemies_OfferedByNameThenId()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var goblin = viewModel.Catalog!.Enemies["goblin"];
        viewModel.Content!.SetEnemies(
            [.. viewModel.Catalog.Enemies.Values, goblin with { Id = "a_goblin" }, goblin with { Id = "z_ant", Name = "Ant" }]);

        var offered = viewModel.EncounterEditing!.Start().OfferedEnemies;

        Assert.Equal(
            ["z_ant", "a_goblin", "goblin"],
            offered.Select(enemy => enemy.Id).Where(id => id is "z_ant" or "a_goblin" or "goblin"));
    }

    [Fact]
    public void ReadEncounterChoices_ImageAddedAfterTheEditorOpened_IsOffered()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        PngHeaderFile.Write(Path.Combine(BackgroundsFolder, "Cave.png"), 96, 54);

        var encounters = viewModel.EncounterEditing!.Start();

        Assert.Contains("Cave.png", encounters.SelectedEncounter!.BackgroundChoices);
    }

    // Its size can't be read, so the game couldn't fit it to the screen either.
    [Fact]
    public void ReadEncounterChoices_ImageThatCantBeRead_IsLeftOut()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        File.WriteAllBytes(Path.Combine(BackgroundsFolder, "Broken.png"), [1, 2, 3]);

        var encounters = viewModel.EncounterEditing!.Start();

        Assert.DoesNotContain("Broken.png", encounters.SelectedEncounter!.BackgroundChoices);
    }

    [Fact]
    public void ReadEncounterChoices_TestWorld_OffersTheMusicFoldersTracks()
    {
        var viewModel = _content.Open(_content.TestWorld);

        var encounters = viewModel.EncounterEditing!.Start();

        Assert.Contains("town/loop.wav", encounters.SelectedEncounter!.MusicChoices);
    }

    // The page's opener reaches the draft's encounters through the shell, as Database > Encounters shows them.
    [Fact]
    public void EncounterEditing_TestWorld_StartsOnItsEncountersInIdOrder()
    {
        var viewModel = _content.Open(_content.TestWorld);

        var ids = viewModel.EncounterEditing!.Start().Encounters.Select(encounter => encounter.Id).ToList();

        Assert.Contains("goblin_trio", ids);
        Assert.Equal(ids.Order(StringComparer.Ordinal), ids);
    }
}
