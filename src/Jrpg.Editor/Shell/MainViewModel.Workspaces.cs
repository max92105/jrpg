using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The mode bar (decision 916): World shows the maps; Database hides them and puts the shown map out of reach of the
/// undo history (its steps waiting as another map's do until World is shown again), Delete selected objects and
/// Ctrl+H's collision (decision 925), and shows the catalog pages of <see cref="DatabaseWorkspace"/>. Leaving Database
/// applies the shown page first, or stays when it refuses (decision 919); Back to previous map shows World first.
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

    /// <summary>The Database mode's pages and the shown one's session; null when the content didn't load.</summary>
    public DatabaseWorkspaceViewModel? DatabaseWorkspace { get; }

    /// <summary>
    /// Alt+Left and Map > Back to previous map: World, then the previous map, neither while the shown Database page
    /// refuses to apply; greyed with the map list's Back.
    /// </summary>
    public IRelayCommand GoBackToMapCommand => field ??= CreateGoBackToMapCommand();

    [RelayCommand]
    private void ShowWorld()
    {
        if (DatabaseWorkspace is { } workspace && !workspace.TryClose())
        {
            return;
        }

        Workspace = EditorWorkspace.World;
        History?.OpenMap = CurrentMap;
        Placing?.IsMapShown = true;
    }

    // A stroke still being dragged becomes its map's undo step first, as when another map is shown. Pressing Database
    // again would start the shown page again and drop what was typed.
    [RelayCommand]
    private void ShowDatabase()
    {
        if (IsDatabaseShown)
        {
            return;
        }

        CurrentMap?.EndStroke();
        Workspace = EditorWorkspace.Database;
        History?.OpenMap = null;
        Placing?.IsMapShown = false;
        DatabaseWorkspace?.Open();
    }

    // The list's order is decision 917's groups: Story, Battle, Items, Trade, Characters. Called by the constructor
    // once the content loaded, so every editing view model is built. A save line quoting the shown page's refusal
    // clears with it.
    private DatabaseWorkspaceViewModel CreateDatabaseWorkspace(CatalogDraft content)
    {
        var workspace = new DatabaseWorkspaceViewModel(
            content,
            [
                CatalogPage.Of("Quests", QuestEditing!),
                CatalogPage.Of("Cinematics", CinematicEditing!),
                CatalogPage.Of("Enemies", EnemyEditing!),
                CatalogPage.Of("Encounters", EncounterEditing!),
                CatalogPage.Of("Skills", SkillEditing!),
                CatalogPage.Of("Effects", TieredEffectEditing!),
                CatalogPage.Of("Statuses", StatusEditing!),
                CatalogPage.Of("Drop tables", DropTableEditing!),
                CatalogPage.Of("Effect strips", EffectStripEditing!),
                CatalogPage.Of("Battle backgrounds", BattleBackgroundEditing!),
                CatalogPage.Of("Items", ItemEditing!),
                CatalogPage.Of("Affixes", AffixEditing!),
                CatalogPage.Of("Shops", ShopEditing!),
                CatalogPage.Of("Character sprites", FieldSpriteEditing!),
            ]);
        workspace.PropertyChanged += OnDatabaseWorkspaceChanged;
        return workspace;
    }

    private RelayCommand CreateGoBackToMapCommand()
    {
        var goBack = Navigation.GoBackCommand;
        var command = new RelayCommand(
            () =>
            {
                ShowWorld();
                if (IsWorldShown)
                {
                    goBack.Execute(null);
                }
            },
            () => goBack.CanExecute(null));
        goBack.CanExecuteChanged += (_, _) => command.NotifyCanExecuteChanged();
        return command;
    }
}
