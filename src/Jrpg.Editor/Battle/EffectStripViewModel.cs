using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Battle;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Battle;

/// <summary>
/// One effect strip as the Effect strips window edits it (decision 659): its file fixed, its frame time and hit frame
/// kept as typed, its swing chosen or left empty for none. It is listed (written to <c>effect-strips.json</c>) while
/// its boxes differ from the unlisted pace (ADR 0048), or still hold the record it was loaded from until Reset to
/// unlisted. <see cref="Problem"/> is a box that isn't a whole number, then a hit frame past the image's frames; the
/// ranges are Data's rules, shown by the window.
/// </summary>
internal sealed partial class EffectStripViewModel : ObservableObject
{
    private static readonly int _unlistedMilliseconds = (int)EffectStripDefinition.UnlistedFrameTime.TotalMilliseconds;

    // A record already at the unlisted pace (Data takes one) is kept while untouched, so an OK changes nothing.
    private EffectStripDefinition? _loaded;

    private EffectStripViewModel(string file, (int Width, int Height)? imageSize)
    {
        File = file;
        FrameCount = imageSize is { Height: > 0 } size ? size.Width / size.Height : null;
        FrameMillisecondsText = TypedNumber.Text(_unlistedMilliseconds);
        HitFrameText = TypedNumber.Text(0);
        SwingFile = "";
    }

    /// <summary>The image it describes, in the effects folder.</summary>
    public string File { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LengthText), nameof(IsListed), nameof(Problem))]
    public partial string FrameMillisecondsText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LengthText), nameof(IsListed), nameof(Problem))]
    public partial string HitFrameText { get; set; }

    /// <summary>The swing an Attack plays first, by file in the effects folder; empty for none.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsListed))]
    public partial string SwingFile { get; set; }

    /// <summary>The image's width over its height; null while the image is missing.</summary>
    public int? FrameCount { get; }

    /// <summary>"12 frames, 0.96 s; hit at 0.16 s", or what keeps it from being worked out.</summary>
    public string LengthText
    {
        get
        {
            if (FrameCount is not { } frames)
            {
                return "image missing";
            }

            if (TypedNumber.Parse(FrameMillisecondsText) is not { } frameTime || TypedNumber.Parse(HitFrameText) is not { } hit)
            {
                return $"{frames} frames";
            }

            return $"{frames} frames, {Seconds((long)frames * frameTime)} s; hit at {Seconds((long)hit * frameTime)} s";
        }
    }

    /// <summary>
    /// Whether its boxes differ from the unlisted pace (80 ms, hit frame 0, no swing), or hold the record it was loaded
    /// from, so it needs a record.
    /// </summary>
    public bool IsListed =>
        TypedNumber.Parse(FrameMillisecondsText) != _unlistedMilliseconds
        || TypedNumber.Parse(HitFrameText) != 0
        || SwingFile.Trim().Length > 0
        || HoldsTheLoadedRecord();

    public string? Problem =>
        TypedNumber.ProblemOf("Frame time", FrameMillisecondsText)
        ?? TypedNumber.ProblemOf("Hit frame", HitFrameText)
        ?? HitFrameProblem();

    /// <param name="strip">Its record in <c>effect-strips.json</c>.</param>
    /// <param name="imageSizeOf">An effects image's width and height by file; null when the folder lacks it.</param>
    public static EffectStripViewModel From(EffectStripDefinition strip, Func<string, (int Width, int Height)?> imageSizeOf) =>
        new(strip.File, imageSizeOf(strip.File))
        {
            FrameMillisecondsText = TypedNumber.Text(strip.FrameMilliseconds),
            HitFrameText = TypedNumber.Text(strip.HitFrame),
            SwingFile = strip.SwingFile ?? "",
            _loaded = strip,
        };

    /// <summary>An effects image no record lists: the unlisted pace's boxes.</summary>
    public static EffectStripViewModel Unlisted(string file, Func<string, (int Width, int Height)?> imageSizeOf) =>
        new(file, imageSizeOf(file));

    /// <summary>Puts the unlisted pace's boxes back and forgets its loaded record, so it needs no record.</summary>
    public void ResetToUnlisted()
    {
        _loaded = null;
        FrameMillisecondsText = TypedNumber.Text(_unlistedMilliseconds);
        HitFrameText = TypedNumber.Text(0);
        SwingFile = "";
        OnPropertyChanged(nameof(IsListed));
    }

    /// <summary>The record as typed, the swing trimmed and none when empty.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public EffectStripDefinition ToStrip() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new EffectStripDefinition
            {
                File = File,
                FrameMilliseconds = TypedNumber.Parse(FrameMillisecondsText)!.Value,
                HitFrame = TypedNumber.Parse(HitFrameText)!.Value,
                SwingFile = SwingFile.Trim() is { Length: > 0 } swing ? swing : null,
            };

    private bool HoldsTheLoadedRecord() =>
        _loaded is { } loaded
        && TypedNumber.Parse(FrameMillisecondsText) == loaded.FrameMilliseconds
        && TypedNumber.Parse(HitFrameText) == loaded.HitFrame
        && SwingFile.Trim() == (loaded.SwingFile ?? "");

    private string? HitFrameProblem() =>
        FrameCount is { } frames && TypedNumber.Parse(HitFrameText) is { } hit && hit >= frames
            ? $"Hit frame {hit} is past the image's {frames} frames (from 0)."
            : null;

    // Long, as a typed frame time has no top: a whole strip's length can pass an int's milliseconds.
    private static string Seconds(long milliseconds) => (milliseconds / 1000.0).ToString("0.00", CultureInfo.InvariantCulture);
}
