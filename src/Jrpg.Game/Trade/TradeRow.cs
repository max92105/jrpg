using Jrpg.Core.Items;
using Jrpg.Core.Trade;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Trade;

/// <summary>
/// One row of the Buy or the Sell list, formatted and measured once when the stock, the bag or the gold changes.
/// </summary>
/// <param name="Item">The base item: a stack, or a piece's base.</param>
/// <param name="Piece">The one piece this row trades; null for a stack (decision 385).</param>
/// <param name="Name">The item's name, measured.</param>
/// <param name="Owned">
/// How many the party owns, measured for right-aligning: a stack's count, or the owned pieces of that base; empty for a
/// piece sold.
/// </param>
/// <param name="Price">What one costs or gives, measured for right-aligning after the coin.</param>
/// <param name="Refusal">Why Core refuses trading one; null when it can be traded (decision 370).</param>
internal sealed record TradeRow(ItemDefinition Item, GearPiece? Piece, MeasuredText Name, MeasuredText Owned, MeasuredText Price, TradeRefusal? Refusal)
{
    public bool IsAvailable => Refusal is null;
}
