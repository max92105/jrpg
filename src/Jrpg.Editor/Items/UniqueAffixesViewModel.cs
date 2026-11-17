using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Trade;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Items;

/// <summary>
/// A unique's fixed affixes as the Items page edits them (decision 882): one row per affix in the record's order, added
/// after the chosen row with the first pool affix no row names at its range's min, removed and moved; and the price
/// they make with the item's (decision 871). <see cref="Problem"/> is the first value that can't be read; the list
/// shows Data's rules (decision 866): an affix named twice, a value of 0, past ±100 or on the wrong side of zero.
/// </summary>
internal sealed partial class UniqueAffixesViewModel : ObservableObject
{
    // The field Data's messages name the fixed affixes by, which the form's own lines follow.
    private const string FixedAffixesField = "fixedAffixes";

    private readonly IReadOnlyDictionary<string, AffixDefinition> _pool;

    private UniqueAffixesViewModel(IReadOnlyDictionary<string, AffixDefinition> pool, IEnumerable<FixedAffixRow> rows)
    {
        _pool = pool;
        AffixChoices = [.. pool.Values.OrderBy(affix => affix.Id, StringComparer.Ordinal).Select(AffixChoice.Of)];
        Rows = [.. rows];
        foreach (var row in Rows)
        {
            row.PropertyChanged += OnRowChanged;
        }

        Rows.CollectionChanged += OnRowsListChanged;
        SelectedRow = Rows.FirstOrDefault();
    }

    /// <summary>
    /// A row's affix picked or value typed, or a row added, removed or moved: what the item relays to the list, never a
    /// row's roll line or a problem's notice, so the list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="fixedAffixes">The record's fixed affixes, each naming an affix of the pool.</param>
    /// <param name="pool">
    /// The draft's affixes by id. The loader refuses a unique naming one it lacks, so every fixed affix the draft holds
    /// is found here; the Affixes page (IU15) must keep every affix a unique names.
    /// </param>
    public static UniqueAffixesViewModel From(
        IReadOnlyList<RolledAffix> fixedAffixes, IReadOnlyDictionary<string, AffixDefinition> pool) =>
        new(
            pool,
            fixedAffixes.Select(affix => new FixedAffixRow(AffixChoice.Of(pool[affix.AffixId]), TypedNumber.Text(affix.Value))));

    /// <summary>A copy of the rows as typed, for a duplicated item: a half-typed value is copied, never read.</summary>
    public static UniqueAffixesViewModel CopyOf(UniqueAffixesViewModel unique) =>
        new(unique._pool, unique.Rows.Select(row => new FixedAffixRow(row.Affix, row.ValueText)));

    /// <summary>Every pool affix by id, whatever gear kinds it rolls on (decision 866).</summary>
    public IReadOnlyList<AffixChoice> AffixChoices { get; }

    public ObservableCollection<FixedAffixRow> Rows { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveRowCommand), nameof(MoveRowUpCommand), nameof(MoveRowDownCommand))]
    public partial FixedAffixRow? SelectedRow { get; set; }

    /// <summary>
    /// "fixedAffixes[0] (power): Value must be a whole number (was 'x')."; null when every value can be read.
    /// </summary>
    public string? Problem => SectionRowProblem.FirstInField(FixedAffixesField, Rows, row => row.Affix.Id, row => row.Problem);

    /// <summary>The rows' affixes in order, each with its pool affix's stat.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public IReadOnlyList<RolledAffix> ToFixedAffixes() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : [.. Rows.Select(row => new RolledAffix(row.Affix.Id, row.Affix.Stat, TypedNumber.Parse(row.ValueText)!.Value))];

    /// <summary>
    /// "Price as a piece: 440, sells for 220": the item's price plus its affixes' gold values (decision 871) and its
    /// share, through Core's trade formula; "Never bought nor sold" with the price box empty; empty while it isn't a
    /// whole number. The values typed don't count: an affix adds its gold whatever its value.
    /// </summary>
    /// <param name="priceText">The item's price box as typed.</param>
    /// <param name="sellPercent">The share of a price a sale pays (<c>rules.json</c>, decision 864).</param>
    public string PieceValueLine(string priceText, int sellPercent)
    {
        if (priceText.Trim().Length == 0)
        {
            return "Never bought nor sold";
        }

        if (TypedNumber.Parse(priceText) is not { } price)
        {
            return "";
        }

        var value = PriceFormula.PieceValue(price, Rows.Select(row => _pool[row.Affix.Id].GoldValue));
        return $"Price as a piece: {value}, sells for {PriceFormula.SalePrice(value, sellPercent)}";
    }

    [RelayCommand(CanExecute = nameof(HasUnlistedAffix))]
    private void AddRow()
    {
        var affix = AffixChoices.First(choice => !IsListed(choice));
        var row = new FixedAffixRow(affix, TypedNumber.Text(affix.Min));
        ListOrder.InsertAfter(Rows, row, SelectedRow);
        SelectedRow = row;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedRow))]
    private void RemoveRow() => SelectedRow = ListOrder.Remove(Rows, SelectedRow!);

    [RelayCommand(CanExecute = nameof(HasSelectedRow))]
    private void MoveRowUp() => ListOrder.MoveBy(Rows, SelectedRow!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedRow))]
    private void MoveRowDown() => ListOrder.MoveBy(Rows, SelectedRow!, 1);

    private bool HasUnlistedAffix() => AffixChoices.Any(choice => !IsListed(choice));

    private bool IsListed(AffixChoice choice) => Rows.Any(row => row.Affix.Id == choice.Id);

    private bool HasSelectedRow() => SelectedRow is not null;

    private void OnRowsListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ListItemWatch.Follow<FixedAffixRow>(
            e,
            row => row.PropertyChanged += OnRowChanged,
            row => row.PropertyChanged -= OnRowChanged);
        RaiseRowsChanged();
    }

    // A row's roll line and problem notices are left out, so the list re-checks once per change (rules/editor.md).
    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(FixedAffixRow.Affix) or nameof(FixedAffixRow.ValueText))
        {
            RaiseRowsChanged();
        }
    }

    // An affix picked or a row added or removed may list or free an affix, which Add affix reads.
    private void RaiseRowsChanged()
    {
        OnPropertyChanged(nameof(Problem));
        AddRowCommand.NotifyCanExecuteChanged();
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
