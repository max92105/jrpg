using System.Text.Json;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Items;

public sealed class StatBonusesConverterTests
{
    [Fact]
    public void Read_CamelCaseNames_ReadsEachStat()
    {
        var bonuses = JsonSerializer.Deserialize<StatBonuses>("""{ "constitution": 3, "slashingResistance": 15 }""", ContentJsonOptions.Strict);

        var expected = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Constitution] = 3, [StatKind.SlashingResistance] = 15 });
        Assert.Equal(expected, bonuses);
    }

    [Fact]
    public void Read_UnknownStat_ThrowsNamingIt()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<StatBonuses>("""{ "luck": 5 }""", ContentJsonOptions.Strict));

        Assert.Equal("bonuses names an unknown stat 'luck'.", exception.Message);
    }

    [Fact]
    public void Read_SameStatTwice_ThrowsNamingIt()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<StatBonuses>("""{ "power": 5, "power": 3 }""", ContentJsonOptions.Strict));

        Assert.Equal("bonuses names 'power' more than once.", exception.Message);
    }

    [Fact]
    public void Write_ThenRead_GivesTheSameBonuses()
    {
        var bonuses = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Will] = 3, [StatKind.FireResistance] = -10 });

        var json = JsonSerializer.Serialize(bonuses, ContentJsonOptions.Strict);

        Assert.Equal(bonuses, JsonSerializer.Deserialize<StatBonuses>(json, ContentJsonOptions.Strict));
    }

    [Fact]
    public void Read_NotAnObject_Throws()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StatBonuses>("[5]", ContentJsonOptions.Strict));
    }
}
