using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Core.Stats;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

// Gear is one row per piece, after the stacks (ADR 0029, decision 345).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class InventoryPageGearTests
{
    private static readonly ItemDefinition _bronzeSword = StartMapScenarios.BelowClosedDoor.Items["bronze_sword"];

    [Fact]
    public void Open_WeaponsTab_ListsEachSwordPiece()
    {
        var session = NewSession();
        session.Inventory.AddGear(GearPiece.Rolled(_bronzeSword, Rarity.Magic, []));
        var page = NewPage(session);

        ShowCategory(page, InventoryCategory.Weapons);

        Assert.Equal(session.Inventory.GearCountOf("bronze_sword"), page.ShownItemIds.Count(id => id == "bronze_sword"));
    }

    // The sword is picked up before the potion stack grows, yet the stacks come first.
    [Fact]
    public void Open_AllTab_ListsStacksThenPieces()
    {
        var session = NewSession();
        session.Inventory.Add(_bronzeSword, 1);
        session.Inventory.Add(StartMapScenarios.BelowClosedDoor.Items["potion"], 1);

        var page = NewPage(session);

        var shown = page.ShownItemIds.ToList();
        Assert.True(shown.LastIndexOf("potion") < shown.IndexOf("bronze_sword"));
    }

    [Fact]
    public void Confirm_OnPiece_DoesNothing()
    {
        var session = NewSession();
        var page = NewPage(session);
        ShowCategory(page, InventoryCategory.Weapons);
        var swords = session.Inventory.GearCountOf(page.ShownItemIds[0]);

        page.Update(Pressing(GameButton.Confirm));

        Assert.False(page.IsChoosingTarget);
        Assert.Null(page.Notice);
        Assert.Equal(swords, session.Inventory.GearCountOf(page.ShownItemIds[0]));
    }

    // One row per piece, so a piece shows no count; a stack keeps its own.
    [Fact]
    public void Show_PieceRow_HasNoQuantity()
    {
        var session = NewSession();
        var list = new InventoryList(session, StartMapScenarios.BelowClosedDoor, RepositoryScreenText.Text.Fonts.FontAt(26), RepositoryScreenText.Text.Messages);

        list.Show(InventoryCategory.All, selectedRow: 0);

        Assert.Null(list.Rows.First(row => row.ItemId == "bronze_sword").Quantity);
        Assert.NotNull(list.Rows.First(row => row.ItemId == "potion").Quantity);
    }

    // A rolled piece's strip shows its rarity and affixes, not its description (decision 361); a plain one keeps it.
    [Fact]
    public void Show_RolledAndPlainPieces_RolledRowIsRareWithItsAffixLine()
    {
        var session = NewSession();
        var rolled = GearPiece.Rolled(_bronzeSword, Rarity.Rare, [new RolledAffix("power", StatKind.Power, 4)]);
        session.Inventory.AddGear(GearPiece.Plain(_bronzeSword));
        session.Inventory.AddGear(rolled);
        var list = new InventoryList(session, StartMapScenarios.BelowClosedDoor, RepositoryScreenText.Text.Fonts.FontAt(26), RepositoryScreenText.Text.Messages);

        list.Show(InventoryCategory.Weapons, selectedRow: 0);

        var rows = list.Rows.Where(row => row.ItemId == "bronze_sword").ToList();
        Assert.Equal((Rarity.Rare, "Rare · +4 Power"), (rows[^1].Rarity, rows[^1].Description));
        Assert.Equal((Rarity.Common, _bronzeSword.Description), (rows[0].Rarity, rows[0].Description));
    }

    // Goes through the tabs with the next-tab button until the category shows.
    private static void ShowCategory(InventoryPage page, InventoryCategory category)
    {
        while (page.Category != category)
        {
            page.Update(Pressing(GameButton.NextTab));
        }
    }

    private static GameSession NewSession() => StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);

    private static InventoryPage NewPage(GameSession session) =>
        new(RepositoryScreenText.Text, new UiTextures(), session, StartMapScenarios.BelowClosedDoor);
}
