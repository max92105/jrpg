using Jrpg.Core.Stats;
using Jrpg.Game.Menus;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

public sealed class StatPreviewTextTests
{
    private static readonly CombatStats _current = new(Attack: 12, Defense: 8);

    [Fact]
    public void Of_StrongerWeapon_AttackIsPositive()
    {
        var text = StatPreviewText.Of(new StatPreview(_current, _current with { Attack = 15 }));

        Assert.Equal(new StatPreviewText.PreviewValue("15", UiTheme.Positive), text.Attack);
    }

    [Fact]
    public void Of_RemovingArmor_DefenseIsNegative()
    {
        var text = StatPreviewText.Of(new StatPreview(_current, _current with { Defense = 5 }));

        Assert.Equal(new StatPreviewText.PreviewValue("5", UiTheme.Negative), text.Defense);
    }

    [Fact]
    public void Of_NoChange_IsPrimaryText()
    {
        var text = StatPreviewText.Of(new StatPreview(_current, _current));

        Assert.Equal(UiTheme.TextPrimary, text.Attack.Color);
        Assert.Equal(UiTheme.TextPrimary, text.Defense.Color);
    }
}
