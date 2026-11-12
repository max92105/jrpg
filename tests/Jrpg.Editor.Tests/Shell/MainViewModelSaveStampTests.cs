namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Ctrl+S against a hand edit made while the editor is open (decision 559): a file the editor didn't change keeps it, a
/// file it changed is refused by name.
/// </summary>
public sealed class MainViewModelSaveStampTests : IDisposable
{
    private const string HandEdit = "\r\n";

    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Save_ObjectsEditedOnDiskWhileOpen_ShowsTheRefusalNamingTheFile()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        File.AppendAllText(_content.ObjectsPath, HandEdit);
        var villager = viewModel.Catalog!.Objects["villager"];
        viewModel.Content!.SetObject(villager with { Category = "Townsfolk" });

        viewModel.SaveCommand.Execute(null);

        Assert.StartsWith("objects.json changed on disk since the editor read it", viewModel.SaveError, StringComparison.Ordinal);
        Assert.True(viewModel.Content.IsDirty);
    }

    [Fact]
    public void Save_TileChangedAfterAHandEditToQuests_KeepsTheQuestsHandEdit()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var questsPath = Path.Combine(_content.ContentCopy, "quests.json");
        File.AppendAllText(questsPath, HandEdit);
        var quests = File.ReadAllBytes(questsPath);
        EditorTestContent.BlockFirstWalkableTile(viewModel);

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.Equal(quests, File.ReadAllBytes(questsPath));
    }
}
