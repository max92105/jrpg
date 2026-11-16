using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// One sprite image's setup as the Character sprites window edits it (ADR 0043): the cut typed as columns and rows with
/// the frame size they give shown read-only (the tile Cut panel's rule, <see cref="GridCut"/>), the walk, the draw
/// scale and the feet line; what stops OK, as the loader would say it; the record it gives and its preview's frame.
/// </summary>
internal sealed partial class FieldSpriteSetupViewModel : ObservableObject
{
    /// <summary>A new setup's frame time (placeholder).</summary>
    public const int NewFrameMilliseconds = 150;

    private readonly CatalogDraft _catalog;
    private readonly (int Width, int Height) _imageSize;

    private FieldSpriteSetupViewModel(string file, (int Width, int Height) imageSize, CatalogDraft catalog)
    {
        File = file;
        _imageSize = imageSize;
        _catalog = catalog;
        Walk.PropertyChanged += OnWalkChanged;
    }

    /// <summary>The image's file name in the field-sprite art folder, the record's key.</summary>
    public string File { get; }

    public FacingCycleSetup Walk { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FrameWidth), nameof(Problem))]
    public partial int Columns { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FrameHeight), nameof(Problem), nameof(RowChoices))]
    public partial int Rows { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial int Scale { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial int FeetPixels { get; set; }

    /// <summary>In image pixels; rounded down when the width doesn't split evenly.</summary>
    public int FrameWidth => GridCut.Plain.LengthAlong(_imageSize.Width, Columns);

    /// <summary>In image pixels; rounded down when the height doesn't split evenly.</summary>
    public int FrameHeight => GridCut.Plain.LengthAlong(_imageSize.Height, Rows);

    /// <summary>
    /// The rows a facing may walk on as the boxes list them, numbered from 1 (decision 568); a box's index is the row.
    /// </summary>
    public IReadOnlyList<int> RowChoices => [.. Enumerable.Range(1, Math.Max(0, Rows))];

    /// <summary>
    /// Why the setup can't be applied, or null: the cut first, then the walk against it, then what the loader would
    /// refuse in the records with this one in place (<see cref="CatalogProblems.OfFieldSprites"/>).
    /// </summary>
    public string? Problem
    {
        get
        {
            if (Columns < 1 || Rows < 1)
            {
                return "Columns and rows must be at least 1.";
            }

            return GridCut.Plain.ProblemAlong(_imageSize.Width, Columns, "columns")
                ?? GridCut.Plain.ProblemAlong(_imageSize.Height, Rows, "rows")
                ?? Walk.ProblemWithin(Columns, Rows)
                ?? CatalogProblems.OfFieldSprites(RecordsWithThis(), _catalog.Current);
        }
    }

    /// <summary>The setup of an image that has a record, starting from it.</summary>
    /// <param name="sheet">The record edited.</param>
    /// <param name="imageSize">The image's size in its own pixels.</param>
    /// <param name="catalog">The records this one is checked among.</param>
    public static FieldSpriteSetupViewModel From(FieldSpriteSheet sheet, (int Width, int Height) imageSize, CatalogDraft catalog)
    {
        var setup = new FieldSpriteSetupViewModel(sheet.File, imageSize, catalog)
        {
            Columns = imageSize.Width / sheet.FrameWidth,
            Rows = imageSize.Height / sheet.FrameHeight,
            Scale = sheet.Scale,
            FeetPixels = sheet.FeetPixels,
        };
        setup.Walk.CopyFrom(sheet.Walk);
        return setup;
    }

    /// <summary>
    /// The setup of an image without a record: the cut and the scale guessed (<see cref="FrameCutGuess"/>,
    /// <see cref="SpriteScaleGuess"/>), the walk on rows down, left, right, up as the player's sheet (the plan's
    /// tunables), feet on the frame's bottom.
    /// </summary>
    public static FieldSpriteSetupViewModel ForNew(string file, (int Width, int Height) imageSize, CatalogDraft catalog)
    {
        var setup = new FieldSpriteSetupViewModel(file, imageSize, catalog)
        {
            Columns = FrameCutGuess.Columns,
            Rows = FrameCutGuess.RowsFor(imageSize.Width, imageSize.Height),
            Scale = SpriteScaleGuess.For(catalog.Current.Tilesheets.Values),
        };
        setup.Walk.CopyFrom(new FacingCycle
        {
            Rows = new FacingRows { Down = 0, Left = 1, Right = 2, Up = 3 },
            Frames = new TileAnimation { FrameCount = FrameCutGuess.Columns, FrameMilliseconds = NewFrameMilliseconds },
        });
        return setup;
    }

    /// <exception cref="InvalidOperationException">
    /// The setup has a <see cref="Problem"/>; its text is the message.
    /// </exception>
    public FieldSpriteSheet ToSheet() => Problem is { } problem ? throw new InvalidOperationException(problem) : Record();

    /// <summary>
    /// Adds the cell under a point of the image to the picking facing's sequence (decision 772): nothing without a
    /// picking facing, outside the whole frames (an image that doesn't split evenly leaves an edge that isn't one) or
    /// while the cut gives no frame. A command, never a throw.
    /// </summary>
    /// <param name="x">The point's distance from the image's left edge, in image pixels.</param>
    /// <param name="y">The point's distance from the image's top edge, in image pixels.</param>
    public void PickCellAt(double x, double y)
    {
        if (Walk.PickingFacing is not { } picking || FrameWidth < 1 || FrameHeight < 1 || x < 0 || y < 0)
        {
            return;
        }

        var column = Math.Floor(x / FrameWidth);
        var row = Math.Floor(y / FrameHeight);
        if (column < Columns && row < Rows)
        {
            picking.AddCell(new SpriteCell((int)column, (int)row));
        }
    }

    /// <summary>
    /// The frame the preview shows for a facing after walking this long; null while there is a problem.
    /// </summary>
    public SpriteFrame? PreviewFrameAt(Facing facing, TimeSpan elapsed) =>
        Problem is null ? Record().WalkFrameAt(facing, elapsed) : null;

    private FieldSpriteSheet Record() => new()
    {
        File = File,
        FrameWidth = FrameWidth,
        FrameHeight = FrameHeight,
        Scale = Scale,
        FeetPixels = FeetPixels,
        Walk = Walk.ToCycle(),
    };

    // The catalog's records with this image's in place of its own, as the save would check them.
    private List<FieldSpriteSheet> RecordsWithThis() =>
        [.. _catalog.Current.FieldSprites.Values.Where(sheet => sheet.File != File), Record()];

    // Every input inside the walk feeds the problem and the preview too (the editor rule on derived notes). The picking
    // facing and the frame count's flag change nothing in the record, so they leave the problem line be.
    private void OnWalkChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(FacingCycleSetup.PickingFacing) or nameof(FacingCycleSetup.IsAnyFacingOnRow)))
        {
            OnPropertyChanged(nameof(Problem));
        }
    }
}
