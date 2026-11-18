using Jrpg.Editor.Balance;

namespace Jrpg.Editor.Tests.Balance;

// The balance table's enemy skill rows (decision 992): each enemy's damaging and healing skills at its own power, by
// Core's own rule for the combatant the battle builds of it.
public sealed class EnemySkillBalanceRowsTests
{
    // The imp's Power is 20; fire_1 gives 2 per Power.
    [Fact]
    public void Of_TieredSkill_ItsPowerTimesPerPower()
    {
        var fireball = ImpRow("Fireball", EnemySkillBalanceRows.Of(BalanceFixtures.World, modifierPermille: 0));

        Assert.Equal((20, "fire_1 x2", 1, "40"), (fireball.Power, fireball.TierLine, fireball.Hits, fireball.AmountText));
    }

    [Fact]
    public void Of_FourHits_ShowsPerHitAndTotal()
    {
        var flurry = ImpRow("Flurry", EnemySkillBalanceRows.Of(BalanceFixtures.World, modifierPermille: 0));

        Assert.Equal("40 x 4 = 160", flurry.AmountText);
    }

    // The imp's tactics name the attack, Fireball, Curse and Mend, its phase Flurry and Fireball again.
    [Fact]
    public void Of_SkillsInTacticsAndPhases_EachOnceInTheOrderNamed()
    {
        var rows = EnemySkillBalanceRows.Of(BalanceFixtures.World, modifierPermille: 0);

        Assert.Equal(["Fireball", "Mend", "Flurry"], rows.Where(row => row.EnemyName == "Imp").Select(row => row.SkillName));
    }

    [Fact]
    public void Of_StatusOnlySkill_NotListed()
    {
        var rows = EnemySkillBalanceRows.Of(BalanceFixtures.World, modifierPermille: 0);

        Assert.DoesNotContain(rows, row => row.SkillName == BalanceFixtures.Curse.Name);
    }

    // The world lists the imp first.
    [Fact]
    public void Of_TwoEnemies_InIdOrder()
    {
        var rows = EnemySkillBalanceRows.Of(BalanceFixtures.World, modifierPermille: 0);

        Assert.Equal(["Bat", "Imp"], rows.Select(row => row.EnemyName).Distinct());
    }

    // Mend's 60 = Power 20 x 3.
    [Fact]
    public void Of_HealAndDamage_OnlyTheHealMarked()
    {
        var rows = EnemySkillBalanceRows.Of(BalanceFixtures.World, modifierPermille: 0);
        var (mend, fireball) = (ImpRow("Mend", rows), ImpRow("Fireball", rows));

        Assert.Equal((true, "60", false), (mend.IsHeal, mend.AmountText, fireball.IsHeal));
    }

    // Fireball's 40 x 0.75 = 30; Mend keeps its 60.
    [Fact]
    public void Of_MinusTwentyFivePercent_LowersDamageNotHeals()
    {
        var rows = EnemySkillBalanceRows.Of(BalanceFixtures.World, modifierPermille: -250);

        Assert.Equal(("30", "60"), (ImpRow("Fireball", rows).AmountText, ImpRow("Mend", rows).AmountText));
    }

    // Power raises no enemy's flat amount (its damage permille is 1000), so Smite's 15 stays 15.
    [Fact]
    public void Of_FlatSkill_NamesItsPower()
    {
        var rows = EnemySkillBalanceRows.Of(BalanceFixtures.World, modifierPermille: 0);
        var smite = Assert.Single(rows, row => row.EnemyName == "Bat");

        Assert.Equal(("flat 15", "15"), (smite.TierLine, smite.AmountText));
    }

    private static EnemySkillBalanceRow ImpRow(string skillName, IReadOnlyList<EnemySkillBalanceRow> rows) =>
        rows.Single(row => row.EnemyName == "Imp" && row.SkillName == skillName);
}
