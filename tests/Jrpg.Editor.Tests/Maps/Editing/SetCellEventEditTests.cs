using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

public sealed class SetCellEventEditTests
{
    private static readonly CellEvent _westExit = new(new GridCell(0, 2), Warp("cell_0_2", "forest"));
    private static readonly CellEvent _eastExit = new(new GridCell(4, 2), Warp("cell_4_2", "start"));

    private static readonly MapDefinition _hall = new(
        new MapLayout { Id = "hall", Width = 5, Height = 5, Layers = [], Objects = [], CellEvents = [_westExit, _eastExit] },
        new MapSettings { Id = "hall", DisplayName = "Hall" });

    private readonly MapDraft _draft = MapDraft.From(_hall);

    [Fact]
    public void Apply_SecondEventOnSameCell_ReplacesFirstInPlace()
    {
        var edit = SetCellEventEdit.For(_hall.Layout.CellEvents, _westExit.Cell, Warp("cell_0_2", "interior_1"));

        edit.Apply(_draft);

        var cellEvents = _draft.ToDefinition().Layout.CellEvents;
        Assert.Equal(2, cellEvents.Count);
        Assert.Equal(new WarpStep("interior_1", new GridCell(1, 1)), Assert.Single(cellEvents[0].Event.Steps));
    }

    [Fact]
    public void For_CellWithoutEvent_AddsItLast()
    {
        var added = Warp("cell_2_2", "forest");

        SetCellEventEdit.For(_hall.Layout.CellEvents, new GridCell(2, 2), added).Apply(_draft);

        Assert.Equal([_westExit, _eastExit, new CellEvent(new GridCell(2, 2), added)], _draft.ToDefinition().Layout.CellEvents);
    }

    [Fact]
    public void Revert_RemovedEvent_PutsItBackInItsPlace()
    {
        var edit = SetCellEventEdit.For(_hall.Layout.CellEvents, _westExit.Cell, null);
        edit.Apply(_draft);
        Assert.Equal([_eastExit], _draft.ToDefinition().Layout.CellEvents);

        edit.Revert(_draft);

        Assert.Equal([_westExit, _eastExit], _draft.ToDefinition().Layout.CellEvents);
    }

    [Fact]
    public void Revert_AddedEvent_RemovesIt()
    {
        var edit = SetCellEventEdit.For(_hall.Layout.CellEvents, new GridCell(2, 2), Warp("cell_2_2", "forest"));
        edit.Apply(_draft);

        edit.Revert(_draft);

        Assert.Equal([_westExit, _eastExit], _draft.ToDefinition().Layout.CellEvents);
    }

    private static MapEvent Warp(string id, string mapId) =>
        new() { Id = id, Activation = EventActivation.Touch, Steps = [new WarpStep(mapId, new GridCell(1, 1))] };
}
