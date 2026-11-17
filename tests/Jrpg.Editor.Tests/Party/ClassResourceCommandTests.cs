using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// The Resources tab's list commands (decision 950): Add a blank after the chosen resource, Remove any, Up and Down,
// each greyed where it can't act; every change of the list told once (decision 1014).
public sealed class ClassResourceCommandTests
{
    // Surge before Fury, opposite to id order, so a list sorted by id shows.
    private static readonly ClassDefinition _scholar = Scholar with { Resources = [Surge, Fury] };

    private static readonly ContentCatalog _draft =
        World with { Classes = new Dictionary<string, ClassDefinition> { [_scholar.Id] = _scholar } };

    private readonly ClassViewModel _class = ClassViewModel.From(_scholar, _draft);

    [Fact]
    public void SelectedResource_FromClass_IsTheFirst() => Assert.Same(_class.Resources[0], _class.SelectedResource);

    [Fact]
    public void AddResource_WithSelection_InsertsAfterIt()
    {
        _class.AddResourceCommand.Execute(null);

        Assert.Equal(["surge", "", "fury"], _class.Resources.Select(resource => resource.Id));
        Assert.True(_class.Resources[1].IsNew);
        Assert.Same(_class.Resources[1], _class.SelectedResource);
    }

    [Fact]
    public void AddResource_NothingSelected_AddsLast()
    {
        _class.SelectedResource = null;

        _class.AddResourceCommand.Execute(null);

        Assert.Equal(["surge", "fury", ""], _class.Resources.Select(resource => resource.Id));
    }

    [Fact]
    public void ToClass_AddedResource_WritesIt()
    {
        _class.AddResourceCommand.Execute(null);
        _class.SelectedResource!.Id = "rage";
        _class.SelectedResource.Name = "Rage";

        var written = _class.ToClass().Resources;

        Assert.Equal([Surge, new ResourceDefinition("rage", "Rage", Max: 1, StartsAt: 0) { Color = "#FFFFFF" }, Fury], written);
    }

    [Fact]
    public void Problem_AddedResourceWithoutId_NamesItsPosition()
    {
        _class.SelectedResource = _class.Resources[^1];

        _class.AddResourceCommand.Execute(null);

        Assert.Equal("Resource #3: The id must not be empty.", _class.Problem);
    }

    [Fact]
    public void Problem_ResourceAdded_RaisesProblemChanged()
    {
        var raised = new List<string?>();
        _class.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _class.AddResourceCommand.Execute(null);

        Assert.Contains(nameof(ClassViewModel.Problem), raised);
    }

    [Fact]
    public void Changed_ResourceAdded_IsRaisedOnce()
    {
        var raised = 0;
        _class.Changed += (_, _) => raised++;

        _class.AddResourceCommand.Execute(null);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_AddedResourceTyped_IsRaised()
    {
        _class.AddResourceCommand.Execute(null);
        var raised = 0;
        _class.Changed += (_, _) => raised++;

        _class.SelectedResource!.Name = "Rage";

        Assert.Equal(1, raised);
    }

    // Decision 950: any resource, a saved one too.
    [Fact]
    public void RemoveResource_SavedResource_RemovesIt()
    {
        _class.SelectedResource = _class.Resources[1];

        _class.RemoveResourceCommand.Execute(null);

        Assert.Equal([Surge], _class.ToClass().Resources);
        Assert.Same(_class.Resources[0], _class.SelectedResource);
    }

    [Fact]
    public void RemoveResource_NothingSelected_IsGreyed()
    {
        _class.SelectedResource = null;

        Assert.False(_class.RemoveResourceCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveResource_OnlyResource_ChoosesNothingAndGreysTheCommands()
    {
        var scholar = ClassViewModel.From(Scholar, World);

        scholar.RemoveResourceCommand.Execute(null);

        Assert.Null(scholar.SelectedResource);
        Assert.Equal(
            (false, false, false),
            (scholar.RemoveResourceCommand.CanExecute(null), scholar.MoveResourceUpCommand.CanExecute(null),
                scholar.MoveResourceDownCommand.CanExecute(null)));
    }

    // The removed resource is no longer watched: typing in it can't reach the list's problem line.
    [Fact]
    public void RemoveResource_RemovedResourceTyped_RaisesNothing()
    {
        var removed = _class.SelectedResource!;
        _class.RemoveResourceCommand.Execute(null);
        var raised = new List<string?>();
        _class.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        _class.Changed += (_, _) => raised.Add(nameof(ClassViewModel.Changed));

        removed.Name = "Rush";

        Assert.Empty(raised);
    }

    [Fact]
    public void Problem_ResourceUsedBySkillRemoved_ShowsDataMessage()
    {
        var mage = Scholar with { Resources = [Fury with { Id = "mana", Name = "Mana" }], Skills = [new("spark")] };
        var catalog = new ClassCatalogViewModel(
            SkillsDraft with
            {
                Classes = new Dictionary<string, ClassDefinition> { [SkilledArcher.Id] = SkilledArcher, [mage.Id] = mage },
            });
        catalog.Choose("scholar");

        catalog.SelectedClass!.RemoveResourceCommand.Execute(null);

        Assert.Equal(
            "classes.json: class 'scholar': skills lists 'spark', which uses 'mana', a resource scholar doesn't have.",
            catalog.Problem);
    }

    [Fact]
    public void MoveResourceUp_FirstRow_IsGreyed() => Assert.False(_class.MoveResourceUpCommand.CanExecute(null));

    [Fact]
    public void MoveResourceDown_LastRow_IsGreyed()
    {
        _class.SelectedResource = _class.Resources[^1];

        Assert.False(_class.MoveResourceDownCommand.CanExecute(null));
    }

    [Fact]
    public void MoveResourceUpAndDown_NothingSelected_AreGreyed()
    {
        _class.SelectedResource = null;

        Assert.Equal((false, false), MovesEnabled());
    }

    [Fact]
    public void MoveResourceDown_MovesItOneDown()
    {
        _class.MoveResourceDownCommand.Execute(null);

        Assert.Equal([Fury, Surge], _class.ToClass().Resources);
        Assert.Equal((true, false), MovesEnabled());
    }

    [Fact]
    public void MoveResourceUp_MovesItOneUp()
    {
        _class.SelectedResource = _class.Resources[1];

        _class.MoveResourceUpCommand.Execute(null);

        Assert.Equal([Fury, Surge], _class.ToClass().Resources);
    }

    // The chosen resource stays chosen as it moves, so only the list's change can re-grey the moves.
    [Fact]
    public void MoveResourceDown_ToTheEnd_RaisesBothMovesCanExecuteChanged()
    {
        var raised = new List<string>();
        _class.MoveResourceUpCommand.CanExecuteChanged += (_, _) => raised.Add("up");
        _class.MoveResourceDownCommand.CanExecuteChanged += (_, _) => raised.Add("down");

        _class.MoveResourceDownCommand.Execute(null);

        Assert.Equal(["up", "down"], raised);
    }

    [Fact]
    public void SelectedResource_Changed_RaisesRemoveAndMovesCanExecuteChanged()
    {
        var raised = new List<string>();
        _class.RemoveResourceCommand.CanExecuteChanged += (_, _) => raised.Add("remove");
        _class.MoveResourceUpCommand.CanExecuteChanged += (_, _) => raised.Add("up");
        _class.MoveResourceDownCommand.CanExecuteChanged += (_, _) => raised.Add("down");

        _class.SelectedResource = _class.Resources[1];

        Assert.Equal(["remove", "up", "down"], raised);
    }

    private (bool Up, bool Down) MovesEnabled() =>
        (_class.MoveResourceUpCommand.CanExecute(null), _class.MoveResourceDownCommand.CanExecute(null));
}
