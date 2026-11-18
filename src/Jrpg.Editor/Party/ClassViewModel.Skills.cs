using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Party;

/// <summary>
/// The class's skills as the Skills tab types them (decision 949): rows in menu order, each added after the chosen row
/// from every skill of the draft not already listed, at level 1, removed and moved. A level typed and every change of
/// the list raise <see cref="Changed"/> once (decision 1014).
/// </summary>
internal sealed partial class ClassViewModel
{
    /// <summary>The class's skills in menu order (ADR 0020), each with its level as typed (ADR 0062).</summary>
    public ObservableCollection<ClassSkillRow> Skills { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveSkillCommand), nameof(MoveSkillUpCommand), nameof(MoveSkillDownCommand))]
    public partial ClassSkillRow? SelectedSkill { get; set; }

    /// <summary>The add box's choice, added by Add; null while nothing is picked.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSkillCommand))]
    public partial RecordChoice? SkillToAdd { get; set; }

    /// <summary>
    /// The add box's entries: the draft's skills by name, then id, the listed ones left out, so a skill is listed once
    /// (Data's rule). Every skill is offered, even one using a resource the class lacks: Data's message names it
    /// (decision 949), so a skill can be added before its resource.
    /// </summary>
    public IReadOnlyList<RecordChoice> SkillsToAdd =>
        RecordChoice.ByName(
            _content.Skills.Values
                .Where(skill => !Skills.Any(row => row.Skill.Id == skill.Id))
                .Select(skill => new RecordChoice(skill.Id, skill.Name)));

    // The first row's problem, in menu order; the rows' own message names the skill.
    private string? SkillsProblem => Skills.Select(row => row.Problem).FirstOrDefault(problem => problem is not null);

    // One row per skill of the record, named through the draft's skills; an id the draft lacks is shown as missing
    // rather than thrown, and Data's message on the line names it.
    private static IEnumerable<ClassSkillRow> SkillRowsOf(ClassDefinition classDefinition, ContentCatalog content) =>
        classDefinition.Skills.Select(
            entry => new ClassSkillRow(
                RecordChoice.Of(entry.SkillId, content.Skills.GetValueOrDefault(entry.SkillId)?.Name),
                TypedNumber.Text(entry.Level)));

    private IReadOnlyList<ClassSkill> SkillsAsTyped() =>
        ListOrder.KeptIfEqual([.. Skills.Select(row => row.ToEntry())], _source.Skills);

    /// <summary>Puts the add box's skill after the chosen row (last when none) at level 1, and chooses it.</summary>
    [RelayCommand(CanExecute = nameof(HasSkillToAdd))]
    private void AddSkill()
    {
        var row = ClassSkillRow.New(SkillToAdd!);
        ListOrder.InsertAfter(Skills, row, SelectedSkill);
        SelectedSkill = row;
        SkillToAdd = null;
    }

    private bool HasSkillToAdd() => SkillToAdd is not null;

    [RelayCommand(CanExecute = nameof(HasSelectedSkill))]
    private void RemoveSkill() => SelectedSkill = ListOrder.Remove(Skills, SelectedSkill!);

    [RelayCommand(CanExecute = nameof(CanMoveSkillUp))]
    private void MoveSkillUp() => ListOrder.MoveBy(Skills, SelectedSkill!, -1);

    [RelayCommand(CanExecute = nameof(CanMoveSkillDown))]
    private void MoveSkillDown() => ListOrder.MoveBy(Skills, SelectedSkill!, 1);

    private bool HasSelectedSkill() => SelectedSkill is not null;

    private bool CanMoveSkillUp() => ListOrder.CanMoveUp(Skills, SelectedSkill);

    private bool CanMoveSkillDown() => ListOrder.CanMoveDown(Skills, SelectedSkill);

    private void WatchSkillRows() =>
        ListItemWatch.WatchRowsOf(
            Skills,
            row => row.PropertyChanged += OnSkillRowTyped,
            row => row.PropertyChanged -= OnSkillRowTyped,
            OnSkillsListChanged);

    private void OnSkillsListChanged()
    {
        MoveSkillUpCommand.NotifyCanExecuteChanged();
        MoveSkillDownCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(SkillsToAdd));
        OnPropertyChanged(nameof(Problem));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    // Only the typed level, never the row's problem notice, so the list runs Data's check once per change.
    private void OnSkillRowTyped(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClassSkillRow.LevelText))
        {
            OnPropertyChanged(nameof(Problem));
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
