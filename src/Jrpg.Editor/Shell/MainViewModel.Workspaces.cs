using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Data.Art;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Party;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The mode bar (decision 916): World shows the maps; Database hides them and puts the shown map out of reach of the
/// undo history (its steps waiting as another map's do until World is shown again), Delete selected objects and
/// Ctrl+H's collision (decision 925), and shows the catalog pages of <see cref="DatabaseWorkspace"/>, the Database
/// menu's entries and a page's Edit... each showing a page. Leaving Database applies the shown page first, or stays
/// when it refuses (decision 919); Back to previous map shows World first.
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

    /// <summary>
    /// A shown page's Edit... reaching another catalog (decision 928): the page with that title on the record with that
    /// key, the shown page applied first or, refused, kept with its refusal on the bar.
    /// </summary>
    public void ShowDatabasePageOnRecord(string title, string key)
    {
        var workspace = DatabaseWorkspace!;
        workspace.ShowPageOnRecord(workspace.PageTitled(title), key);
    }

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

    // Pressing Database again would start the shown page again and drop what was typed.
    [RelayCommand]
    private void ShowDatabase()
    {
        if (IsDatabaseShown)
        {
            return;
        }

        PutTheMapOutOfReach();
        DatabaseWorkspace?.Open();
    }

    // The Database menu's entries and NPCs > Character sprites (decision 926). From World the page opens at once, the
    // last page shown never built; in Database a shown page that refuses to apply stays, its refusal on the bar.
    [RelayCommand(CanExecute = nameof(HasDatabasePage))]
    private void ShowDatabasePage(string title)
    {
        var workspace = DatabaseWorkspace!;
        var page = workspace.PageTitled(title);
        if (IsDatabaseShown)
        {
            workspace.ShowPageCommand.Execute(page);
            return;
        }

        PutTheMapOutOfReach();
        workspace.Open(page);
    }

    // A stroke still being dragged becomes its map's undo step first, as when another map is shown.
    private void PutTheMapOutOfReach()
    {
        CurrentMap?.EndStroke();
        Workspace = EditorWorkspace.Database;
        History?.OpenMap = null;
        Placing?.IsMapShown = false;
    }

    private bool HasDatabasePage(string title) => DatabaseWorkspace?.HasPage(title) == true;

    // The list's order is decision 917's groups: Story, Battle, Items, Trade, Characters. Called by the constructor
    // once the content loaded, so every editing view model is built. A save line quoting the shown page's refusal
    // clears with it.
    private DatabaseWorkspaceViewModel CreateDatabaseWorkspace(CatalogDraft content)
    {
        var workspace = new DatabaseWorkspaceViewModel(
            content,
            [
                CatalogPage.Of("Quests", CatalogGroup.Story, QuestEditing!),
                CatalogPage.Of("Cinematics", CatalogGroup.Story, CinematicEditing!),
                CatalogPage.Of("Enemies", CatalogGroup.Battle, EnemyEditing!),
                CatalogPage.Of("Encounters", CatalogGroup.Battle, EncounterEditing!),
                CatalogPage.Of("Skills", CatalogGroup.Battle, SkillEditing!),
                CatalogPage.Of("Effects", CatalogGroup.Battle, TieredEffectEditing!),
                CatalogPage.Of("Statuses", CatalogGroup.Battle, StatusEditing!),
                CatalogPage.Of("Drop tables", CatalogGroup.Battle, DropTableEditing!),
                CatalogPage.Of("Effect strips", CatalogGroup.Battle, EffectStripEditing!),
                CatalogPage.Of("Battle backgrounds", CatalogGroup.Battle, BattleBackgroundEditing!),
                CatalogPage.Of("Items", CatalogGroup.Items, ItemEditing!),
                CatalogPage.Of("Affixes", CatalogGroup.Items, AffixEditing!),
                CatalogPage.Of("Shops", CatalogGroup.Trade, ShopEditing!),
                CatalogPage.Of("Character sprites", CatalogGroup.Characters, FieldSpriteEditing!),
                CatalogPage.Of("Classes", CatalogGroup.Characters, new ClassEditingViewModel(content)),
                CatalogPage.Of(
                    "Party members",
                    CatalogGroup.Characters,
                    new PartyMemberEditingViewModel(content, folder => ArtFolders.ImagesIn(ArtRoot, folder))),
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
