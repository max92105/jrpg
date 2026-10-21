using Jrpg.Core.Battle;
using Jrpg.Core.Items;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// The party's consumables, open in place of the commands, in pickup order. An item Core can't use now (a revive with
/// nobody fallen) stays listed, greyed, so the bag reads the same as in the menu.
/// </summary>
internal sealed class BattleItemChoice : IBattleListChoice
{
    private readonly ListCursor _cursor;
    private readonly bool[] _isUsable;

    /// <param name="battle">On a party member's turn.</param>
    /// <param name="owned">The party's inventory, with at least one consumable.</param>
    /// <param name="items">Every item by id, from the catalog.</param>
    public BattleItemChoice(BattleState battle, Inventory owned, IReadOnlyDictionary<string, ItemDefinition> items)
    {
        var consumables = owned.Entries.Where(entry => items[entry.ItemId].Effect is not null).ToList();
        Items = [.. consumables.Select(entry => items[entry.ItemId])];
        Quantities = [.. consumables.Select(entry => entry.Quantity)];
        _isUsable = [.. Items.Select(battle.CanUse)];
        _cursor = new ListCursor(Items.Count, _ => true, IBattleListChoice.VisibleRows);
    }

    public IReadOnlyList<ItemDefinition> Items { get; }

    /// <summary>How many copies of each item are owned, in the same order.</summary>
    public IReadOnlyList<int> Quantities { get; }

    public ItemDefinition Selected => Items[_cursor.Selected];

    public int SelectedIndex => _cursor.Selected;

    /// <summary>The index of the first row in the visible window.</summary>
    public int FirstVisible => _cursor.FirstVisible;

    /// <summary>Whether the item at <paramref name="index"/> can be used now; the rest are greyed.</summary>
    public bool IsUsable(int index) => _isUsable[index];

    public void MoveUp() => _cursor.MoveUp();

    public void MoveDown() => _cursor.MoveDown();
}
