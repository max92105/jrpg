using System.Text.Json;

namespace Jrpg.Data.Content;

internal static class ContentFileReader
{
    public static T Read<T>(string path) => Read<T>(path, ContentJsonOptions.Strict);

    /// <param name="path">The content file.</param>
    /// <param name="options">The strict options, with any catalog reference converters the file needs.</param>
    public static T Read<T>(string path, JsonSerializerOptions options)
    {
        var fileName = Path.GetFileName(path);
        try
        {
            using var stream = File.OpenRead(path);
            return JsonSerializer.Deserialize<T>(stream, options)
                ?? throw new ContentValidationException($"{fileName}: the file holds null instead of content.");
        }
        catch (FileNotFoundException exception)
        {
            throw new ContentValidationException($"{fileName}: the file is missing.", exception);
        }
        catch (JsonException exception)
        {
            var location = exception.Path is null ? fileName : $"{fileName} at {exception.Path}";
            throw new ContentValidationException($"{location}: {exception.Message}", exception);
        }
        catch (NotSupportedException exception)
        {
            // Thrown for a value the serializer can't create, such as an event step without its "type". The message holds the path.
            throw new ContentValidationException($"{fileName}: {exception.Message}", exception);
        }
    }
}
