using Jrpg.Core.Geometry;
using Jrpg.Core.Sprites;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Sprites;

/// <summary>
/// The rules for <c>field-sprites.json</c> (ADR 0043): each record names a PNG once, cuts it in frames of at least a
/// pixel, draws at a scale from 1 to <see cref="FieldSpriteSheet.MaxScale"/>, keeps its feet inside the frame, reads
/// rows from 0 and walks through at least two frames shown for at least a millisecond; a facing's sequence (ADR 0054)
/// holds at least one cell, each at a column and a row from 0. The startup art check, not this one, checks that the
/// frames lie inside the image.
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
        ?? AnimationProblems.Find("walk.frames", walk.Frames)
        ?? (walk.Sequences is { } sequences ? FindSequencesProblem(sequences) : null);

    // Checked whether or not a facing's row is read (decision 767); the facings in the enum's order.
    private static string? FindSequencesProblem(FacingSequences sequences) =>
        Enum.GetValues<Facing>()
            .Select(facing => sequences.Of(facing) is { } sequence ? FindSequenceProblem(facing, sequence) : null)
            .FirstOrDefault(problem => problem is not null);

    private static string? FindSequenceProblem(Facing facing, FacingSequence sequence)
    {
        var field = $"walk.sequences.{ContentJsonOptions.EnumName(facing)}.cells";
        if (sequence.Cells.Count == 0)
        {
            return $"{field} must hold at least 1 cell.";
        }

        for (var index = 0; index < sequence.Cells.Count; index++)
        {
            var cell = sequence.Cells[index];
            if ((RangeProblems.AtLeast($"{field}[{index}].column", cell.Column, 0)
                ?? RangeProblems.AtLeast($"{field}[{index}].row", cell.Row, 0)) is { } problem)
            {
                return problem;
            }
        }

        return null;
    }
}
