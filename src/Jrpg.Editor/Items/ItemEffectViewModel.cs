using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Effects;

namespace Jrpg.Editor.Items;

/// <summary>
/// A consumable's effect as the Items page edits it (decision 881): its kind, its targeting, its power and a damage's
/// type, written over the record it came from, so its statuses, hits and range are carried. Items keep a flat power
/// (decision 873), so no tier is offered. A rest's power box and the type box off a damage are hidden and never read:
/// no type is written, and the power is the record's when its kind had no power box either, else 0.
/// <see cref="Problem"/> is only the power box's; the list shows Data's rules: a damage's type, the power's range, a
/// rest's targeting.
/// </summary>
internal sealed partial class ItemEffectViewModel : ObservableObject
{
    // An extra turn is a skill's, and Data refuses it on an item; no item grants a status alone.
    private static readonly EffectKind[] _itemKinds =
        [EffectKind.Damage, EffectKind.RestoreHp, EffectKind.Revive, EffectKind.Rest];

    private readonly Effect _source;

    private ItemEffectViewModel(Effect source)
    {
        _source = source;
        KindChoices = [.. _itemKinds.Union([source.Kind])];
        Kind = source.Kind;
        Targeting = source.Targeting;
        PowerText = TypedNumber.Text(source.Power);
        DamageType = source.DamageType;
    }

    /// <summary>
    /// A box typed or picked: what the item relays to the list, never a hidden box's or the problem's notice, so the
    /// list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="effect">The record's effect the boxes start from.</param>
    public static ItemEffectViewModel From(Effect effect) => new(effect);

    /// <summary>
    /// A copy of the boxes as typed, for a duplicated item: a half-typed power is copied, never read.
    /// </summary>
    public static ItemEffectViewModel CopyOf(ItemEffectViewModel effect) =>
        new(effect._source)
        {
            Kind = effect.Kind,
            Targeting = effect.Targeting,
            PowerText = effect.PowerText,
            DamageType = effect.DamageType,
        };

    /// <summary>
    /// Decision 881's four kinds, then the record's own when a hand edit gave it another kind Data takes (a status
    /// grant), so the box never misses its value.
    /// </summary>
    public IReadOnlyList<EffectKind> KindChoices { get; }

    /// <summary>
    /// Every targeting but an aura's and an extra turn's: an item has no formation to reach along nor a turn order, and
    /// Data refuses both on it, so a loaded item's targeting is always among these.
    /// </summary>
    public IReadOnlyList<Targeting> TargetingChoices { get; } =
        [.. Enum.GetValues<Targeting>().Except([Targeting.OtherCombatant, Targeting.AlliesInRange])];

    public IReadOnlyList<DamageType> DamageTypeChoices { get; } = Enum.GetValues<DamageType>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TakesPower), nameof(TakesDamageType), nameof(Problem))]
    public partial EffectKind Kind { get; set; }

    [ObservableProperty]
    public partial Targeting Targeting { get; set; }

    /// <summary>The HP it deals, heals or revives with, whoever uses it; read only while it takes a power.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string PowerText { get; set; }

    /// <summary>A damage's type; null until one is picked, read only for a damage.</summary>
    [ObservableProperty]
    public partial DamageType? DamageType { get; set; }

    /// <summary>A rest (the inn's rule, decision 230) and a status grant move no HP by a power.</summary>
    public bool TakesPower => EffectKindFields.ReadsPower(Kind);

    public bool TakesDamageType => EffectKindFields.ReadsDamageType(Kind);

    /// <summary>
    /// "Power must be a whole number (was 'x')." while it takes a power and the box isn't one; null otherwise.
    /// </summary>
    public string? Problem => TakesPower ? TypedNumber.ProblemOf("Power", PowerText) : null;

    /// <summary>
    /// The record's effect with the typed kind, targeting, power and type, the hidden power for a kind without one and
    /// no type off a damage.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public Effect ToEffect() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : _source with
            {
                Kind = Kind,
                Targeting = Targeting,
                Power = TakesPower ? TypedNumber.Parse(PowerText)!.Value : HiddenPower,
                DamageType = TakesDamageType ? DamageType : null,
            };

    // A hand-written rest or status grant keeps its unread power, so an untouched one records no undo step; a heal
    // turned rest writes 0, the power nothing reads.
    private int HiddenPower => EffectKindFields.ReadsPower(_source.Kind) ? 0 : _source.Power;

    partial void OnKindChanged(EffectKind value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnTargetingChanged(Targeting value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnPowerTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnDamageTypeChanged(DamageType? value) => Changed?.Invoke(this, EventArgs.Empty);
}
