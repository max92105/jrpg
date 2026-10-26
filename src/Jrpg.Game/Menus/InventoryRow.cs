using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// One owned item's line as the Inventory page shows it, built when the category or the items change so drawing formats
/// nothing.
/// </summary>
internal sealed record InventoryRow(string ItemId, string Name, string IconFile, MeasuredText Quantity, string Description, bool IsUsable);
