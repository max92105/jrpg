using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Items;

/// <summary>
/// An equipment's form as the Items page edits it (decision 880): its kind and, for a weapon only, its damage and
/// damage type as typed, written over the record it came from, so the bonuses are carried. Off a weapon the two boxes
/// are hidden, never read, and written as Data requires (damage 0, no type); they start empty on anything but a weapon,
/// so a kind turned weapon asks for them. <see cref="Problem"/> is only the damage box's: Data's rules (at least 1, a
/// type required) are the list's.
/// </summary>
internal sealed partial class ItemEquipmentViewModel : ObservableObject
{
    private readonly EquipmentStats _source;

    private ItemEquipmentViewModel(EquipmentStats source)
    {
        _source = source;
        Kind = source.Kind;
        DamageText = source.Kind == EquipmentKind.Weapon ? TypedNumber.Text(source.Damage) : "";
        DamageType = source.DamageType;
    }

    /// <summary>
    /// A box typed or picked: what the item relays to the list, never the weapon state's or the problem's notice, so
    /// the list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="equipment">The record's equipment the boxes start from.</param>
    public static ItemEquipmentViewModel From(EquipmentStats equipment) => new(equipment);

    /// <summary>
    /// A copy of the boxes as typed, for a duplicated item: a half-typed damage is copied, never read.
    /// </summary>
    public static ItemEquipmentViewModel CopyOf(ItemEquipmentViewModel equipment) =>
        new(equipment._source)
        {
            Kind = equipment.Kind,
            DamageText = equipment.DamageText,
            DamageType = equipment.DamageType,
        };

    public IReadOnlyList<EquipmentKind> KindChoices { get; } = Enum.GetValues<EquipmentKind>();

    public IReadOnlyList<DamageType> DamageTypeChoices { get; } = Enum.GetValues<DamageType>();

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
    /// "Damage must be a whole number (was 'x')." for a weapon whose damage box isn't a whole number; null otherwise.
    /// </summary>
    public string? Problem => IsWeapon ? TypedNumber.ProblemOf("Damage", DamageText) : null;

    /// <summary>
    /// The record's equipment with the typed kind, a weapon's damage and type, damage 0 and no type off one.
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
            };

    partial void OnKindChanged(EquipmentKind value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnDamageTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnDamageTypeChanged(DamageType? value) => Changed?.Invoke(this, EventArgs.Empty);
}
