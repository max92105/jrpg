using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Progression;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Progression;

/// <summary>
/// The Experience page's fill boxes as typed (decision 957): the first amount, the growth percent and the max level,
/// opening on the table's first amount, <see cref="OpeningGrowthPercent"/> and its max level (design section 1).
/// <see cref="Problem"/> is the first box that isn't a whole number, then <see cref="ExperienceCurve.ProblemOf"/>.
/// </summary>
internal sealed partial class ExperienceCurveBoxes : ObservableObject
{
    /// <summary>The growth the boxes open on: today's table grows by it (decision 957).</summary>
    public const int OpeningGrowthPercent = 20;

    /// <param name="table">The table the page opened on; Data gives it one entry at least.</param>
    public ExperienceCurveBoxes(ExperienceTable table)
    {
        FirstText = TypedNumber.Text(table.ExperienceToNextLevel[0]);
        GrowthText = TypedNumber.Text(OpeningGrowthPercent);
        MaxLevelText = TypedNumber.Text(table.MaxLevel);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string FirstText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string GrowthText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string MaxLevelText { get; set; }

    /// <summary>
    /// Why Fill is greyed: "Growth must be a whole number (was 'x').", then the curve's own; null when it fills.
    /// </summary>
    public string? Problem =>
        TypedNumber.ProblemOf("First", FirstText)
        ?? TypedNumber.ProblemOf("Growth", GrowthText)
        ?? TypedNumber.ProblemOf("Max level", MaxLevelText)
        ?? ExperienceCurve.ProblemOf(First, Growth, MaxLevel);

    // Read only once every box is a number.
    private int First => TypedNumber.Parse(FirstText)!.Value;

    private int Growth => TypedNumber.Parse(GrowthText)!.Value;

    private int MaxLevel => TypedNumber.Parse(MaxLevelText)!.Value;

    /// <summary>The rows the curve makes, one per level step.</summary>
    /// <exception cref="InvalidOperationException">A box isn't a whole number; Fill is greyed on it.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The curve has a problem; Fill is greyed on it.</exception>
    public IReadOnlyList<int> ToRows() => ExperienceCurve.Rows(First, Growth, MaxLevel);
}
