using Jrpg.Core.Items;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// Choosing what goes in a slot (legacy <c>EquipmentSelectionView</c>): "Remove" first, then the owned items the member
/// can wear there, with a scrolling cursor.
/// </summary>
internal sealed class EquipmentChoice
{
    /// <summary>How many rows show at once.</summary>
    public const int VisibleRows = 12;

    private readonly ListCursor _cursor;

    /// <param name="wearable">The items, in pickup order (<c>PartyEquipment.WearableFor</c>).</param>
    public EquipmentChoice(IReadOnlyList<ItemDefinition> wearable)
    {
        Items = wearable;
        _cursor = new ListCursor(wearable.Count + 1, _ => true, VisibleRows);
    }

    /// <summary>The items after the "Remove" row.</summary>
    public IReadOnlyList<ItemDefinition> Items { get; }

    /// <summary>The row index, "Remove" being 0.</summary>
    public int SelectedRow => _cursor.Selected;

    public int FirstVisibleRow => _cursor.FirstVisible;

    /// <summary>The item under the cursor; null on "Remove".</summary>
    public ItemDefinition? Highlighted => _cursor.Selected == 0 ? null : Items[_cursor.Selected - 1];

    /// <returns>The row confirmed this update, or null.</returns>
    public EquipmentPick? Update(InputFrame input)
    {
        if (input.IsPressed(GameButton.Up))
        {
            _cursor.MoveUp();
        }

        if (input.IsPressed(GameButton.Down))
        {
            _cursor.MoveDown();
        }

        return input.IsPressed(GameButton.Confirm) ? new EquipmentPick(Highlighted) : null;
    }
}
