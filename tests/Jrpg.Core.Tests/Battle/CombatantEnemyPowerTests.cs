using Jrpg.Core.Battle;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

// An enemy's content Power reaches its battle stats, which its tiered skills multiply (decision 787).
public sealed class CombatantEnemyPowerTests
{
    [Fact]
    public void FromEnemy_Power_IsCopiedIntoItsStats()
    {
        var chief = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithPower(7).Build());

        Assert.Equal(7, chief.Stats.Power);
    }
}
