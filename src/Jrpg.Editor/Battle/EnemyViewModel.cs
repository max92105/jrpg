using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Battle;
using Jrpg.Core.Stats;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Battle;

/// <summary>
/// One enemy as the Enemies window edits it (ADR 0045): its id (typed while new, fixed once the draft holds it,
/// decision 582), name and numbers as typed. What no form edits yet (its art, rewards, tactics and phases) is carried
/// from the record it came from, so the window never wipes it; a duplicate carries it too (decision 585).
/// <see cref="Problem"/> is the typed id's or a number that isn't one; the ranges are Data's rules, shown by the list.
/// </summary>
internal sealed partial class EnemyViewModel : ObservableObject
{
    // What Add starts from (design tunables): the validator's lowest legal numbers, blunt, no art.
    private static readonly EnemyDefinition _blank = new()
    {
        Id = "",
        Name = "",
        PortraitFile = "",
        BattleSpriteFile = "",
        AttackEffectFile = "",
        IsSpriteFlipped = false,
        MaxHp = 1,
        Damage = 0,
        DamageType = DamageType.Blunt,
        Accuracy = 75,
        Dodge = 0,
        CritChance = 0,
        CrowdControlResistance = 0,
        ExperienceReward = 0,
    };

    private readonly EnemyDefinition _original;

    private EnemyViewModel(EnemyDefinition original, string id)
    {
        _original = original;
        Id = id;
        Name = original.Name;
        MaxHpText = TypedNumber.Text(original.MaxHp);
        DamageText = TypedNumber.Text(original.Damage);
        DamageType = original.DamageType;
        AccuracyText = TypedNumber.Text(original.Accuracy);
        DodgeText = TypedNumber.Text(original.Dodge);
        CritChanceText = TypedNumber.Text(original.CritChance);
        CrowdControlResistanceText = TypedNumber.Text(original.CrowdControlResistance);
        ExperienceRewardText = TypedNumber.Text(original.ExperienceReward);
    }

    public static EnemyViewModel From(EnemyDefinition enemy) => new(enemy, enemy.Id);

    /// <summary>An enemy with nothing typed yet, from the tunable defaults.</summary>
    public static EnemyViewModel New() => new(_blank, "") { IsNew = true };

    /// <summary>
    /// A new enemy copying the chosen one as typed, its id left to type (decision 585); its tactics come along. Copied
    /// from the boxes, not the record, so a half-typed enemy duplicates too.
    /// </summary>
    public static EnemyViewModel DuplicateOf(EnemyViewModel chosen) => new(chosen._original, "")
    {
        IsNew = true,
        Name = chosen.Name,
        MaxHpText = chosen.MaxHpText,
        DamageText = chosen.DamageText,
        DamageType = chosen.DamageType,
        AccuracyText = chosen.AccuracyText,
        DodgeText = chosen.DodgeText,
        CritChanceText = chosen.CritChanceText,
        CrowdControlResistanceText = chosen.CrowdControlResistanceText,
        ExperienceRewardText = chosen.ExperienceRewardText,
    };

    /// <summary>
    /// True while the enemy was added this session: the only time its id can be typed (decision 582).
    /// </summary>
    public bool IsNew { get; private init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string Name { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string MaxHpText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string DamageText { get; set; }

    [ObservableProperty]
    public partial DamageType DamageType { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string AccuracyText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string DodgeText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string CritChanceText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string CrowdControlResistanceText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string ExperienceRewardText { get; set; }

    /// <summary>The list's line: "goblin: Goblin", or a placeholder while a new enemy has no id.</summary>
    public string Label => Id.Length > 0 ? $"{Id}: {Name}" : "(new enemy)";

    /// <summary>Why the typed id or a number can't be read; null when the record can be built.</summary>
    public string? Problem => (IsNew ? ObjectIds.ProblemOf(Id, []) : null) ?? FirstNumberProblem();

    /// <summary>The record it came from with the edited fields over it.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public EnemyDefinition ToEnemy() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : _original with
            {
                Id = Id,
                Name = Name.Trim(),
                MaxHp = TypedNumber.Parse(MaxHpText)!.Value,
                Damage = TypedNumber.Parse(DamageText)!.Value,
                DamageType = DamageType,
                Accuracy = TypedNumber.Parse(AccuracyText)!.Value,
                Dodge = TypedNumber.Parse(DodgeText)!.Value,
                CritChance = TypedNumber.Parse(CritChanceText)!.Value,
                CrowdControlResistance = TypedNumber.Parse(CrowdControlResistanceText)!.Value,
                ExperienceReward = TypedNumber.Parse(ExperienceRewardText)!.Value,
            };

    // In the form's order, so the problem names the first box the eye reaches.
    private string? FirstNumberProblem() =>
        TypedNumber.ProblemOf("Max HP", MaxHpText)
        ?? TypedNumber.ProblemOf("Damage", DamageText)
        ?? TypedNumber.ProblemOf("Accuracy", AccuracyText)
        ?? TypedNumber.ProblemOf("Dodge", DodgeText)
        ?? TypedNumber.ProblemOf("Crit chance", CritChanceText)
        ?? TypedNumber.ProblemOf("Crowd control resistance", CrowdControlResistanceText)
        ?? TypedNumber.ProblemOf("Experience", ExperienceRewardText);
}
