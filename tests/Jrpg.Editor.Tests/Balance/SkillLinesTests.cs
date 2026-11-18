using Jrpg.Core.Stats;
using Jrpg.Editor.Balance;

namespace Jrpg.Editor.Tests.Balance;

// A skill's amount with the table's damage modifier, which joins the user's own modifiers rather than replacing them.
public sealed class SkillLinesTests
{
    // Power 10 x 2 x (1 + 0.10 - 0.25) = 17; the table's modifier in the sum's place would give 15.
    [Fact]
    public void AmountText_UserWithAModifier_TableModifierAddedToTheSum()
    {
        var gearedUser = new CombatStats(0) { Power = 10, DamageModifierPermille = 100 };

        var text = SkillLines.AmountText(BalanceFixtures.Fireball.Effect, gearedUser, modifierPermille: -250);

        Assert.Equal("17", text);
    }
}
