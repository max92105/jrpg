using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Data.Content;
using Jrpg.Editor.Events;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Events;

/// <summary>
/// A warp step's destination: picked on the test world's maps, and where the step leads (decision 506).
/// </summary>
public sealed class EventStepViewModelWarpTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly OpenMaps _noneOpen = new();
    private static readonly WarpMaps _maps = new(_noneOpen.EntriesWith(_catalog), _noneOpen.LayoutsWith(_catalog), _catalog);

    [Fact]
    public void StartPickingDestination_ValidStep_StartsOnItsMapAndCell()
    {
        var step = EventStepViewModel.From(new WarpStep("start", new GridCell(2, 3)));

        var picker = step.StartPickingDestination(_maps);

        Assert.Equal("start", picker.MapId);
        Assert.Equal(new GridCell(2, 3), picker.Cell);
    }

    [Fact]
    public void StartPickingDestination_CellHalfTyped_StartsWithNoCell()
    {
        var step = EventStepViewModel.From(new WarpStep("start", new GridCell(2, 3)));
        step.CellYText = "";

        Assert.Null(step.StartPickingDestination(_maps).Cell);
    }

    [Fact]
    public void StartPickingDestination_NewStep_StartsWithNoMap()
    {
        Assert.Null(EventStepViewModel.New(EventStepKind.Warp).StartPickingDestination(_maps).MapId);
    }

    [Fact]
    public void TakeDestination_PickedMapAndCell_SetsTheStepsFields()
    {
        var step = EventStepViewModel.New(EventStepKind.Warp);
        var picker = step.StartPickingDestination(_maps);
        picker.MapId = "forest";
        picker.ChooseCell(new GridCell(4, 5));

        step.TakeDestination(picker);

        Assert.Equal(new WarpStep("forest", new GridCell(4, 5)), step.ToStep());
    }

    [Fact]
    public void TakeDestination_PickerWithAProblem_Throws()
    {
        var step = EventStepViewModel.New(EventStepKind.Warp);
        var picker = step.StartPickingDestination(_maps);
        picker.MapId = "forest";

        var thrown = Assert.Throws<InvalidOperationException>(() => step.TakeDestination(picker));

        Assert.Equal("Click the cell to arrive on.", thrown.Message);
    }

    [Fact]
    public void Destination_ValidWarp_IsItsMapAndCell()
    {
        var step = EventStepViewModel.From(new WarpStep("start", new GridCell(2, 3)));

        Assert.Equal(new WarpDestination("start", new GridCell(2, 3)), step.Destination);
    }

    [Fact]
    public void Destination_CellHalfTyped_IsNull()
    {
        var step = EventStepViewModel.From(new WarpStep("start", new GridCell(2, 3)));
        step.CellXText = "x";

        Assert.Null(step.Destination);
    }

    [Fact]
    public void Destination_NotAWarp_IsNull()
    {
        Assert.Null(EventStepViewModel.New(EventStepKind.Dialogue).Destination);
    }
}
