using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Effects;

namespace Jrpg.Editor.Skills;

/// <summary>
/// A skill's effect as the Skills page edits it (decision 980): a tier, whose record sets the kind, the damage type and
/// the amount (ADR 0056), or none and a typed kind, type and power; then the targeting, its range and the hits. A box
/// the tier or the kind doesn't read is greyed and written empty: power 0, no type, range 0. <see cref="Problem"/> is
/// only a box that isn't a whole number; Data's rules (a type only on damage, hits above 1 only on damage) are the
/// list's. The statuses granted are the skill's, never this section's.
/// </summary>
internal sealed partial class SkillEffectSection : ObservableObject
{
    private readonly IReadOnlyList<TierChoice> _tiers;

    // The chosen tier's record, taken from the choices as it is picked (design 2.6, "Picks resolve at apply": one
    // session at a time, so nothing changes the records while the page is open).
    private TieredEffect? _tier;

    private SkillEffectSection(IReadOnlyList<TierChoice> tiers, TieredEffect? tier)
    {
        _tiers = tiers;
        _tier = tier;
        TierId = tier?.Id ?? "";
        PowerText = "";
        RangeText = "";
        HitsText = "";
    }

    /// <summary>
    /// A box typed or picked: what the skill relays to the list, never a greyed state's or the problem's notice, so the
    /// list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="effect">The record's effect the boxes start from.</param>
    /// <param name="tiers">
    /// What the Tier box offers, as <see cref="TierChoice.AllIn"/> lists the draft's records.
    /// </param>
    public static SkillEffectSection From(Effect effect, IReadOnlyList<TierChoice> tiers) =>
        new(tiers, effect.Tier)
        {
            Kind = effect.Kind,
            DamageType = effect.DamageType,
            PowerText = TypedNumber.Text(effect.Power),
            Targeting = effect.Targeting,
            RangeText = TypedNumber.Text(effect.Range),
            HitsText = TypedNumber.Text(effect.Hits),
        };

    /// <summary>A copy of the boxes as typed, for a duplicated skill: a half-typed box is copied, never read.</summary>
    public static SkillEffectSection CopyOf(SkillEffectSection section) =>
        new(section._tiers, section._tier)
        {
            Kind = section.Kind,
            DamageType = section.DamageType,
            PowerText = section.PowerText,
            Targeting = section.Targeting,
            RangeText = section.RangeText,
            HitsText = section.HitsText,
        };

    /// <summary>Every kind a skill can have: a rest is an item's only (decision 230).</summary>
    public IReadOnlyList<EffectKind> Kinds { get; } = [.. Enum.GetValues<EffectKind>().Where(kind => kind != EffectKind.Rest)];

    public IReadOnlyList<Targeting> Targetings { get; } = Enum.GetValues<Targeting>();

    public IReadOnlyList<DamageType> DamageTypes { get; } = Enum.GetValues<DamageType>();

    /// <summary>The tier's record id; empty for a flat power.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanTypeKind), nameof(CanTypePower), nameof(CanTypeDamageType))]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string TierId { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanTypePower), nameof(CanTypeDamageType), nameof(Problem))]
    public partial EffectKind Kind { get; set; }

    /// <summary>
    /// A damage's type; null for any other kind. A tiered damage shows its record's, which Data requires it repeats.
    /// </summary>
    [ObservableProperty]
    public partial DamageType? DamageType { get; set; }

    /// <summary>HP before its user's Power raises it, read only without a tier.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string PowerText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanTypeRange), nameof(Problem))]
    public partial Targeting Targeting { get; set; }

    /// <summary>
    /// How many places on each side an aura reaches; read only for <see cref="Targeting.AlliesInRange"/>.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string RangeText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string HitsText { get; set; }

    /// <summary>Only without a tier, whose record sets the kind, the type and the amount.</summary>
    public bool CanTypeKind => TierId.Length == 0;

    public bool CanTypePower => CanTypeKind && FieldsFor(Kind).ReadsPower;

    public bool CanTypeDamageType => CanTypeKind && FieldsFor(Kind).ReadsDamageType;

    public bool CanTypeRange => Targeting == Targeting.AlliesInRange;

    /// <summary>
    /// The first box read that isn't a whole number, by its name: "Hits must be a whole number (was 'x')."; null when
    /// every box read can be.
    /// </summary>
    public string? Problem =>
        (CanTypePower ? TypedNumber.ProblemOf("Power", PowerText) : null)
        ?? (CanTypeRange ? TypedNumber.ProblemOf("Range", RangeText) : null)
        ?? TypedNumber.ProblemOf("Hits", HitsText);

    /// <summary>
    /// The effect as typed with the chosen tier's record, a greyed power written 0, a greyed type none, a range 0 off
    /// an aura, and no status granted.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public Effect ToEffect() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new Effect(
                Kind,
                CanTypePower ? TypedNumber.Parse(PowerText)!.Value : 0,
                Targeting,
                CanTypeRange ? TypedNumber.Parse(RangeText)!.Value : 0,
                TypedNumber.Parse(HitsText)!.Value,
                FieldsFor(Kind).ReadsDamageType ? DamageType : null)
            { Tier = _tier };

    // Which boxes beside the targeting and the hits a kind reads; a rest is never offered, and Data refuses it on a
    // skill at load.
    private static (bool ReadsPower, bool ReadsDamageType) FieldsFor(EffectKind kind) =>
        kind switch
        {
            EffectKind.Damage => (true, true),
            EffectKind.RestoreHp or EffectKind.Revive => (true, false),
            EffectKind.GrantStatus or EffectKind.ReadyExtraTurn => (false, false),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "A skill's effect can't be this kind."),
        };

    // Starting on the effect's own record changes nothing; picking another takes its kind, its type and its amount, and
    // picking none keeps them to type over.
    partial void OnTierIdChanged(string value)
    {
        if (value != (_tier?.Id ?? ""))
        {
            _tier = _tiers.Single(choice => choice.Id == value).Tier;
            if (_tier is not null)
            {
                Kind = _tier.Kind;
                DamageType = _tier.DamageType;
                PowerText = TypedNumber.Text(0);
            }
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    partial void OnKindChanged(EffectKind value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnDamageTypeChanged(DamageType? value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnPowerTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnTargetingChanged(Targeting value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnRangeTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnHitsTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);
}
