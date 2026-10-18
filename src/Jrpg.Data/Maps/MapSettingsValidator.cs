using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Maps;

internal static class MapSettingsValidator
{
    /// <summary>Checks one settings file; <paramref name="fileName"/> is its path under the content folder, and the map id must equal its file name.</summary>
    public static void Validate(string fileName, MapSettings settings)
    {
        var expectedId = Path.GetFileNameWithoutExtension(fileName);
        if (settings.Id != expectedId)
        {
            throw new ContentValidationException($"{fileName}: id must equal the file name '{expectedId}' (was '{settings.Id}').");
        }

        if (string.IsNullOrWhiteSpace(settings.DisplayName))
        {
            throw new ContentValidationException($"{fileName}: displayName must not be empty.");
        }
    }
}
