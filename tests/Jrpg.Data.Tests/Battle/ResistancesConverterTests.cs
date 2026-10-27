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
