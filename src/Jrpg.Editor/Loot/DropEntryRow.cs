using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Loot;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Loot;

/// <summary>
/// One drop entry as the Drop tables window edits it (decision 597): its item, fixed once added, and its chance and
/// quantity boxes kept as typed. <see cref="Problem"/> is only a box that isn't a whole number; the ranges are Data's
/// rules, shown by the window's problem line.
/// </summary>
internal sealed partial class DropEntryRow : ObservableObject
{
    private DropEntryRow(string itemId, string itemName)
    {
        ItemId = itemId;
        ItemLabel = $"{itemName} ({itemId})";
    }

    /// <param name="entry">The entry as the draft holds it.</param>
    /// <param name="itemName">The item's name, for the row's label.</param>
    public static DropEntryRow From(DropEntry entry, string itemName) => new(entry.ItemId, itemName)
    {
        ChanceText = TypedNumber.Text(entry.Chance),
        MinQuantityText = TypedNumber.Text(entry.MinQuantity),
        MaxQuantityText = TypedNumber.Text(entry.MaxQuantity),
    };

    public string ItemId { get; }

    /// <summary>"Potion (potion)": the name, and the id that tells two items named alike apart.</summary>
    public string ItemLabel { get; }

    /// <summary>
    /// The row's position from 0, as Data's messages number entries (decision 598); the table sets it.
    /// </summary>
    [ObservableProperty]
    public partial int Index { get; set; }

    /// <summary>The chance in whole percents, as typed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string ChanceText { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string MinQuantityText { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string MaxQuantityText { get; set; } = "";

    /// <summary>
    /// The first box, in the row's order, that isn't a whole number; null when the entry can be built.
    /// </summary>
    public string? Problem =>
        TypedNumber.ProblemOf("Chance", ChanceText)
        ?? TypedNumber.ProblemOf("Min", MinQuantityText)
        ?? TypedNumber.ProblemOf("Max", MaxQuantityText);

    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public DropEntry ToEntry() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new DropEntry(ItemId, TypedNumber.Parse(ChanceText)!.Value)
            {
                MinQuantity = TypedNumber.Parse(MinQuantityText)!.Value,
                MaxQuantity = TypedNumber.Parse(MaxQuantityText)!.Value,
            };
}
