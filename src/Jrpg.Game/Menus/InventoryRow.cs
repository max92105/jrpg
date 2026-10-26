using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>One owned item's line on the Inventory page, formatted and measured when the items change.</summary>
/// <param name="ItemId">The item's catalog id.</param>
/// <param name="Name">Its name.</param>
/// <param name="IconFile">Its icon's file name.</param>
/// <param name="Quantity">How many the party owns, measured for right-aligning.</param>
/// <param name="Description">What it is, shown under the list while it is selected.</param>
/// <param name="IsUsable">Whether it can be used from the menu; an unusable item is drawn greyed out.</param>
internal sealed record InventoryRow(string ItemId, string Name, string IconFile, MeasuredText Quantity, string Description, bool IsUsable);
