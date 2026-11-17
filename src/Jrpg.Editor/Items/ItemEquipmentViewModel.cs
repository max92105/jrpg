using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Items;

/// <summary>
/// An equipment's form as the Items page edits it (decision 880): its kind, for a weapon only its damage and damage
/// type, and its bonus rows as typed, written over the record it came from. Off a weapon the two damage boxes are
/// hidden, never read, and written as Data requires (damage 0, no type); they start empty on anything but a weapon, so
/// a kind turned weapon asks for them. A bonus row is added after the chosen one with the first stat a base item may
/// give that no row names, until every one is named. <see cref="Problem"/> is the damage box's, then the first bonus
/// value that can't be read, then a stat named twice; the list shows Data's rules: a weapon's damage and type, and the
/// bound on a bonus's value.
/// </summary>
internal sealed partial class ItemEquipmentViewModel : ObservableObject
{
    // The field Data's messages name the bonuses by, which the form's own lines follow.
    private const string BonusesField = "bonuses";

    // An added row's value: the least bonus Data takes, a 0 naming a stat for nothing.
    private const int NewBonusValue = 1;

    private readonly EquipmentStats _source;

    private ItemEquipmentViewModel(EquipmentStats source, IEnumerable<StatBonusRow> bonuses)
    {
        _source = source;
        Kind = source.Kind;
        DamageText = source.Kind == EquipmentKind.Weapon ? TypedNumber.Text(source.Damage) : "";
        DamageType = source.DamageType;
        Bonuses = [.. bonuses];
        foreach (var row in Bonuses)
        {
            row.PropertyChanged += OnBonusChanged;
        }

        Bonuses.CollectionChanged += OnBonusesListChanged;
        SelectedBonus = Bonuses.FirstOrDefault();
    }

    /// <summary>
    /// A box typed or picked, a bonus row's included, or a bonus row added or removed: what the item relays to the
    /// list, never the weapon state's or a problem's notice, so the list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="equipment">
    /// The record's equipment the boxes start from; its bonuses become rows in <see cref="StatKind"/> order, the order
    /// the file writes them in (decision 862).
    /// </param>
    public static ItemEquipmentViewModel From(EquipmentStats equipment) =>
        new(
            equipment,
            equipment.Bonuses.Values
                .OrderBy(bonus => bonus.Key)
                .Select(bonus => new StatBonusRow(bonus.Key, TypedNumber.Text(bonus.Value))));

    /// <summary>
    /// A copy of the boxes and rows as typed, for a duplicated item: a half-typed number is copied, never read.
    /// </summary>
    public static ItemEquipmentViewModel CopyOf(ItemEquipmentViewModel equipment) =>
        new(equipment._source, equipment.Bonuses.Select(row => new StatBonusRow(row.Stat, row.ValueText)))
        {
            Kind = equipment.Kind,
            DamageText = equipment.DamageText,
            DamageType = equipment.DamageType,
        };

    public IReadOnlyList<EquipmentKind> KindChoices { get; } = Enum.GetValues<EquipmentKind>();

    public IReadOnlyList<DamageType> DamageTypeChoices { get; } = Enum.GetValues<DamageType>();

    /// <summary>
    /// Every stat a base item may give, in <see cref="StatKind"/> order: the affix-only ones are rolls' alone (ADR
    /// 0030), and Data refuses them on a loaded item, so a saved bonus always names one of these.
    /// </summary>
    public IReadOnlyList<StatKind> StatChoices { get; } = [.. Enum.GetValues<StatKind>().Except(StatKinds.AffixOnly)];

    public ObservableCollection<StatBonusRow> Bonuses { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveBonusCommand))]
    public partial StatBonusRow? SelectedBonus { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWeapon), nameof(Problem))]
    public partial EquipmentKind Kind { get; set; }

    /// <summary>A weapon's raw damage as typed; read only for a weapon.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string DamageText { get; set; }

    /// <summary>A weapon's hits' type; null until one is picked, read only for a weapon.</summary>
    [ObservableProperty]
    public partial DamageType? DamageType { get; set; }

    /// <summary>Only a weapon is swung (decision 330), so only its damage boxes are shown.</summary>
    public bool IsWeapon => Kind == EquipmentKind.Weapon;

    /// <summary>
    /// "Damage must be a whole number (was 'x')." for a weapon whose damage box isn't a whole number, then "bonuses[1]
    /// (power): Value must be a whole number (was 'x').", then "bonuses names power twice."; null otherwise.
    /// </summary>
    public string? Problem =>
        (IsWeapon ? TypedNumber.ProblemOf("Damage", DamageText) : null)
        ?? SectionRowProblem.FirstInField(BonusesField, Bonuses, row => ContentSpelling.Of(row.Stat), row => row.Problem)
        ?? RepeatedStatProblem();

    /// <summary>
    /// The record's equipment with the rows' bonuses, the typed kind, a weapon's damage and type, and damage 0 and no
    /// type off a weapon.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public EquipmentStats ToEquipment() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : _source with
            {
                Kind = Kind,
                Damage = IsWeapon ? TypedNumber.Parse(DamageText)!.Value : 0,
                DamageType = IsWeapon ? DamageType : null,
                Bonuses = TypedBonuses(),
            };

    private StatBonuses TypedBonuses() =>
        new(Bonuses.ToDictionary(row => row.Stat, row => TypedNumber.Parse(row.ValueText)!.Value));

    private string? RepeatedStatProblem() =>
        Bonuses.GroupBy(row => row.Stat).FirstOrDefault(rows => rows.Count() > 1) is { } repeated
            ? $"{BonusesField} names {ContentSpelling.Of(repeated.Key)} twice."
            : null;

    [RelayCommand(CanExecute = nameof(HasUnlistedStat))]
    private void AddBonus()
    {
        var row = new StatBonusRow(StatChoices.First(stat => !IsListed(stat)), TypedNumber.Text(NewBonusValue));
        ListOrder.InsertAfter(Bonuses, row, SelectedBonus);
        SelectedBonus = row;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedBonus))]
    private void RemoveBonus() => SelectedBonus = ListOrder.Remove(Bonuses, SelectedBonus!);

    private bool HasUnlistedStat() => StatChoices.Any(stat => !IsListed(stat));

    private bool IsListed(StatKind stat) => Bonuses.Any(row => row.Stat == stat);

    private bool HasSelectedBonus() => SelectedBonus is not null;

    private void OnBonusesListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ListItemWatch.Follow<StatBonusRow>(
            e,
            row => row.PropertyChanged += OnBonusChanged,
            row => row.PropertyChanged -= OnBonusChanged);
        RaiseBonusesChanged();
    }

    // A row's problem notice is left out, so the list re-checks once per change (.claude/rules/editor.md).
    private void OnBonusChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(StatBonusRow.Stat) or nameof(StatBonusRow.ValueText))
        {
            RaiseBonusesChanged();
        }
    }

    // A stat picked or a row added or removed may list or free a stat, which Add bonus reads.
    private void RaiseBonusesChanged()
    {
        OnPropertyChanged(nameof(Problem));
        AddBonusCommand.NotifyCanExecuteChanged();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    partial void OnKindChanged(EquipmentKind value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnDamageTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnDamageTypeChanged(DamageType? value) => Changed?.Invoke(this, EventArgs.Empty);
}
