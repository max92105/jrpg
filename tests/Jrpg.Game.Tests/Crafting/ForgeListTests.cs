using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Game.Crafting;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Crafting;

// The Forge works owned pieces only (decision 369): the list is Inventory.Gear in pickup order, stacks left out.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ForgeListTests
{
    private static readonly ItemDefinition _bronzeSword = StartMapScenarios.BelowClosedDoor.Items["bronze_sword"];
    private static readonly ItemDefinition _woodenShield = StartMapScenarios.BelowClosedDoor.Items["wooden_shield"];

    [Fact]
    public void Show_OwnedGear_ListsEveryPieceInPickupOrder()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        session.Inventory.Add(StartMapScenarios.BelowClosedDoor.Items["potion"], 3);
        var shield = GearPiece.Rolled(_woodenShield, Rarity.Magic, []);
        var sword = GearPiece.Plain(_bronzeSword);
        session.Inventory.AddGear(shield);
        session.Inventory.AddGear(sword);
        var list = NewList(session);

        list.Show(0);

        Assert.Equal(session.Inventory.Gear, list.Rows.Select(row => row.Piece));
        Assert.Equal(list.Rows[0].Piece, list.SelectedPiece);
    }

    [Fact]
    public void Show_NoPieces_HasNoCursor()
    {
        var list = NewList(ForgeSessions.WithoutLoosePieces());

        list.Show(0);

        Assert.Null(list.Cursor);
        Assert.Null(list.SelectedPiece);
    }

    [Fact]
    public void MoveDown_HighlightsTheNextPiece()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        session.Inventory.AddGear(GearPiece.Plain(_bronzeSword));
        session.Inventory.AddGear(GearPiece.Rolled(_woodenShield, Rarity.Rare, []));
        var list = NewList(session);
        list.Show(0);
        var first = list.SelectedPiece;

        list.MoveDown();

        Assert.Equal(list.Rows[1].Piece, list.SelectedPiece);
        Assert.NotEqual(first, list.SelectedPiece);
    }

    // One line per affix, in roll order, so the detail column draws them as rows (decision 361).
    [Fact]
    public void Show_RolledPiece_RowHasItsAffixLines()
    {
        var session = ForgeSessions.WithoutLoosePieces();
        var power = new RolledAffix("power", StatKind.Power, 4);
        var agility = new RolledAffix("agility", StatKind.Agility, 2);
        session.Inventory.AddGear(GearPiece.Rolled(_bronzeSword, Rarity.Rare, [power, agility]));
        var list = NewList(session);

        list.Show(0);

        var messages = RepositoryScreenText.Text.Messages;
        Assert.Equal([AffixText.Of(power, messages), AffixText.Of(agility, messages)], list.Rows[0].AffixLines);
        Assert.Equal(messages.Text(MessageIds.RarityRare), list.Rows[0].RarityName);
    }

    private static ForgeList NewList(Core.Session.GameSession session) => new(session.Inventory, RepositoryScreenText.Text.Messages);
}
