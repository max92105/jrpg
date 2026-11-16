using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Effects;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Skills;

/// <summary>
/// One skill as the Skills page edits it (ADR 0045): its id (typed while the skill is new, fixed once the draft holds
/// it, decision 582), name, description, icon, sound, own strip and quick flag as typed. What no box edits yet (its
/// costs and effect) is carried from the record it came from, so the page never wipes it. <see cref="Problem"/> is only
/// the typed id's: the rest is Data's rules, which the list shows for every skill at once.
/// </summary>
internal sealed partial class SkillViewModel : ObservableObject
{
    // A new skill's power when the content has no damage tier to start from: a flat damage that deals something.
    private const int FlatStartingPower = 1;

    private readonly SkillDefinition _original;

    // The saved skills' ids: fixed and never removed in a session (decision 581), so read once.
    private readonly IReadOnlyCollection<string> _takenIds;

    private SkillViewModel(SkillDefinition original, string idText, IReadOnlyCollection<string> takenIds)
    {
        _original = original;
        _takenIds = takenIds;
        IdText = idText;
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

    public static SkillViewModel From(SkillDefinition skill) => new(skill, skill.Id, []);

    /// <summary>A skill added this session: its id left to type, the rest as the starting record has it.</summary>
    /// <param name="start">The record whose fields the new skill starts with; its id is not used.</param>
    /// <param name="takenIds">The saved skills' ids, which the typed id must not take.</param>
    public static SkillViewModel ForNew(SkillDefinition start, IReadOnlyCollection<string> takenIds) =>
        new(start, "", takenIds) { IsNew = true };

    /// <summary>
    /// A skill as Add starts it (decision 977): every text and file empty, the given tier's damage on a single enemy,
    /// so the id, name and sound are what is left to type.
    /// </summary>
    /// <param name="firstDamageTier">
    /// The content's first damage tier by id; with none, a flat damage of power 1, which Data's message then refuses.
    /// </param>
    /// <param name="takenIds">The saved skills' ids, which the typed id must not take.</param>
    public static SkillViewModel New(TieredEffect? firstDamageTier, IReadOnlyCollection<string> takenIds) =>
        ForNew(
            new SkillDefinition { Id = "", Name = "", Description = "", IconFile = "", Effect = StartingEffect(firstDamageTier) },
            takenIds);

    /// <summary>
    /// A new skill copying the chosen one as typed, its id left to type (decision 585). Copied from the boxes, not
    /// through <see cref="ToSkill"/>, so a half-typed skill duplicates too.
    /// </summary>
    /// <param name="chosen">The skill chosen in the list, saved or added.</param>
    /// <param name="takenIds">The saved skills' ids, which the typed id must not take.</param>
    public static SkillViewModel DuplicateOf(SkillViewModel chosen, IReadOnlyCollection<string> takenIds) =>
        ForNew(chosen.TypedRecord(), takenIds);

    /// <summary>
    /// True while the skill was added this session: the only time its id can be typed (decision 582).
    /// </summary>
    public bool IsNew { get; private init; }

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

    /// <summary>Why the typed id can't be used; null for a saved skill or a usable id.</summary>
    public string? Problem => IsNew ? ObjectIds.ProblemOf(Id, _takenIds) : null;

    /// <summary>
    /// The record it came from with the typed fields over it: the strip trimmed, and none when its box is empty.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public SkillDefinition ToSkill() => Problem is { } problem ? throw new InvalidOperationException(problem) : TypedRecord();

    // A flat power has no type to repeat, and Data refuses a damage without one; Core's first type stands in until the
    // Effect section's box picks another.
    private static Effect StartingEffect(TieredEffect? tier) =>
        tier is null
            ? new Effect(EffectKind.Damage, FlatStartingPower, Targeting.SingleEnemy, DamageType: Enum.GetValues<DamageType>()[0])
            : new Effect(tier.Kind, 0, Targeting.SingleEnemy, DamageType: tier.DamageType) { Tier = tier };

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
