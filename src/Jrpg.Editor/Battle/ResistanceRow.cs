using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Battle;

/// <summary>
/// One damage type's resistance as typed, a whole percent (ADR 0027); one row per <see cref="DamageType"/>, so a type
/// added to Core appears on the page (design key decision 3). The range is Data's rule, shown by the list.
/// </summary>
internal sealed partial class ResistanceRow : ObservableObject
{
    /// <param name="type">The damage type.</param>
    /// <param name="percent">The resistance as the record holds it, a whole percent.</param>
    public ResistanceRow(DamageType type, int percent)
    {
        Type = type;
        PercentText = TypedNumber.Text(percent);
    }

    public DamageType Type { get; }

    /// <summary>The row's label: the type's name.</summary>
    public string Name => Type.ToString();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string PercentText { get; set; }

    /// <summary>Why the box can't be read; null when it holds a whole number.</summary>
    public string? Problem => TypedNumber.ProblemOf($"{Name} resistance", PercentText);
}
