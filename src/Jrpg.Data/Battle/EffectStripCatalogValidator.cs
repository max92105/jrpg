using Jrpg.Core.Battle;
using Jrpg.Data.Content;

namespace Jrpg.Data.Battle;

/// <summary>
/// The rules for <c>effect-strips.json</c> (ADR 0048): each strip named once by its file, each frame shown at least a
/// millisecond, its hit on a frame from 0, a swing named when there is one. Whether the images are there and the hit
/// frame lies within the strip is the startup art check's.
/// </summary>
internal static class EffectStripCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<EffectStripDefinition> strips) =>
        new ContentListValidator<EffectStripDefinition>(fileName, "effect strip", strip => strip.File)
            .Validate(strips, FindProblem);

    private static string? FindProblem(EffectStripDefinition strip) =>
        RangeProblems.AtLeast("frameMilliseconds", strip.FrameMilliseconds, 1)
        ?? RangeProblems.AtLeast("hitFrame", strip.HitFrame, 0)
        ?? (strip.SwingFile is { } swing && string.IsNullOrWhiteSpace(swing) ? "swingFile must not be blank." : null);
}
