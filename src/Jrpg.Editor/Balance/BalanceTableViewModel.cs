using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Balance;

/// <summary>
/// The balance table's boxes and rows (decisions 992, 993): three typed levels and a typed damage modifier in percent,
/// the line naming a box that can't be read, and the rows read through the boxes, empty while that line shows (the
/// enemy rows read no box). The rows' builders do the building; this only reads the boxes and hands them on.
/// </summary>
internal sealed partial class BalanceTableViewModel : ObservableObject
{
    /// <summary>
    /// The highest typed damage modifier, a hundredfold (decision 1021): past the developer's sheet's 473 % many times
    /// over, and in permille, with a member's gear and status modifiers added, far within an <see cref="int"/>.
    /// </summary>
    public const int MaxModifierPercent = 10_000;

    // -100 % cancels every amount by Core's rule, which tells nothing about a skill.
    private const int LowestModifierPercent = -99;

    private const int PermillePerPercent = 10;

    // What the table opens with (decision 992): a new member, mid-game and late game, with no modifier.
    private const int StartingFirstLevel = 1;
    private const int StartingSecondLevel = 10;
    private const int StartingThirdLevel = 20;
    private const int StartingModifierPercent = 0;

    private const int LowestLevel = 1;

    private readonly ContentCatalog _content;

    /// <param name="content">The catalog whose classes, enemies, skills and rules the rows read.</param>
    public BalanceTableViewModel(ContentCatalog content)
    {
        _content = content;
        FirstLevelText = TypedNumber.Text(StartingFirstLevel);
        SecondLevelText = TypedNumber.Text(StartingSecondLevel);
        ThirdLevelText = TypedNumber.Text(StartingThirdLevel);
        ModifierPercentText = TypedNumber.Text(StartingModifierPercent);
        EnemyRows = EnemyBalanceRows.Of(content);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BoxesProblem), nameof(ClassSkillRows), nameof(EnemySkillRows), nameof(ClassRows))]
    public partial string FirstLevelText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BoxesProblem), nameof(ClassSkillRows), nameof(EnemySkillRows), nameof(ClassRows))]
    public partial string SecondLevelText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BoxesProblem), nameof(ClassSkillRows), nameof(EnemySkillRows), nameof(ClassRows))]
    public partial string ThirdLevelText { get; set; }

    /// <summary>
    /// Added to each user's damage modifiers' sum, so it raises damage, never healing (decision 786).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BoxesProblem), nameof(ClassSkillRows), nameof(EnemySkillRows), nameof(ClassRows))]
    public partial string ModifierPercentText { get; set; }

    /// <summary>
    /// "Level 2 must be a whole number from 1 to 20.", for the first box that can't be read; null when all can. Never
    /// named <c>Problem</c>: the Database workspace refuses leaving a page with a problem, and a read-only table has
    /// nothing to keep the developer on.
    /// </summary>
    public string? BoxesProblem =>
        LevelProblem(1, FirstLevelText)
        ?? LevelProblem(2, SecondLevelText)
        ?? LevelProblem(3, ThirdLevelText)
        ?? (TypedModifierPercent() is null
            ? $"Damage modifier must be a whole number from {LowestModifierPercent} to {MaxModifierPercent}."
            : null);

    /// <summary>
    /// Each class's damaging and healing skills at the typed levels; empty while a box can't be read.
    /// </summary>
    public IReadOnlyList<SkillBalanceRow> ClassSkillRows =>
        ReadBoxes() is { } boxes ? ClassSkillBalanceRows.Of(_content, boxes.Levels, boxes.ModifierPermille) : [];

    /// <summary>
    /// Each enemy's damaging and healing skills at its power, with the typed modifier; empty while a box can't be read.
    /// </summary>
    public IReadOnlyList<EnemySkillBalanceRow> EnemySkillRows =>
        ReadBoxes() is { } boxes ? EnemySkillBalanceRows.Of(_content, boxes.ModifierPermille) : [];

    /// <summary>Each class's max HP and Power at the typed levels; empty while a box can't be read.</summary>
    public IReadOnlyList<ClassBalanceRow> ClassRows =>
        ReadBoxes() is { } boxes ? ClassBalanceRows.Of(_content, boxes.Levels) : [];

    /// <summary>Each enemy's HP, power and attack, which no box changes, so listed whatever the boxes hold.</summary>
    public IReadOnlyList<EnemyBalanceRow> EnemyRows { get; }

    private int MaxLevel => _content.Rules.Experience.MaxLevel;

    /// <returns>The boxes as read; null while one can't be.</returns>
    private TypedBoxes? ReadBoxes() =>
        TypedLevels() is { } levels && TypedModifierPercent() is { } percent
            ? new TypedBoxes(levels, percent * PermillePerPercent)
            : null;

    private string? LevelProblem(int boxNumber, string text) =>
        TypedLevel(text) is null ? $"Level {boxNumber} must be a whole number from {LowestLevel} to {MaxLevel}." : null;

    /// <returns>The three levels in the boxes' order; null while one can't be read.</returns>
    private int[]? TypedLevels()
    {
        string[] texts = [FirstLevelText, SecondLevelText, ThirdLevelText];
        int[] levels = [.. texts.Select(TypedLevel).OfType<int>()];
        return levels.Length == texts.Length ? levels : null;
    }

    private int? TypedLevel(string text) =>
        TypedNumber.Parse(text) is { } level && level >= LowestLevel && level <= MaxLevel ? level : null;

    private int? TypedModifierPercent() =>
        TypedNumber.Parse(ModifierPercentText) is { } percent and >= LowestModifierPercent and <= MaxModifierPercent
            ? percent
            : null;

    /// <param name="Levels">The three levels in the boxes' order.</param>
    /// <param name="ModifierPermille">The typed modifier in permille, as Core sums the damage modifiers.</param>
    private readonly record struct TypedBoxes(int[] Levels, int ModifierPermille);
}
