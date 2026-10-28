using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class VictoryDropsViewTests
{
    private static readonly ItemDefinition _potion = BattleScenarios.Catalog.Items["potion"];
    private static readonly ItemDefinition _sword = BattleScenarios.Catalog.Items["bronze_sword"];

    [Fact]
    public void RowsOf_StackAndPiece_StackHasQuantityPieceNone()
    {
        var drops = new Drops([new ItemDrop(_potion, 3)], [GearPiece.Plain(_sword)]);

        var rows = VictoryDropsView.RowsOf(drops, RepositoryScreenText.Text.Messages, RepositoryScreenText.Text.Fonts.FontAt(26));

        Assert.Equal([_potion.Name, _sword.Name], rows.Select(row => row.Name));
        Assert.Equal("×3", rows[0].Tag?.Text);
        Assert.Null(rows[1].Tag);
    }

    // One more than fits: the last row that fits counts itself and the one past it.
    [Fact]
    public void RowsOf_MoreThanFit_LastRowCountsTheRest()
    {
        var drops = new Drops([], [.. Enumerable.Repeat(GearPiece.Plain(_sword), VictoryDropsView.Capacity + 1)]);

        var rows = VictoryDropsView.RowsOf(drops, RepositoryScreenText.Text.Messages, RepositoryScreenText.Text.Fonts.FontAt(26));

        Assert.Equal(VictoryDropsView.Capacity, rows.Count);
        Assert.Equal(("+2 more", null), (rows[^1].Name, rows[^1].IconFile));
    }

    // Exactly as many as fit: every drop shows, with no "+n more" row.
    [Fact]
    public void RowsOf_ExactlyCapacity_ShowsEveryDrop()
    {
        var drops = new Drops([], [.. Enumerable.Repeat(GearPiece.Plain(_sword), VictoryDropsView.Capacity)]);

        var rows = VictoryDropsView.RowsOf(drops, RepositoryScreenText.Text.Messages, RepositoryScreenText.Text.Fonts.FontAt(26));

        Assert.Equal(VictoryDropsView.Capacity, rows.Count);
        Assert.All(rows, row => Assert.Equal(_sword.Name, row.Name));
    }

    // A rolled piece names its rarity where a stack shows its count; a plain one names nothing there.
    [Fact]
    public void RowsOf_RarePiece_IsRareAndNamed()
    {
        var drops = new Drops([], [GearPiece.Rolled(_sword, Rarity.Rare, []), GearPiece.Plain(_sword)]);

        var rows = VictoryDropsView.RowsOf(drops, RepositoryScreenText.Text.Messages, RepositoryScreenText.Text.Fonts.FontAt(26));

        Assert.Equal((Rarity.Rare, "Rare"), (rows[0].Rarity, rows[0].Tag?.Text));
        Assert.Equal((Rarity.Common, null), (rows[1].Rarity, rows[1].Tag?.Text));
    }
}
