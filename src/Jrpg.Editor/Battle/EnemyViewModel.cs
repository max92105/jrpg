using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Battle;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Battle;

/// <summary>
/// One enemy as the Enemies page edits it (ADR 0045): its id (a saved one's retyped too, the encounters naming it
/// following, which overturns decision 582 for enemies), name, numbers, resistances, rewards, art and rules as typed,
/// each written over the record it came from; a duplicate copies them as typed (decision 585). <see cref="Problem"/> is
/// the typed id's or a box that isn't a number, the art's loop boxes, then the first rule's; the ranges are Data's
/// rules, shown by the list.
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

    // The rules come in built, since a duplicate copies the chosen one's boxes rather than reading a record.
    private EnemyViewModel(EnemyDefinition original, string id, EnemyTacticsViewModel tactics)
    {
        _original = original;
        Id = id;
        Name = original.Name;
        MaxHpText = TypedNumber.Text(original.MaxHp);
        DamageText = TypedNumber.Text(original.Damage);
        PowerText = TypedNumber.Text(original.Power);
        DamageType = original.DamageType;
        AccuracyText = TypedNumber.Text(original.Accuracy);
        DodgeText = TypedNumber.Text(original.Dodge);
        CritChanceText = TypedNumber.Text(original.CritChance);
        CrowdControlResistanceText = TypedNumber.Text(original.CrowdControlResistance);
        ExperienceRewardText = TypedNumber.Text(original.ExperienceReward);
        IsBoss = original.IsBoss;
        Resistances = WatchedResistanceRows(original);
        ReadRewards(original);
        Art = new EnemyArtViewModel(original);
        Art.PropertyChanged += (_, _) => OnPropertyChanged(nameof(Art));
        Tactics = tactics;
        Tactics.Changed += OnTacticsChanged;
    }

    public static EnemyViewModel From(EnemyDefinition enemy) => new(enemy, enemy.Id, EnemyTacticsViewModel.From(enemy));

    /// <summary>An enemy with nothing typed yet, from the tunable defaults.</summary>
    public static EnemyViewModel New() => new(_blank, "", EnemyTacticsViewModel.From(_blank)) { IsNew = true };

    /// <summary>
    /// A new enemy copying the chosen one as typed, its rules as typed too, its id left to type (decision 585). Copied
    /// from the boxes, not the record, so a half-typed enemy or rule duplicates too.
    /// </summary>
    public static EnemyViewModel DuplicateOf(EnemyViewModel chosen)
    {
        var copy = new EnemyViewModel(chosen._original, "", EnemyTacticsViewModel.CopyOf(chosen.Tactics))
        {
            IsNew = true,
            Name = chosen.Name,
            MaxHpText = chosen.MaxHpText,
            DamageText = chosen.DamageText,
            PowerText = chosen.PowerText,
            DamageType = chosen.DamageType,
            AccuracyText = chosen.AccuracyText,
            DodgeText = chosen.DodgeText,
            CritChanceText = chosen.CritChanceText,
            CrowdControlResistanceText = chosen.CrowdControlResistanceText,
            ExperienceRewardText = chosen.ExperienceRewardText,
            IsBoss = chosen.IsBoss,
        };
        copy.CopyResistancesFrom(chosen);
        copy.CopyRewardsFrom(chosen);
        copy.Art.CopyFrom(chosen.Art);
        return copy;
    }

    /// <summary>Its three pictures and the flip; a change in them is this enemy's change.</summary>
    public EnemyArtViewModel Art { get; }

    /// <summary>
    /// True while the enemy was added this session: the only time it can be removed (decision 581).
    /// </summary>
    public bool IsNew { get; private init; }

    /// <summary>
    /// The id the draft holds it under, whatever is typed since; null for an enemy added this session.
    /// </summary>
    public string? SavedId => IsNew ? null : _original.Id;

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

    /// <summary>What its tiered skills multiply, as typed (decision 794); 0 for an enemy with none.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string PowerText { get; set; }

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

    /// <summary>Whether the party can't run from a battle it is in (decisions 407, 414).</summary>
    [ObservableProperty]
    public partial bool IsBoss { get; set; }

    /// <summary>The list's line: "goblin: Goblin", or a placeholder while a new enemy has no id.</summary>
    public string Label => Id.Length > 0 ? $"{Id}: {Name}" : "(new enemy)";

    /// <summary>Why the typed id, a number or a rule can't be read; null when the record can be built.</summary>
    public string? Problem =>
        ObjectIds.ProblemOf(Id, []) ?? FirstNumberProblem() ?? FirstResistanceProblem() ?? FirstGoldProblem()
        ?? Art.Problem ?? Tactics.Problem;

    /// <summary>The record it came from with every field as typed over it, its rules rebuilt from their rows.</summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public EnemyDefinition ToEnemy() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : Art.ApplyTo(_original with
            {
                Id = Id,
                Name = Name.Trim(),
                MaxHp = TypedNumber.Parse(MaxHpText)!.Value,
                Damage = TypedNumber.Parse(DamageText)!.Value,
                Power = TypedNumber.Parse(PowerText)!.Value,
                DamageType = DamageType,
                Accuracy = TypedNumber.Parse(AccuracyText)!.Value,
                Dodge = TypedNumber.Parse(DodgeText)!.Value,
                CritChance = TypedNumber.Parse(CritChanceText)!.Value,
                CrowdControlResistance = TypedNumber.Parse(CrowdControlResistanceText)!.Value,
                ExperienceReward = TypedNumber.Parse(ExperienceRewardText)!.Value,
                Resistances = TypedResistances(),
                Gold = TypedGold(),
                DropTableId = TypedDropTable(),
                IsBoss = IsBoss,
                Tactics = Tactics.ToTactics(),
                Phases = Tactics.ToPhases(),
            });

    // In the form's order, so the problem names the first box the eye reaches.
    private string? FirstNumberProblem() =>
        TypedNumber.ProblemOf("Max HP", MaxHpText)
        ?? TypedNumber.ProblemOf("Damage", DamageText)
        ?? TypedNumber.ProblemOf("Power", PowerText)
        ?? TypedNumber.ProblemOf("Accuracy", AccuracyText)
        ?? TypedNumber.ProblemOf("Dodge", DodgeText)
        ?? TypedNumber.ProblemOf("Crit chance", CritChanceText)
        ?? TypedNumber.ProblemOf("Crowd control resistance", CrowdControlResistanceText)
        ?? TypedNumber.ProblemOf("Experience", ExperienceRewardText);
}
