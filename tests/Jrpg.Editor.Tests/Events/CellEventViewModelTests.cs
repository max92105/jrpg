using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

public sealed class CellEventViewModelTests
{
    private static readonly StepChoices _choices = new(["potion"], ["chest"], ["forest", "start"]);

    private static readonly CellEvent _exit = new(
        new GridCell(6, 19),
        new MapEvent { Id = "cell_6_19", Activation = EventActivation.Touch, Steps = [new WarpStep("start", new GridCell(5, 5))] });

    [Fact]
    public void Constructor_CellWithoutEvent_StartsAWalkedIntoEventNamedAfterTheCell()
    {
        var cellEvent = new CellEventViewModel(new GridCell(3, 4), null, _choices);

        var mapEvent = cellEvent.Event.ToEvent();
        Assert.Equal(("cell_3_4", EventActivation.Touch), (mapEvent.Id, mapEvent.Activation));
        Assert.Equal("Add at least one step.", cellEvent.Problem);
    }

    [Fact]
    public void ToEdit_NewWarp_AddsTheCellEvent()
    {
        var cellEvent = new CellEventViewModel(new GridCell(3, 4), null, _choices);
        cellEvent.Event.AddWarpCommand.Execute(null);
        cellEvent.Event.SelectedStep!.MapId = "forest";

        var edit = cellEvent.ToEdit([_exit])!;

        Assert.Null(edit.Before);
        Assert.Equal((1, new GridCell(3, 4)), (edit.Index, edit.After!.Cell));
        Assert.Equal(new WarpStep("forest", new GridCell(0, 0)), Assert.Single(edit.After.Event.Steps));
    }

    [Fact]
    public void ToEdit_Unchanged_IsNull()
    {
        Assert.Null(new CellEventViewModel(_exit.Cell, _exit, _choices).ToEdit([_exit]));
    }

    [Fact]
    public void ToEdit_WarpRetargeted_ReplacesTheEvent()
    {
        var cellEvent = new CellEventViewModel(_exit.Cell, _exit, _choices);
        cellEvent.Event.SelectedStep = cellEvent.Event.Steps[0];
        cellEvent.Event.SelectedStep.MapId = "forest";

        var edit = cellEvent.ToEdit([_exit])!;

        Assert.Equal((0, _exit), (edit.Index, edit.Before));
        Assert.Equal("cell_6_19", edit.After!.Event.Id);
    }
}
