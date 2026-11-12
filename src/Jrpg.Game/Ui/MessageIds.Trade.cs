namespace Jrpg.Game.Ui;

// The Shop.
internal static partial class MessageIds
{
    /// <summary>
    /// Placeholders <c>{previous}</c> and <c>{next}</c> (the tab buttons' labels): the Shop footer's line.
    /// </summary>
    public const string ShopHint = "shop_hint";

    /// <summary>The Shop's line while its stock holds nothing to buy.</summary>
    public const string ShopEmpty = "shop_empty";

    /// <summary>The Shop's Buy tab.</summary>
    public const string ShopBuy = "shop_buy";

    /// <summary>The Shop's Sell tab.</summary>
    public const string ShopSell = "shop_sell";

    /// <summary>The Sell list's line while the party owns nothing to sell.</summary>
    public const string ShopNothingToSell = "shop_nothing_to_sell";

    /// <summary>The Shop lists' column titles: the item, how many are owned, the price.</summary>
    public const string ShopColumnItem = "shop_column_item";

    public const string ShopColumnOwned = "shop_column_owned";

    public const string ShopColumnPrice = "shop_column_price";

    /// <summary>Placeholder <c>{count}</c>: the detail column's owned line.</summary>
    public const string ShopOwned = "shop_owned";

    /// <summary>
    /// Why a trade is refused, under the greyed row (decision 370): the purse holds less than the price.
    /// </summary>
    public const string ShopRefusedShortOfGold = "shop_refused_short_of_gold";

    /// <summary>Why a trade is refused: the stack can't take more copies.</summary>
    public const string ShopRefusedStackFull = "shop_refused_stack_full";

    /// <summary>Why a trade is refused: the item has no price, or its sell share is worth nothing.</summary>
    public const string ShopRefusedNotForSale = "shop_refused_not_for_sale";

    /// <summary>Placeholder <c>{name}</c>: the notice after a purchase (decision 380).</summary>
    public const string ShopBought = "shop_bought";

    /// <summary>Placeholders <c>{name}</c> and <c>{count}</c>: the notice after buying copies of a stack.</summary>
    public const string ShopBoughtCount = "shop_bought_count";

    /// <summary>The quantity picker's label (decision 377).</summary>
    public const string ShopQuantity = "shop_quantity";

    /// <summary>Placeholder <c>{name}</c>: the question before a piece is sold (decision 379).</summary>
    public const string ShopSellConfirm = "shop_sell_confirm";

    /// <summary>Placeholder <c>{name}</c>: the notice after selling a piece.</summary>
    public const string ShopSold = "shop_sold";

    /// <summary>Placeholders <c>{name}</c> and <c>{count}</c>: the notice after selling copies of a stack.</summary>
    public const string ShopSoldCount = "shop_sold_count";
}
