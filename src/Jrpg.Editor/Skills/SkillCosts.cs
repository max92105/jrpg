using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Skills;

/// <summary>
/// A skill's costs as the Skills page edits them (decision 979), two row lists as Core holds them: its resource changes
/// and its requirements, each added after the chosen row and removed. <see cref="Problem"/> is the first row that can't
/// be built; Data's rules are the list's. <see cref="Changed"/> tells the skill of each typed or listed change.
/// </summary>
internal sealed partial class SkillCosts : ObservableObject
{
    // A new row's starting boxes (design tunables): a change spending one point, a requirement of the least Data takes.
    private const int NewChangeAmount = -1;
    private const int NewNeedAtLeast = 1;

    private readonly string _firstResourceId;

    private SkillCosts(IEnumerable<ResourceChangeRow> changes, IEnumerable<ResourceNeedRow> needs, string firstResourceId)
    {
        _firstResourceId = firstResourceId;
        Changes = [.. changes];
        Needs = [.. needs];
        foreach (var row in Changes.Cast<INotifyPropertyChanged>().Concat(Needs))
        {
            row.PropertyChanged += OnRowChanged;
        }

        Changes.CollectionChanged += OnRowsListChanged;
        Needs.CollectionChanged += OnRowsListChanged;
        SelectedChange = Changes.FirstOrDefault();
        SelectedNeed = Needs.FirstOrDefault();
    }

    /// <summary>
    /// A row's resource or number typed, or a row added or removed; never a row's problem notice, so the list re-checks
    /// once per change (<c>.claude/rules/editor.md</c>).
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="skill">The record whose costs the rows start from.</param>
    /// <param name="firstResourceId">
    /// The first resource the boxes offer, an added row's; empty when none is offered.
    /// </param>
    public static SkillCosts From(SkillDefinition skill, string firstResourceId) =>
        new(
            skill.ResourceChanges.Select(change => new ResourceChangeRow(change.ResourceId, TypedNumber.Text(change.Amount))),
            skill.Requirements.Select(need => new ResourceNeedRow(need.ResourceId, TypedNumber.Text(need.AtLeast))),
            firstResourceId);

    /// <summary>A copy of the rows as typed, for a duplicated skill: a half-typed box is copied, never read.</summary>
    public static SkillCosts CopyOf(SkillCosts costs) =>
        new(
            costs.Changes.Select(row => new ResourceChangeRow(row.ResourceId, row.AmountText)),
            costs.Needs.Select(row => new ResourceNeedRow(row.ResourceId, row.AtLeastText)),
            costs._firstResourceId);

    public ObservableCollection<ResourceChangeRow> Changes { get; }

    public ObservableCollection<ResourceNeedRow> Needs { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveChangeCommand))]
    public partial ResourceChangeRow? SelectedChange { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveNeedCommand))]
    public partial ResourceNeedRow? SelectedNeed { get; set; }

    /// <summary>
    /// "Spends 2 mana, gains 1 focus; needs 1 faith" from the rows that can be read, or "Free" when none can.
    /// </summary>
    public string SummaryText
    {
        get
        {
            var changes = Changes.Where(row => row.Problem is null).Select(row => row.ToChange()).ToList();
            // Negated as a long: a typed int.MinValue has no int opposite.
            var spends = Phrase(
                "spends",
                changes.Where(change => change.Amount < 0).Select(change => $"{-(long)change.Amount} {change.ResourceId}"));
            var gains = Phrase(
                "gains",
                changes.Where(change => change.Amount > 0).Select(change => $"{change.Amount} {change.ResourceId}"));
            var needs = Phrase(
                "needs",
                Needs.Where(row => row.Problem is null)
                    .Select(row => row.ToRequirement())
                    .Select(need => $"{need.AtLeast} {need.ResourceId}"));
            var summary = JoinPresent("; ", JoinPresent(", ", spends, gains), needs);
            return summary.Length == 0 ? "Free" : char.ToUpperInvariant(summary[0]) + summary[1..];
        }
    }

    /// <summary>
    /// The first row that can't be built, numbered from 1 as the page shows it: "Changes row 2: Amount must be a whole
    /// number (was 'x')."; null when every row can.
    /// </summary>
    public string? Problem =>
        FirstProblem("Changes", Changes, row => row.Problem) ?? FirstProblem("Needs", Needs, row => row.Problem);

    /// <exception cref="InvalidOperationException">A change row has a problem.</exception>
    public IReadOnlyList<ResourceChange> ToChanges() => [.. Changes.Select(row => row.ToChange())];

    /// <exception cref="InvalidOperationException">A need row has a problem.</exception>
    public IReadOnlyList<ResourceRequirement> ToRequirements() => [.. Needs.Select(row => row.ToRequirement())];

    private static string Phrase(string verb, IEnumerable<string> amounts) =>
        amounts.ToList() is { Count: > 0 } parts ? $"{verb} {string.Join(" and ", parts)}" : "";

    private static string JoinPresent(string separator, params string[] parts) =>
        string.Join(separator, parts.Where(part => part.Length > 0));

    private static string? FirstProblem<T>(string list, ObservableCollection<T> rows, Func<T, string?> problemOf)
    {
        var index = rows.ToList().FindIndex(row => problemOf(row) is not null);
        return index < 0 ? null : $"{list} row {index + 1}: {problemOf(rows[index])}";
    }

    // After the chosen row, or last when none is chosen.
    [RelayCommand]
    private void AddChange()
    {
        var row = new ResourceChangeRow(_firstResourceId, TypedNumber.Text(NewChangeAmount));
        Changes.Insert(SelectedChange is null ? Changes.Count : Changes.IndexOf(SelectedChange) + 1, row);
        SelectedChange = row;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedChange))]
    private void RemoveChange() => SelectedChange = ListOrder.Remove(Changes, SelectedChange!);

    private bool HasSelectedChange() => SelectedChange is not null;

    [RelayCommand]
    private void AddNeed()
    {
        var row = new ResourceNeedRow(_firstResourceId, TypedNumber.Text(NewNeedAtLeast));
        Needs.Insert(SelectedNeed is null ? Needs.Count : Needs.IndexOf(SelectedNeed) + 1, row);
        SelectedNeed = row;
    }

    [RelayCommand(CanExecute = nameof(HasSelectedNeed))]
    private void RemoveNeed() => SelectedNeed = ListOrder.Remove(Needs, SelectedNeed!);

    private bool HasSelectedNeed() => SelectedNeed is not null;

    private void OnRowsListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ListItemWatch.Follow<INotifyPropertyChanged>(
            e,
            row => row.PropertyChanged += OnRowChanged,
            row => row.PropertyChanged -= OnRowChanged);
        RaiseChanged();
    }

    // A need row's resource box is named as a change row's, so one name covers both.
    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ResourceChangeRow.ResourceId) or nameof(ResourceChangeRow.AmountText)
            or nameof(ResourceNeedRow.AtLeastText))
        {
            RaiseChanged();
        }
    }

    private void RaiseChanged()
    {
        OnPropertyChanged(nameof(SummaryText));
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
