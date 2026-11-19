using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Shell;

/// <summary>
/// One Undo and Redo over the open maps' and the catalogs' histories, in the order the steps were recorded (ADR 0014,
/// decision 117); each open map keeps its steps while another is shown (decision 503). A catalog undo that would remove
/// a record still in use is refused, with the reason in <see cref="Refusal"/>. Both first apply the shown Database
/// page's typed changes, or are refused with the page's own refusal (decision 920; the `.DatabasePage` partial).
/// </summary>
internal sealed partial class CombinedHistory : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly UndoJournal _journal = new();
    private readonly OpenMaps _openMaps;
    private MapViewModel? _openMap;

    /// <param name="catalog">The catalogs whose steps share the order with the open maps'.</param>
    /// <param name="openMaps">Every open map, read by the refusals as it is now (decision 502).</param>
    /// <param name="database">
    /// The Database mode, whose shown page an undo or redo applies first, listened to so its refusal shown here clears
    /// with the page's.
    /// </param>
    public CombinedHistory(CatalogDraft catalog, OpenMaps openMaps, DatabaseWorkspaceViewModel database)
    {
        _catalog = catalog;
        _openMaps = openMaps;
        _database = database;
        database.PropertyChanged += OnDatabaseChanged;
        catalog.StepRecorded += (_, _) => RecordStep(UndoStep.Catalogs);
        catalog.RedoDropped += (_, _) =>
        {
            _journal.ForgetUndoneCatalogSteps();
            NotifyUndoRedoChanged();
        };
        catalog.Rewriting += (_, _) =>
        {
            _journal.Clear();
            NotifyUndoRedoChanged();
        };
    }

    /// <summary>
    /// The shown map: Undo and Redo reach its steps and the catalogs'; another map's wait until it is shown.
    /// </summary>
    public MapViewModel? OpenMap
    {
        get => _openMap;
        set
        {
            _openMap?.StepRecorded -= OnMapStepRecorded;
            _openMap = value;
            _openMap?.StepRecorded += OnMapStepRecorded;
            NotifyUndoRedoChanged();
        }
    }

    /// <summary>
    /// Why the last undo or redo was refused: a record the undo would remove is still in use, or the shown Database
    /// page can't apply; null after any other undo, redo or edit.
    /// </summary>
    [ObservableProperty]
    public partial string? Refusal { get; private set; }

    /// <summary>
    /// Undoes the newest step, whether it went to the shown map or to the catalogs (decision 117); the shown page's
    /// typed changes are that step when it has any.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (!TryApplyShownPage())
        {
            return;
        }

        // A stroke still being dragged is the newest step.
        _openMap?.EndStroke();
        if (!_journal.CanUndo(ShownMapId))
        {
            return;
        }

        Refusal = _journal.NextUndo(ShownMapId)?.Target == UndoTarget.Catalogs ? RefusalOfCatalogUndo() : null;
        if (Refusal is not null)
        {
            return;
        }

        if (_journal.Undo(ShownMapId).Target == UndoTarget.Map)
        {
            _openMap!.UndoCommand.Execute(null);
        }
        else
        {
            _catalog.Undo();
        }

        NotifyUndoRedoChanged();
    }

    // The shown page's typed changes, applied, are a new step, which drops the redo as any edit does (decision 920).
    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        if (!TryApplyShownPage())
        {
            return;
        }

        _openMap?.EndStroke();
        if (!_journal.CanRedo(ShownMapId))
        {
            return;
        }

        Refusal = null;

        if (_journal.Redo(ShownMapId).Target == UndoTarget.Map)
        {
            _openMap!.RedoCommand.Execute(null);
        }
        else
        {
            _catalog.Redo();
        }

        NotifyUndoRedoChanged();
    }

    // In the order checked: the first map whose event names a quest or a stage the undo would remove (decision 394), or
    // opens a shop it would remove (decision 581), or recruits or runs only while leading a party member it would
    // remove (decision 954), or plays a cinematic it would remove (decision 630), or gives an item it would remove
    // (decision 883); the first map whose random battles roll an encounter it would remove (decision 907); the first
    // map placing or swapping to an object it would remove; the first tile cut from a sheet it would remove; the first
    // tile it would remove that something still uses (decision 117), each named with the first user the check found.
    private string? RefusalOfCatalogUndo()
    {
        if (RefusalOfEventNamedUndo() is { } named)
        {
            return named;
        }

        if (RefusalOfEncounterUndo() is { } rolled)
        {
            return rolled;
        }

        foreach (var objectId in _catalog.ObjectsRemovedByUndo())
        {
            if (CatalogUsage.UsersOfObject(_catalog.Current, objectId, _openMaps.Layouts) is [var map, ..])
            {
                return $"Can't undo: object {objectId} is used on {map}.";
            }
        }

        foreach (var sheetId in _catalog.SheetsRemovedByUndo())
        {
            if (CatalogUsage.TilesCutFrom(sheetId, _catalog.Current) is [var cutTileId, ..])
            {
                return $"Can't undo: tile {cutTileId} is cut from sheet {sheetId}.";
            }
        }

        foreach (var tileId in _catalog.TilesRemovedByUndo())
        {
            if (CatalogUsage.UsersOfTile(tileId, _catalog.Current, _openMaps.Layouts) is [var user, ..])
            {
                return $"Can't undo: tile {tileId} is used on {user}.";
            }
        }

        return null;
    }

    // The quests, shops, party members, cinematics and items a map's event names, checked apart from the objects,
    // sheets and tiles so the catalog undo's check stays under the build's complexity bound (CA1502).
    private string? RefusalOfEventNamedUndo() =>
        RefusalOfQuestUndo() ?? RefusalOfShopUndo() ?? RefusalOfPartyMemberUndo() ?? RefusalOfCinematicUndo()
        ?? RefusalOfItemUndo();

    private string? RefusalOfQuestUndo()
    {
        var openLayouts = _openMaps.Layouts;
        foreach (var questId in _catalog.QuestsRemovedByUndo())
        {
            if (CatalogUsage.UsersOfQuest(_catalog.Current, questId, null, openLayouts) is [var map, ..])
            {
                return $"Can't undo: quest {questId} is used on {map}.";
            }
        }

        foreach (var (questId, stageId) in _catalog.QuestStagesRemovedByUndo())
        {
            if (CatalogUsage.UsersOfQuest(_catalog.Current, questId, stageId, openLayouts) is [var map, ..])
            {
                return $"Can't undo: stage {stageId} of quest {questId} is used on {map}.";
            }
        }

        return null;
    }

    // A saved shop is never removed by its window (decision 581), so only an added one can be undone away.
    private string? RefusalOfShopUndo() =>
        RefusalNamingFirstUser(
            "shop",
            "opened",
            _catalog.ShopsRemovedByUndo(),
            shopId => CatalogUsage.UsersOfShop(_catalog.Current, shopId, _openMaps.Layouts));

    // A saved member is never removed by its page (decision 953), so only an added one can be undone away.
    private string? RefusalOfPartyMemberUndo() =>
        RefusalNamingFirstUser(
            "party member",
            "used",
            _catalog.PartyMembersRemovedByUndo(),
            memberId => CatalogUsage.UsersOfPartyMember(_catalog.Current, memberId, _openMaps.Layouts));

    // A saved cinematic is never removed by its window (decision 630), so only an added one can be undone away.
    private string? RefusalOfCinematicUndo() =>
        RefusalNamingFirstUser(
            "cinematic",
            "played",
            _catalog.CinematicsRemovedByUndo(),
            cinematicId => CatalogUsage.UsersOfCinematic(_catalog.Current, cinematicId, _openMaps.Layouts));

    // A saved item is never removed by its page (decision 877), so only an added one can be undone away.
    private string? RefusalOfItemUndo() =>
        RefusalNamingFirstUser(
            "item",
            "given",
            _catalog.ItemsRemovedByUndo(),
            itemId => CatalogUsage.UsersOfItem(_catalog.Current, itemId, _openMaps.Layouts));

    // A saved encounter is never removed by its page (decision 905), so only an added one can be undone away. Random
    // battles are a map setting, so the open maps are read whole, their unsaved settings included (decision 502).
    private string? RefusalOfEncounterUndo()
    {
        var content = _openMaps.ContentWith(_catalog.Current);
        return RefusalNamingFirstUser(
            "encounter",
            "rolled",
            _catalog.EncountersRemovedByUndo(),
            encounterId => CatalogUsage.UsersOfEncounter(content, encounterId));
    }

    // The first removed record a map still names, said as "Can't undo: shop armory is opened on map start." for the
    // kind "shop" used as "opened".
    private static string? RefusalNamingFirstUser(
        string kind,
        string usedAs,
        IReadOnlyList<string> removedIds,
        Func<string, IReadOnlyList<string>> usersOf)
    {
        foreach (var id in removedIds)
        {
            if (usersOf(id) is [var map, ..])
            {
                return $"Can't undo: {kind} {id} is {usedAs} on {map}.";
            }
        }

        return null;
    }

    /// <summary>
    /// Orders a step a map took while it wasn't the shown one (a link from the Encounters page): undone once that map
    /// is shown, as its other steps.
    /// </summary>
    public void RecordStepOnMap(string mapId) => RecordStep(UndoStep.OnMap(mapId));

    private string? ShownMapId => _openMap?.Map.Id;

    private bool CanUndo() => _journal.CanUndo(ShownMapId);

    private bool CanRedo() => _journal.CanRedo(ShownMapId);

    private void OnMapStepRecorded(object? sender, EventArgs e) => RecordStep(UndoStep.OnMap(_openMap!.Map.Id));

    private void RecordStep(UndoStep step)
    {
        Refusal = null;
        _journal.Record(step);
        NotifyUndoRedoChanged();
    }

    private void NotifyUndoRedoChanged()
    {
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }
}
