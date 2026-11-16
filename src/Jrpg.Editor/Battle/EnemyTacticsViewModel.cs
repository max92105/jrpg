using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Battle;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Battle;

/// <summary>
/// All of one enemy's rules as the Tactics tab edits them (decision 849): the list from the start, then one list per
/// phase in the file's order, phases added after the last and removed. Each phase's list carries Data's name for it,
/// "phases[1].tactics", kept right as phases come and go, so the first problem points at the row the page shows.
/// <see cref="Changed"/> tells the owner of each typed or listed change.
/// </summary>
internal sealed partial class EnemyTacticsViewModel : ObservableObject
{
    // A new phase's threshold box (decision 859; design.md, section 1): half the HP for the first phase, then half the
    // last phase's, never under the least percent Data takes. Typed over anyway.
    private const int FirstPhaseThreshold = 50;
    private const int LeastThreshold = 1;

    // The list from the start comes first.
    private EnemyTacticsViewModel(IEnumerable<TacticListViewModel> lists)
    {
        Lists = [.. lists];
        foreach (var list in Lists)
        {
            list.RulesChanged += OnListRulesChanged;
        }

        Lists.CollectionChanged += OnListsChanged;
        NamePhaseFields();
        SelectedList = Lists[0];
    }

    /// <summary>
    /// A list's rules or threshold typed, or a phase added or removed; never a list's own label or problem notice, nor
    /// a choice of list or rule, so the owner re-checks once per edit (<c>.claude/rules/editor.md</c>).
    /// </summary>
    public event EventHandler? Changed;

    public static EnemyTacticsViewModel From(EnemyDefinition enemy) =>
        new([TacticListViewModel.FromStart(enemy.Tactics), .. enemy.Phases.Select(TacticListViewModel.ForPhase)]);

    /// <summary>A copy of every list as typed, a problem included, so a copy never throws.</summary>
    public static EnemyTacticsViewModel CopyOf(EnemyTacticsViewModel tactics) =>
        new(tactics.Lists.Select(TacticListViewModel.CopyOf));

    /// <summary>The list from the start, then each phase's in the file's order.</summary>
    public ObservableCollection<TacticListViewModel> Lists { get; }

    /// <summary>The list whose rules the tab shows; null while the lists' box has none chosen.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemovePhaseCommand))]
    public partial TacticListViewModel? SelectedList { get; set; }

    /// <summary>
    /// "Uses: cleave, field_dressing", each skill the rules name once, in list order, which are the enemy's skills
    /// (decision 851); "Uses: only the attack" when they name none.
    /// </summary>
    public string SkillsUsedLine =>
        string.Join(", ", NamedSkillIds) is { Length: > 0 } ids ? $"Uses: {ids}" : "Uses: only the attack";

    /// <summary>The first list's problem, in list order; null when every list can be built.</summary>
    public string? Problem => Lists.Select(list => list.Problem).FirstOrDefault(problem => problem is not null);

    /// <exception cref="InvalidOperationException">The list from the start has a problem.</exception>
    public IReadOnlyList<EnemyAction> ToTactics() => Lists[0].ToActions();

    /// <exception cref="InvalidOperationException">A phase's list has a problem.</exception>
    public IReadOnlyList<EnemyPhase> ToPhases() => [.. Lists.Skip(1).Select(list => list.ToPhase())];

    // The skill ids in the rules' order; the attack's empty id names no skill.
    private IEnumerable<string> NamedSkillIds =>
        Lists.SelectMany(list => list.Rules).Select(row => row.SkillId).Where(id => id.Length > 0).Distinct();

    // Half the last phase's threshold rounded down, while it is a number; a new phase starts under the last one.
    private int NewThreshold =>
        Lists[^1] is { IsPhase: true } last && TypedNumber.Parse(last.ThresholdText) is { } typed
            ? Math.Max(typed / 2, LeastThreshold)
            : FirstPhaseThreshold;

    [RelayCommand]
    private void AddPhase()
    {
        var phase = TacticListViewModel.NewPhase(NewThreshold);
        Lists.Add(phase);
        SelectedList = phase;
    }

    // The list before is chosen first, so the lists' box never sees its chosen list taken away.
    [RelayCommand(CanExecute = nameof(IsPhaseChosen))]
    private void RemovePhase()
    {
        var place = Lists.IndexOf(SelectedList!);
        SelectedList = Lists[place - 1];
        Lists.RemoveAt(place);
    }

    private bool IsPhaseChosen() => SelectedList is { IsPhase: true };

    private void OnListsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ListItemWatch.Follow<TacticListViewModel>(
            e,
            list => list.RulesChanged += OnListRulesChanged,
            list => list.RulesChanged -= OnListRulesChanged);
        NamePhaseFields();
        RaiseChanged();
    }

    private void OnListRulesChanged(object? sender, EventArgs e) => RaiseChanged();

    private void RaiseChanged()
    {
        OnPropertyChanged(nameof(SkillsUsedLine));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    // Data names a phase's rules by its place among the phases, which a removal shifts.
    private void NamePhaseFields() =>
        ListOrder.Renumber(Lists.Skip(1).ToList(), (phase, place) => phase.Field = $"phases[{place}].tactics");
}
