namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Map > Settings applying the fields the settings record gained (decision 527), on the test world's forest.
/// </summary>
public sealed class MainViewModelMapSettingsTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void TryApplySettings_AreaLevelTyped_SetsItAsOneStep()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var settings = viewModel.StartEditingSettings()!;
        settings.AreaLevelText = "4";

        Assert.True(viewModel.TryApplySettings(settings));

        Assert.Equal(4, viewModel.CurrentMap!.Map.Settings.AreaLevel);
        Assert.True(viewModel.CurrentMap.IsDirty);
    }

    [Fact]
    public void TryApplySettings_AreaLevelTyped_Undo_RemovesIt()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var settings = viewModel.StartEditingSettings()!;
        settings.AreaLevelText = "4";
        viewModel.TryApplySettings(settings);

        viewModel.CurrentMap!.UndoCommand.Execute(null);

        Assert.Null(viewModel.CurrentMap.Map.Settings.AreaLevel);
        Assert.False(viewModel.CurrentMap.IsDirty);
    }

    [Fact]
    public void TryApplySettings_LevelProblem_AppliesNothing()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var settings = viewModel.StartEditingSettings()!;
        settings.AreaLevelText = "0";
        settings.DisplayName = "Deep forest";

        Assert.False(viewModel.TryApplySettings(settings));

        Assert.False(viewModel.CurrentMap!.IsDirty);
    }
}
