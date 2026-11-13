using System.Text.Json;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Battle;

public sealed class ResistancesConverterTests
{
    [Fact]
    public void Read_PercentsByType_HoldsPermilleAndZeroForTheRest()
    {
        var resistances = JsonSerializer.Deserialize<Resistances>("""{ "fire": -25, "blunt": 25 }""", ContentJsonOptions.Strict);

        Assert.Equal(Resistances.None with { Fire = -250, Blunt = 250 }, resistances);
    }

    // Decision 587: whole percents in the enum's order, a type at 0 left out.
    [Fact]
    public void Write_MixedResistances_WritesPercentsInTypeOrder()
    {
        var resistances = Resistances.None with { Fire = -250, Blunt = 250 };

        Assert.Equal("""{"blunt":25,"fire":-25}""", JsonSerializer.Serialize(resistances, ContentJsonOptions.Strict));
    }

    [Fact]
    public void Write_None_WritesEmptyObject()
    {
        Assert.Equal("{}", JsonSerializer.Serialize(Resistances.None, ContentJsonOptions.Strict));
    }

    [Theory]
    [MemberData(nameof(EveryType))]
    public void RoundTrip_EveryType_ReadsBackTheSame(DamageType type)
    {
        var resistances = Resistances.From(each => each == type ? -170 : 0);

        var written = JsonSerializer.Serialize(resistances, ContentJsonOptions.Strict);

        Assert.Equal(resistances, JsonSerializer.Deserialize<Resistances>(written, ContentJsonOptions.Strict));
    }

    public static TheoryData<DamageType> EveryType() => [.. Enum.GetValues<DamageType>()];

    [Fact]
    public void Read_UnknownType_ThrowsNamingIt()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<Resistances>("""{ "poison": 10 }""", ContentJsonOptions.Strict));

        Assert.Contains("'poison'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Read_SameTypeTwice_ThrowsNamingIt()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<Resistances>("""{ "fire": 10, "fire": 20 }""", ContentJsonOptions.Strict));

        Assert.Contains("'fire'", exception.Message, StringComparison.Ordinal);
    }
}
