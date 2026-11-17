using Jrpg.Core.Battle;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Battle;

/// <summary>
/// The rules for <c>animated-backgrounds.json</c> (ADR 0059, decision 897): each background named once by its file, a
/// frame at least a pixel wide and tall, and an animation of at least two frames each shown at least a millisecond.
/// Whether the image is there and holds the grid is the startup art check's.
/// </summary>
internal static class AnimatedBackgroundCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<AnimatedBackgroundDefinition> backgrounds) =>
        new ContentListValidator<AnimatedBackgroundDefinition>(fileName, "animated background", background => background.File)
            .Validate(backgrounds, FindProblem);

    private static string? FindProblem(AnimatedBackgroundDefinition background) =>
        RangeProblems.AtLeast("frameWidth", background.FrameWidth, 1)
        ?? RangeProblems.AtLeast("frameHeight", background.FrameHeight, 1)
        ?? FindAnimationProblem(background.Animation);

    // The loader refuses a null animation itself; an edited record reaches here without the serializer.
    private static string? FindAnimationProblem(TileAnimation? animation) =>
        animation is null ? "animation is required." : AnimationProblems.Find("animation", animation);
}
