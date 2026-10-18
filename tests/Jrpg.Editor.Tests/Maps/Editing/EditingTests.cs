using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

public sealed class EditingTests
{
    private const int GroundZ = -5;

    private static readonly GridCell _left = new(0, 0);
    private static readonly GridCell _right = new(1, 0);

    // A 2x1 meadow: one ground layer of grass (1).
    private static readonly MapDefinition _meadow = new(
        new MapLayout { Id = "meadow", Width = 2, Height = 1, Layers = [new TileLayer { Z = GroundZ, Rows = [[1, 1]] }], Objects = [] },
        new MapSettings { Id = "meadow", DisplayName = "Meadow" });

    private readonly MapDraft _draft = MapDraft.From(_meadow);

    [Fact]
    public void MapDraft_ToDefinition_Unchanged_EqualsSource()
    {
        Assert.Equivalent(_meadow, _draft.ToDefinition(), strict: true);
    }

    [Fact]
    public void MapDraft_ToDefinition_LaterEdit_LeavesEarlierDefinitionAlone()
    {
        var before = _draft.ToDefinition();

        _draft.SetTileId(GroundZ, _left, 2);

        Assert.Equal(1, before.Layout.Layers[0].TileIdAt(_left));
        Assert.Equal(2, _draft.ToDefinition().Layout.Layers[0].TileIdAt(_left));
    }

    [Fact]
    public void MapDraft_SetTileId_NewZ_InsertsLayerInZOrder()
    {
        _draft.SetTileId(-3, _left, 4);
        _draft.SetTileId(-7, _right, 5);

        var layers = _draft.ToDefinition().Layout.Layers;

        Assert.Equal([-7, GroundZ, -3], layers.Select(layer => layer.Z));
        Assert.Equal([[0, 5]], layers[0].Rows);
    }

    [Fact]
    public void MapDraft_ToDefinition_EmptyLayer_IsOmitted()
    {
        _draft.SetTileId(-3, _left, 4);
        _draft.SetTileId(-3, _left, TileLayer.EmptyTileId);

        Assert.Equal([GroundZ], _draft.ToDefinition().Layout.Layers.Select(layer => layer.Z));
    }

    [Fact]
    public void MapDraft_TileIdAt_ZWithoutLayer_IsEmpty()
    {
        Assert.Equal(TileLayer.EmptyTileId, _draft.TileIdAt(-3, _left));
    }

    [Fact]
    public void SetTilesEdit_Revert_RestoresPreviousIds()
    {
        var edit = Paint(_left, from: 1, to: 2);
        edit.Apply(_draft);

        edit.Revert(_draft);

        Assert.Equal(1, _draft.TileIdAt(GroundZ, _left));
    }

    [Fact]
    public void EditHistory_Undo_RevertsLastEdit()
    {
        var history = new EditHistory(_draft);
        history.Do(Paint(_left, from: 1, to: 2));
        history.Do(Paint(_right, from: 1, to: 3));

        history.Undo();

        Assert.Equal((2, 1), (_draft.TileIdAt(GroundZ, _left), _draft.TileIdAt(GroundZ, _right)));
        Assert.True(history.CanRedo);
    }

    [Fact]
    public void EditHistory_Redo_ReappliesTheUndoneEdit()
    {
        var history = new EditHistory(_draft);
        history.Do(Paint(_left, from: 1, to: 2));
        history.Undo();

        history.Redo();

        Assert.Equal(2, _draft.TileIdAt(GroundZ, _left));
    }

    [Fact]
    public void EditHistory_DoAfterUndo_ClearsRedo()
    {
        var history = new EditHistory(_draft);
        history.Do(Paint(_left, from: 1, to: 2));
        history.Undo();

        history.Do(Paint(_right, from: 1, to: 3));

        Assert.False(history.CanRedo);
    }

    [Fact]
    public void EditHistory_IsDirty_AfterUndoToSavedPoint_IsFalse()
    {
        var history = new EditHistory(_draft);
        history.Do(Paint(_left, from: 1, to: 2));
        history.MarkSaved();
        history.Do(Paint(_right, from: 1, to: 3));

        history.Undo();

        Assert.False(history.IsDirty);
    }

    [Fact]
    public void EditHistory_IsDirty_SavedStateDroppedByANewEdit_StaysDirty()
    {
        // Saved after one edit; undoing it and painting something else can never return to the saved state.
        var history = new EditHistory(_draft);
        history.Do(Paint(_left, from: 1, to: 2));
        history.MarkSaved();
        history.Undo();
        history.Do(Paint(_right, from: 1, to: 3));

        history.Undo();

        Assert.True(history.IsDirty);
    }

    [Fact]
    public void EditHistory_Record_AppliedEdit_UndoesIt()
    {
        var history = new EditHistory(_draft);
        var edit = Paint(_left, from: 1, to: 2);
        edit.Apply(_draft);

        history.Record(edit);
        history.Undo();

        Assert.Equal(1, _draft.TileIdAt(GroundZ, _left));
        Assert.False(history.IsDirty);
    }

    private static SetTilesEdit Paint(GridCell cell, int from, int to) => new(GroundZ, [new TileChange(cell, from, to)]);
}
