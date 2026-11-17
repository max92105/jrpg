using Jrpg.Core.Party;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// A class's skills as the Skills tab types them (decision 949): the rows in menu order with their levels (ADR 0062),
// written back as typed, a level that isn't a whole number the class's own problem, the rest Data's message.
public sealed class ClassSkillsTests
{
    private static readonly int _maxLevel = SkillsDraft.Rules.Experience.MaxLevel;

    [Fact]
    public void Skills_FromClass_ListsThemInItsOrderWithLevels()
    {
        var archer = ClassViewModel.From(SkilledArcher, SkillsDraft);

        Assert.Equal(
            [("Taunt (taunt)", "1"), ("Expose (expose)", "4")],
            archer.Skills.Select(row => (row.Skill.Label, row.LevelText)));
    }

    [Fact]
    public void Skills_IdTheDraftLacks_ShowsItAsMissing()
    {
        var archer = ClassViewModel.From(SkilledArcher with { Skills = [new("volley")] }, SkillsDraft);

        Assert.Equal("(missing) volley", Assert.Single(archer.Skills).Skill.Label);
    }

    // The missing row is written back as it came, so Data's message, not a throw, names it on the line.
    [Fact]
    public void Problem_IdTheDraftLacks_ShowsDataMessage()
    {
        var archer = SkilledArcher with { Skills = [new("volley")] };

        var catalog = new ClassCatalogViewModel(
            SkillsDraft with { Classes = new Dictionary<string, ClassDefinition> { [archer.Id] = archer } });

        Assert.Equal(["volley"], catalog.SelectedClass!.ToClass().SkillIds);
        Assert.Equal("classes.json: class 'archer': skills has unknown skill id 'volley'.", catalog.Problem);
    }

    [Fact]
    public void ToClass_UntouchedSkills_EqualsTheSource() =>
        Assert.Equal(SkilledArcher, ClassViewModel.From(SkilledArcher, SkillsDraft).ToClass());

    [Fact]
    public void ToClass_TypedLevel_WritesIt()
    {
        var archer = ClassViewModel.From(SkilledArcher, SkillsDraft);

        archer.Skills[0].LevelText = "6";

        Assert.Equal([new ClassSkill("taunt") { Level = 6 }, new ClassSkill("expose") { Level = 4 }], archer.ToClass().Skills);
    }

    [Fact]
    public void Problem_LevelNotANumber_NamesTheSkill()
    {
        var archer = ClassViewModel.From(SkilledArcher, SkillsDraft);

        archer.Skills[1].LevelText = "x";

        Assert.Equal("Level of Expose must be a whole number (was 'x').", archer.Problem);
    }

    [Fact]
    public void PropertyChanged_LevelTyped_RaisesProblem()
    {
        var archer = ClassViewModel.From(SkilledArcher, SkillsDraft);
        var raised = new List<string?>();
        archer.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        archer.Skills[0].LevelText = "x";

        Assert.Equal([nameof(ClassViewModel.Problem)], raised);
    }

    [Fact]
    public void Changed_LevelTyped_IsRaisedOnce()
    {
        var archer = ClassViewModel.From(SkilledArcher, SkillsDraft);
        var raised = 0;
        archer.Changed += (_, _) => raised++;

        archer.Skills[0].LevelText = "2";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void DuplicateOf_TypedLevel_CopiesTheRowsAsTyped()
    {
        var archer = ClassViewModel.From(SkilledArcher, SkillsDraft);
        archer.Skills[1].LevelText = "x";

        var copy = ClassViewModel.DuplicateOf(archer);

        Assert.Equal(["1", "x"], copy.Skills.Select(row => row.LevelText));
        Assert.DoesNotContain(copy.Skills, row => archer.Skills.Contains(row));
    }

    [Fact]
    public void PropertyChanged_LevelTypedInTheList_RaisesProblem()
    {
        var catalog = new ClassCatalogViewModel(SkillsDraft);
        var raised = new List<string?>();
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        catalog.SelectedClass!.Skills[0].LevelText = "2";

        Assert.Equal([nameof(ClassCatalogViewModel.Problem)], raised);
    }

    [Fact]
    public void Problem_LevelAtMax_IsNull()
    {
        var catalog = new ClassCatalogViewModel(SkillsDraft);

        catalog.SelectedClass!.Skills[1].LevelText = $"{_maxLevel}";

        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void Problem_LevelPastMax_ShowsDataMessage()
    {
        var catalog = new ClassCatalogViewModel(SkillsDraft);

        catalog.SelectedClass!.Skills[1].LevelText = $"{_maxLevel + 1}";

        Assert.Equal(
            $"classes.json: class 'archer': skills lists 'expose' at level {_maxLevel + 1}, past the max level {_maxLevel}.",
            catalog.Problem);
    }

    [Fact]
    public void Problem_SkillUsingResourceTheClassLacks_ShowsDataMessage()
    {
        var scholar = Scholar with { Skills = [new(Spark.Id)] };

        var catalog = new ClassCatalogViewModel(
            SkillsDraft with { Classes = new Dictionary<string, ClassDefinition> { [scholar.Id] = scholar } });

        Assert.Equal(
            "classes.json: class 'scholar': skills lists 'spark', which uses 'mana', a resource scholar doesn't have.",
            catalog.Problem);
    }
}
