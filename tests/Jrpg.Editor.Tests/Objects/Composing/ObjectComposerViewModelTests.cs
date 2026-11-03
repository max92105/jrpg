using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;
using Jrpg.Editor.Objects.Composing;
using Jrpg.Editor.Tests.Shell;

namespace Jrpg.Editor.Tests.Objects.Composing;

public sealed class ObjectComposerViewModelTests : IDisposable
{
    // A 2 x 2 chest with an outline and a zone; each sheet cell's tile id below is 10 x column + row.
    private static readonly ObjectDefinition _chest = new()
    {
        Id = "chest",
        Category = "Chests",
        Width = 2,
        Height = 2,
        Tiles = [new ObjectTile(0, 0, 1)],
        CollisionPolygon = [new Point2D(0, 0.5), new Point2D(1, 0.5), new Point2D(1, 1)],
        AbovePlayerZones = [[new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 0.5)]],
    };

    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Problem_NothingPainted_AsksForACell()
    {
        var composer = ObjectComposerViewModel.ForNew(Painting(ObjectCanvas.Empty(3, 3)), Naming("tree_4", "Trees"));

        Assert.Equal("Paint at least one cell.", composer.Problem);
        Assert.Equal("New object", composer.Title);
    }

    [Fact]
    public void Problem_NewWithATakenId_IsTheIdProblem()
    {
        var composer = ObjectComposerViewModel.ForNew(Painting(Painted(2, 2)), Naming("chest", "Trees"));

        Assert.Equal("The id chest is taken.", composer.Problem);
    }

    [Fact]
    public void Problem_EditOfAnIdThePatternRefuses_IsNull()
    {
        var composer = ObjectComposerViewModel.ForEdit(Painting(Painted(2, 2)), _chest with { Id = "Old-Chest" }, ["Chests"]);

        Assert.Null(composer.Problem);
        Assert.False(composer.IsIdEditable);
        Assert.Equal("Edit object 'Old-Chest'", composer.Title);
    }

    [Fact]
    public void ToObject_New_HasTheTypedIdTrimmedCategoryNoOutlineNoZones()
    {
        var composer = ObjectComposerViewModel.ForNew(Painting(Painted(2, 1)), Naming("tree_4", " Trees "));

        var made = composer.ToObject(TileIdOf);

        Assert.Equal(("tree_4", "Trees", 2, 1), (made.Id, made.Category, made.Width, made.Height));
        Assert.Equal([new ObjectTile(0, 0, 0), new ObjectTile(1, 0, 10)], made.Tiles);
        Assert.Empty(made.CollisionPolygon);
        Assert.Empty(made.AbovePlayerZones);
    }

    [Fact]
    public void ToObject_EditOtherSize_KeepsIdOutlineAndZonesTakesSizeTilesAndCategory()
    {
        var composer = ObjectComposerViewModel.ForEdit(Painting(Painted(3, 1)), _chest, ["Chests"]);
        composer.Naming.Category = "Boxes";

        var made = composer.ToObject(TileIdOf);

        Assert.Equal(("chest", "Boxes", 3, 1), (made.Id, made.Category, made.Width, made.Height));
        Assert.Equal([new ObjectTile(0, 0, 0), new ObjectTile(1, 0, 10), new ObjectTile(2, 0, 20)], made.Tiles);
        Assert.Same(_chest.CollisionPolygon, made.CollisionPolygon);
        Assert.Same(_chest.AbovePlayerZones, made.AbovePlayerZones);
    }

    [Fact]
    public void ToObject_WithAProblem_Throws()
    {
        var composer = ObjectComposerViewModel.ForNew(Painting(ObjectCanvas.Empty(3, 3)), Naming("tree_4", "Trees"));

        Assert.Throws<InvalidOperationException>(() => composer.ToObject(TileIdOf));
    }

    [Fact]
    public void ShouldOpenShapesAfterSave_EditSameSize_IsFalse() =>
        Assert.False(ObjectComposerViewModel.ForEdit(Painting(Painted(2, 2)), _chest, []).ShouldOpenShapesAfterSave);

    [Fact]
    public void ShouldOpenShapesAfterSave_EditOtherSize_IsTrue() =>
        Assert.True(ObjectComposerViewModel.ForEdit(Painting(Painted(2, 3)), _chest, []).ShouldOpenShapesAfterSave);

    [Fact]
    public void ShouldOpenShapesAfterSave_New_IsTrue() =>
        Assert.True(ObjectComposerViewModel.ForNew(Painting(Painted(2, 2)), Naming("tree_4", "Trees")).ShouldOpenShapesAfterSave);

    [Fact]
    public void Problem_FirstStrokeOnABlankCanvas_IsRaisedAndCleared()
    {
        var composer = ObjectComposerViewModel.ForNew(Painting(ObjectCanvas.Empty(2, 2)), Naming("tree_4", "Trees"));
        var raised = RaisedBy(composer);
        var sheet = composer.Painting.Picker.SelectedSheet!;
        composer.Painting.Picker.SelectCellAt(sheet.CellLeft(0), sheet.CellTop(0));

        composer.Painting.BeginStroke();
        composer.Painting.PaintAt(new GridCell(0, 0));
        composer.Painting.EndStroke();

        Assert.Contains(nameof(ObjectComposerViewModel.Problem), raised);
        Assert.Null(composer.Problem);
    }

    [Fact]
    public void Problem_SizeTypo_IsRaised()
    {
        var composer = ObjectComposerViewModel.ForNew(Painting(Painted(2, 2)), Naming("tree_4", "Trees"));
        var raised = RaisedBy(composer);

        composer.Painting.ColumnsText = "x";

        Assert.Contains(nameof(ObjectComposerViewModel.Problem), raised);
        Assert.Equal("Columns must be a whole number from 1 to 32.", composer.Problem);
    }

    [Fact]
    public void Problem_CategoryCleared_IsRaised()
    {
        var composer = ObjectComposerViewModel.ForNew(Painting(Painted(2, 2)), Naming("tree_4", "Trees"));
        var raised = RaisedBy(composer);

        composer.Naming.Category = "";

        Assert.Contains(nameof(ObjectComposerViewModel.Problem), raised);
        Assert.Equal("The category must not be empty.", composer.Problem);
    }

    [Fact]
    public void Problem_IdTypedAfterAProblem_IsRaised()
    {
        var composer = ObjectComposerViewModel.ForNew(Painting(Painted(2, 2)), Naming("chest", "Trees"));
        var raised = new List<string?>();
        composer.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        composer.Naming.Id = "tree_4";

        Assert.Contains(nameof(ObjectComposerViewModel.Problem), raised);
        Assert.Null(composer.Problem);
    }

    private static List<string?> RaisedBy(ObjectComposerViewModel composer)
    {
        var raised = new List<string?>();
        composer.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        return raised;
    }

    private static int TileIdOf(SheetCell cell) => (10 * cell.Column) + cell.Row;

    private static ObjectCanvas Painted(int columns, int rows) => ObjectCanvas.Of(new SheetCellRectangle("forest", 0, 0, columns, rows));

    private static ObjectNaming Naming(string id, string category) => new(["chest"], []) { Id = id, Category = category };

    private CanvasPainting Painting(ObjectCanvas start) => new(start, _content.Open(_content.TestWorld).Painting!.Browser);
}
