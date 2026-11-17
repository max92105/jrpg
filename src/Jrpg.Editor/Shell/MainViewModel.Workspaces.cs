using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The mode bar (decision 916): World shows the maps; Database hides them and puts the shown map out of reach of the
/// undo history (its steps waiting as another map's do until World is shown again), Delete selected objects and
/// Ctrl+H's collision (decision 925). Back to previous map shows World first.
/// </summary>
internal sealed partial class MainViewModel
{
    /// <summary>The half of the editor shown; World at start, neither remembered between runs (decision 922).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWorldShown), nameof(IsDatabaseShown))]
    [NotifyCanExecuteChangedFor(nameof(ToggleCollisionCommand))]
    public partial EditorWorkspace Workspace { get; private set; }

    public bool IsWorldShown => Workspace == EditorWorkspace.World;

    public bool IsDatabaseShown => Workspace == EditorWorkspace.Database;

    /// <summary>
    /// Alt+Left and Map > Back to previous map: World, then the previous map; greyed with the map list's Back.
    /// </summary>
    public IRelayCommand GoBackToMapCommand => field ??= CreateGoBackToMapCommand();

    [RelayCommand]
    private void ShowWorld()
    {
        Workspace = EditorWorkspace.World;
        History?.OpenMap = CurrentMap;
        Placing?.IsMapShown = true;
    }

    // A stroke still being dragged becomes its map's undo step first, as when another map is shown.
    [RelayCommand]
    private void ShowDatabase()
    {
        CurrentMap?.EndStroke();
        Workspace = EditorWorkspace.Database;
        History?.OpenMap = null;
        Placing?.IsMapShown = false;
    }

    private RelayCommand CreateGoBackToMapCommand()
    {
        var goBack = Navigation.GoBackCommand;
        var command = new RelayCommand(
            () =>
            {
                ShowWorld();
                goBack.Execute(null);
            },
            () => goBack.CanExecute(null));
        goBack.CanExecuteChanged += (_, _) => command.NotifyCanExecuteChanged();
        return command;
    }
}
