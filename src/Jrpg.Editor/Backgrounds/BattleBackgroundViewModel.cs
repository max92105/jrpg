using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Battle;
using Jrpg.Core.Sprites;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Backgrounds;

/// <summary>
/// One image of the backgrounds folder as the Battle backgrounds page shows it (decisions 901, 902): a still picture
/// while no record describes it, else animated, its frame time kept as typed, the only field the page edits (its frame
/// size and count come from its import). <see cref="Problem"/> is the typed time, then Data's rules for the record and
/// its grid, which the page's problem line shows. Reads no file: the image's size comes in from the list's lookup. A
/// PNG imported over its file makes the list build a still row in its place; a GIF's member comes with BG9's import.
/// </summary>
internal sealed partial class BattleBackgroundViewModel : ObservableObject
{
    private const string FrameTimeProblem = "Frame time must be a whole number of milliseconds from 1.";

    private readonly AnimatedBackgroundDefinition? _record;
    private readonly (int Width, int Height)? _size;

    private BattleBackgroundViewModel(string file, AnimatedBackgroundDefinition? record, (int Width, int Height)? size)
    {
        File = file;
        _record = record;
        _size = size;
        FrameMillisecondsText = record is null ? "" : TypedNumber.Text(record.Animation.FrameMilliseconds);
        PreviewCells = record is { } animated && size is { } image && RecordProblemOf(animated, size) is null
            ? CellsOf(animated, image.Width)
            : [];
    }

    /// <summary>The image it describes, in the backgrounds folder.</summary>
    public string File { get; }

    /// <summary>True while the lookup has no size for it: the image is missing or unreadable (decision 442).</summary>
    public bool IsImageMissing => _size is null;

    /// <summary>True while a record describes it, so it plays as a grid of frames.</summary>
    public bool IsAnimated => _record is not null;

    /// <summary>
    /// An animated one's frame time in milliseconds as typed, the box its preview plays at; empty for a still.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string FrameMillisecondsText { get; set; }

    /// <summary>
    /// "Still, 1920 x 1080 px", "Animated: 4 frames of 960 x 540 px in a 2 x 2 grid", "Image missing"; an animated one
    /// whose image can't hold its grid gives the image's size, as the problem line says why.
    /// </summary>
    public string KindLine => (_size, _record) switch
    {
        (null, _) => "Image missing",
        ({ } image, null) => $"Still, {image.Width} x {image.Height} px",
        (_, { } record) when PreviewCells.Count > 0 => AnimatedLine(record),
        ({ } image, _) => $"Animated, {image.Width} x {image.Height} px",
    };

    /// <summary>An animated one's frame size in its sheet's pixels; null for a still.</summary>
    public (int Width, int Height)? FrameSize => _record is { } record ? (record.FrameWidth, record.FrameHeight) : null;

    /// <summary>
    /// An animated one's cells in play order, each frame's origin over its image's width divided by the frame size;
    /// empty for a still (the page shows the whole image as one frame), and while the record or its grid has a problem,
    /// since a frame's origin is undefined there.
    /// </summary>
    public IReadOnlyList<SpriteCell> PreviewCells { get; }

    /// <summary>Why the record can't be built: the typed time first, then Data's; null for a still.</summary>
    public string? Problem =>
        _record is { } record ? TypedFrameTimeProblem() ?? RecordProblemOf(WithTypedTime(record), _size) : null;

    /// <param name="file">The image's file in the backgrounds folder.</param>
    /// <param name="record">Its record in <c>animated-backgrounds.json</c>; null for a still picture.</param>
    /// <param name="size">The image's width and height; null when the image is missing or unreadable.</param>
    public static BattleBackgroundViewModel From(
        string file, AnimatedBackgroundDefinition? record, (int Width, int Height)? size) => new(file, record, size);

    /// <summary>
    /// The record with the typed frame time, equal by content to the one it came from while the time is unchanged, so
    /// an untouched page records no undo step; null for a still.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public AnimatedBackgroundDefinition? ToRecord() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : _record is { } record ? WithTypedTime(record) : null;

    private string? TypedFrameTimeProblem() => TypedNumber.Parse(FrameMillisecondsText) is >= 1 ? null : FrameTimeProblem;

    private AnimatedBackgroundDefinition WithTypedTime(AnimatedBackgroundDefinition record) =>
        record with
        {
            Animation = record.Animation with { FrameMilliseconds = TypedNumber.Parse(FrameMillisecondsText)!.Value },
        };

    // Data's record rules before its grid's: the grid divides by the frame size, which only the record rules keep from
    // 0 (BG4's review). With no size, the grid is the startup check's to refuse.
    private static string? RecordProblemOf(AnimatedBackgroundDefinition record, (int Width, int Height)? size) =>
        CatalogProblems.OfAnimatedBackgrounds([record])
        ?? (size is { } image ? ArtReferenceCheck.GridProblemOf(record, image) : null);

    // Core's FrameOrigin is the one rule for where a frame sits; a cell is its origin in whole frames.
    private static SpriteCell[] CellsOf(AnimatedBackgroundDefinition record, int imageWidth) =>
    [
        .. Enumerable.Range(0, record.Animation.FrameCount)
            .Select(frame => record.FrameOrigin(frame, imageWidth))
            .Select(origin => new SpriteCell(origin.X / record.FrameWidth, origin.Y / record.FrameHeight)),
    ];

    // The grid the frames fill, read from their cells rather than worked out again beside FrameOrigin.
    private string AnimatedLine(AnimatedBackgroundDefinition record) =>
        $"Animated: {record.Animation.FrameCount} frames of {record.FrameWidth} x {record.FrameHeight} px in a "
        + $"{PreviewCells.Max(cell => cell.Column) + 1} x {PreviewCells.Max(cell => cell.Row) + 1} grid";
}
