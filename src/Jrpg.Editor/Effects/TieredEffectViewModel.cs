using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Effects;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Effects;

/// <summary>
/// One tiered effect as the Effects page edits it (ADR 0056, decision 983): its id (typed while the effect is new,
/// fixed once the draft holds it, decision 582), kind, damage type and per-Power as typed, with the skills naming it
/// and its amount at a shown Power as read-only lines. <see cref="Problem"/> is only the typed id's and the per-Power
/// box's: the rest is Data's rules, which the list shows for every effect at once.
/// </summary>
internal sealed partial class TieredEffectViewModel : ObservableObject
{
    // The Power the amount line reads (decision 983): an editor's starting value, which has no content home.
    private const int ShownPower = 10;

    // Data's least per-Power, so a new effect leaves only its id to type.
    private const int StartingPerPower = 1;

    // The boxes start on the record's fields: a new effect's id is empty.
    private TieredEffectViewModel(TieredEffect start, IReadOnlyList<string> userIds)
    {
        IdText = start.Id;
        Kind = start.Kind;
        DamageType = start.DamageType;
        PerPowerText = TypedNumber.Text(start.PerPower);
        UsedByText = userIds.Count == 0 ? "Used by no skill" : $"Used by: {string.Join(", ", userIds)}";
    }

    /// <summary>
    /// A typed box changed: what the list's problem line listens to, never the id's, the lines' or the problem's own
    /// notices, so the list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="effect">The record as the draft holds it.</param>
    /// <param name="skills">The draft's skills, whose tiers name the records.</param>
    public static TieredEffectViewModel From(TieredEffect effect, IEnumerable<SkillDefinition> skills) =>
        new(
            effect,
            [
                .. skills
                    .Where(skill => skill.Effect.Tier?.Id == effect.Id)
                    .Select(skill => skill.Id)
                    .Order(StringComparer.Ordinal),
            ]);

    /// <summary>
    /// An effect as Add starts it: its id empty, a damage of Core's first type at the least per-Power, so the id is
    /// what is left to type.
    /// </summary>
    /// <param name="takenIds">The saved effects' ids, which the typed id must not take.</param>
    public static TieredEffectViewModel New(IReadOnlyCollection<string> takenIds) =>
        new(new TieredEffect("", EffectKind.Damage, StartingPerPower) { DamageType = Enum.GetValues<DamageType>()[0] }, [])
        {
            IsNew = true,
            TakenIds = takenIds,
        };

    /// <summary>
    /// A new effect copying the chosen one's boxes as typed, so a half-typed one duplicates too, under the id given
    /// (decision 985: the chosen one's next tier), still editable.
    /// </summary>
    /// <param name="chosen">The effect chosen in the list, saved or added.</param>
    /// <param name="nextId">The id the copy starts with.</param>
    /// <param name="takenIds">The saved effects' ids, which the typed id must not take.</param>
    public static TieredEffectViewModel DuplicateOf(
        TieredEffectViewModel chosen,
        string nextId,
        IReadOnlyCollection<string> takenIds) =>
        new(new TieredEffect(nextId, chosen.Kind, StartingPerPower) { DamageType = chosen.DamageType }, [])
        {
            // Copied as typed, a half-typed value included: the record's per-Power above is only a placeholder.
            PerPowerText = chosen.PerPowerText,
            IsNew = true,
            TakenIds = takenIds,
        };

    /// <summary>
    /// True while the effect was added this session: the only time its id can be typed (decision 582).
    /// </summary>
    public bool IsNew { get; private init; }

    /// <summary>The only kinds <c>effects.json</c> takes: a record is a damage or a heal (ADR 0056).</summary>
    public IReadOnlyList<EffectKind> Kinds { get; } = [EffectKind.Damage, EffectKind.RestoreHp];

    public IReadOnlyList<DamageType> DamageTypes { get; } = Enum.GetValues<DamageType>();

    /// <summary>The id as typed; the page lets it be typed only while <see cref="IsNew"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Id), nameof(Problem))]
    public partial string IdText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanTypeDamageType))]
    public partial EffectKind Kind { get; set; }

    /// <summary>
    /// A damage's type, kept while the kind is a heal so switching back finds it; written only for a damage.
    /// </summary>
    [ObservableProperty]
    public partial DamageType? DamageType { get; set; }

    /// <summary>What each point of its user's Power gives (decision 782).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AmountText), nameof(Problem))]
    public partial string PerPowerText { get; set; }

    public string Id => IdText.Trim();

    /// <summary>A heal has no type (ADR 0056).</summary>
    public bool CanTypeDamageType => Kind == EffectKind.Damage;

    /// <summary>
    /// "Used by: bolt, fireball", the draft's skills naming the record by id, or "Used by no skill"; a new effect is
    /// named by none.
    /// </summary>
    public string UsedByText { get; }

    /// <summary>
    /// "At Power 10: 20 per hit", the typed record's own amount before modifiers and target (decision 1001); empty
    /// while the per-Power box isn't a whole number.
    /// </summary>
    public string AmountText =>
        TypedNumber.Parse(PerPowerText) is { } perPower
            ? $"At Power {ShownPower}: {new TieredEffect(Id, Kind, perPower).AmountAt(ShownPower)} per hit"
            : "";

    // The saved effects' ids, which a new effect's typed id must not take: fixed and never removed in a session
    // (decision 581), so read once; a saved effect's id isn't typed.
    private IReadOnlyCollection<string> TakenIds { get; init; } = [];

    /// <summary>
    /// Why the effect can't be built: the typed id while new, then the per-Power box; null when it can.
    /// </summary>
    public string? Problem =>
        (IsNew ? ObjectIds.ProblemOf(Id, TakenIds) : null) ?? TypedNumber.ProblemOf("Per Power", PerPowerText);

    /// <summary>The record as typed: the type written only for a damage.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public TieredEffect ToEffect() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new TieredEffect(Id, Kind, TypedNumber.Parse(PerPowerText)!.Value)
            {
                DamageType = CanTypeDamageType ? DamageType : null,
            };

    partial void OnIdTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnKindChanged(EffectKind value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnDamageTypeChanged(DamageType? value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnPerPowerTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);
}
