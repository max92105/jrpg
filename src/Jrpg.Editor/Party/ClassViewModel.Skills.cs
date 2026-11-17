using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    [
        .. _content.Skills.Values
            .Where(skill => !Skills.Any(row => row.Skill.Id == skill.Id))
            .OrderBy(skill => skill.Name, StringComparer.Ordinal)
            .ThenBy(skill => skill.Id, StringComparer.Ordinal)
            .Select(skill => new RecordChoice(skill.Id, skill.Name)),
    ];

    // The first row's problem, in menu order; the rows' own message names the skill.
    private string? SkillsProblem => Skills.Select(row => row.Problem).FirstOrDefault(problem => problem is not null);

    // One row per skill of the record, named through the draft's skills; an id the draft lacks is shown as missing
    // rather than thrown, and Data's message on the line names it.
    private static IEnumerable<ClassSkillRow> SkillRowsOf(ClassDefinition classDefinition, ContentCatalog content) =>
        classDefinition.Skills.Select(
            entry => new ClassSkillRow(ChoiceOf(entry.SkillId, content), TypedNumber.Text(entry.Level)));

    private static RecordChoice ChoiceOf(string skillId, ContentCatalog content) =>
        content.Skills.TryGetValue(skillId, out var skill)
            ? new RecordChoice(skillId, skill.Name)
            : RecordChoice.Missing(skillId);

    // The record's own list while the rows match it in order, so an untouched class equals its record (the record
    // compares its lists by reference).
    private IReadOnlyList<ClassSkill> SkillsAsTyped()
    {
        IReadOnlyList<ClassSkill> typed = [.. Skills.Select(row => row.ToEntry())];
        return typed.SequenceEqual(_source.Skills) ? _source.Skills : typed;
    }

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

    private bool CanMoveSkillUp() => SelectedSkill is { } chosen && Skills.IndexOf(chosen) > 0;

    private bool CanMoveSkillDown() => SelectedSkill is { } chosen && Skills.IndexOf(chosen) < Skills.Count - 1;

    private void WatchSkillRows()
    {
        foreach (var row in Skills)
        {
            row.PropertyChanged += OnSkillRowTyped;
        }

        Skills.CollectionChanged += OnSkillsListChanged;
    }

    // A move changes where the chosen row stands, so the moves' greying is read again with the list.
    private void OnSkillsListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ListItemWatch.Follow<ClassSkillRow>(
            e,
            row => row.PropertyChanged += OnSkillRowTyped,
            row => row.PropertyChanged -= OnSkillRowTyped);
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
