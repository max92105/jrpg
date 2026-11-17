using System.Collections.ObjectModel;
using System.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Party;

/// <summary>The class's skills as the Skills tab types them (decision 949).</summary>
internal sealed partial class ClassViewModel
{
    /// <summary>The class's skills in menu order (ADR 0020), each with its level as typed (ADR 0062).</summary>
    public ObservableCollection<ClassSkillRow> Skills { get; }

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

    private void WatchSkillRows()
    {
        foreach (var row in Skills)
        {
            row.PropertyChanged += OnSkillRowTyped;
        }
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
