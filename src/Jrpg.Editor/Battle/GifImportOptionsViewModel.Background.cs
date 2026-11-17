using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Data.Art;
using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The dialog's background kind, opened from the Battle backgrounds page (decisions 898, 903, 904): one GIF becomes a
/// grid sheet at a frame height chosen among <see cref="HeightChoices"/>, never enlarged and never cleared, at the
/// typed pace; a line names the sheet, its grid and its video memory, and the problem line refuses frames that one
/// sheet can't hold.
/// </summary>
internal sealed partial class GifImportOptionsViewModel
{
    // A sheet's video memory: 4 bytes a pixel, in MB of 1,000,000 bytes.
    private const int BytesPerPixel = 4;
    private const double BytesPerMegabyte = 1_000_000;

    // The 1080 px screen's whole divisors (decision 898), tallest first: a 16:9 frame at one of them is scaled up by a
    // whole factor, so the battle's point sampling stays even.
    private static readonly int[] _screenDivisors = [1080, 540, 360, 270, 216, 180];

    // Averaging a big GIF takes a moment, so each height's frames are worked out once while the dialog is open.
    private readonly Dictionary<int, ComposedGif> _previewByHeight = [];

    /// <summary>True for the Battle backgrounds page's kind, whose height is chosen rather than typed.</summary>
    public bool IsBackground { get; private init; }

    /// <summary>
    /// The background kind's frame heights, tallest first: the GIF's own capped at 1080, then each divisor of 1080
    /// below it; the divisors alone when the GIF can't be read; empty in the other kinds.
    /// </summary>
    public IReadOnlyList<BackgroundHeightChoice> HeightChoices { get; private init; } = [];

    /// <summary>The background kind's frame height in the sheet, one of <see cref="HeightChoices"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(CanApply), nameof(SheetLine), nameof(PreviewAtChosenHeight))]
    public partial int SelectedHeight { get; set; }

    /// <summary>
    /// "24 frames of 480 x 270 px: a 3840 x 810 sheet (8 x 3), 12.4 MB of video memory"; empty while the frames don't
    /// fit one sheet, without a GIF, and in the other kinds.
    /// </summary>
    public string SheetLine
    {
        get
        {
            if (!IsBackground || Gif is not { } gif || !FitsOneSheet(gif, SelectedHeight))
            {
                return "";
            }

            var frameWidth = FrameWidthAt(gif, SelectedHeight);
            var (width, height) = FrameGrid.SheetSize(frameWidth, SelectedHeight, gif.Frames.Count);
            var megabytes = (double)width * height * BytesPerPixel / BytesPerMegabyte;
            return $"{gif.Frames.Count} frames of {frameWidth} x {SelectedHeight} px: a {width} x {height} sheet "
                + $"({width / frameWidth} x {height / SelectedHeight}), "
                + $"{megabytes.ToString("0.0", CultureInfo.InvariantCulture)} MB of video memory";
        }
    }

    /// <summary>
    /// The background kind's preview: the GIF's frames at the chosen height, averaged below its own as the sheet holds
    /// them (decision 904); null without a GIF and in the other kinds, whose preview shows the GIF as read.
    /// </summary>
    public ComposedGif? PreviewAtChosenHeight => IsBackground && Gif is { } gif ? PreviewAt(gif, SelectedHeight) : null;

    /// <summary>
    /// A battle background from the Battle backgrounds page, one GIF at a time, its pace typed (decision 903). The
    /// chosen height starts at the GIF's own, capped at 1080, when its sheet holds every frame, else at the tallest
    /// choice that does, else at the GIF's own capped at 1080, the problem line saying why (decision 898).
    /// </summary>
    /// <param name="gif">
    /// The chosen GIF; null when it can't be read, and the import then names it among its refusals.
    /// </param>
    public static GifImportOptionsViewModel ForBackground(ComposedGif? gif)
    {
        var heights = HeightsFor(gif);
        return new(gif)
        {
            IsBackground = true,
            IsPaceTyped = true,
            HeightChoices = [.. heights.Select(height => new BackgroundHeightChoice(height, LabelOf(height, gif)))],
            SelectedHeight = gif is null ? heights[0] : heights.FirstOrDefault(height => FitsOneSheet(gif, height), heights[0]),
        };
    }

    private static int[] HeightsFor(ComposedGif? gif)
    {
        if (gif is null)
        {
            return _screenDivisors;
        }

        var tallest = Math.Min(gif.Height, _screenDivisors[0]);
        return [tallest, .. _screenDivisors.Where(divisor => divisor < tallest)];
    }

    private string? SheetProblem()
    {
        if (!IsBackground || Gif is not { } gif || FitsOneSheet(gif, SelectedHeight))
        {
            return null;
        }

        var smallest = HeightChoices[^1].Height;
        if (FrameWidthAt(gif, smallest) > TextureLimit.MaxSide)
        {
            return $"Its frames are wider than {TextureLimit.MaxSide} px at every height offered "
                + $"({FrameWidthAt(gif, smallest)} px at {smallest} px): it can't be a battle background.";
        }

        var mostFrames = FrameGrid.MostFrames(FrameWidthAt(gif, SelectedHeight), SelectedHeight);
        return $"{gif.Frames.Count} frames don't fit a {TextureLimit.MaxSide} px sheet at {SelectedHeight} px "
            + $"({mostFrames} at most): choose a smaller height.";
    }

    private ComposedGif PreviewAt(ComposedGif gif, int height)
    {
        if (height == gif.Height)
        {
            return gif;
        }

        if (!_previewByHeight.TryGetValue(height, out var scaled))
        {
            var frames = FrameScaler.ScaleAveraged(gif.Frames, gif.Width, gif.Height, height);
            scaled = gif with { Frames = frames, Width = FrameWidthAt(gif, height), Height = height };
            _previewByHeight[height] = scaled;
        }

        return scaled;
    }

    private static int FrameWidthAt(ComposedGif gif, int height) => FrameScaler.WidthAt(gif.Width, gif.Height, height);

    private static bool FitsOneSheet(ComposedGif gif, int height) =>
        gif.Frames.Count <= FrameGrid.MostFrames(FrameWidthAt(gif, height), height);

    private static string LabelOf(int height, ComposedGif? gif) =>
        height == gif?.Height ? $"{height} px (the GIF's)" : $"{height} px";
}
