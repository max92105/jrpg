namespace Jrpg.Game.Menus;

/// <summary>One category on the Inventory page's rail: what it filters, its heading and its icon.</summary>
internal sealed record CategoryTab(InventoryCategory Category, string LabelId, string IconFile);
