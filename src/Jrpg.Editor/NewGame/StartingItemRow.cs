using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Items;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.NewGame;

/// <summary>
/// One starting item of the new game as the New game page types it (decision 968): the item, fixed, and its quantity as
/// typed. <see cref="Problem"/> is only a quantity that isn't a whole number; one below 1 or past the item's max stack,
/// or an item listed twice, is Data's rule, shown by the list.
/// </summary>
internal sealed partial class StartingItemRow : ObservableObject
{
    // An added item starts as one copy, the fewest Data accepts.
    private const int AddedQuantity = 1;

    /// <param name="item">The item, or a missing one when the draft lacks its id.</param>
    /// <param name="quantityText">The quantity as typed.</param>
    public StartingItemRow(RecordChoice item, string quantityText)
    {
        Item = item;
        QuantityText = quantityText;
    }

    /// <summary>A row as Add starts it: one copy of the item.</summary>
    public static StartingItemRow New(RecordChoice item) => new(item, TypedNumber.Text(AddedQuantity));

    public RecordChoice Item { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string QuantityText { get; set; }

    /// <summary>"Quantity of Potion must be a whole number (was 'x')."; null when the quantity can be read.</summary>
    public string? Problem => TypedNumber.ProblemOf($"Quantity of {Item.Name}", QuantityText);

    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public InventoryEntry ToEntry() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new InventoryEntry(Item.Id, TypedNumber.Parse(QuantityText)!.Value);
}
