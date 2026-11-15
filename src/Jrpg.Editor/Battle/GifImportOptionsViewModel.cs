using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The GIF import's dialog as typed (decisions 637, 638): the battle height each frame is scaled to, with a note on
/// whether that keeps pixel art crisp, and whether the top-left color is a background to clear.
/// </summary>
internal sealed partial class GifImportOptionsViewModel : ObservableObject
{
    // Under 16 px a monster is a speck; past 1024 its strip of a few frames passes the widest image the game loads.
    private const int LowestHeight = 16;
    private const int HighestHeight = 1024;

    private readonly int? _gifHeight;

    /// <param name="gifHeight">The first GIF's canvas height, for the note; null when it can't be read.</param>
    public GifImportOptionsViewModel(int? gifHeight)
    {
        _gifHeight = gifHeight;
        HeightText = TypedNumber.Text(GifImportOptions.Default.BattleHeight);
        RemovesBackground = GifImportOptions.Default.RemovesBackground;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(CanApply), nameof(FactorNote))]
    public partial string HeightText { get; set; }

    [ObservableProperty]
    public partial bool RemovesBackground { get; set; }

    public string? Problem =>
        TypedHeight() is null ? $"Battle height must be a whole number from {LowestHeight} to {HighestHeight}." : null;

    public bool CanApply => Problem is null;

    /// <summary>
    /// "2x the GIF's 64 px: crisp", or "softened" off a whole factor; empty without a height to compare.
    /// </summary>
    public string FactorNote =>
        _gifHeight is { } source && TypedHeight() is { } height
            ? $"{((double)height / source).ToString("0.##", CultureInfo.InvariantCulture)}x the GIF's {source} px: "
                + (FrameScaler.IsWholeFactor(source, height) ? "crisp" : "softened")
            : "";

    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>; OK is greyed then.</exception>
    public GifImportOptions ToOptions() =>
        TypedHeight() is { } height
            ? new GifImportOptions(height, RemovesBackground)
            : throw new InvalidOperationException(Problem);

    private int? TypedHeight() =>
        TypedNumber.Parse(HeightText) is { } height and >= LowestHeight and <= HighestHeight ? height : null;
}
