using Jrpg.Core.Stats;
using Jrpg.Editor.Balance;

namespace Jrpg.Editor.Tests.Balance;

// The balance table's enemy rows (decision 992): each enemy's HP, power and basic attack as the battle builds it.
public sealed class EnemyBalanceRowsTests
{
    [Fact]
    public void Of_Enemy_ReadsHpPowerAndAttack()
    {
        var rows = EnemyBalanceRows.Of(BalanceFixtures.World);

        Assert.Equal(new EnemyBalanceRow("Imp", 80, 20, 9, DamageType.Fire), rows.Single(row => row.EnemyName == "Imp"));
    }

    // The world lists the imp first.
    [Fact]
    public void Of_TwoEnemies_InIdOrder()
    {
        var rows = EnemyBalanceRows.Of(BalanceFixtures.World);

        Assert.Equal(["Bat", "Imp"], rows.Select(row => row.EnemyName));
    }
}
