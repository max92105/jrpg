using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

/// <summary>The Inventory page's tabs by what they are, so a test never counts the presses to reach one.</summary>
internal static class InventoryPageTabs
{
    /// <summary>Presses the next-tab button until the category shows.</summary>
    public static void Show(InventoryPage page, InventoryCategory category)
    {
        while (page.Category != category)
        {
            page.Update(Pressing(GameButton.NextTab));
        }
    }
}
