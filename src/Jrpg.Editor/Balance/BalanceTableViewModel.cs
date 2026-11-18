using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Balance;

/// <summary>
/// The balance table's boxes and rows (decisions 992, 993): three typed levels and a typed damage modifier in percent,
/// the line naming a box that can't be read, and the rows read through the boxes, empty while that line shows. The
/// rows' builders do the building; this only reads the boxes and hands them on.
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

    /// <param name="content">The catalog whose classes, skills and rules the rows read.</param>
    public BalanceTableViewModel(ContentCatalog content)
    {
        _content = content;
        FirstLevelText = TypedNumber.Text(StartingFirstLevel);
        SecondLevelText = TypedNumber.Text(StartingSecondLevel);
        ThirdLevelText = TypedNumber.Text(StartingThirdLevel);
        ModifierPercentText = TypedNumber.Text(StartingModifierPercent);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BoxesProblem), nameof(ClassSkillRows))]
    public partial string FirstLevelText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BoxesProblem), nameof(ClassSkillRows))]
    public partial string SecondLevelText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BoxesProblem), nameof(ClassSkillRows))]
    public partial string ThirdLevelText { get; set; }

    /// <summary>
    /// Added to each user's damage modifiers' sum, so it raises damage, never healing (decision 786).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BoxesProblem), nameof(ClassSkillRows))]
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
        TypedLevels() is { } levels && TypedModifierPercent() is { } percent
            ? ClassSkillBalanceRows.Of(_content, levels, percent * PermillePerPercent)
            : [];

    private int MaxLevel => _content.Rules.Experience.MaxLevel;

    private string? LevelProblem(int box, string text) =>
        TypedLevel(text) is null ? $"Level {box} must be a whole number from {LowestLevel} to {MaxLevel}." : null;

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
}
