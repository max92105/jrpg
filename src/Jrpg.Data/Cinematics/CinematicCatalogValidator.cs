using System.Globalization;
using Jrpg.Core.Cinematics;
using Jrpg.Data.Content;

namespace Jrpg.Data.Cinematics;

/// <summary>
/// The rules for <c>cinematics.json</c> (ADR 0046): at least one still and no null still; each still names a picture,
/// shows for a finite time above 0, and has a subtitle, voice and zoom that are absent or well formed; the stills
/// together last a time the game can count; the track is absent or named. Problems name the cinematic and the still.
/// </summary>
internal static class CinematicCatalogValidator
{
    // A TimeSpan counts up to about 29,000 years; the game sums the stills' times into one.
    private static readonly double _longestTotalSeconds = TimeSpan.MaxValue.TotalSeconds;

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
        : still.Caption is { } caption && string.IsNullOrWhiteSpace(caption) ? "caption must hold text or be left out."
        : still.VoiceFile is { } voice && string.IsNullOrWhiteSpace(voice) ? "voiceFile must name a voice line or be left out."
        : still.Zoom is { } zoom ? FindZoomProblem(zoom)
        : null;

    private static string DescribeSecondsProblem(double seconds) =>
        string.Create(CultureInfo.InvariantCulture, $"seconds must be above 0 (was {seconds}).");

    private static string? FindZoomProblem(StillZoom zoom) =>
        RangeProblems.Within("zoom startPercent", zoom.StartPercent, StillZoom.MinPercent, StillZoom.MaxPercent)
        ?? RangeProblems.Within("zoom endPercent", zoom.EndPercent, StillZoom.MinPercent, StillZoom.MaxPercent);
}
