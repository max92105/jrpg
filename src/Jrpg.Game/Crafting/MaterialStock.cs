using Jrpg.Core.Items;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Crafting;

/// <summary>
/// The Forge's materials panel: every material in the catalog with how many the party owns, 0 included (decision 370:
/// the costs name them), in id order since the catalog keeps no file order. Rebuilt when the items change, so drawing
/// formats nothing.
/// </summary>
internal sealed class MaterialStock
{
    private readonly Inventory _inventory;
    private readonly ItemDefinition[] _materials;
    private readonly SheetRowLook _look;
    private MaterialCount[] _rows = [];

    /// <param name="inventory">Whose counts are shown.</param>
    /// <param name="items">The catalog's items; the materials among them are listed.</param>
    /// <param name="look">Measures the counts, which it draws as the rows' values.</param>
    public MaterialStock(Inventory inventory, IReadOnlyDictionary<string, ItemDefinition> items, SheetRowLook look)
    {
        _inventory = inventory;
        _materials = [.. items.Values.Where(item => item.IsMaterial).OrderBy(item => item.Id, StringComparer.Ordinal)];
        _look = look;
        Refresh();
    }

    public IReadOnlyList<MaterialCount> Rows => _rows;

    /// <summary>Counts the materials again after the items changed.</summary>
    public void Refresh() =>
        _rows = [.. _materials.Select(item => new MaterialCount(item.Name, _look.Measure(NumberText.Of(_inventory.QuantityOf(item.Id)))))];
}
