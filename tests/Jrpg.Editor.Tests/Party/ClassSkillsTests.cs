using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// A class's skills as the Skills tab types them (decision 949): the rows in menu order with their levels (ADR 0062),
// written back as typed, a level that isn't a whole number the class's own problem, the rest Data's message.
public sealed class ClassSkillsTests
{
    // Built here: it spends mana, which the scholar lacks.
    private static readonly SkillDefinition _spark = World.Skills["expose"] with
    {
        Id = "spark",
        Name = "Spark",
        ResourceChanges = [new ResourceChange("mana", -1)],
    };

    // Opposite to id order, so a list sorted by id would show.
    private static readonly ClassDefinition _archer =
        Archer with { Skills = [new("taunt"), new("expose") { Level = 4 }] };

    private static readonly ContentCatalog _draft = World with
    {
        Skills = new Dictionary<string, SkillDefinition>
        {
            ["expose"] = World.Skills["expose"],
            ["taunt"] = World.Skills["taunt"],
            [_spark.Id] = _spark,
        },
        Classes = new Dictionary<string, ClassDefinition> { [_archer.Id] = _archer, [Scholar.Id] = Scholar },
    };

    private static readonly int _maxLevel = _draft.Rules.Experience.MaxLevel;

    [Fact]
    public void Skills_FromClass_ListsThemInItsOrderWithLevels()
    {
        var archer = ClassViewModel.From(_archer, _draft);

        Assert.Equal(
            [("Taunt (taunt)", "1"), ("Expose (expose)", "4")],
            archer.Skills.Select(row => (row.Skill.Label, row.LevelText)));
    }

    [Fact]
    public void Skills_IdTheDraftLacks_ShowsItAsMissing()
    {
        var archer = ClassViewModel.From(_archer with { Skills = [new("volley")] }, _draft);

        Assert.Equal("(missing) volley", Assert.Single(archer.Skills).Skill.Label);
    }

    // The missing row is written back as it came, so Data's message, not a throw, names it on the line.
    [Fact]
    public void Problem_IdTheDraftLacks_ShowsDataMessage()
    {
        var archer = _archer with { Skills = [new("volley")] };

        var catalog = new ClassCatalogViewModel(
            _draft with { Classes = new Dictionary<string, ClassDefinition> { [archer.Id] = archer } });

        Assert.Equal(["volley"], catalog.SelectedClass!.ToClass().SkillIds);
        Assert.Equal("classes.json: class 'archer': skills has unknown skill id 'volley'.", catalog.Problem);
    }

    [Fact]
    public void ToClass_UntouchedSkills_EqualsTheSource() =>
        Assert.Equal(_archer, ClassViewModel.From(_archer, _draft).ToClass());

    [Fact]
    public void ToClass_TypedLevel_WritesIt()
    {
        var archer = ClassViewModel.From(_archer, _draft);

        archer.Skills[0].LevelText = "6";

        Assert.Equal([new ClassSkill("taunt") { Level = 6 }, new ClassSkill("expose") { Level = 4 }], archer.ToClass().Skills);
    }

    [Fact]
    public void Problem_LevelNotANumber_NamesTheSkill()
    {
        var archer = ClassViewModel.From(_archer, _draft);

        archer.Skills[1].LevelText = "x";

        Assert.Equal("Level of Expose must be a whole number (was 'x').", archer.Problem);
    }

    [Fact]
    public void PropertyChanged_LevelTyped_RaisesProblem()
    {
        var archer = ClassViewModel.From(_archer, _draft);
        var raised = new List<string?>();
        archer.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        archer.Skills[0].LevelText = "x";

        Assert.Equal([nameof(ClassViewModel.Problem)], raised);
    }

    [Fact]
    public void Changed_LevelTyped_IsRaisedOnce()
    {
        var archer = ClassViewModel.From(_archer, _draft);
        var raised = 0;
        archer.Changed += (_, _) => raised++;

        archer.Skills[0].LevelText = "2";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void DuplicateOf_TypedLevel_CopiesTheRowsAsTyped()
    {
        var archer = ClassViewModel.From(_archer, _draft);
        archer.Skills[1].LevelText = "x";

        var copy = ClassViewModel.DuplicateOf(archer);

        Assert.Equal(["1", "x"], copy.Skills.Select(row => row.LevelText));
        Assert.DoesNotContain(copy.Skills, row => archer.Skills.Contains(row));
    }

    [Fact]
    public void PropertyChanged_LevelTypedInTheList_RaisesProblem()
    {
        var catalog = new ClassCatalogViewModel(_draft);
        var raised = new List<string?>();
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        catalog.SelectedClass!.Skills[0].LevelText = "2";

        Assert.Equal([nameof(ClassCatalogViewModel.Problem)], raised);
    }

    [Fact]
    public void Problem_LevelAtMax_IsNull()
    {
        var catalog = new ClassCatalogViewModel(_draft);

        catalog.SelectedClass!.Skills[1].LevelText = $"{_maxLevel}";

        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void Problem_LevelPastMax_ShowsDataMessage()
    {
        var catalog = new ClassCatalogViewModel(_draft);

        catalog.SelectedClass!.Skills[1].LevelText = $"{_maxLevel + 1}";

        Assert.Equal(
            $"classes.json: class 'archer': skills lists 'expose' at level {_maxLevel + 1}, past the max level {_maxLevel}.",
            catalog.Problem);
    }

    [Fact]
    public void Problem_SkillUsingResourceTheClassLacks_ShowsDataMessage()
    {
        var scholar = Scholar with { Skills = [new(_spark.Id)] };

        var catalog = new ClassCatalogViewModel(
            _draft with { Classes = new Dictionary<string, ClassDefinition> { [scholar.Id] = scholar } });

        Assert.Equal(
            "classes.json: class 'scholar': skills lists 'spark', which uses 'mana', a resource scholar doesn't have.",
            catalog.Problem);
    }
}
