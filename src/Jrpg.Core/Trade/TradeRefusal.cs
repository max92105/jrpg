namespace Jrpg.Core.Trade;

/// <summary>Why a trade is refused; a refused trade changes nothing.</summary>
public enum TradeRefusal
{
    /// <summary>The stack can't take that many more copies.</summary>
    StackFull,

    /// <summary>The purse holds less than the total.</summary>
    ShortOfGold,
}
