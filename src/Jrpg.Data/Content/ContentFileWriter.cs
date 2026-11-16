using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jrpg.Data.Content;

/// <summary>Writes a content file in exactly the format <see cref="ContentFileReader"/> reads.</summary>
internal static class ContentFileWriter
{
    private static readonly JsonSerializerOptions _options = CreateOptions();

    public static void Write<T>(string path, T content) => File.WriteAllBytes(path, Serialize(content));

    /// <summary>
    /// The file's bytes as <see cref="Write"/> would lay them, for a write planned or compared in memory.
    /// </summary>
    public static byte[] Serialize<T>(T content) => SerializeWith(content, _options);

    /// <summary>
    /// The file's bytes laid out as <see cref="Write"/> lays them, with converters added for this file only, such as
    /// the catalog reference converters that write a record as its id in <c>skills.json</c> (decision 972).
    /// </summary>
    public static byte[] Serialize<T>(T content, params JsonConverter[] converters) =>
        SerializeWith(content, WithConverters(converters));

    private static byte[] SerializeWith<T>(T content, JsonSerializerOptions options)
    {
        using var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, content, options);
        stream.Write(Encoding.UTF8.GetBytes(options.NewLine));
        return stream.ToArray();
    }

    private static JsonSerializerOptions CreateOptions()
    {
        // Indentation, CRLF and the final newline follow .editorconfig, so a generated file reads like a hand-written one.
        var options = new JsonSerializerOptions(ContentJsonOptions.Strict) { WriteIndented = true, NewLine = "\r\n" };
        options.MakeReadOnly();
        return options;
    }

    // Converters naming records are built over the catalog being written, so the options can't be shared.
    private static JsonSerializerOptions WithConverters(JsonConverter[] converters)
    {
        var options = new JsonSerializerOptions(_options);
        foreach (var converter in converters)
        {
            options.Converters.Add(converter);
        }

        options.MakeReadOnly();
        return options;
    }
}
