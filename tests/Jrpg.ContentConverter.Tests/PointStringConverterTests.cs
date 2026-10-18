using System.Globalization;
using System.Text.Json;
using Jrpg.Core.Geometry;

namespace Jrpg.ContentConverter.Tests;

public sealed class PointStringConverterTests
{
    private static readonly JsonSerializerOptions _options = new() { Converters = { new PointStringConverter() } };

    [Fact]
    public void Read_UnderFrenchCulture_ParsesDotAsDecimal()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
        try
        {
            var point = JsonSerializer.Deserialize<Point2D>("\"0.25,0.5\"", _options);

            Assert.Equal(new Point2D(0.25, 0.5), point);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData("\"0.25\"")]
    [InlineData("\"0,25,0.5\"")]
    [InlineData("\"x,0.5\"")]
    public void Read_NotAPoint_ThrowsNamingTheText(string json)
    {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Point2D>(json, _options));

        Assert.Contains(json.Trim('"'), exception.Message, StringComparison.Ordinal);
    }
}
