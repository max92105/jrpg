using System.Text;
using System.Text.Json;

namespace Jrpg.Data.Content;

/// <summary>Writes a content file in exactly the format <see cref="ContentFileReader"/> reads.</summary>
internal static class ContentFileWriter
{
    private static readonly JsonSerializerOptions _options = CreateOptions();

    public static void Write<T>(string path, T content)
    {
        using var stream = File.Create(path);
        JsonSerializer.Serialize(stream, content, _options);
        stream.Write(Encoding.UTF8.GetBytes(_options.NewLine));
    }

    private static JsonSerializerOptions CreateOptions()
    {
        // Indentation, CRLF and the final newline follow .editorconfig, so a generated file reads like a hand-written one.
        var options = new JsonSerializerOptions(ContentJsonOptions.Strict) { WriteIndented = true, NewLine = "\r\n" };
        options.MakeReadOnly();
        return options;
    }
}
