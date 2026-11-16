using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Statuses;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Skills;

/// <summary>
/// The statuses a skill's effect grants, as the Skills page edits them (decision 981): rows in the order the effect
/// lands them, each added after the chosen row, moved and removed. <see cref="Problem"/> is the first row that can't be
/// built; Data's rules are the list's. <see cref="Changed"/> tells the skill of each typed or listed change.
/// </summary>
internal sealed partial class SkillGrants : ObservableObject
{
    // The draft's statuses when the page opened (design 2.6, "Picks resolve at apply": one session at a time, so
    // nothing changes the records while the page is open).
    private readonly IReadOnlyDictionary<string, StatusDefinition> _statuses;

    // The status an added row starts on; null when the draft has none, which greys Add.
    private readonly string? _firstStatusId;

    private SkillGrants(
        IEnumerable<StatusGrantRow> rows,
        IReadOnlyDictionary<string, StatusDefinition> statuses,
        string? firstStatusId)
    {
        _statuses = statuses;
        _firstStatusId = firstStatusId;
        Rows = [.. rows];
        foreach (var row in Rows)
        {
            row.PropertyChanged += OnRowChanged;
        }

        Rows.CollectionChanged += OnRowsListChanged;
        SelectedRow = Rows.FirstOrDefault();
    }

    /// <summary>
    /// A row's status or number typed, or a row added, moved or removed; never a row's problem notice, so the list
    /// re-checks once per change (<c>.claude/rules/editor.md</c>).
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="grants">The record's grants the rows start from.</param>
    /// <param name="choices">What the page offers: the statuses an added row starts on and the rows resolve to.</param>
    public static SkillGrants From(IReadOnlyList<StatusGrant> grants, SkillChoices choices) =>
        new(grants.Select(StatusGrantRow.From), choices.StatusRecords, choices.Statuses is [var first, ..] ? first.Id : null);

    /// <summary>A copy of the rows as typed, for a duplicated skill: a half-typed box is copied, never read.</summary>
    public static SkillGrants CopyOf(SkillGrants grants) =>
        new(
            grants.Rows.Select(row => new StatusGrantRow(row.StatusId, row.TurnsText, row.ChanceText)),
            grants._statuses,
            grants._firstStatusId);

    public ObservableCollection<StatusGrantRow> Rows { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveRowCommand), nameof(MoveUpCommand), nameof(MoveDownCommand))]
    public partial StatusGrantRow? SelectedRow { get; set; }

    /// <summary>
    /// The first row that can't be built, numbered from 1 as the page shows it: "Statuses row 2: Turns must be a whole
    /// number (was 'x')."; null when every row can.
    /// </summary>
    public string? Problem => SectionRowProblem.FirstOf("Statuses", Rows, row => row.Problem);

    /// <exception cref="InvalidOperationException">A row has a problem.</exception>
    public IReadOnlyList<StatusGrant> ToGrants() => [.. Rows.Select(row => row.ToGrant(_statuses))];

    [RelayCommand(CanExecute = nameof(HasStatusToGrant))]
    private void AddRow()
    {
        var row = StatusGrantRow.New(_firstStatusId!);
        ListOrder.InsertAfter(Rows, row, SelectedRow);
        SelectedRow = row;
    }

    private bool HasStatusToGrant() => _firstStatusId is not null;

    [RelayCommand(CanExecute = nameof(HasSelectedRow))]
    private void RemoveRow() => SelectedRow = ListOrder.Remove(Rows, SelectedRow!);

    [RelayCommand(CanExecute = nameof(HasSelectedRow))]
    private void MoveUp() => ListOrder.MoveBy(Rows, SelectedRow!, -1);

    [RelayCommand(CanExecute = nameof(HasSelectedRow))]
    private void MoveDown() => ListOrder.MoveBy(Rows, SelectedRow!, 1);

    private bool HasSelectedRow() => SelectedRow is not null;

    private void OnRowsListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ListItemWatch.Follow<StatusGrantRow>(
            e,
            row => row.PropertyChanged += OnRowChanged,
            row => row.PropertyChanged -= OnRowChanged);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(StatusGrantRow.StatusId) or nameof(StatusGrantRow.TurnsText)
            or nameof(StatusGrantRow.ChanceText))
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
