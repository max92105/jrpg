namespace Jrpg.Core.Trade;

/// <summary>One trade as offered, worked out before anything changes so the screen only shows it.</summary>
/// <param name="Gold">What the trade costs (a purchase) or gives (a sale), for the whole quantity.</param>
/// <param name="Refusal">Why it can't be done; null when it can.</param>
public sealed record TradeQuote(int Gold, TradeRefusal? Refusal)
{
    public bool IsRefused => Refusal is not null;
}
