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
/// The placed object panel's position and layer (decision 513), on the forest without its own objects and two 1 x 1
/// chests: "chest_1" at a free (2.25, 3.5) on layer 1 with an event, "chest_2" at (6, 6).
/// </summary>
public sealed class PlacedObjectEditingTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly MapEvent _talk = new() { Id = "chest_1_0", Activation = EventActivation.Interact, Steps = [] };

    private readonly ObjectPlacingViewModel _placing;

    public PlacedObjectEditingTests()
    {
        var forest = _catalog.Maps["forest"];
        PlacedObject[] chests =
        [
            new() { Uid = "chest_1", ObjectId = "chest", Position = new Point2D(2.25, 3.5), Z = 1, Events = [_talk] },
            new() { Uid = "chest_2", ObjectId = "chest", Position = new Point2D(6, 6), Z = 1 },
        ];
        var map = new MapDefinition(forest.Layout with { Objects = chests }, forest.Settings);
        _placing = new ObjectPlacingViewModel(new CatalogDraft(_catalog), new LayerChoiceViewModel(), new OpenMaps()) { OpenMap = new MapViewModel(map) };
    }

    private PlacedObjectEditing Editing => _placing.Editing;

    private MapViewModel Map => _placing.OpenMap!;

    [Fact]
    public void HasObject_OneSelected_ShowsItsUidPositionAndLayer()
    {
        SelectChestOne();

        Assert.True(Editing.HasObject);
        Assert.Equal(("chest_1", "2.25", "3.5", "1"), (Editing.Uid, Editing.XText, Editing.YText, Editing.ZText));
    }

    [Fact]
    public void HasObject_TwoSelected_IsFalse()
    {
        _placing.SelectObjectsIn(new Point2D(0, 0), new Point2D(10, 10));

        Assert.False(Editing.HasObject);
    }

    [Fact]
    public void HasObject_NoneSelected_IsFalse()
    {
        Assert.False(Editing.HasObject);
    }

    [Fact]
    public void Apply_PositionTyped_MovesIt()
    {
        SelectChestOne();
        Editing.XText = "4.5";
        Editing.YText = "7";

        Editing.ApplyCommand.Execute(null);

        Assert.Equal(new Point2D(4.5, 7), Map.Map.Layout.Objects[0].Position);
    }

    [Fact]
    public void Apply_ThenUndo_RestoresThePositionInOneStep()
    {
        SelectChestOne();
        (Editing.XText, Editing.YText) = ("4.5", "7");
        Editing.ApplyCommand.Execute(null);

        Map.UndoCommand.Execute(null);

        Assert.Equal(new Point2D(2.25, 3.5), Map.Map.Layout.Objects[0].Position);
        Assert.False(Map.IsDirty);
    }

    [Fact]
    public void Apply_LayerTyped_ChangesItsZOnly()
    {
        SelectChestOne();
        Editing.ZText = "-2";

        Editing.ApplyCommand.Execute(null);

        var edited = Map.Map.Layout.Objects[0];
        Assert.Equal(("chest_1", "chest", -2, new Point2D(2.25, 3.5)), (edited.Uid, edited.ObjectId, edited.Z, edited.Position));
        Assert.Equal([_talk], edited.Events);
        Assert.Equal("chest_2", Map.Map.Layout.Objects[1].Uid);
    }

    [Fact]
    public void Apply_NothingChanged_RecordsNoStep()
    {
        SelectChestOne();

        Editing.ApplyCommand.Execute(null);

        Assert.False(Map.IsDirty);
    }

    [Theory]
    [InlineData("2,5", "3.5", "1", "X and Y must be numbers, with a dot for decimals.")]
    [InlineData("2.25", "", "1", "X and Y must be numbers, with a dot for decimals.")]
    [InlineData("2.25", "3.5", "1.5", "The layer must be a whole number.")]
    [InlineData("2.25", "3.5", "x", "The layer must be a whole number.")]
    [InlineData("NaN", "3.5", "1", "X and Y must be numbers, with a dot for decimals.")]
    [InlineData("2.25", "Infinity", "1", "X and Y must be numbers, with a dot for decimals.")]
    public void Problem_InvalidFields_NamesTheField(string x, string y, string z, string expected)
    {
        SelectChestOne();

        (Editing.XText, Editing.YText, Editing.ZText) = (x, y, z);

        Assert.Equal(expected, Editing.Problem);
    }

    [Fact]
    public void Problem_CenterOffTheMap_SaysSo()
    {
        SelectChestOne();

        Editing.XText = "-0.6";

        Assert.Equal("The object's center must stay on the map.", Editing.Problem);
    }

    // The forest's last column: a 1 x 1 object's center at width - 0.5 is still on the map.
    [Fact]
    public void Problem_CenterOnTheLastCell_IsNull()
    {
        SelectChestOne();

        Editing.XText = (Map.Map.Layout.Width - 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

        Assert.Null(Editing.Problem);
    }

    // The chest typed onto the last column, then made 2 wide: its center now falls past the map's right edge.
    [Fact]
    public void Problem_ObjectWidenedInTheCatalog_IsCheckedAgainAndRaised()
    {
        var draft = new CatalogDraft(_catalog);
        var placing = new ObjectPlacingViewModel(draft, new LayerChoiceViewModel(), new OpenMaps()) { OpenMap = _placing.OpenMap };
        placing.SelectObjectAt(new Point2D(2.75, 4));
        placing.Editing.XText = (Map.Map.Layout.Width - 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        var changed = new List<string?>();
        placing.Editing.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        draft.SetObject(_catalog.Objects["chest"] with { Width = 2 });

        Assert.Equal("The object's center must stay on the map.", placing.Editing.Problem);
        Assert.Contains(nameof(PlacedObjectEditing.Problem), changed);
    }

    [Fact]
    public void Problem_NegativeLayer_IsNull()
    {
        SelectChestOne();

        Editing.ZText = "-1";

        Assert.Null(Editing.Problem);
    }

    [Fact]
    public void ApplyCommand_ProblemTyped_CannotExecuteAndIsRaised()
    {
        SelectChestOne();
        var raised = 0;
        Editing.ApplyCommand.CanExecuteChanged += (_, _) => raised++;

        Editing.ZText = "x";

        Assert.False(Editing.ApplyCommand.CanExecute(null));
        Assert.True(raised > 0);
    }

    [Fact]
    public void MapUndo_WithThePanelShowingIt_ShowsTheRestoredPosition()
    {
        SelectChestOne();
        _placing.MoveObject("chest_1", new Point2D(8, 8));

        Map.UndoCommand.Execute(null);

        Assert.Equal(("2.25", "3.5"), (Editing.XText, Editing.YText));
    }

    [Fact]
    public void MapUndo_RemovingTheObject_EmptiesThePanel()
    {
        _placing.Browser.SelectedObject = _catalog.Objects["chest"];
        _placing.PlaceObjectAt(new Point2D(10.5, 10.5));
        _placing.SelectObjectAt(new Point2D(10.5, 10.5));
        Assert.True(Editing.HasObject);

        Map.UndoCommand.Execute(null);

        Assert.False(Editing.HasObject);
        Assert.Equal("", Editing.Uid);
    }

    private void SelectChestOne() => _placing.SelectObjectAt(new Point2D(2.75, 4));
}
