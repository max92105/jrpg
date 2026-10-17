using System.Text.Json;

namespace Jrpg.Data.Content;

internal static class ContentFileReader
{
    public static T Read<T>(string path)
    {
        var fileName = Path.GetFileName(path);
        try
        {
            using var stream = File.OpenRead(path);
            return JsonSerializer.Deserialize<T>(stream, ContentJsonOptions.Strict)
                ?? throw new ContentValidationException($"{fileName}: the file holds null instead of content.");
        }
        catch (JsonException exception)
        {
            var location = exception.Path is null ? fileName : $"{fileName} at {exception.Path}";
            throw new ContentValidationException($"{location}: {exception.Message}", exception);
        }
    }
}
