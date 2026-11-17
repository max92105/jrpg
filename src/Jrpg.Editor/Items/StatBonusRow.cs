using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Items;

/// <summary>
/// One bonus of an equipment as the Items page types it (decision 880): a stat and its value. <see cref="Problem"/> is
/// only a value that can't be read; a 0, a value past the bound or a stat only affixes give is Data's rule, and a stat
/// listed twice the form's, since the record's bonuses can't hold one twice.
/// </summary>
internal sealed partial class StatBonusRow : ObservableObject
{
    /// <param name="stat">The stat the bonus adds to.</param>
    /// <param name="valueText">The value as typed.</param>
    public StatBonusRow(StatKind stat, string valueText)
    {
        Stat = stat;
        ValueText = valueText;
    }

    [ObservableProperty]
    public partial StatKind Stat { get; set; }

    /// <summary>Whole points for a primary, max HP or speed, whole percents for the rest (ADR 0026).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string ValueText { get; set; }

    /// <summary>"Value must be a whole number (was 'x')."; null when the value can be read.</summary>
    public string? Problem => TypedNumber.ProblemOf("Value", ValueText);
}
