using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// One line on the Inventory page, a stack or a gear piece, formatted and measured when the items change.
/// </summary>
/// <param name="ItemId">The item's catalog id: a piece's base.</param>
/// <param name="Name">Its name.</param>
/// <param name="IconFile">Its icon's file name.</param>
/// <param name="Quantity">
/// How many the party owns, measured for right-aligning; null for a piece, one row each (decision 345).
/// </param>
/// <param name="Description">What it is, shown under the list while it is selected.</param>
/// <param name="IsUsable">Whether it can be used from the menu; an unusable item is drawn greyed out.</param>
internal sealed record InventoryRow(string ItemId, string Name, string IconFile, MeasuredText? Quantity, string Description, bool IsUsable);
