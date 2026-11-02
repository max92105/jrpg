using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Session;
using Jrpg.Core.Stats;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Trade;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Trade;

// What a question's Yes carries out sounds, the question doesn't (decision 435): the coins once Core dealt; the picker,
// a value chosen, ticks on a change and sounds its Cancel; the sale question is silent, its No included. Read through
// the scene, as the flow reads it.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ShopSceneDealSoundTests
{
    private static readonly IReadOnlyDictionary<string, ItemDefinition> _items = StartMapScenarios.BelowClosedDoor.Items;

    [Fact]
    public void Update_ConfirmOnAStockPiece_AsksTheDealSound()
    {
        var shop = ShopInTheBuyList(Purse.MaxGold);
        ShopDriver.MoveDownUntil(shop, row => row.Piece is not null);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.NotNull(shop.Notice);
        Assert.Equal([UiSoundFiles.Effects.Deal], shop.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnAStackRow_AsksTheConfirmSound()
    {
        var shop = ShopOnThePotionPicker(Purse.MaxGold);

        Assert.NotNull(shop.Picker);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], shop.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnASellStackRow_AsksTheConfirmSound()
    {
        var shop = ShopOnASellStack();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.NotNull(shop.Picker);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], shop.Sounds);
    }

    [Fact]
    public void Update_UpInThePicker_AsksTheMoveSound()
    {
        var shop = ShopOnThePotionPicker(Purse.MaxGold);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));

        Assert.Equal(2, shop.Picker!.Count);
        Assert.Equal([UiSoundFiles.Effects.MenuMove], shop.Sounds);
    }

    // Gold for one potion only: Up wraps back to the same count.
    [Fact]
    public void Update_UpInAPickerOfOne_AsksNothing()
    {
        var shop = ShopOnThePotionPicker(OnePotionsPrice());

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));

        Assert.Equal(1, shop.Picker!.Count);
        Assert.Empty(shop.Sounds);
    }

    [Fact]
    public void Update_CancelInThePicker_AsksTheCancelSound()
    {
        var shop = ShopOnThePotionPicker(Purse.MaxGold);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Cancel));

        Assert.Null(shop.Picker);
        Assert.Equal([UiSoundFiles.Effects.MenuCancel], shop.Sounds);
    }

    [Fact]
    public void Update_YesInTheBuyPicker_AsksTheDealSound()
    {
        var shop = ShopOnThePotionPicker(Purse.MaxGold);

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.NotNull(shop.Notice);
        Assert.Equal([UiSoundFiles.Effects.Deal], shop.Sounds);
    }

    [Fact]
    public void Update_YesInTheSellPicker_AsksTheDealSound()
    {
        var shop = ShopOnASellStack();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.NotNull(shop.Notice);
        Assert.Equal([UiSoundFiles.Effects.Deal], shop.Sounds);
    }

    [Fact]
    public void Update_ConfirmOnAnOwnedPiece_AsksNothing()
    {
        var shop = ShopOnTheRolledSword();

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.NotNull(shop.Prompt);
        Assert.Empty(shop.Sounds);
    }

    // The question starts on No; moving to Yes is silent too.
    [Fact]
    public void Update_NoInTheSaleQuestion_AsksNothing()
    {
        var shop = ShopOnTheRolledSword();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));
        var movedToYes = shop.Sounds.ToList();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.Empty(movedToYes);
        Assert.Null(shop.Prompt);
        Assert.Null(shop.Notice);
        Assert.Empty(shop.Sounds);
    }

    [Fact]
    public void Update_YesInTheSaleQuestion_AsksTheDealSound()
    {
        var shop = ShopOnTheRolledSword();
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Up));

        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        Assert.NotNull(shop.Notice);
        Assert.Equal([UiSoundFiles.Effects.Deal], shop.Sounds);
    }

    // The counter writes into the scene's list, so the scene's clear covers its asks.
    [Fact]
    public void Update_UpdateAfterADeal_AsksNothing()
    {
        var shop = ShopOnThePotionPicker(Purse.MaxGold);
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));

        shop.Update(TimeSpan.Zero, default);

        Assert.Empty(shop.Sounds);
    }

    // The rows quote their refusal when built, so the purse is set before the Shop opens.
    private static ShopScene ShopInTheBuyList(int gold)
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        Assert.True(session.Party.Purse.TrySpend(session.Party.Purse.Gold));
        session.Party.Purse.Add(gold);
        var shop = new ShopScene(RepositoryScreenText.Text, new UiTextures(), session, session.ShopFor("general_store"));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        return shop;
    }

    private static ShopScene ShopOnThePotionPicker(int gold)
    {
        var shop = ShopInTheBuyList(gold);
        ShopDriver.MoveDownUntil(shop, row => row.Item.Id == "potion" && row.Piece is null);
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        return shop;
    }

    private static int OnePotionsPrice()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        return session.ShopFor("general_store").QuoteBuy(_items["potion"], 1).Gold;
    }

    private static ShopScene ShopInTheSellList(Action<GameSession>? own = null)
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        own?.Invoke(session);
        var shop = new ShopScene(RepositoryScreenText.Text, new UiTextures(), session, session.ShopFor("general_store"));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Right));
        shop.Update(TimeSpan.Zero, Pressing(GameButton.Confirm));
        return shop;
    }

    private static ShopScene ShopOnASellStack()
    {
        var shop = ShopInTheSellList();
        ShopDriver.MoveDownUntil(shop, row => row.Piece is null && row.IsAvailable);
        return shop;
    }

    private static ShopScene ShopOnTheRolledSword()
    {
        var sword = GearPiece.Rolled(_items["bronze_sword"], Rarity.Magic, [new RolledAffix("power", StatKind.Power, 2)]);
        var shop = ShopInTheSellList(session => session.Inventory.AddGear(sword));
        ShopDriver.MoveDownUntil(shop, row => ReferenceEquals(row.Piece, sword));
        return shop;
    }
}
