using System.Globalization;
using Jrpg.Core.Cinematics;
using Jrpg.Data.Content;

namespace Jrpg.Data.Cinematics;

/// <summary>
/// The rules for <c>cinematics.json</c> (ADR 0046): at least one still and no null still; each still names a picture,
/// shows for a finite time above 0, has subtitles that each show for a finite time above 0 and together no longer than
/// the still, and has a voice, zoom and focus that are absent or well formed; the stills together last a time the game
/// can count; the track is absent or named. A problem names its cinematic and, when it is a still's, that still too.
/// </summary>
internal static class CinematicCatalogValidator
{
    // A TimeSpan counts up to about 29,000 years; the game sums the stills' times into one.
    private static readonly double _longestTotalSeconds = TimeSpan.MaxValue.TotalSeconds;

    // Tenths typed in the editor don't sum exactly (0.1 + 0.2), so the subtitles may pass the still by a rounding.
    private const double CaptionSecondsTolerance = 1e-6;

    public static void Validate(string fileName, IReadOnlyList<CinematicDefinition> cinematics) =>
        new ContentListValidator<CinematicDefinition>(fileName, "cinematic", cinematic => cinematic.Id)
            .Validate(cinematics, FindProblem);

    private static string? FindProblem(CinematicDefinition cinematic)
    {
        if (cinematic.MusicFile is { } music && string.IsNullOrWhiteSpace(music))
        {
            return "musicFile must name a track or be left out.";
        }

        if (cinematic.Stills is not { Count: > 0 })
        {
            return "stills must list at least one still.";
        }

        for (var index = 0; index < cinematic.Stills.Count; index++)
        {
            if (FindStillProblem(cinematic.Stills[index]) is { } problem)
            {
                return $"stills[{index}]: {problem}";
            }
        }

        return cinematic.Stills.Sum(still => still.Seconds) < _longestTotalSeconds
            ? null
            : "the stills' seconds together must be a time the game can count.";
    }

    // The serializer doesn't apply nullable annotations to list elements, so a stray null gets here.
    private static string? FindStillProblem(CinematicStill? still) =>
        still is null ? "is null."
        : string.IsNullOrWhiteSpace(still.ImageFile) ? "imageFile must name a picture."
        : !double.IsFinite(still.Seconds) || still.Seconds <= 0 ? DescribeSecondsProblem(still.Seconds)
        : FindOptionalFieldProblem(still);

    // A bad zoom is named before a bad focus; a focus is checked with or without a zoom (decision 750).
    private static string? FindOptionalFieldProblem(CinematicStill still) =>
        FindCaptionsProblem(still) is { } captions ? captions
        : still.VoiceFile is { } voice && string.IsNullOrWhiteSpace(voice) ? "voiceFile must name a voice line or be left out."
        : (still.Zoom is { } zoom ? FindZoomProblem(zoom) : null) ?? (still.Focus is { } focus ? FindFocusProblem(focus) : null);

    // A subtitle's text may be empty, a pause between two lines; the serializer lets a null entry or text through.
    private static string? FindCaptionsProblem(CinematicStill still)
    {
        if (still.Captions is null)
        {
            return "captions must be a list or be left out.";
        }

        var total = 0d;
        for (var index = 0; index < still.Captions.Count; index++)
        {
            if (still.Captions[index] is not { Text: not null } caption)
            {
                return $"captions[{index}] must hold a text, empty for a pause.";
            }

            if (!double.IsFinite(caption.Seconds) || caption.Seconds <= 0)
            {
                return $"captions[{index}]: {DescribeSecondsProblem(caption.Seconds)}";
            }

            total += caption.Seconds;
        }

        return total > still.Seconds + CaptionSecondsTolerance
            ? string.Create(
                CultureInfo.InvariantCulture,
                $"captions show for {total} seconds together, longer than the still's {still.Seconds}.")
            : null;
    }

    private static string DescribeSecondsProblem(double seconds) =>
        string.Create(CultureInfo.InvariantCulture, $"seconds must be above 0 (was {seconds}).");

    private static string? FindZoomProblem(StillZoom zoom) =>
        RangeProblems.Within("zoom startPercent", zoom.StartPercent, StillZoom.MinPercent, StillZoom.MaxPercent)
        ?? RangeProblems.Within("zoom endPercent", zoom.EndPercent, StillZoom.MinPercent, StillZoom.MaxPercent);

    private static string? FindFocusProblem(StillFocus focus) =>
        RangeProblems.Within("focus xPercent", focus.XPercent, StillFocus.MinPercent, StillFocus.MaxPercent)
        ?? RangeProblems.Within("focus yPercent", focus.YPercent, StillFocus.MinPercent, StillFocus.MaxPercent);
}
