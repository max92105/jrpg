using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Effects;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Skills;

/// <summary>
/// One skill as the Skills page edits it (ADR 0045): its id (typed while the skill is new, fixed once the draft holds
/// it, decision 582), name, description, icon, sound, own strip, quick flag, costs, effect and the statuses it grants
/// as typed. <see cref="Problem"/> is only the typed id's and the sections' boxes': the rest is Data's rules, which the
/// list shows for every skill at once.
/// </summary>
internal sealed partial class SkillViewModel : ObservableObject
{
    // A new skill's power when the content has no damage tier to start from: a flat damage that deals something.
    private const int FlatStartingPower = 1;

    private readonly SkillDefinition _original;

    // The id box starts on the original's id: a new skill's is empty.
    private SkillViewModel(SkillDefinition original, SkillCosts costs, SkillEffectSection effect, SkillGrants grants)
    {
        _original = original;
        Costs = costs;
        Effect = effect;
        Grants = grants;
        costs.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
        effect.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
        grants.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
        IdText = original.Id;
        Name = original.Name;
        Description = original.Description;
        IconFile = original.IconFile;
        SoundFile = original.SoundFile;
        EffectFile = original.EffectFile ?? "";
        IsQuick = original.IsQuick;
    }

    /// <summary>
    /// A typed box changed: what the list's problem line listens to, never the id's or the problem's own notices, so
    /// the list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="skill">The record as the draft holds it.</param>
    /// <param name="choices">
    /// What the page's boxes offer: an added cost row's resource, the tiers, the statuses.
    /// </param>
    public static SkillViewModel From(SkillDefinition skill, SkillChoices choices) =>
        new(
            skill,
            SkillCosts.From(skill, FirstResourceOf(choices)),
            SkillEffectSection.From(skill.Effect, choices.Tiers),
            SkillGrants.From(skill.Effect.Statuses, choices));

    /// <summary>A skill added this session: its id left to type, the rest as the starting record has it.</summary>
    /// <param name="start">The record whose fields the new skill starts with; its id is not used.</param>
    /// <param name="takenIds">The saved skills' ids, which the typed id must not take.</param>
    /// <param name="choices">
    /// What the page's boxes offer: an added cost row's resource, the tiers, the statuses.
    /// </param>
    public static SkillViewModel ForNew(SkillDefinition start, IReadOnlyCollection<string> takenIds, SkillChoices choices) =>
        new(
            start with { Id = "" },
            SkillCosts.From(start, FirstResourceOf(choices)),
            SkillEffectSection.From(start.Effect, choices.Tiers),
            SkillGrants.From(start.Effect.Statuses, choices))
        { IsNew = true, TakenIds = takenIds };

    /// <summary>
    /// A skill as Add starts it (decision 977): every text and file empty, the given tier's damage on a single enemy,
    /// so the id, name and sound are what is left to type.
    /// </summary>
    /// <param name="firstDamageTier">
    /// The content's first damage tier by id; with none, a flat damage of power 1, which Data's message then refuses.
    /// </param>
    /// <param name="takenIds">The saved skills' ids, which the typed id must not take.</param>
    /// <param name="choices">
    /// What the page's boxes offer: an added cost row's resource, the tiers, the statuses.
    /// </param>
    public static SkillViewModel New(TieredEffect? firstDamageTier, IReadOnlyCollection<string> takenIds, SkillChoices choices) =>
        ForNew(
            new SkillDefinition { Id = "", Name = "", Description = "", IconFile = "", Effect = StartingEffect(firstDamageTier) },
            takenIds,
            choices);

    /// <summary>
    /// A new skill copying the chosen one as typed, its id left to type (decision 585). Copied from the boxes, not
    /// through <see cref="ToSkill"/>, so a half-typed skill duplicates too.
    /// </summary>
    /// <param name="chosen">The skill chosen in the list, saved or added.</param>
    /// <param name="takenIds">The saved skills' ids, which the typed id must not take.</param>
    public static SkillViewModel DuplicateOf(SkillViewModel chosen, IReadOnlyCollection<string> takenIds) =>
        new(
            chosen.TypedRecord() with { Id = "" },
            SkillCosts.CopyOf(chosen.Costs),
            SkillEffectSection.CopyOf(chosen.Effect),
            SkillGrants.CopyOf(chosen.Grants))
        { IsNew = true, TakenIds = takenIds };

    /// <summary>
    /// True while the skill was added this session: the only time its id can be typed (decision 582).
    /// </summary>
    public bool IsNew { get; private init; }

    /// <summary>Its resource changes and requirements as typed (decision 979).</summary>
    public SkillCosts Costs { get; }

    /// <summary>Its effect as typed, but the statuses it grants (decision 980).</summary>
    public SkillEffectSection Effect { get; }

    /// <summary>The statuses its effect grants as typed, in order (decision 981).</summary>
    public SkillGrants Grants { get; }

    /// <summary>The id as typed; the page lets it be typed only while <see cref="IsNew"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Id), nameof(Problem))]
    public partial string IdText { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    /// <summary>Shown while choosing the skill; it may be empty.</summary>
    [ObservableProperty]
    public partial string Description { get; set; }

    /// <summary>The image in the icons folder.</summary>
    [ObservableProperty]
    public partial string IconFile { get; set; }

    /// <summary>The sound in the audio effects folder played as its action starts (decision 423).</summary>
    [ObservableProperty]
    public partial string SoundFile { get; set; }

    /// <summary>
    /// The strip in the effects folder its action plays instead of its user's attack strip (decision 678); empty when
    /// it plays the user's.
    /// </summary>
    [ObservableProperty]
    public partial string EffectFile { get; set; }

    /// <summary>Keeps the actor's turn (decision 202); Data refuses it on a skill that spends nothing.</summary>
    [ObservableProperty]
    public partial bool IsQuick { get; set; }

    public string Id => IdText.Trim();

    // The saved skills' ids, which a new skill's typed id must not take: fixed and never removed in a session (decision
    // 581), so read once; a saved skill's id isn't typed.
    private IReadOnlyCollection<string> TakenIds { get; init; } = [];

    /// <summary>
    /// Why the skill can't be built: the typed id while new, then a cost row's, an effect box's, then a grant row's;
    /// null when it can.
    /// </summary>
    public string? Problem =>
        (IsNew ? ObjectIds.ProblemOf(Id, TakenIds) : null) ?? Costs.Problem ?? Effect.Problem ?? Grants.Problem;

    /// <summary>
    /// The record it came from with the typed fields over it: the strip trimmed, and none when its box is empty; the
    /// costs from their rows; the effect from its boxes, granting its rows' statuses.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public SkillDefinition ToSkill() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : TypedRecord() with
            {
                ResourceChanges = Costs.ToChanges(),
                Requirements = Costs.ToRequirements(),
                Effect = Effect.ToEffect() with { Statuses = Grants.ToGrants() },
            };

    // An added cost row starts on it; empty when the boxes offer no resource.
    private static string FirstResourceOf(SkillChoices choices) => choices.Resources is [var first, ..] ? first.Id : "";

    // A flat power has no type to repeat, and Data refuses a damage without one; Core's first type stands in until the
    // Effect section's box picks another.
    private static Effect StartingEffect(TieredEffect? tier) =>
        tier is null
            ? new Effect(EffectKind.Damage, FlatStartingPower, Targeting.SingleEnemy, DamageType: Enum.GetValues<DamageType>()[0])
            : new Effect(tier.Kind, 0, Targeting.SingleEnemy, DamageType: tier.DamageType) { Tier = tier };

    // The costs' rows, the effect's boxes and the grant rows are left out: a duplicate copies them as typed (their
    // CopyOf), and ToSkill alone reads what they hold.
    private SkillDefinition TypedRecord() =>
        _original with
        {
            Id = Id,
            Name = Name.Trim(),
            Description = Description,
            IconFile = IconFile,
            SoundFile = SoundFile,
            EffectFile = EffectFile.Trim() is { Length: > 0 } strip ? strip : null,
            IsQuick = IsQuick,
        };

    partial void OnIdTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnNameChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnDescriptionChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnIconFileChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnSoundFileChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnEffectFileChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnIsQuickChanged(bool value) => Changed?.Invoke(this, EventArgs.Empty);
}
