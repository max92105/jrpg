using System.Text.Json;
using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// A content id read as the record it names, and written back as its id (decision 789), over records built here.
public sealed class CatalogReferenceConverterTests
{
    private static readonly TieredEffect _fire1 = new("fire_1", EffectKind.Damage, 2) { DamageType = DamageType.Fire };

    private static readonly JsonSerializerOptions _options = ContentJsonOptions.StrictWith(
        new CatalogReferenceConverter<TieredEffect>(
            new Dictionary<string, TieredEffect> { ["fire_1"] = _fire1 }, effect => effect.Id, "effect"));

    [Fact]
    public void Read_KnownId_ReturnsTheRecord()
    {
        Assert.Same(_fire1, JsonSerializer.Deserialize<TieredEffect>("\"fire_1\"", _options));
    }

    [Fact]
    public void Read_UnknownId_ThrowsNamingTheIdAndTheRecordName()
    {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TieredEffect>("\"fire_9\"", _options));

        Assert.Equal("no effect 'fire_9'.", exception.Message);
    }

    [Fact]
    public void Read_NumberInsteadOfId_Throws()
    {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TieredEffect>("2", _options));

        Assert.Equal("the effect must be named by its id, a string.", exception.Message);
    }

    [Fact]
    public void Write_Record_WritesItsId()
    {
        Assert.Equal("\"fire_1\"", JsonSerializer.Serialize(_fire1, _options));
    }
}
