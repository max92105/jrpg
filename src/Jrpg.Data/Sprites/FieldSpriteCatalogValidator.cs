using Jrpg.Core.Sprites;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Sprites;

/// <summary>
/// The rules for <c>field-sprites.json</c> (ADR 0043): each record names a PNG once, cuts it in frames of at least a
/// pixel, draws at a scale from 1 to <see cref="FieldSpriteSheet.MaxScale"/>, keeps its feet inside the frame, reads
/// rows from 0 and walks through at least two frames shown for at least a millisecond. Whether the frames lie inside
/// the image is the startup art check's.
/// </summary>
internal static class FieldSpriteCatalogValidator
{
    private const string PngExtension = ".png";

    public static void Validate(string fileName, IReadOnlyList<FieldSpriteSheet> sheets) =>
        new ContentListValidator<FieldSpriteSheet>(fileName, "field sprite", sheet => sheet.File).Validate(sheets, FindProblem);

    private static string? FindProblem(FieldSpriteSheet sheet)
    {
        if (!sheet.File.EndsWith(PngExtension, StringComparison.Ordinal))
        {
            return $"file must name a {PngExtension} image.";
        }

        return RangeProblems.AtLeast("frameWidth", sheet.FrameWidth, 1)
            ?? RangeProblems.AtLeast("frameHeight", sheet.FrameHeight, 1)
            ?? RangeProblems.Within("scale", sheet.Scale, 1, FieldSpriteSheet.MaxScale)
            ?? RangeProblems.Within("feetPixels", sheet.FeetPixels, 0, sheet.FrameHeight - 1)
            ?? FindWalkProblem(sheet.Walk);
    }

    private static string? FindWalkProblem(FacingCycle walk) =>
        RangeProblems.AtLeast("walk.rows.down", walk.Rows.Down, 0)
        ?? RangeProblems.AtLeast("walk.rows.up", walk.Rows.Up, 0)
        ?? RangeProblems.AtLeast("walk.rows.left", walk.Rows.Left, 0)
        ?? RangeProblems.AtLeast("walk.rows.right", walk.Rows.Right, 0)
        ?? AnimationProblems.Find("walk.frames", walk.Frames);
}
