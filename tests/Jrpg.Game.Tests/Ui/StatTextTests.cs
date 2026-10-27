using Jrpg.Core.Stats;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class StatTextTests
{
    private static readonly UiMessages _messages = RepositoryScreenText.Text.Messages;

    [Fact]
    public void ValueOf_FireResistance_EndsWithPercent()
    {
        Assert.Equal("25 %", StatText.ValueOf(StatKind.FireResistance, 250, _messages));
    }

    [Fact]
    public void ValueOf_NinetySevenPermille_ShowsNinePercent()
    {
        Assert.Equal("9 %", StatText.ValueOf(StatKind.CritChance, 97, _messages));
    }

    [Fact]
    public void ValueOf_Power_IsTheNumber()
    {
        Assert.Equal("15", StatText.ValueOf(StatKind.Power, 15, _messages));
    }

    [Fact]
    public void ValueOf_MaxHp_IsTheNumber()
    {
        Assert.Equal("250", StatText.ValueOf(StatKind.MaxHp, 250, _messages));
    }
}
