using Jrpg.Core.Session;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Trade;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Trade;

// Opens the start map's general store and reaches rows and categories by what they are, never by a count of presses.
internal static class ShopDriver
{
    public static (ShopScene Shop, GameSession Session) OpenGeneralStore()
    {
        var session = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor);
        return (new ShopScene(RepositoryScreenText.Text, new UiTextures(), session, session.ShopFor("general_store")), session);
    }

    // Fails rather than loops forever when no row matches: the cursor wraps, so coming back to the start means none.
    public static void MoveDownUntil(ShopScene shop, Func<TradeRow, bool> wanted)
    {
        var start = shop.SelectedRow;
        while (shop.SelectedRow is not { } row || !wanted(row))
        {
            shop.Update(TimeSpan.Zero, Pressing(GameButton.Down));
            Assert.NotSame(start, shop.SelectedRow);
        }
    }

    // The rail wraps too, so coming back to the category it started on fails the loop.
    public static void ShowCategory(ShopScene shop, InventoryCategory wanted)
    {
        var start = shop.Category;
        while (shop.Category != wanted)
        {
            shop.Update(TimeSpan.Zero, Pressing(GameButton.NextTab));
            Assert.NotEqual(start, shop.Category);
        }
    }
}
