using Jrpg.Core.Items;
using Jrpg.Core.Trade;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Trade;

/// <summary>
/// One stock entry as the Buy list shows it, formatted and measured once when the stock, the bag or the gold changes.
/// </summary>
/// <param name="Item">The base item: the stack sold by the copy, or a rolled piece's base.</param>
/// <param name="Piece">The rolled piece this row sells; null for a stack (decision 385).</param>
/// <param name="Name">The item's name, measured.</param>
/// <param name="Owned">
/// How many the party owns, measured for right-aligning: a stack's count, or the owned pieces of that base.
/// </param>
/// <param name="Price">What one costs, measured for right-aligning after the coin.</param>
/// <param name="Refusal">Why Core refuses buying one; null when it can be bought (decision 370).</param>
internal sealed record StockRow(ItemDefinition Item, GearPiece? Piece, MeasuredText Name, MeasuredText Owned, MeasuredText Price, TradeRefusal? Refusal)
{
    public bool IsAvailable => Refusal is null;
}
