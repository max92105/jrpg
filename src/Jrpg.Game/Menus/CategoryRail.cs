using System.Collections.Immutable;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The Inventory page's category rail: which category the list shows, and whether the cursor is on the rail rather
/// than on the list (question 152). Up and Down wrap round the rail.
/// </summary>
internal sealed class CategoryRail
{
    private readonly ListCursor _cursor;

    public CategoryRail()
    {
        _cursor = new ListCursor(Tabs.Count, _ => true);
    }

    /// <summary>The rail, top to bottom, in one place so the order of categories, headings and icons can't drift.</summary>
    public IReadOnlyList<CategoryTab> Tabs { get; } = ImmutableArray.Create(
        new CategoryTab(InventoryCategory.All, MessageIds.InventoryAll, UiArtFiles.Icons.AllItems),
        new CategoryTab(InventoryCategory.Consumables, MessageIds.InventoryConsumables, UiArtFiles.Icons.Consumables),
        new CategoryTab(InventoryCategory.Weapons, MessageIds.InventoryWeapons, UiArtFiles.Icons.Weapons),
        new CategoryTab(InventoryCategory.Armor, MessageIds.InventoryArmor, UiArtFiles.Icons.Armor));

    public int Selected => _cursor.Selected;

    public InventoryCategory Category => Tabs[_cursor.Selected].Category;

    public bool HasCursor { get; private set; }

    /// <summary>The chosen tab's highlight, gliding between tabs (decision 262).</summary>
    public SelectionMotion Selection { get; } = new();

    public void MoveUp()
    {
        _cursor.MoveUp();
        Selection.Follow(_cursor);
    }

    public void MoveDown()
    {
        _cursor.MoveDown();
        Selection.Follow(_cursor);
    }

    public void TakeCursor() => HasCursor = true;

    public void ReturnCursor() => HasCursor = false;

    /// <summary>Back to every item, with the cursor on the list, as when the page opens.</summary>
    public void Reset()
    {
        _cursor.Select(0);
        Selection.JumpTo(_cursor);
        HasCursor = false;
    }
}
