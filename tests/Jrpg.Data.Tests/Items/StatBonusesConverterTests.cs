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

    // The name content writes the stat with (the content enum name, as affixes.json's stat), pinned against a rename.
    [Fact]
    public void Read_DamageBonus_IsNamedDamageBonus()
    {
        var bonuses = JsonSerializer.Deserialize<StatBonuses>("""{ "damageBonus": 20 }""", ContentJsonOptions.Strict);

        Assert.Equal(20, bonuses!.Of(StatKind.DamageBonus));
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

    // A frozen dictionary keeps no order, so the written order must come from the converter (decision 862).
    [Fact]
    public void Write_BonusesBuiltOutOfKindOrder_WritesThemInKindOrder()
    {
        var outOfOrder = new Dictionary<StatKind, int> { [StatKind.FireResistance] = 10, [StatKind.Constitution] = 3 };

        var json = JsonSerializer.Serialize(new StatBonuses(outOfOrder), ContentJsonOptions.Strict);

        Assert.Equal("""{"constitution":3,"fireResistance":10}""", json);
    }

    [Fact]
    public void Read_NotAnObject_Throws()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<StatBonuses>("[5]", ContentJsonOptions.Strict));
    }
}
