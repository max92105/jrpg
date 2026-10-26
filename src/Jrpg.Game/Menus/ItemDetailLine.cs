using Microsoft.Xna.Framework;

namespace Jrpg.Game.Menus;

/// <summary>
/// The strip under the Inventory page's selected item: what the last use did, until the player moves on; otherwise the
/// item's description.
/// </summary>
internal sealed class ItemDetailLine
{
    /// <summary>The last use's result, until the player moves on; null otherwise.</summary>
    public string? Notice { get; private set; }

    public void ShowNotice(string notice) => Notice = notice;

    public void ClearNotice() => Notice = null;

    /// <param name="selected">The selected row; null when the category is empty.</param>
    /// <returns>The notice if there is one, else the selected item's description, else nothing.</returns>
    public string TextFor(InventoryRow? selected) => Notice ?? selected?.Description ?? string.Empty;

    /// <summary>
    /// Where the strip goes: right under the selected row, or right above it when there's no room left under it before
    /// the list's bottom, so it never leaves the list.
    /// </summary>
    /// <param name="selectedRow">The selected row's band, where the glide has it.</param>
    /// <param name="listBottom">The bottom of the list's last visible row.</param>
    /// <param name="stripHeight">The strip's height in pixels.</param>
    public static int TopFor(Rectangle selectedRow, int listBottom, int stripHeight) =>
        selectedRow.Bottom + stripHeight <= listBottom ? selectedRow.Bottom : selectedRow.Y - stripHeight;
}
