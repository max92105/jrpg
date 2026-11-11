namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Map > Settings on the test world's forest: the resize, asking before it drops anything, and the fields applied, each
/// one undo step (decisions 527, 528).
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
    public void TryApplySettings_BannerUnticked_Undo_TicksItAgain()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var settings = viewModel.StartEditingSettings()!;
        settings.ShowsBanner = false;
        Assert.True(viewModel.TryApplySettings(settings));
        Assert.False(viewModel.CurrentMap!.Map.Settings.ShowsBanner);

        viewModel.CurrentMap.UndoCommand.Execute(null);

        Assert.True(viewModel.CurrentMap.Map.Settings.ShowsBanner);
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

    [Fact]
    public void TryApplySettings_ShrinkRefused_KeepsTheMapAsItIs()
    {
        var viewModel = _content.Open(_content.TestWorld, answer: false);
        var settings = viewModel.StartEditingSettings()!;
        settings.WidthText = "1";
        settings.DisplayName = "Small forest";

        Assert.False(viewModel.TryApplySettings(settings));

        Assert.StartsWith("Resizing to 1 x ", Assert.Single(_content.Questions), StringComparison.Ordinal);
        Assert.False(viewModel.CurrentMap!.IsDirty);
    }

    [Fact]
    public void TryApplySettings_ShrinkAccepted_ResizesAndRenamesAsTwoUndoSteps()
    {
        var viewModel = _content.Open(_content.TestWorld, answer: true);
        var settings = viewModel.StartEditingSettings()!;
        settings.WidthText = "1";
        settings.DisplayName = "Small forest";

        Assert.True(viewModel.TryApplySettings(settings));

        var map = viewModel.CurrentMap!;
        Assert.Equal((1, "Small forest"), (map.Map.Layout.Width, map.Map.Settings.DisplayName));
        map.UndoCommand.Execute(null);
        Assert.Equal((1, "Forest"), (map.Map.Layout.Width, map.Map.Settings.DisplayName));
        map.UndoCommand.Execute(null);
        Assert.False(map.IsDirty);
    }

    [Fact]
    public void TryApplySettings_Grow_ResizesWithoutAsking()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var settings = viewModel.StartEditingSettings()!;
        settings.HeightText = "100";

        Assert.True(viewModel.TryApplySettings(settings));

        Assert.Equal(100, viewModel.CurrentMap!.Map.Layout.Height);
        Assert.Empty(_content.Questions);
    }

    [Fact]
    public void TryApplySettings_NothingChanged_AddsNoUndoStep()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.True(viewModel.TryApplySettings(viewModel.StartEditingSettings()!));

        Assert.False(viewModel.CurrentMap!.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void TryApplySettings_EmptySizeBox_AppliesNothing()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var settings = viewModel.StartEditingSettings()!;
        settings.HeightText = "";

        Assert.False(viewModel.TryApplySettings(settings));
        Assert.False(viewModel.CurrentMap!.IsDirty);
    }
}
