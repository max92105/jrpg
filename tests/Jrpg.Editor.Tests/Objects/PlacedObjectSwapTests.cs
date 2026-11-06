using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

/// <summary>
/// Swapping a placed object's object in the panel (decision 513), on the forest without its own objects and one chest,
/// "chest_1" at (2, 2) on layer 1, whose events each test builds.
/// </summary>
public sealed class PlacedObjectSwapTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private readonly CatalogDraft _draft = new(_catalog);

    [Fact]
    public void ObjectIds_ListEveryCatalogObjectSorted()
    {
        var placing = PlacingChestWith();
        var editing = placing.Editing;

        Assert.Equal(_catalog.Objects.Keys.Order(StringComparer.Ordinal), editing.ObjectIds);
    }

    [Fact]
    public void Apply_ObjectSwapped_KeepsUidPositionLayerAndEvents()
    {
        var talk = EventOf("chest_1_0");
        var placing = PlacingChestWith(talk);
        var editing = placing.Editing;
        editing.ObjectId = "bench";

        editing.ApplyCommand.Execute(null);

        var swapped = placing.OpenMap!.Map.Layout.Objects[0];
        Assert.Equal(("chest_1", "bench", new Point2D(2, 2), 1), (swapped.Uid, swapped.ObjectId, swapped.Position, swapped.Z));
        Assert.Equal([talk], swapped.Events);
    }

    [Fact]
    public void Apply_ObjectSwapped_UndoBringsTheOldObjectBack()
    {
        var placing = PlacingChestWith();
        var editing = placing.Editing;
        editing.ObjectId = "bench";
        editing.ApplyCommand.Execute(null);

        placing.OpenMap!.UndoCommand.Execute(null);

        Assert.Equal("chest", placing.OpenMap!.Map.Layout.Objects[0].ObjectId);
        Assert.Equal("chest", editing.ObjectId);
    }

    [Fact]
    public void Warning_EventWaitsForAnObjectTheSwapNeverReaches_NamesTheEvent()
    {
        var editing = PlacingChestWith(EventOf("chest_1_0") with { OnlyWhenObjectId = "chest" }).Editing;

        editing.ObjectId = "bench";

        Assert.Equal("Event chest_1_0 runs only while the object is chest; this one never becomes it.", editing.Warning);
        Assert.True(editing.ApplyCommand.CanExecute(null));
    }

    // The chest on the last column, swapped to a 2-wide object: the new object's size puts its center past the edge.
    [Fact]
    public void Problem_SwappedToAWiderObjectOnTheEdge_SaysTheCenterLeavesTheMap()
    {
        _draft.SetObject(_catalog.Objects["chest"] with { Id = "wide_box", Width = 2 });
        var placing = PlacingChestWith();
        var editing = placing.Editing;
        editing.XText = (placing.OpenMap!.Map.Layout.Width - 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

        editing.ObjectId = "wide_box";

        Assert.Equal("The object's center must stay on the map.", editing.Problem);
    }

    [Fact]
    public void Warning_EventWaitsForTheNewObject_IsNull()
    {
        var editing = PlacingChestWith(EventOf("chest_1_0") with { OnlyWhenObjectId = "bench" }).Editing;

        editing.ObjectId = "bench";

        Assert.Null(editing.Warning);
    }

    [Fact]
    public void Warning_EventWaitsForASwapTarget_IsNull()
    {
        var opened = EventOf("chest_1_1") with { OnlyWhenObjectId = "chest_opened" };
        var opening = EventOf("chest_1_0") with { Steps = [new SwapObjectStep("chest_opened")] };
        var placing = PlacingChestWith(opening, opened);
        var editing = placing.Editing;

        editing.ObjectId = "bench";

        Assert.Null(editing.Warning);
    }

    // A new object chosen in the box, not applied, then its making undone: the box names what's wrong.
    [Fact]
    public void ObjectIds_ChosenObjectUndoneAway_LeavesTheListAndTheChoiceGetsAProblem()
    {
        var placing = PlacingChestWith();
        var editing = placing.Editing;
        _draft.SetObject(_catalog.Objects["chest"] with { Id = "test_box" });
        editing.ObjectId = "test_box";

        _draft.Undo();

        Assert.DoesNotContain("test_box", editing.ObjectIds);
        Assert.Equal("Choose an object.", editing.Problem);
    }

    [Fact]
    public void ObjectIds_NewObjectComposed_JoinsTheList()
    {
        var placing = PlacingChestWith();
        var editing = placing.Editing;
        var changed = new List<string?>();
        editing.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        _draft.SetObject(_catalog.Objects["chest"] with { Id = "test_box" });

        Assert.Contains("test_box", editing.ObjectIds);
        Assert.Contains(nameof(PlacedObjectEditing.ObjectIds), changed);
    }

    // The forest with chest_1 alone, holding these events, selected so the panel shows it.
    private ObjectPlacingViewModel PlacingChestWith(params MapEvent[] events)
    {
        var forest = _catalog.Maps["forest"];
        var chest = new PlacedObject { Uid = "chest_1", ObjectId = "chest", Position = new Point2D(2, 2), Z = 1, Events = events };
        var map = new MapDefinition(forest.Layout with { Objects = [chest] }, forest.Settings);
        var placing = new ObjectPlacingViewModel(_draft, new LayerChoiceViewModel(), new OpenMaps()) { OpenMap = new MapViewModel(map) };
        placing.SelectObjectAt(new Point2D(2.5, 2.5));
        return placing;
    }

    private static MapEvent EventOf(string id) => new() { Id = id, Activation = EventActivation.Interact, Steps = [] };
}
