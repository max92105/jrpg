using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

/// <summary>
/// Copy, Cut and Paste of placed objects (decision 512), on the forest without its own objects and two 1 x 1 chests:
/// "chest_1" at (2, 2) with a talk event, "chest_2" at (5, 3).
/// </summary>
public sealed class ObjectCopyingTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly MapEvent _talk = new() { Id = "chest_1_0", Activation = EventActivation.Interact, Prompt = "Read", Steps = [] };

    private readonly ObjectPlacingViewModel _placing;

    public ObjectCopyingTests()
    {
        var forest = _catalog.Maps["forest"];
        PlacedObject[] chests =
        [
            new() { Uid = "chest_1", ObjectId = "chest", Position = new Point2D(2, 2), Z = 1, Events = [_talk] },
            new() { Uid = "chest_2", ObjectId = "chest", Position = new Point2D(5, 3), Z = 1 },
        ];
        var map = new MapDefinition(forest.Layout with { Objects = chests }, forest.Settings);
        _placing = new ObjectPlacingViewModel(new CatalogDraft(_catalog), new LayerChoiceViewModel(), new OpenMaps()) { OpenMap = new MapViewModel(map) };
    }

    private ObjectCopying Copying => _placing.Copying;

    private IReadOnlyList<PlacedObject> Objects => _placing.OpenMap!.Map.Layout.Objects;

    [Fact]
    public void CopyCommand_NothingSelected_CannotExecute()
    {
        Assert.False(Copying.CopyCommand.CanExecute(null));
    }

    [Fact]
    public void CopyCommand_SelectionMade_CanExecuteAndIsRaised()
    {
        var raised = 0;
        Copying.CopyCommand.CanExecuteChanged += (_, _) => raised++;

        _placing.SelectObjectAt(new Point2D(2.5, 2.5));

        Assert.True(Copying.CopyCommand.CanExecute(null));
        Assert.True(raised > 0);
    }

    [Fact]
    public void Copy_ThenPasteCenteredAt_AddsCopiesWithTheirEvents()
    {
        CopyChestOne();

        Copying.PasteCenteredAt(new Point2D(10.5, 10.5));

        var copy = Objects[^1];
        Assert.Equal(("chest", "Read"), (copy.ObjectId, Assert.Single(copy.Events).Prompt));
    }

    [Fact]
    public void PasteCenteredAt_Undo_RemovesTheCopiesInOneStep()
    {
        _placing.SelectObjectsIn(new Point2D(0, 0), new Point2D(10, 10));
        Copying.CopyCommand.Execute(null);
        Copying.PasteCenteredAt(new Point2D(10, 10));

        _placing.OpenMap!.UndoCommand.Execute(null);

        Assert.Equal(["chest_1", "chest_2"], Objects.Select(placed => placed.Uid));
        Assert.False(_placing.OpenMap.IsDirty);
    }

    // The group spans (2, 2) to (6, 4): 4 x 2 cells, so centered on (10, 10) its top-left is (8, 9).
    [Fact]
    public void PasteCenteredAt_Group_CentersItsBoundsOnThePoint()
    {
        _placing.SelectObjectsIn(new Point2D(0, 0), new Point2D(10, 10));
        Copying.CopyCommand.Execute(null);

        Copying.PasteCenteredAt(new Point2D(10, 10));

        Assert.Equal([new Point2D(8, 9), new Point2D(11, 10)], Objects.Skip(2).Select(placed => placed.Position));
    }

    [Fact]
    public void PasteOnGridCenteredAt_FreePoint_PutsTheGroupsTopLeftOnWholeCells()
    {
        CopyChestOne();

        Copying.PasteOnGridCenteredAt(new Point2D(10.3, 10.3));

        Assert.Equal(new Point2D(10, 10), Objects[^1].Position);
    }

    [Fact]
    public void PasteCenteredAt_SelectsThePastedObjects()
    {
        CopyChestOne();

        Copying.PasteCenteredAt(new Point2D(10.5, 10.5));

        Assert.Equal([Objects[^1].Uid], _placing.SelectedObjectUids);
    }

    [Fact]
    public void Cut_RemovesTheSelectionAndKeepsTheCopy()
    {
        _placing.SelectObjectAt(new Point2D(2.5, 2.5));

        Copying.CutCommand.Execute(null);

        Assert.Equal(["chest_2"], Objects.Select(placed => placed.Uid));
        Assert.True(Copying.CanPaste);
    }

    [Fact]
    public void Cut_Undo_PutsTheObjectBackInOneStep()
    {
        _placing.SelectObjectAt(new Point2D(2.5, 2.5));
        Copying.CutCommand.Execute(null);

        _placing.OpenMap!.UndoCommand.Execute(null);

        Assert.Equal(["chest_1", "chest_2"], Objects.Select(placed => placed.Uid));
    }

    [Fact]
    public void PasteCenteredAt_PointOffTheMap_RecordsNoStep()
    {
        CopyChestOne();

        Copying.PasteCenteredAt(new Point2D(-2, 3));

        Assert.False(_placing.OpenMap!.IsDirty);
    }

    // A copied event swaps chest_1 to an object the catalog doesn't have: the save would refuse the paste.
    [Fact]
    public void CanPaste_ACopiedSwapNamesAnUnknownObject_IsFalse()
    {
        _placing.OpenMap!.Do(new SetObjectEventsEdit("chest_1", [_talk], [_talk with { Steps = [new SwapObjectStep("no_such_object")] }]));

        CopyChestOne();

        Assert.False(Copying.CanPaste);
    }

    [Fact]
    public void PasteCenteredAt_NothingCopied_RecordsNoStep()
    {
        Copying.PasteCenteredAt(new Point2D(10.5, 10.5));

        Assert.False(_placing.OpenMap!.IsDirty);
        Assert.False(Copying.CanPaste);
    }

    // The original's prompt changed after Ctrl+C: the paste has the prompt as it was copied.
    [Fact]
    public void PasteCenteredAt_ObjectEditedAfterTheCopy_PastesItAsCopied()
    {
        CopyChestOne();
        _placing.OpenMap!.Do(new SetObjectEventsEdit("chest_1", [_talk], [_talk with { Prompt = "Open" }]));

        Copying.PasteCenteredAt(new Point2D(10.5, 10.5));

        Assert.Equal("Read", Assert.Single(Objects[^1].Events).Prompt);
    }

    private void CopyChestOne()
    {
        _placing.SelectObjectAt(new Point2D(2.5, 2.5));
        Copying.CopyCommand.Execute(null);
    }
}
