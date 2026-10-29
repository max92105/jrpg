using Jrpg.Core.Items;
using Jrpg.Game.Crafting;
using Jrpg.Game.Tests.Exploration;

namespace Jrpg.Game.Tests.Crafting;

// The Forge works owned pieces only (decision 369): the list is Inventory.Gear in pickup order, stacks left out.
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
        var list = new ForgeList(session.Inventory);

        list.Show(0);

        Assert.Equal(session.Inventory.Gear, list.Pieces);
        Assert.Equal(list.Pieces[0], list.SelectedPiece);
    }

    [Fact]
    public void Show_NoPieces_HasNoCursor()
    {
        var list = new ForgeList(ForgeSessions.WithoutLoosePieces().Inventory);

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
        var list = new ForgeList(session.Inventory);
        list.Show(0);
        var first = list.SelectedPiece;

        list.MoveDown();

        Assert.Equal(list.Pieces[1], list.SelectedPiece);
        Assert.NotEqual(first, list.SelectedPiece);
    }

}
