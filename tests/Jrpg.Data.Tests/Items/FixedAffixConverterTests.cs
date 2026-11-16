using System.Text.Json;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Data.Items;

namespace Jrpg.Data.Tests.Items;

// A unique's fixed affix read with its stat from the pool and written back as its id and value (ADR 0058), over a pool
// built here whose affix id differs from its stat's name, so the stat can only come from the pool.
public sealed class FixedAffixConverterTests
{
    private static readonly AffixDefinition _might = new()
    {
        Id = "might",
        Stat = StatKind.Power,
        Min = 2,
        Max = 5,
        Kinds = [EquipmentKind.Weapon],
        Weight = 10,
        GoldValue = 20,
    };

    private static readonly JsonSerializerOptions _options = ContentJsonOptions.StrictWith(
        new FixedAffixConverter(new Dictionary<string, AffixDefinition> { ["might"] = _might }));

    public static TheoryData<string, string> OtherShapes => new()
    {
        {
            """{ "affixId": "might", "value": 7, "stat": "power" }""",
            "a fixed affix holds affixId and value once each, nothing else (found 'stat')."
        },
        {
            """{ "affixId": "might", "value": 7, "value": 8 }""",
            "a fixed affix holds affixId and value once each, nothing else (found 'value')."
        },
        { """{ "value": 7 }""", "a fixed affix needs its affixId." },
        { """{ "affixId": 3, "value": 7 }""", "a fixed affix's affixId must be a string." },
        { """{ "affixId": "might", "value": 1.5 }""", "a fixed affix's value must be a whole number." },
        { """{ "affixId": "might", "value": "7" }""", "a fixed affix's value must be a whole number." },
    };

    [Fact]
    public void Read_KnownAffix_ResolvesItsStat()
    {
        var affix = JsonSerializer.Deserialize<RolledAffix>("""{ "affixId": "might", "value": 7 }""", _options);

        Assert.Equal(new RolledAffix("might", StatKind.Power, 7), affix);
    }

    [Fact]
    public void Read_UnknownAffix_ThrowsNamingIt()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<RolledAffix>("""{ "affixId": "spite", "value": 7 }""", _options));

        Assert.Equal("no affix 'spite'.", exception.Message);
    }

    [Fact]
    public void Read_ValueMissing_Throws()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<RolledAffix>("""{ "affixId": "might" }""", _options));

        Assert.Equal("a fixed affix needs its value.", exception.Message);
    }

    [Fact]
    public void Read_NumberInsteadOfObject_Throws()
    {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<RolledAffix>("5", _options));

        Assert.Equal("a fixed affix must be an object of affixId and value.", exception.Message);
    }

    [Theory]
    [MemberData(nameof(OtherShapes))]
    public void Read_OtherShape_ThrowsNamingWhatIsWrong(string json, string expected)
    {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<RolledAffix>(json, _options));

        Assert.Equal(expected, exception.Message);
    }

    [Fact]
    public void Write_FixedAffix_WritesItsIdAndValueOnly()
    {
        var json = JsonSerializer.Serialize(new RolledAffix("might", StatKind.Power, 7), _options);

        Assert.Equal("""{"affixId":"might","value":7}""", json);
    }
}
