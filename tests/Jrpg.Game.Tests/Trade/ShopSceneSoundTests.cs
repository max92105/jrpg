using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Trade;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Trade;

// Move, confirm and cancel in the Shop, as at the Forge; a press that changes nothing and a greyed row are silent
// (decision 421). The deal's own sounds are the counter's.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ShopSceneSoundTests
{
    [Fact]
    public void Update_RightOnTheBuyTab_AsksTheMoveSound()
    {
        var (shop, _) = ShopDriver.OpenGeneralStore();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        Assert.Equal([UiSoundFiles.Effects.MenuMove], shop.Sounds);
    }

    // The tabs don't wrap, so Right on Sell changes nothing.
    [Fact]
    public void Update_RightOnTheSellTab_AsksNothing()
    {
        var (shop, _) = ShopDriver.OpenGeneralStore();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        Assert.Equal(ShopTab.Sell, shop.Tab);
        Assert.Empty(shop.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnTheTabs_AsksTheConfirmSound()
    {
        var (shop, _) = ShopDriver.OpenGeneralStore();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], shop.Sounds);
    }

    [Fact]
    public void Update_CancelOnTheTabs_AsksTheCancelSound()
    {
        var (shop, _) = ShopDriver.OpenGeneralStore();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));

        Assert.Equal([UiSoundFiles.Effects.MenuCancel], shop.Sounds);
    }

    [Fact]
    public void Update_DownInAList_AsksTheMoveSound()
    {
        var shop = ShopInTheBuyList();
        var first = shop.SelectedRow;

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        Assert.NotSame(first, shop.SelectedRow);
        Assert.Equal([UiSoundFiles.Effects.MenuMove], shop.Sounds);
    }

    // The bag holds no materials at the start, so the Sell list's Materials category has no row to move to.
    [Fact]
    public void Update_DownInAnEmptyList_AsksNothing()
    {
        var (shop, _) = ShopDriver.OpenGeneralStore();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        ShopDriver.ShowCategory(shop, InventoryCategory.Materials);
        Assert.Null(shop.SelectedRow);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        Assert.Empty(shop.Sounds);
    }

    [Fact]
    public void Update_NextTabButtonInAList_AsksTheMoveSound()
    {
        var shop = ShopInTheBuyList();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.NextTab));

        Assert.NotEqual(InventoryCategory.All, shop.Category);
        Assert.Equal([UiSoundFiles.Effects.MenuMove], shop.Sounds);
    }

    [Fact]
    public void Update_RightOnAGearRowWithTwoMembers_AsksTheMoveSound()
    {
        var (shop, session) = ShopDriver.OpenGeneralStore();
        Assert.True(session.Party.TryAddToActive(session.Party.ReserveMembers[0]));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        ShopDriver.MoveDownUntil(shop, row => row.Piece is not null);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        Assert.Same(session.Party.ActiveMembers[1], shop.PreviewedMember);
        Assert.Equal([UiSoundFiles.Effects.MenuMove], shop.Sounds);
    }

    // A stack row shows no member, so switching one there changes nothing on screen.
    [Fact]
    public void Update_RightOnAStackRowWithTwoMembers_AsksNothing()
    {
        var (shop, session) = ShopDriver.OpenGeneralStore();
        Assert.True(session.Party.TryAddToActive(session.Party.ReserveMembers[0]));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        Assert.Null(shop.SelectedRow!.Piece);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        Assert.Empty(shop.Sounds);
    }

    // With the leader alone active, Right comes back to the same member.
    [Fact]
    public void Update_RightWithOneActiveMember_AsksNothing()
    {
        var (shop, session) = ShopDriver.OpenGeneralStore();
        Assert.Single(session.Party.ActiveMembers);
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        ShopDriver.MoveDownUntil(shop, row => row.Piece is not null);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));

        Assert.Empty(shop.Sounds);
    }

    [Fact]
    public void Update_CancelInAList_AsksTheCancelSound()
    {
        var shop = ShopInTheBuyList();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));

        Assert.True(shop.IsOnTabs);
        Assert.Equal([UiSoundFiles.Effects.MenuCancel], shop.Sounds);
    }

    // An empty purse greys every stock row, so Confirm opens nothing; the rows quote their refusal when built, so the
    // purse is emptied before the Shop opens.
    [Fact]
    public void Update_ConfirmOnAGreyedRow_AsksNothing()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        Assert.True(session.Party.Purse.TrySpend(session.Party.Purse.Gold));
        var shop = new ShopScene(RepositoryScreenText.Text, new UiTextures(), session, session.ShopFor("general_store"));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        Assert.False(shop.SelectedRow!.IsAvailable);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Null(shop.Picker);
        Assert.Empty(shop.Sounds);
    }

    [Fact]
    public void Update_UpdateAfterAMove_AsksNothing()
    {
        var shop = ShopInTheBuyList();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        shop.Update(TimeSpan.Zero, default);

        Assert.Empty(shop.Sounds);
    }

    private static ShopScene ShopInTheBuyList()
    {
        var (shop, _) = ShopDriver.OpenGeneralStore();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        return shop;
    }
}
