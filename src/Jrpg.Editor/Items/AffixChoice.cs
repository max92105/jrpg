using Jrpg.Core.Loot;
using Jrpg.Core.Stats;

namespace Jrpg.Editor.Items;

/// <summary>
/// One pool affix a unique's fixed affix row offers (decision 882): what it raises and its roll range, shown for
/// reference only, since a unique's value is free of the range (decision 866). Compared by value, so a row's affix
/// matches its box's entry.
/// </summary>
internal sealed record AffixChoice(string Id, StatKind Stat, int Min, int Max)
{
    public static AffixChoice Of(AffixDefinition affix) => new(affix.Id, affix.Stat, affix.Min, affix.Max);

    /// <summary>The box's line: "power: Power".</summary>
    public string Label => $"{Id}: {Stat}";
}
