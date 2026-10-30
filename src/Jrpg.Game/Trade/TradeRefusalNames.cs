using Jrpg.Core.Trade;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Trade;

/// <summary>Each refusal's line in ui.json (ADR 0010), shown under a greyed row (decision 370).</summary>
internal static class TradeRefusalNames
{
    public static string IdOf(TradeRefusal refusal) => refusal switch
    {
        TradeRefusal.StackFull => MessageIds.ShopRefusedStackFull,
        TradeRefusal.ShortOfGold => MessageIds.ShopRefusedShortOfGold,
        TradeRefusal.NotForSale => MessageIds.ShopRefusedNotForSale,
        _ => throw new ArgumentOutOfRangeException(nameof(refusal), refusal, "A refusal with no line in ui.json."),
    };
}
