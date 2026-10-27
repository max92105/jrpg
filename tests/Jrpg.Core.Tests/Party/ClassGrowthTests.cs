using Jrpg.Core.Party;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

public sealed class ClassGrowthTests
{
    private static readonly ClassDefinition _fighter = new(
        "fighter",
        "Fighter",
        "Fights.",
        Base: new ClassStats(MaxHp: 220, Defense: 8, Speed: 12),
        Growth: new ClassStats(MaxHp: 22, Defense: 1, Speed: 1));

    private static readonly ClassDefinition _primaryClass = new ClassBuilder()
        .WithPrimaries(new PrimaryStats(15, 8, 6, 14, 8), new PrimaryStats(2, 1, 1, 2, 1))
        .Build();

    [Fact]
    public void StatsAt_LevelThree_SpeedGrowsTwice()
    {
        var stats = ClassGrowth.StatsAt(_fighter, level: 3);

        Assert.Equal(14, stats.Speed);
    }

    [Fact]
    public void StatsAt_LevelOne_IsBase()
    {
        Assert.Equal(_fighter.Base, ClassGrowth.StatsAt(_fighter, level: 1));
    }

    [Fact]
    public void StatsAt_LevelFive_AddsFourGrowths()
    {
        Assert.Equal(new ClassStats(MaxHp: 308, Defense: 12, Speed: 16), ClassGrowth.StatsAt(_fighter, level: 5));
    }

    [Fact]
    public void StatsAt_LevelOne_IsBasePrimaries()
    {
        var stats = ClassGrowth.StatsAt(_primaryClass, level: 1);

        Assert.Equal(new PrimaryStats(Power: 15, Agility: 8, Intelligence: 6, Constitution: 14, Will: 8), stats.Primaries);
    }

    [Fact]
    public void StatsAt_LevelThree_GrowsEachPrimaryTwice()
    {
        var stats = ClassGrowth.StatsAt(_primaryClass, level: 3);

        Assert.Equal(new PrimaryStats(Power: 19, Agility: 10, Intelligence: 8, Constitution: 18, Will: 10), stats.Primaries);
    }

    [Fact]
    public void StatsAt_LevelThree_GrowsAccuracy()
    {
        var characterClass = new ClassBuilder()
            .WithBase(new ClassStats(MaxHp: 100, Defense: 5, Speed: 10) { Accuracy = 5 })
            .WithGrowth(new ClassStats(0, 0, 0) { Accuracy = 1 })
            .Build();

        var stats = ClassGrowth.StatsAt(characterClass, level: 3);

        Assert.Equal(7, stats.Accuracy);
    }

    [Fact]
    public void StatsAt_LevelBelowOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassGrowth.StatsAt(_fighter, level: 0));
    }
}
