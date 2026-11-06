using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Viewport;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>
/// The composer window's state (decision 455): the painting, the canvas's zoom, the naming of a new object or the
/// edited original, what stops Save, and the object Save makes, new, a duplicate or the original.
/// </summary>
internal sealed partial class ObjectComposerViewModel : ObservableObject
{
    private ObjectComposerViewModel(CanvasPainting painting, ObjectNaming naming, ObjectDefinition? original, ObjectDefinition? shapeSource)
    {
        (Painting, Naming, Original, ShapeSource) = (painting, naming, original, shapeSource);
        painting.PropertyChanged += OnPartChanged;
        naming.PropertyChanged += OnPartChanged;
    }

    public CanvasPainting Painting { get; }

    public ObjectNaming Naming { get; }

    /// <summary>The canvas's own zoom, at the map's cell size each time the composer opens (decision 484).</summary>
    public CellZoom CanvasZoom { get; } = new((int)MapDrawing.CellPixels);

    /// <summary>The object being edited; null for a new one, a duplicate included.</summary>
    public ObjectDefinition? Original { get; }

    /// <summary>
    /// The object whose outline and zones the saved one takes: the edited original, or the one duplicated (decision
    /// 517); null for a new object, which the Shapes window draws next.
    /// </summary>
    public ObjectDefinition? ShapeSource { get; }

    /// <summary>A new object's id is typed, a duplicate's too; an edited one keeps its own (decision 449).</summary>
    public bool IsIdEditable => Original is null;

    public string Title => (Original, ShapeSource) switch
    {
        ({ } original, _) => $"Edit object '{original.Id}'",
        (null, { } duplicated) => $"Duplicate of '{duplicated.Id}'",
        _ => "New object",
    };

    /// <summary>
    /// What stops Save, the first of the size, the id while new (an edited object keeps its own, even one hand-made
    /// against <see cref="ObjectIds"/>), the category and an empty canvas; null when nothing.
    /// </summary>
    public string? Problem =>
        Painting.SizeProblem
        ?? (IsIdEditable ? Naming.IdProblem : null)
        ?? Naming.CategoryProblem
        ?? (Painting.Canvas.IsBlank ? "Paint at least one cell." : null);

    /// <summary>
    /// The Shapes window follows Save for a new object, and for an edited or duplicated one whose size differs from
    /// the source of its shapes, its outline and zones stretched with it (decision 462).
    /// </summary>
    public bool ShouldOpenShapesAfterSave =>
        ShapeSource is null || (Painting.Canvas.Width, Painting.Canvas.Height) != (ShapeSource.Width, ShapeSource.Height);

    public static ObjectComposerViewModel ForNew(CanvasPainting painting, ObjectNaming naming) => new(painting, naming, null, null);

    /// <summary>A new object on the canvas a map selection made (decision 514), with its drawing warning.</summary>
    public static ObjectComposerViewModel ForNewFromMap(CanvasPainting painting, ObjectNaming naming, string? mapLayerWarning) =>
        new(painting, naming, null, null) { MapLayerWarning = mapLayerWarning, CanReplaceMapCells = true, IsReplacingMapCells = true };

    /// <summary>A new object copied from another (decision 517): its id typed, its category the same.</summary>
    /// <param name="painting">The painting, started from the duplicated object's picture.</param>
    /// <param name="duplicated">The object copied, whose outline and zones the new one takes.</param>
    /// <param name="naming">A new object's naming, with the duplicated object's category.</param>
    public static ObjectComposerViewModel ForDuplicate(CanvasPainting painting, ObjectDefinition duplicated, ObjectNaming naming) =>
        new(painting, naming, null, duplicated);

    /// <summary>
    /// How an object made from map cells will draw differently from them (decision 516); null otherwise, and for New
    /// object and Edit object.
    /// </summary>
    public string? MapLayerWarning { get; private init; }

    /// <summary>
    /// Whether the composer offers to put the object in place of the map cells it was made from (decision 515).
    /// </summary>
    public bool CanReplaceMapCells { get; private init; }

    /// <summary>Whether Save also replaces the map cells with the object; ticked at the start when it can.</summary>
    [ObservableProperty]
    public partial bool IsReplacingMapCells { get; set; }

    /// <summary>Whether the composer shows <see cref="MapLayerWarning"/>; its line takes no room otherwise.</summary>
    public bool HasMapLayerWarning => MapLayerWarning is not null;

    /// <param name="painting">The painting, started from the original's picture.</param>
    /// <param name="original">The object edited.</param>
    /// <param name="categories">The categories objects use today.</param>
    public static ObjectComposerViewModel ForEdit(CanvasPainting painting, ObjectDefinition original, IReadOnlyList<string> categories) =>
        new(painting, new ObjectNaming([], categories) { Id = original.Id, Category = original.Category }, original, original);

    /// <summary>
    /// A new object with no outline and no zones, which the Shapes window draws next; a duplicate with the copied
    /// object's outline and zones under its typed id; or the original with its new size, tiles and category, keeping
    /// its id, outline and zones, so every placement and event naming it is untouched.
    /// </summary>
    /// <param name="tileIdOf">The tile cut from each painted sheet cell; every one is a tile by now.</param>
    /// <exception cref="InvalidOperationException"><see cref="Problem"/> isn't null.</exception>
    public ObjectDefinition ToObject(Func<SheetCell, int> tileIdOf)
    {
        if (Problem is { } problem)
        {
            throw new InvalidOperationException(problem);
        }

        var canvas = Painting.Canvas;
        var category = Naming.Category.Trim();
        // An edited object's naming holds its own id (ForEdit), so the typed id serves every case.
        return ShapeSource is null
            ? new ObjectDefinition
            {
                Id = Naming.Id,
                Category = category,
                Width = canvas.Width,
                Height = canvas.Height,
                Tiles = canvas.TilesOf(tileIdOf),
                CollisionPolygon = [],
                AbovePlayerZones = [],
            }
            : ShapeSource with { Id = Naming.Id, Width = canvas.Width, Height = canvas.Height, Tiles = canvas.TilesOf(tileIdOf), Category = category };
    }

    private void OnPartChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CanvasPainting.Canvas) or nameof(CanvasPainting.SizeProblem) or nameof(ObjectNaming.IdProblem) or nameof(ObjectNaming.CategoryProblem))
        {
            OnPropertyChanged(nameof(Problem));
        }
    }
}
