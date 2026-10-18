using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jrpg.ContentConverter;

/// <summary>
/// Reads the legacy editor's PascalCase files strictly, so a field the converter doesn't know about
/// fails the run instead of being dropped without anyone noticing.
/// </summary>
internal static class LegacyFileReader
{
    private static readonly JsonSerializerOptions _options = CreateOptions();

    /// <param name="assetsRoot">The legacy <c>Assets/</c> folder.</param>
    /// <param name="relativePath">The file's path under the assets folder, as messages name it.</param>
    public static T Read<T>(string assetsRoot, string relativePath)
    {
        try
        {
            using var stream = File.OpenRead(Path.Combine(assetsRoot, relativePath));
            return JsonSerializer.Deserialize<T>(stream, _options)
                ?? throw new ConversionException($"{relativePath}: the file holds null.");
        }
        catch (JsonException exception)
        {
            var location = exception.Path is null ? relativePath : $"{relativePath} at {exception.Path}";
            throw new ConversionException($"{location}: {exception.Message}", exception);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new ConversionException($"{relativePath}: {exception.Message}", exception);
        }
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            RespectNullableAnnotations = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            AllowDuplicateProperties = false,
        };
        options.Converters.Add(new PointStringConverter());

        // Shared static instance: read-only so it isn't static mutable state.
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }
}
