using Jrpg.Core.Stats;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class StatComparisonTextTests
{
    private static readonly UiMessages _messages = RepositoryScreenText.Text.Messages;

    [Fact]
    public void Of_LowerFireResistance_IsDownInPercent()
    {
        var comparison = new StatComparison([new StatComparison.Change(MessageIds.StatFireResistance, StatKind.FireResistance, 100, 50)]);

        var line = Assert.Single(StatComparisonText.Of(comparison, _messages).Lines);

        Assert.Equal(("10 %", "5 %", UiTheme.Negative), (line.Before, line.After, line.Color));
    }

    [Fact]
    public void Of_AttackUp_IsAPlainNumberInTheRiseColor()
    {
        var comparison = new StatComparison([new StatComparison.Change(MessageIds.StatAttack, Kind: null, 10, 18)]);

        var line = Assert.Single(StatComparisonText.Of(comparison, _messages).Lines);

        Assert.Equal(("10", "18", UiTheme.Positive), (line.Before, line.After, line.Color));
    }

    [Fact]
    public void Of_NineChanges_ShowsEightAndSaysOneMore()
    {
        var changes = Enum.GetValues<StatKind>().Take(9).Select(kind => new StatComparison.Change(StatNames.IdOf(kind), kind, 0, 1));

        var text = StatComparisonText.Of(new StatComparison([.. changes]), _messages);

        Assert.Equal(StatComparisonText.MostLines, text.Lines.Length);
        Assert.Equal(_messages.Format(MessageIds.EquipmentMoreChanges, MessageArgument.Number("count", 1)), text.More);
    }

    [Fact]
    public void Of_EightChanges_SaysNoMore()
    {
        var changes = Enum.GetValues<StatKind>().Take(8).Select(kind => new StatComparison.Change(StatNames.IdOf(kind), kind, 0, 1));

        Assert.Null(StatComparisonText.Of(new StatComparison([.. changes]), _messages).More);
    }
}
