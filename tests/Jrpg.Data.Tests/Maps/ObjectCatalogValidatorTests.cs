using System.Globalization;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Tests.Maps;

public sealed class ObjectCatalogValidatorTests
{
    private const string FileName = "generated/objects.json";

    private static readonly ObjectDefinition _chest = new()
    {
        Id = "chest",
        Width = 1,
        Height = 1,
        CollisionPolygon = [new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1)],
    };

    [Fact]
    public void Validate_ValidObjects_DoesNotThrow()
    {
        var exception = Record.Exception(
            () => ObjectCatalogValidator.Validate(FileName, [_chest, _chest with { Id = "rug", CollisionPolygon = [] }]));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_TwoPointPolygon_ThrowsNamingTheObject()
    {
        var chest = _chest with { CollisionPolygon = [new Point2D(0, 0), new Point2D(1, 1)] };

        var exception = Assert.Throws<ContentValidationException>(() => ObjectCatalogValidator.Validate(FileName, [chest]));

        Assert.Equal("generated/objects.json: object 'chest': collisionPolygon must be empty or have at least 3 points (had 2).", exception.Message);
    }

    [Theory]
    [InlineData(1.5, 0.5)]
    [InlineData(0.5, -0.25)]
    public void Validate_PointOutsideUnitSquare_ThrowsNamingTheObject(double x, double y)
    {
        var chest = _chest with { CollisionPolygon = [new Point2D(0, 0), new Point2D(x, y), new Point2D(1, 1)] };

        var exception = Assert.Throws<ContentValidationException>(() => ObjectCatalogValidator.Validate(FileName, [chest]));

        Assert.StartsWith(
            string.Create(CultureInfo.InvariantCulture, $"generated/objects.json: object 'chest': collisionPolygon point ({x}, {y}) must be within 0..1"),
            exception.Message,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Validate_EmptySize_ThrowsNamingTheObject(int width, int height)
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ObjectCatalogValidator.Validate(FileName, [_chest with { Width = width, Height = height }]));

        Assert.Equal($"generated/objects.json: object 'chest': width and height must be at least 1 (was {width}x{height}).", exception.Message);
    }
}
