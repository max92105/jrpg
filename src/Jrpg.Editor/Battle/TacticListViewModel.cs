using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Battle;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Battle;

/// <summary>
/// One list of an enemy's rules as the Tactics tab edits it (decision 849): the list from the start, or a phase's with
/// its threshold box. Its rules are numbered from 0 as Data numbers them, added after the chosen one, removed and
/// moved. <see cref="Problem"/> is only what keeps the list from being built; Data's rules (a percent's bounds, a
/// target that doesn't fit, an empty phase, the phases' order) are the enemy's, shown by the page.
/// <see cref="RulesChanged"/> tells the owner of each typed or listed change.
/// </summary>
internal sealed partial class TacticListViewModel : ObservableObject
{
    private const string StartField = "tactics";

    // A phase's field until its owner names its place among the phases (EB5).
    private const string FirstPhaseField = "phases[0].tactics";

    // A null threshold is the list from the start, which has no box for one.
    private TacticListViewModel(string field, string? threshold, IEnumerable<TacticRuleRow> rules)
    {
        Field = field;
        IsPhase = threshold is not null;
        ThresholdText = threshold ?? "";
        Rules = [.. rules];
        foreach (var row in Rules)
        {
            row.PropertyChanged += OnRuleChanged;
        }

        Rules.CollectionChanged += OnRulesListChanged;
        Renumber();
        SelectedRule = Rules.FirstOrDefault();
    }

    /// <summary>
    /// A rule's box typed, picked or checked, the threshold typed, or a rule added, removed or moved; never a rule's
    /// own sentence, number or problem notice, so the owner re-checks once per change (<c>.claude/rules/editor.md</c>).
    /// </summary>
    public event EventHandler? RulesChanged;

    public static TacticListViewModel FromStart(IReadOnlyList<EnemyAction> tactics) =>
        new(StartField, null, tactics.Select(TacticRuleRow.From));

    public static TacticListViewModel ForPhase(EnemyPhase phase) =>
        new(FirstPhaseField, TypedNumber.Text(phase.BelowHpPercent), phase.Tactics.Select(TacticRuleRow.From));

    /// <summary>A phase with one new rule in it, since Data refuses a phase without any.</summary>
    public static TacticListViewModel NewPhase(int threshold) =>
        new(FirstPhaseField, TypedNumber.Text(threshold), [TacticRuleRow.New()]);

    /// <summary>A copy of the boxes as typed, a problem included, so a copy never throws.</summary>
    public static TacticListViewModel CopyOf(TacticListViewModel list) =>
        new(list.Field, list.IsPhase ? list.ThresholdText : null, list.Rules.Select(TacticRuleRow.CopyOf));

    public bool IsPhase { get; }

    /// <summary>The HP percent a phase starts below, as typed; empty for the list from the start.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string ThresholdText { get; set; }

    /// <summary>
    /// Data's name for the list, "tactics" or "phases[1].tactics", which a rule's problem starts with; its owner sets a
    /// phase's from its place.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string Field { get; set; }

    public ObservableCollection<TacticRuleRow> Rules { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveRuleCommand), nameof(MoveRuleUpCommand), nameof(MoveRuleDownCommand))]
    public partial TacticRuleRow? SelectedRule { get; set; }

    /// <summary>
    /// The lists' line: "From the start", or "Below 50 % HP" with the threshold as typed, "?" while empty.
    /// </summary>
    public string Label => IsPhase ? $"Below {ShownThreshold} % HP" : "From the start";

    /// <summary>
    /// A phase's threshold that isn't a whole number, then the first rule's problem as "phases[0].tactics[2]: Weight
    /// must be a whole number (was 'x')."; null when the list can be built.
    /// </summary>
    public string? Problem =>
        (IsPhase ? TypedNumber.ProblemOf("Threshold", ThresholdText) : null)
        ?? SectionRowProblem.FirstInField(Field, Rules, row => row.Problem);

    /// <exception cref="InvalidOperationException">A rule has a problem.</exception>
    public IReadOnlyList<EnemyAction> ToActions() => [.. Rules.Select(row => row.ToAction())];

    /// <exception cref="InvalidOperationException">
    /// The list is the one from the start, or there is a <see cref="Problem"/>.
    /// </exception>
    public EnemyPhase ToPhase()
    {
        if (!IsPhase)
        {
            throw new InvalidOperationException("The list from the start has no threshold to build a phase from.");
        }

        return Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new EnemyPhase { BelowHpPercent = TypedNumber.Parse(ThresholdText)!.Value, Tactics = ToActions() };
    }

    [RelayCommand]
    private void AddRule()
    {
        var row = TacticRuleRow.New();
        ListOrder.InsertAfter(Rules, row, SelectedRule);
        SelectedRule = row;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedRule))]
    private void RemoveRule() => SelectedRule = ListOrder.Remove(Rules, SelectedRule!);

    [RelayCommand(CanExecute = nameof(HasSelectedRule))]
    private void MoveRuleUp() => ListOrder.MoveBy(Rules, SelectedRule!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedRule))]
    private void MoveRuleDown() => ListOrder.MoveBy(Rules, SelectedRule!, 1);

    private bool HasSelectedRule() => SelectedRule is not null;

    // The box as typed, trimmed; "?" while empty, as a rule's sentence shows its number, the problem line naming it.
    private string ShownThreshold => ThresholdText.Trim() is { Length: > 0 } typed ? typed : "?";

    partial void OnThresholdTextChanged(string value) => RulesChanged?.Invoke(this, EventArgs.Empty);

    // Each rule's number is Data's tactics[i], so any change to the list renumbers.
    private void OnRulesListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ListItemWatch.Follow<TacticRuleRow>(
            e,
            row => row.PropertyChanged += OnRuleChanged,
            row => row.PropertyChanged -= OnRuleChanged);
        Renumber();
        RulesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnRuleChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TacticRuleRow.When) or nameof(TacticRuleRow.NumberText)
            or nameof(TacticRuleRow.SkillId) or nameof(TacticRuleRow.On) or nameof(TacticRuleRow.WeightText)
            or nameof(TacticRuleRow.IsPriority) or nameof(TacticRuleRow.IsOncePerBattle))
        {
            RulesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Renumber() => ListOrder.Renumber(Rules, (row, index) => row.Index = index);
}
