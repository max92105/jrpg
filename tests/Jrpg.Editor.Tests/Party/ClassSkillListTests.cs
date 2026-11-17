using Jrpg.Core.Party;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// The Skills tab's list (decision 949): the add box offering the draft's skills not listed, by name; Add after the
// chosen row at level 1, Remove, Up and Down, each greyed where it can't act; every change of the list told once.
public sealed class ClassSkillListTests
{
    private readonly ClassViewModel _archer = ClassViewModel.From(SkilledArcher, SkillsDraft);

    [Fact]
    public void SkillsToAdd_ClassWithoutSkills_ListsTheDraftsByName()
    {
        var scholar = ClassViewModel.From(Scholar, SkillsDraft);

        Assert.Equal(["Expose (expose)", "Taunt (taunt)", "Zap (spark)"], scholar.SkillsToAdd.Select(choice => choice.Label));
    }

    [Fact]
    public void SkillsToAdd_LeavesListedSkillsOut() => Assert.Equal(["spark"], _archer.SkillsToAdd.Select(choice => choice.Id));

    [Fact]
    public void SkillsToAdd_AfterAdd_RaisesChanged()
    {
        var raised = new List<string?>();
        _archer.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        _archer.SkillToAdd = _archer.SkillsToAdd[0];

        _archer.AddSkillCommand.Execute(null);

        Assert.Contains(nameof(ClassViewModel.SkillsToAdd), raised);
        Assert.Empty(_archer.SkillsToAdd);
    }

    [Fact]
    public void AddSkill_WithSelection_InsertsAfterItAtLevelOne()
    {
        _archer.SkillToAdd = _archer.SkillsToAdd[0];

        _archer.AddSkillCommand.Execute(null);

        Assert.Equal(["taunt", "spark", "expose"], _archer.Skills.Select(row => row.Skill.Id));
        Assert.Equal(("1", null), (_archer.Skills[1].LevelText, _archer.SkillToAdd));
        Assert.Same(_archer.Skills[1], _archer.SelectedSkill);
    }

    [Fact]
    public void AddSkill_NothingSelected_AddsLast()
    {
        _archer.SelectedSkill = null;
        _archer.SkillToAdd = _archer.SkillsToAdd[0];

        _archer.AddSkillCommand.Execute(null);

        Assert.Equal(["taunt", "expose", "spark"], _archer.Skills.Select(row => row.Skill.Id));
    }

    [Fact]
    public void AddSkill_NothingToAdd_IsGreyed() => Assert.False(_archer.AddSkillCommand.CanExecute(null));

    [Fact]
    public void SkillToAdd_Picked_RaisesAddCanExecuteChanged()
    {
        var raised = 0;
        _archer.AddSkillCommand.CanExecuteChanged += (_, _) => raised++;

        _archer.SkillToAdd = _archer.SkillsToAdd[0];

        Assert.Equal(1, raised);
    }

    [Fact]
    public void ToClass_AddedSkill_WritesItAtLevelOne()
    {
        _archer.SkillToAdd = _archer.SkillsToAdd[0];

        _archer.AddSkillCommand.Execute(null);

        Assert.Equal(new ClassSkill("spark"), _archer.ToClass().Skills[1]);
    }

    [Fact]
    public void RemoveSkill_TakesTheSelectedOut()
    {
        _archer.RemoveSkillCommand.Execute(null);

        Assert.Equal("expose", Assert.Single(_archer.Skills).Skill.Id);
        Assert.Same(_archer.Skills[0], _archer.SelectedSkill);
    }

    [Fact]
    public void RemoveSkill_NothingSelected_IsGreyed()
    {
        _archer.SelectedSkill = null;

        Assert.False(_archer.RemoveSkillCommand.CanExecute(null));
    }

    // The removed row is no longer watched: typing in it can't reach the list's problem line.
    [Fact]
    public void Changed_RemovedRowTyped_IsNotRaised()
    {
        var removed = _archer.SelectedSkill!;
        _archer.RemoveSkillCommand.Execute(null);
        var raised = 0;
        _archer.Changed += (_, _) => raised++;

        removed.LevelText = "2";

        Assert.Equal(0, raised);
    }

    [Fact]
    public void Changed_AddedRowTyped_IsRaised()
    {
        _archer.SkillToAdd = _archer.SkillsToAdd[0];
        _archer.AddSkillCommand.Execute(null);
        var raised = 0;
        _archer.Changed += (_, _) => raised++;

        _archer.SelectedSkill!.LevelText = "2";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_SkillAdded_IsRaisedOnce()
    {
        var raised = 0;
        _archer.Changed += (_, _) => raised++;
        _archer.SkillToAdd = _archer.SkillsToAdd[0];

        _archer.AddSkillCommand.Execute(null);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void MoveSkillUp_FirstRow_IsGreyed() => Assert.False(_archer.MoveSkillUpCommand.CanExecute(null));

    [Fact]
    public void MoveSkillDown_LastRow_IsGreyed()
    {
        _archer.SelectedSkill = _archer.Skills[^1];

        Assert.False(_archer.MoveSkillDownCommand.CanExecute(null));
    }

    [Fact]
    public void MoveSkillDown_MovesItOneDown()
    {
        _archer.MoveSkillDownCommand.Execute(null);

        Assert.Equal(["expose", "taunt"], _archer.Skills.Select(row => row.Skill.Id));
        Assert.Equal((true, false), (_archer.MoveSkillUpCommand.CanExecute(null), _archer.MoveSkillDownCommand.CanExecute(null)));
    }

    // The chosen row stays chosen as it moves, so only the list's change can re-grey the moves.
    [Fact]
    public void MoveSkillDown_ToTheEnd_RaisesBothMovesCanExecuteChanged()
    {
        var raised = new List<string>();
        _archer.MoveSkillUpCommand.CanExecuteChanged += (_, _) => raised.Add("up");
        _archer.MoveSkillDownCommand.CanExecuteChanged += (_, _) => raised.Add("down");

        _archer.MoveSkillDownCommand.Execute(null);

        Assert.Equal(["up", "down"], raised);
    }

    [Fact]
    public void SelectedSkill_Changed_RaisesRemoveAndMovesCanExecuteChanged()
    {
        var raised = new List<string>();
        _archer.RemoveSkillCommand.CanExecuteChanged += (_, _) => raised.Add("remove");
        _archer.MoveSkillUpCommand.CanExecuteChanged += (_, _) => raised.Add("up");
        _archer.MoveSkillDownCommand.CanExecuteChanged += (_, _) => raised.Add("down");

        _archer.SelectedSkill = _archer.Skills[1];

        Assert.Equal(["remove", "up", "down"], raised);
    }

    [Fact]
    public void Problem_AddedSkillUsingResourceTheClassLacks_ShowsDataMessage()
    {
        var catalog = new ClassCatalogViewModel(SkillsDraft);
        catalog.Choose("scholar");
        var scholar = catalog.SelectedClass!;
        scholar.SkillToAdd = scholar.SkillsToAdd.Single(choice => choice.Id == "spark");

        scholar.AddSkillCommand.Execute(null);

        Assert.Equal(
            "classes.json: class 'scholar': skills lists 'spark', which uses 'mana', a resource scholar doesn't have.",
            catalog.Problem);
    }

    [Fact]
    public void Problem_AddedSkillRemoved_IsNull()
    {
        var catalog = new ClassCatalogViewModel(SkillsDraft);
        catalog.Choose("scholar");
        var scholar = catalog.SelectedClass!;
        scholar.SkillToAdd = scholar.SkillsToAdd.Single(choice => choice.Id == "spark");
        scholar.AddSkillCommand.Execute(null);

        scholar.RemoveSkillCommand.Execute(null);

        Assert.Null(catalog.Problem);
    }
}
