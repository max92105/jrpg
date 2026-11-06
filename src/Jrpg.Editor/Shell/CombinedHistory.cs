using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Shell;

/// <summary>
/// One Undo and Redo over the open maps' and the catalogs' histories, in the order the steps were recorded (ADR 0014,
/// decision 117); each open map keeps its steps while another is shown (decision 503). A catalog undo that would remove
/// a record still in use is refused, with the reason in <see cref="Refusal"/>.
/// </summary>
internal sealed partial class CombinedHistory : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly UndoJournal _journal = new();
    private readonly OpenMaps _openMaps;
    private MapViewModel? _openMap;

    /// <param name="catalog">The catalogs whose steps share the order with the open maps'.</param>
    /// <param name="openMaps">Every open map, read by the refusals as it is now (decision 502).</param>
    public CombinedHistory(CatalogDraft catalog, OpenMaps openMaps)
    {
        _catalog = catalog;
        _openMaps = openMaps;
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
    /// Why the last undo was refused: a record it would remove is still in use; null after any other undo, redo or
    /// edit.
    /// </summary>
    [ObservableProperty]
    public partial string? Refusal { get; private set; }

    /// <summary>Undoes the newest step, whether it went to the shown map or to the catalogs (decision 117).</summary>
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
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

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
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

    // In the order checked: the first map whose event names a quest or a stage the undo would remove (decision 394);
    // the first map placing or swapping to an object it would remove; the first tile cut from a sheet it would remove;
    // the first tile it would remove that something still uses, named with its first user (decision 117).
    private string? RefusalOfCatalogUndo()
    {
        if (RefusalOfQuestUndo() is { } quest)
        {
            return quest;
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
