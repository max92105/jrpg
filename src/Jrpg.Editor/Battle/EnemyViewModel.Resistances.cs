using System.ComponentModel;
using Jrpg.Core.Battle;
using Jrpg.Core.Stats;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Battle;

/// <summary>An enemy's resistances, one row per damage type, whole percents typed and permille written.</summary>
internal sealed partial class EnemyViewModel
{
    /// <summary>One row per damage type, in Core's order.</summary>
    public IReadOnlyList<ResistanceRow> Resistances { get; }

    // The rows from the record's permille, each watched so its change counts as the enemy's.
    private List<ResistanceRow> WatchedResistanceRows(EnemyDefinition original)
    {
        List<ResistanceRow> rows = [.. Enum.GetValues<DamageType>()
            .Select(type => new ResistanceRow(type, StatFormula.WholePercentOf(original.Resistances.Of(type))))];
        foreach (var row in rows)
        {
            row.PropertyChanged += OnResistanceChanged;
        }

        return rows;
    }

    private void CopyResistancesFrom(EnemyViewModel chosen)
    {
        foreach (var (row, from) in Resistances.Zip(chosen.Resistances))
        {
            row.PercentText = from.PercentText;
        }
    }

    // Called once the rows are known to parse; the rows follow the enum's order, so a type indexes its row.
    private Resistances TypedResistances() =>
        Core.Stats.Resistances.From(type => StatFormula.PermilleOf(TypedNumber.Parse(Resistances[(int)type].PercentText)!.Value));

    private string? FirstResistanceProblem() =>
        Resistances.Select(row => row.Problem).FirstOrDefault(problem => problem is not null);

    // A row's typed percent is the enemy's change: the list reads the enemy, not its rows.
    private void OnResistanceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ResistanceRow.PercentText))
        {
            OnPropertyChanged(nameof(Resistances));
            OnPropertyChanged(nameof(Problem));
        }
    }
}
