namespace Jrpg.Core.Trade;

/// <summary>What trading gives, from <c>rules.json</c> <c>trade</c> (ADR 0031); every number is content's.</summary>
public sealed record TradeRules
{
    /// <summary>
    /// The share of an item's price a sale pays, a whole percent. 0 by default, so every sale is refused until content
    /// says otherwise, as empty crafting rules refuse every Forge action.
    /// </summary>
    public int SellPercent { get; init; }
}
