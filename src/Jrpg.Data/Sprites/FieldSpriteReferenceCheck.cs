using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Data.Content;

namespace Jrpg.Data.Sprites;

/// <summary>
/// The cross-file rule of ADR 0043 (decision 567): every field sprite an object or the new game names has a record,
/// matched by file name in exact case, since the game looks the record up by the name as written.
/// </summary>
internal static class FieldSpriteReferenceCheck
{
    /// <exception cref="ContentValidationException">
    /// A named sprite has no record; the message names who names it.
    /// </exception>
    public static void Check(
        string fileName,
        IReadOnlyDictionary<string, FieldSpriteSheet> sheets,
        IEnumerable<ObjectDefinition> objects,
        NewGameDefinition newGame)
    {
        foreach (var definition in objects)
        {
            if (definition.FieldSpriteFile is { } file && !sheets.ContainsKey(file))
            {
                throw new ContentValidationException(
                    $"{fileName}: object '{definition.Id}' names field sprite '{file}', which has no record.");
            }
        }

        if (!sheets.ContainsKey(newGame.FieldSpriteFile))
        {
            throw new ContentValidationException(
                $"{fileName}: the new game names field sprite '{newGame.FieldSpriteFile}', which has no record.");
        }
    }
}
