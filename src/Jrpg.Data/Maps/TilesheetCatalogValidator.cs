using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Maps;

internal static class TilesheetCatalogValidator
{
    public static void Validate(string fileName, IReadOnlyList<TilesheetDefinition> sheets) =>
        new ContentListValidator<TilesheetDefinition>(fileName, "tilesheet", sheet => sheet.Id)
            .Validate(sheets, FindProblem);

    private static string? FindProblem(TilesheetDefinition sheet)
    {
        if (string.IsNullOrWhiteSpace(sheet.File))
        {
            return "file must not be empty.";
        }

        if (sheet.TileWidth < 1 || sheet.TileHeight < 1)
        {
            return $"tileWidth and tileHeight must be at least 1 (was {sheet.TileWidth} x {sheet.TileHeight}).";
        }

        return sheet.Margin < 0 || sheet.Spacing < 0
            ? $"margin and spacing must not be negative (was {sheet.Margin} and {sheet.Spacing})."
            : null;
    }
}
