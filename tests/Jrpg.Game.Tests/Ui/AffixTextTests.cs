using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Game.Tests.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class AffixTextTests
{
    private static readonly UiMessages _messages = RepositoryScreenText.Text.Messages;

    [Fact]
    public void Of_Primary_ReadsPlusPoints()
    {
        var text = AffixText.Of(new RolledAffix("power", StatKind.Power, 4), _messages);

        Assert.Equal("+4 Power", text);
    }

    // A percent stat's affix is in whole percents, as an item's bonus (ADR 0026), not permille.
    [Fact]
    public void Of_PercentStat_ReadsPlusPercent()
    {
        var text = AffixText.Of(new RolledAffix("crit_chance", StatKind.CritChance, 2), _messages);

        Assert.Equal("+2 % Crit chance", text);
    }

    [Fact]
    public void Of_NegativeValue_KeepsItsMinusOnly()
    {
        var text = AffixText.Of(new RolledAffix("frail", StatKind.Constitution, -3), _messages);

        Assert.Equal("-3 Constitution", text);
    }

    [Fact]
    public void LineOf_RarePiece_ListsRarityThenAffixesInOrder()
    {
        var piece = GearPiece.Rolled(
            BattleScenarios.Catalog.Items["bronze_sword"],
            Rarity.Rare,
            [new RolledAffix("power", StatKind.Power, 4), new RolledAffix("crit_chance", StatKind.CritChance, 2)]);

        var line = AffixText.LineOf(piece, _messages);

        Assert.Equal("Rare · +4 Power · +2 % Crit chance", line);
    }
}
