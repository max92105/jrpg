using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Game.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// Choosing what goes in a slot: "Remove" first, then the owned items the member can wear there, then the items other
/// members wear there, one row per wearer (decision 137), with a scrolling cursor.
/// </summary>
internal sealed class EquipmentChoice
{
    /// <summary>How many rows show at once.</summary>
    public const int VisibleRows = 12;

    private readonly ListCursor _cursor;

    /// <param name="wearable">The owned items, in pickup order (<c>PartyEquipment.WearableFor</c>).</param>
    /// <param name="takeable">What other members wear there, in party order (<c>PartyEquipment.TakeableFor</c>).</param>
    public EquipmentChoice(IReadOnlyList<ItemDefinition> wearable, IReadOnlyList<WornItem> takeable)
    {
        Items = wearable;
        Takeable = takeable;
        _cursor = new ListCursor(RowCount, _ => true, VisibleRows);
    }

    /// <summary>The owned items after the "Remove" row.</summary>
    public IReadOnlyList<ItemDefinition> Items { get; }

    /// <summary>The worn items after the owned ones.</summary>
    public IReadOnlyList<WornItem> Takeable { get; }

    public int RowCount => 1 + Items.Count + Takeable.Count;

    /// <summary>The row index, "Remove" being 0.</summary>
    public int SelectedRow => _cursor.Selected;

    public int FirstVisibleRow => _cursor.FirstVisible;

    /// <summary>The item under the cursor, owned or worn; null on "Remove".</summary>
    public ItemDefinition? Highlighted => WornAt(SelectedRow)?.Item ?? (SelectedRow == 0 ? null : Items[SelectedRow - 1]);

    /// <summary>Who wears the highlighted item; null on "Remove" and on owned items.</summary>
    public PartyMember? HighlightedWearer => WornAt(SelectedRow)?.Wearer;

    /// <summary>The worn item on a row, or null on "Remove" and owned rows.</summary>
    public WornItem? WornAt(int row) => row > Items.Count ? Takeable[row - Items.Count - 1] : null;

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

        return input.IsPressed(GameButton.Confirm) ? new EquipmentPick(Highlighted, HighlightedWearer) : null;
    }
}
