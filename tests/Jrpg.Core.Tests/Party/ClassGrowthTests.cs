using Jrpg.Core.Party;

namespace Jrpg.Core.Tests.Party;

public sealed class ClassGrowthTests
{
    private static readonly ClassDefinition _fighter = new(
        "fighter",
        "Fighter",
        "Fights.",
        Base: new ClassStats(MaxHp: 220, Attack: 24, Defense: 8),
        Growth: new ClassStats(MaxHp: 22, Attack: 3, Defense: 1));

    [Fact]
    public void StatsAt_LevelOne_IsBase()
    {
        Assert.Equal(_fighter.Base, ClassGrowth.StatsAt(_fighter, level: 1));
    }

    [Fact]
    public void StatsAt_LevelFive_AddsFourGrowths()
    {
        Assert.Equal(new ClassStats(MaxHp: 308, Attack: 36, Defense: 12), ClassGrowth.StatsAt(_fighter, level: 5));
    }

    [Fact]
    public void StatsAt_LevelBelowOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassGrowth.StatsAt(_fighter, level: 0));
    }
}
