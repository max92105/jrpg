using Microsoft.Xna.Framework;

namespace Jrpg.Game.Menus;

/// <summary>
/// The Inventory page's strip under the selected item: what the last use did, until the player moves on, otherwise
/// what the item is.
/// </summary>
internal sealed class ItemDetailLine
{
    /// <summary>The last use's result, until the player moves on; null otherwise.</summary>
    public string? Notice { get; private set; }

    public void ShowNotice(string notice) => Notice = notice;

    public void ClearNotice() => Notice = null;

    /// <param name="selected">The selected row; null when the category is empty.</param>
    /// <returns>The notice, else the selected item's description; null when there is neither.</returns>
    public string? TextFor(InventoryRow? selected) => Notice ?? selected?.Description;

    /// <summary>
    /// Where the strip goes: right under the selected row, over the rows below, or above it when that would cross the
    /// list's bottom, so it never leaves the list.
    /// </summary>
    /// <param name="selectedRow">The selected row's area, at the glide's place.</param>
    /// <param name="listBottom">The visible rows' bottom edge.</param>
    /// <param name="stripHeight">The strip's height, in pixels.</param>
    public static int TopFor(Rectangle selectedRow, int listBottom, int stripHeight) =>
        selectedRow.Bottom + stripHeight <= listBottom ? selectedRow.Bottom : selectedRow.Y - stripHeight;
}
