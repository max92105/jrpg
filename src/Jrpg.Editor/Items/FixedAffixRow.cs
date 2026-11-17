using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Items;

/// <summary>
/// One fixed affix of a unique as the Items page types it (decision 882): a pool affix and its value.
/// <see cref="Problem"/> is only a value that can't be read; a 0, a value past the bound, one on the wrong side of zero
/// or an affix named twice is Data's rule (decision 866).
/// </summary>
internal sealed partial class FixedAffixRow : ObservableObject
{
    /// <param name="affix">The pool affix the row names.</param>
    /// <param name="valueText">The value as typed.</param>
    public FixedAffixRow(AffixChoice affix, string valueText)
    {
        Affix = affix;
        ValueText = valueText;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RollLine))]
    public partial AffixChoice Affix { get; set; }

    /// <summary>In the affix stat's bonus unit, as an item's bonus (ADR 0026).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string ValueText { get; set; }

    /// <summary>"rolls 2..5": the affix's range on a rolled piece, for reference; the value may leave it.</summary>
    public string RollLine => $"rolls {Affix.Min}..{Affix.Max}";

    /// <summary>"Value must be a whole number (was 'x')."; null when the value can be read.</summary>
    public string? Problem => TypedNumber.ProblemOf("Value", ValueText);
}
