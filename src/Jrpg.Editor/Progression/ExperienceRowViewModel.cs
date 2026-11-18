using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Progression;

/// <summary>
/// One level step of the experience table as the Experience page types it (decision 956): the experience from
/// <see cref="Level"/> to the next as typed, and the total to reach that next level, which the table works out.
/// <see cref="Problem"/> is only an amount that isn't a whole number; one below 1 is Data's rule, shown by the table.
/// </summary>
internal sealed partial class ExperienceRowViewModel : ObservableObject
{
    /// <param name="level">The level the step starts at, from 1.</param>
    /// <param name="amountText">The experience the step needs, as typed.</param>
    public ExperienceRowViewModel(int level, string amountText)
    {
        Level = level;
        AmountText = amountText;
    }

    public int Level { get; }

    /// <summary>"Level 4 to 5".</summary>
    public string Label => LabelOf(Level);

    /// <summary>The label of the step starting at <paramref name="level"/>, for the fill's line too.</summary>
    public static string LabelOf(int level) => $"Level {level} to {level + 1}";

    [ObservableProperty]
    public partial string AmountText { get; set; }

    /// <summary>
    /// The experience from level 1 to the next level, this row's included; a <c>long</c>, since rows near the largest
    /// whole number add past it. Null while this box or one above it isn't a whole number.
    /// </summary>
    [ObservableProperty]
    public partial long? TotalToReach { get; set; }

    /// <summary>The amount, or null while the box isn't a whole number.</summary>
    public int? Amount => TypedNumber.Parse(AmountText);

    /// <summary>"Level 4 to 5 must be a whole number (was 'x')."; null when the amount can be read.</summary>
    public string? Problem => TypedNumber.ProblemOf(Label, AmountText);
}
