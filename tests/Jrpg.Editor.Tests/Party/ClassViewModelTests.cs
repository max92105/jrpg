using Jrpg.Core.Party;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// One class as the Classes page types it, written over the record it came from: its boxes, its problem, its label and
// the Changed event the list's problem line listens to.
public sealed class ClassViewModelTests
{
    [Fact]
    public void From_Class_ShowsEveryBaseAndGrowthNumber()
    {
        var scholar = ClassViewModel.From(Scholar, World);

        string[] shown = [.. TextsOf(scholar.Base), .. TextsOf(scholar.Growth)];
        Assert.Equal(["140", "9", "3", "11", "7", "17", "5", "13", "12", "2", "4", "6", "8", "10", "14", "16"], shown);
    }

    [Fact]
    public void New_IdTyped_IsTheBlankClass()
    {
        var added = ClassViewModel.New(World);

        added.Id = "ranger";

        Assert.True(added.IsNew);
        Assert.Equal(new ClassDefinition("ranger", "", "", new(MaxHp: 1, Speed: 0), new(MaxHp: 0, Speed: 0)), added.ToClass());
    }

    [Fact]
    public void ToClass_Untouched_EqualsTheSource() => Assert.Equal(Scholar, ClassViewModel.From(Scholar, World).ToClass());

    [Fact]
    public void ToClass_TypedGrowth_SetsGrowth()
    {
        var scholar = ClassViewModel.From(Scholar, World);

        scholar.Growth.PowerText = "7";

        Assert.Equal(7, scholar.ToClass().Growth.Primaries.Power);
    }

    [Fact]
    public void ToClass_NameTyped_KeepsItsSkillsAndResources()
    {
        var scholar = ClassViewModel.From(Scholar with { Skills = [new("heal"), new("focus_bolt")] }, World);

        scholar.Name = "Sage";

        var written = scholar.ToClass();
        Assert.Equal(["heal", "focus_bolt"], written.SkillIds);
        Assert.Equal(Scholar.Resources, written.Resources);
    }

    [Fact]
    public void ToClass_NameAndDescriptionWithSpaces_TrimsThem()
    {
        var scholar = ClassViewModel.From(Scholar, World);

        scholar.Name = "  Sage ";
        scholar.Description = " Reads. ";

        var written = scholar.ToClass();
        Assert.Equal("Sage", written.Name);
        Assert.Equal("Reads.", written.Description);
    }

    [Fact]
    public void ToClass_WithProblem_Throws()
    {
        var scholar = ClassViewModel.From(Scholar, World);

        scholar.Base.SpeedText = "fast";

        Assert.Throws<InvalidOperationException>(() => scholar.ToClass());
    }

    [Fact]
    public void Problem_SavedClassUntouched_IsNull() => Assert.Null(ClassViewModel.From(Scholar, World).Problem);

    // The growth's box typed first: the base's is named, as the page shows it first.
    [Fact]
    public void Problem_BaseAndGrowthBoxesNotNumbers_NamesTheBase()
    {
        var scholar = ClassViewModel.From(Scholar, World);

        scholar.Growth.MaxHpText = "y";
        scholar.Base.WillText = "x";

        Assert.Equal("Base will must be a whole number (was 'x').", scholar.Problem);
    }

    [Fact]
    public void Problem_GrowthBoxNotANumber_NamesIt()
    {
        var scholar = ClassViewModel.From(Scholar, World);

        scholar.Growth.WillText = "x";

        Assert.Equal("Growth will must be a whole number (was 'x').", scholar.Problem);
    }

    [Fact]
    public void Problem_NewClassWithBadId_NamesTheId()
    {
        var added = ClassViewModel.New(World);

        added.Id = "Ranger";

        Assert.Equal("The id may only use lowercase letters, digits and _ (like tree_4).", added.Problem);
    }

    [Fact]
    public void Problem_NewClassWithId_IsNull()
    {
        var added = ClassViewModel.New(World);

        added.Id = "ranger";

        Assert.Null(added.Problem);
    }

    // A saved class's id box is read-only: its id is never checked as typed.
    [Fact]
    public void Problem_SavedClass_IgnoresIdRule()
    {
        var scholar = ClassViewModel.From(Scholar, World);

        scholar.Id = "";

        Assert.Null(scholar.Problem);
    }

    [Fact]
    public void Label_SavedClass_NamesIdAndName() => Assert.Equal("scholar: Scholar", ClassViewModel.From(Scholar, World).Label);

    [Fact]
    public void Label_NewClassWithoutId_SaysNewClass() => Assert.Equal("(new class)", ClassViewModel.New(World).Label);

    [Fact]
    public void PropertyChanged_IdTyped_RaisesLabelAndProblem()
    {
        var added = ClassViewModel.New(World);
        var raised = new List<string?>();
        added.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        added.Id = "ranger";

        Assert.Equal([nameof(ClassViewModel.Id), nameof(ClassViewModel.Label), nameof(ClassViewModel.Problem)], raised);
    }

    [Fact]
    public void PropertyChanged_NameTyped_RaisesLabel()
    {
        var scholar = ClassViewModel.From(Scholar, World);
        var raised = new List<string?>();
        scholar.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        scholar.Name = "Sage";

        Assert.Equal([nameof(ClassViewModel.Name), nameof(ClassViewModel.Label)], raised);
        Assert.Equal("scholar: Sage", scholar.Label);
    }

    [Fact]
    public void PropertyChanged_BaseBoxTyped_RaisesProblemAndLevels()
    {
        var scholar = ClassViewModel.From(Scholar, World);
        var raised = new List<string?>();
        scholar.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        scholar.Base.AgilityText = "x";

        Assert.Equal(
            [nameof(ClassViewModel.Problem), nameof(ClassViewModel.LevelsProblem), nameof(ClassViewModel.LevelRows)],
            raised);
    }

    [Fact]
    public void PropertyChanged_GrowthBoxTyped_RaisesProblemAndLevels()
    {
        var scholar = ClassViewModel.From(Scholar, World);
        var raised = new List<string?>();
        scholar.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        scholar.Growth.AgilityText = "x";

        Assert.Equal(
            [nameof(ClassViewModel.Problem), nameof(ClassViewModel.LevelsProblem), nameof(ClassViewModel.LevelRows)],
            raised);
    }

    [Fact]
    public void Changed_BoxTyped_IsRaisedOnce()
    {
        var scholar = ClassViewModel.From(Scholar, World);
        var raised = 0;
        scholar.Changed += (_, _) => raised++;

        scholar.Growth.SpeedText = "3";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_IdTyped_IsRaised()
    {
        var added = ClassViewModel.New(World);
        var raised = 0;
        added.Changed += (_, _) => raised++;

        added.Id = "ranger";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_NameTyped_IsRaised()
    {
        var scholar = ClassViewModel.From(Scholar, World);
        var raised = 0;
        scholar.Changed += (_, _) => raised++;

        scholar.Name = "Sage";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_DescriptionTyped_IsRaised()
    {
        var scholar = ClassViewModel.From(Scholar, World);
        var raised = 0;
        scholar.Changed += (_, _) => raised++;

        scholar.Description = "Reads.";

        Assert.Equal(1, raised);
    }
}
