using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Items;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Loot;

/// <summary>
/// One equipment kind's box on an affix as the Affixes page types it (decision 884): ticked when the affix may roll on
/// that kind of gear.
/// </summary>
internal sealed partial class EquipmentKindToggle : ObservableObject
{
    /// <param name="kind">The kind of gear the box stands for.</param>
    /// <param name="isChosen">Whether the box starts ticked.</param>
    public EquipmentKindToggle(EquipmentKind kind, bool isChosen)
    {
        Kind = kind;
        IsChosen = isChosen;
    }

    public EquipmentKind Kind { get; }

    /// <summary>"offHand": the kind as the files and Data's messages spell it.</summary>
    public string Label => ContentSpelling.Of(Kind);

    [ObservableProperty]
    public partial bool IsChosen { get; set; }
}
