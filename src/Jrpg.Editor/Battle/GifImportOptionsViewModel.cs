using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The GIF import's dialog as typed (decisions 637, 638, 987): the battle height each frame is scaled to, with a note
/// on whether that keeps pixel art crisp, whether the top-left color is a background to clear, and the frame time the
/// first chosen GIF plays at, typed only where the import keeps it. Built by one factory per place that imports GIFs;
/// the Battle backgrounds page's kind chooses a sheet's frame height instead of typing one (decision 903).
/// </summary>
internal sealed partial class GifImportOptionsViewModel : ObservableObject
{
    // Under 16 px a monster is a speck; past 1024 its strip of a few frames passes the widest image the game loads.
    private const int LowestHeight = 16;
    private const int HighestHeight = 1024;

    // Data refuses a loop under 1 ms a frame.
    private const int LowestFrameMilliseconds = 1;

    // The box's pace when the first GIF can't be read, as a GIF's unusable delay plays (decision 632); the import then
    // refuses that GIF, so this pace only fills the box.
    private const int UnreadFrameMilliseconds = 100;

    private GifImportOptionsViewModel(ComposedGif? gif)
    {
        Gif = gif;
        HeightText = TypedNumber.Text(GifImportOptions.Default.FrameHeight);
        RemovesBackground = GifImportOptions.Default.RemovesBackground;
        FrameMillisecondsText = TypedNumber.Text(gif?.FrameMilliseconds ?? UnreadFrameMilliseconds);
    }

    /// <summary>The first chosen GIF, which the dialog plays; null when it can't be read.</summary>
    public ComposedGif? Gif { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(CanApply), nameof(FactorNote))]
    public partial string HeightText { get; set; }

    [ObservableProperty]
    public partial bool RemovesBackground { get; set; }

    /// <summary>Filled with the GIF's most common delay; editable only while <see cref="IsPaceTyped"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(CanApply), nameof(FrameMilliseconds))]
    public partial string FrameMillisecondsText { get; set; }

    /// <summary>
    /// Whether the typed pace is the imported loop's: a battle sprite's, or an effect strip's on its own page, one GIF
    /// at a time.
    /// </summary>
    public bool IsPaceTyped { get; private init; }

    /// <summary>Why the pace box is greyed; empty while it is typed.</summary>
    public string PaceNote { get; private init; } = "";

    /// <summary>The pace box as a number from 1, for the preview; null while it isn't one.</summary>
    public int? FrameMilliseconds =>
        TypedNumber.Parse(FrameMillisecondsText) is { } frameTime and >= LowestFrameMilliseconds ? frameTime : null;

    public string? Problem => (ChosenHeight(), IsPaceTyped, FrameMilliseconds) switch
    {
        (null, _, _) => $"Battle height must be a whole number from {LowestHeight} to {HighestHeight}.",
        (_, true, null) => $"Frame time must be a whole number from {LowestFrameMilliseconds}.",
        _ => SheetProblem(),
    };

    public bool CanApply => Problem is null;

    /// <summary>
    /// "2x the GIF's 64 px: crisp", or "softened" off a whole factor; empty without a height to compare.
    /// </summary>
    public string FactorNote =>
        Gif is { Height: var source } && TypedHeight() is { } height
            ? $"{((double)height / source).ToString("0.##", CultureInfo.InvariantCulture)}x the GIF's {source} px: "
                + (FrameScaler.IsWholeFactor(source, height) ? "crisp" : "softened")
            : "";

    /// <summary>
    /// Battle sprites: one GIF's loop takes the typed pace; several each keep their own (decision 987).
    /// </summary>
    /// <param name="first">The first chosen GIF; null when it can't be read.</param>
    /// <param name="gifCount">How many GIFs were chosen, at least one.</param>
    public static GifImportOptionsViewModel ForBattleSprites(ComposedGif? first, int gifCount) =>
        gifCount == 1 ? new(first) { IsPaceTyped = true } : new(first) { PaceNote = "each keeps its own pace" };

    /// <summary>
    /// Attack effects from the Enemies page, whose GIF's pace is dropped (decisions 639, 988): the pace box is greyed.
    /// </summary>
    /// <param name="first">The first chosen GIF; null when it can't be read.</param>
    /// <param name="gifCount">How many GIFs were chosen, at least one.</param>
    public static GifImportOptionsViewModel ForEnemyEffects(ComposedGif? first, int gifCount) =>
        new(first) { PaceNote = gifCount == 1 ? "set its pace in Effect strips" : "set their pace in Effect strips" };

    /// <summary>
    /// Attack effects from the Effect strips page: one GIF's typed pace becomes its row's frame time (decision 988);
    /// several are paced in the page's list afterwards.
    /// </summary>
    /// <param name="first">The first chosen GIF; null when it can't be read.</param>
    /// <param name="gifCount">How many GIFs were chosen, at least one.</param>
    public static GifImportOptionsViewModel ForEffectStrips(ComposedGif? first, int gifCount) =>
        gifCount == 1 ? new(first) { IsPaceTyped = true } : new(first) { PaceNote = "set each one's pace in the list" };

    /// <summary>
    /// The typed pace is carried only while <see cref="IsPaceTyped"/>; the height is the background kind's chosen one,
    /// else the typed one.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>; OK is greyed then.</exception>
    public GifImportOptions ToOptions() =>
        ChosenHeight() is { } height && CanApply
            ? new GifImportOptions(height, RemovesBackground) { FrameMilliseconds = IsPaceTyped ? FrameMilliseconds : null }
            : throw new InvalidOperationException(Problem);

    private int? ChosenHeight() => IsBackground ? SelectedHeight : TypedHeight();

    private int? TypedHeight() =>
        TypedNumber.Parse(HeightText) is { } height and >= LowestHeight and <= HighestHeight ? height : null;
}
