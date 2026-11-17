using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Party;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Party;

/// <summary>
/// One skill of a class as the Skills tab types it (decision 949): the skill, fixed, and the level a member learns it
/// at (ADR 0062) as typed. <see cref="Problem"/> is only a level that isn't a whole number; a level below 1 or past the
/// max, or a skill the class can't pay for, is Data's rule, shown by the list.
/// </summary>
internal sealed partial class ClassSkillRow : ObservableObject
{
    // An added skill is known from the start, as an entry naming no level (ADR 0062).
    private const int AddedLevel = 1;

    /// <param name="skill">The skill, or a missing one when the draft lacks its id.</param>
    /// <param name="levelText">The level as typed.</param>
    public ClassSkillRow(RecordChoice skill, string levelText)
    {
        Skill = skill;
        LevelText = levelText;
    }

    /// <summary>A row as Add starts it: the skill known from level 1.</summary>
    public static ClassSkillRow New(RecordChoice skill) => new(skill, TypedNumber.Text(AddedLevel));

    public RecordChoice Skill { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string LevelText { get; set; }

    /// <summary>"Level of Fireball must be a whole number (was 'x')."; null when the level can be read.</summary>
    public string? Problem => TypedNumber.ProblemOf($"Level of {Skill.Name}", LevelText);

    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public ClassSkill ToEntry() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : new ClassSkill(Skill.Id) { Level = TypedNumber.Parse(LevelText)!.Value };
}
