using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Battle;

namespace Jrpg.Editor.Battle;

/// <summary>
/// What an enemy drops when it falls: its gold range (none, or a min and a max) and the drop table it rolls, none or
/// one of the draft's tables.
/// </summary>
internal sealed partial class EnemyViewModel
{
    /// <summary>Whether it drops gold; the range boxes count only while it does.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial bool HasGold { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string GoldMinText { get; set; } = "0";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string GoldMaxText { get; set; } = "0";

    /// <summary>The drop table rolled when it falls, by id; empty for none.</summary>
    [ObservableProperty]
    public partial string DropTableId { get; set; } = "";

    private void ReadRewards(EnemyDefinition original)
    {
        HasGold = original.Gold is not null;
        GoldMinText = TypedNumber.Text(original.Gold?.Min ?? 0);
        GoldMaxText = TypedNumber.Text(original.Gold?.Max ?? 0);
        DropTableId = original.DropTableId ?? "";
    }

    private void CopyRewardsFrom(EnemyViewModel chosen)
    {
        HasGold = chosen.HasGold;
        GoldMinText = chosen.GoldMinText;
        GoldMaxText = chosen.GoldMaxText;
        DropTableId = chosen.DropTableId;
    }

    // Called once the boxes are known to parse: gold only while it drops some.
    private GoldRange? TypedGold() =>
        HasGold ? new GoldRange(TypedNumber.Parse(GoldMinText)!.Value, TypedNumber.Parse(GoldMaxText)!.Value) : null;

    private string? TypedDropTable() => DropTableId.Length > 0 ? DropTableId : null;

    private string? FirstGoldProblem() =>
        HasGold ? TypedNumber.ProblemOf("Gold min", GoldMinText) ?? TypedNumber.ProblemOf("Gold max", GoldMaxText) : null;
}
