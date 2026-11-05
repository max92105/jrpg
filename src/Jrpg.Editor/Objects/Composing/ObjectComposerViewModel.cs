using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Viewport;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>
/// The composer window's state (decision 455): the painting, the canvas's zoom, the naming of a new object or the
/// edited original, what stops Save, and the object Save makes.
/// </summary>
internal sealed partial class ObjectComposerViewModel : ObservableObject
{
    private ObjectComposerViewModel(CanvasPainting painting, ObjectNaming naming, ObjectDefinition? original)
    {
        (Painting, Naming, Original) = (painting, naming, original);
        painting.PropertyChanged += OnPartChanged;
        naming.PropertyChanged += OnPartChanged;
    }

    public CanvasPainting Painting { get; }

    public ObjectNaming Naming { get; }

    /// <summary>The canvas's own zoom, at the map's cell size each time the composer opens (decision 484).</summary>
    public CellZoom CanvasZoom { get; } = new((int)MapDrawing.CellPixels);

    /// <summary>The object being edited; null for a new one.</summary>
    public ObjectDefinition? Original { get; }

    /// <summary>A new object's id is typed; an edited one keeps its own (decision 449).</summary>
    public bool IsIdEditable => Original is null;

    public string Title => Original is null ? "New object" : $"Edit object '{Original.Id}'";

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
    /// The Shapes window follows Save for a new object, and for an edited one whose size changed, its outline and zones
    /// stretched with it (decision 462).
    /// </summary>
    public bool ShouldOpenShapesAfterSave =>
        Original is null || (Painting.Canvas.Width, Painting.Canvas.Height) != (Original.Width, Original.Height);

    public static ObjectComposerViewModel ForNew(CanvasPainting painting, ObjectNaming naming) => new(painting, naming, null);

    /// <param name="painting">The painting, started from the original's picture.</param>
    /// <param name="original">The object edited.</param>
    /// <param name="categories">The categories objects use today.</param>
    public static ObjectComposerViewModel ForEdit(CanvasPainting painting, ObjectDefinition original, IReadOnlyList<string> categories) =>
        new(painting, new ObjectNaming([], categories) { Id = original.Id, Category = original.Category }, original);

    /// <summary>
    /// A new object with no outline and no zones, which the Shapes window draws next; or the original with its new
    /// size, tiles and category, keeping its id, outline and zones, so every placement and event naming it is
    /// untouched.
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
        return Original is null
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
            : Original with { Width = canvas.Width, Height = canvas.Height, Tiles = canvas.TilesOf(tileIdOf), Category = category };
    }

    private void OnPartChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CanvasPainting.Canvas) or nameof(CanvasPainting.SizeProblem) or nameof(ObjectNaming.IdProblem) or nameof(ObjectNaming.CategoryProblem))
        {
            OnPropertyChanged(nameof(Problem));
        }
    }
}
