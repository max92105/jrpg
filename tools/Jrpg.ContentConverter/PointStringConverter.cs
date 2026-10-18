using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Jrpg.Core.Geometry;

namespace Jrpg.ContentConverter;

/// <summary>
/// Reads the legacy editor's WPF points, written as <c>"x,y"</c> strings with a dot as the decimal separator
/// whatever the machine's culture.
/// </summary>
internal sealed class PointStringConverter : JsonConverter<Point2D>
{
    public override Point2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.GetString() ?? string.Empty;
        var parts = text.Split(',');
        if (parts.Length != 2
            || !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            throw new JsonException($"'{text}' isn't a point written as \"x,y\".");
        }

        return new Point2D(x, y);
    }

    // Content files store points as objects; only the legacy files use this string form, and they are only read.
    public override void Write(Utf8JsonWriter writer, Point2D value, JsonSerializerOptions options) =>
        throw new NotSupportedException("Legacy points are only read.");
}
