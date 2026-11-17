using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// The Classes page's list (decisions 945, 946): the classes in id order, the added ones after, Duplicate copying the
// chosen class as typed, only an added class removable, and the problem line: the chosen class's own, then another's,
// then Data's message for the list.
public sealed class ClassCatalogViewModelTests
{
    // The scholar first: the list's id order puts the archer before it.
    private static readonly ContentCatalog _content =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")) with
        {
            Classes = new Dictionary<string, ClassDefinition> { [Scholar.Id] = Scholar, [Archer.Id] = Archer },
        };

    private readonly ClassCatalogViewModel _catalog = new(_content);

    [Fact]
    public void Constructor_Classes_ListsThemInIdOrder()
    {
        Assert.Equal(["archer", "scholar"], _catalog.Classes.Select(listed => listed.Id));
        Assert.Same(_catalog.Classes[0], _catalog.SelectedClass);
    }

    [Fact]
    public void Problem_Constructed_IsNull() => Assert.Null(_catalog.Problem);

    [Fact]
    public void AddClass_AddsBlankLastAndChoosesIt()
    {
        _catalog.AddClassCommand.Execute(null);

        var added = _catalog.Classes[^1];
        Assert.Equal(3, _catalog.Classes.Count);
        Assert.Same(added, _catalog.SelectedClass);
        Assert.Equal((true, "", "1"), (added.IsNew, added.Id, added.Base.MaxHpText));
    }

    // A half-typed box duplicates as typed: the copy is made from the boxes, never from a record that would throw.
    [Fact]
    public void DuplicateClass_TypedBoxes_CopiesThemWithAnEmptyId()
    {
        var archer = _catalog.Classes[0];
        archer.Name = "Bowman";
        archer.Base.PowerText = "1x";
        archer.Growth.WillText = "3";

        _catalog.DuplicateClassCommand.Execute(null);

        var copy = _catalog.Classes[1];
        Assert.Same(copy, _catalog.SelectedClass);
        Assert.Equal((true, "", "Bowman", Archer.Description), (copy.IsNew, copy.Id, copy.Name, copy.Description));
        Assert.Equal(TextsOf(archer.Base), TextsOf(copy.Base));
        Assert.Equal(TextsOf(archer.Growth), TextsOf(copy.Growth));
    }

    [Fact]
    public void DuplicateClass_ClassWithResources_CopiesThem()
    {
        _catalog.Choose("scholar");

        _catalog.DuplicateClassCommand.Execute(null);

        var copy = _catalog.SelectedClass!;
        copy.Id = "sage";
        Assert.Equal(Scholar.Resources, copy.ToClass().Resources);
    }

    [Fact]
    public void DuplicateClass_ClassWithSkills_CopiesThem()
    {
        _catalog.Choose("archer");

        _catalog.DuplicateClassCommand.Execute(null);

        var copy = _catalog.SelectedClass!;
        copy.Id = "hunter";
        Assert.Equal(["expose"], copy.ToClass().SkillIds);
    }

    [Fact]
    public void DuplicateClass_NothingChosen_IsGreyed()
    {
        var empty = new ClassCatalogViewModel(_content with { Classes = new Dictionary<string, ClassDefinition>() });

        Assert.False(empty.DuplicateClassCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveClass_SavedClass_IsGreyed() => Assert.False(_catalog.RemoveClassCommand.CanExecute(null));

    [Fact]
    public void RemoveClass_AddedClass_RemovesIt()
    {
        _catalog.AddClassCommand.Execute(null);
        var added = _catalog.SelectedClass!;

        _catalog.RemoveClassCommand.Execute(null);

        Assert.DoesNotContain(added, _catalog.Classes);
        Assert.Same(_catalog.Classes[^1], _catalog.SelectedClass);
    }

    [Fact]
    public void SelectedClass_AddedClassChosen_RaisesDuplicateAndRemoveCanExecuteChanged()
    {
        var raised = new List<string>();
        _catalog.DuplicateClassCommand.CanExecuteChanged += (_, _) => raised.Add("Duplicate");
        _catalog.RemoveClassCommand.CanExecuteChanged += (_, _) => raised.Add("Remove");

        _catalog.AddClassCommand.Execute(null);

        Assert.Equal(["Duplicate", "Remove"], raised);
        Assert.True(_catalog.RemoveClassCommand.CanExecute(null));
    }

    [Fact]
    public void Problem_TwoClassesWithOneId_NamesTheId()
    {
        _catalog.AddClassCommand.Execute(null);

        _catalog.SelectedClass!.Id = "scholar";

        Assert.Equal("Class scholar is listed twice.", _catalog.Problem);
    }

    [Fact]
    public void Problem_AnotherClassUnreadable_NamesIt()
    {
        _catalog.Classes[1].Growth.SpeedText = "x";

        Assert.Equal("Class scholar: Growth speed must be a whole number (was 'x').", _catalog.Problem);
    }

    // The scholar comes after the archer in the list, so only the chosen-first rule puts its problem on the line.
    [Fact]
    public void Problem_TwoClassesUnreadable_ShowsTheChosenOneFirst()
    {
        _catalog.Classes[0].Base.SpeedText = "x";
        _catalog.Classes[1].Base.AccuracyText = "y";

        _catalog.Choose("scholar");

        Assert.Equal("Base accuracy must be a whole number (was 'y').", _catalog.Problem);
    }

    [Fact]
    public void Problem_EmptyDescription_ShowsDataMessage()
    {
        _catalog.Classes[1].Description = "";

        Assert.Equal("classes.json: class 'scholar': description must not be empty.", _catalog.Problem);
    }

    [Fact]
    public void Problem_TypedChange_RaisesProblemChanged()
    {
        var raised = new List<string?>();
        _catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _catalog.Classes[1].Growth.WillText = "2";

        Assert.Equal([nameof(ClassCatalogViewModel.Problem)], raised);
    }

    [Fact]
    public void Problem_ClassChosen_RaisesProblemChanged()
    {
        var raised = new List<string?>();
        _catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _catalog.Choose("scholar");

        Assert.Contains(nameof(ClassCatalogViewModel.Problem), raised);
    }

    [Fact]
    public void ToClasses_WithProblem_Throws()
    {
        _catalog.Classes[0].Base.MaxHpText = "";

        Assert.Throws<InvalidOperationException>(() => _catalog.ToClasses());
    }

    [Fact]
    public void ToClasses_NameTyped_GivesTheClassesInTheListsOrder()
    {
        _catalog.Classes[1].Name = "Sage";

        Assert.Equal([Archer, Scholar with { Name = "Sage" }], _catalog.ToClasses());
    }

    [Fact]
    public void Choose_KnownId_ChoosesIt()
    {
        _catalog.Choose("scholar");

        Assert.Same(_catalog.Classes[1], _catalog.SelectedClass);
    }

    [Fact]
    public void Choose_UnknownId_KeepsTheChoice()
    {
        _catalog.Choose("scholar");

        _catalog.Choose("bard");

        Assert.Equal("scholar", _catalog.SelectedClass!.Id);
    }

    [Fact]
    public void ChosenKey_NothingChosen_IsNull()
    {
        _catalog.SelectedClass = null;

        Assert.Null(_catalog.ChosenKey);
    }

    [Fact]
    public void ChosenKey_NewClassWithoutId_IsNull()
    {
        _catalog.AddClassCommand.Execute(null);

        Assert.Null(_catalog.ChosenKey);
    }

    [Fact]
    public void ChosenKey_SavedClassChosen_IsItsId() => Assert.Equal("archer", _catalog.ChosenKey);
}
