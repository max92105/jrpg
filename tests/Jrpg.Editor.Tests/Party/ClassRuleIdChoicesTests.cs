using System.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// The ids a resource's strain and dominance combos list (decision 951): the class's other resources' ids as typed,
// offered again when an id is typed or the list changes, and the rule's own id kept listed whatever happens to it.
public sealed class ClassRuleIdChoicesTests
{
    private static readonly ClassDefinition _scholar = Scholar with { Resources = [Surge, Fury] };

    private static readonly ContentCatalog _draft =
        World with { Classes = new Dictionary<string, ClassDefinition> { [_scholar.Id] = _scholar } };

    private readonly ClassViewModel _class = ClassViewModel.From(_scholar, _draft);

    private ClassResourceViewModel SurgeRow => _class.Resources[0];

    [Fact]
    public void OverChoices_FromClass_OffersTheOtherIds() => Assert.Equal(["fury"], SurgeRow.Dominance.OverChoices);

    [Fact]
    public void LowersStartOfChoices_FromClass_OffersTheOtherIds() =>
        Assert.Equal(["fury"], SurgeRow.Strain.LowersStartOfChoices);

    // Added after the chosen Surge, its id still to type.
    [Fact]
    public void OverChoices_ResourceAdded_LeavesItsEmptyIdOut()
    {
        _class.AddResourceCommand.Execute(null);

        Assert.Equal(["fury"], SurgeRow.Dominance.OverChoices);
    }

    [Fact]
    public void OverChoices_ResourceAdded_OffersItTheOthers()
    {
        _class.AddResourceCommand.Execute(null);

        Assert.Equal(["surge", "fury"], _class.SelectedResource!.Dominance.OverChoices);
    }

    [Fact]
    public void LowersStartOfChoices_IdTyped_ListsIt()
    {
        _class.AddResourceCommand.Execute(null);

        _class.SelectedResource!.Id = "calm";

        Assert.Equal(["calm", "fury"], SurgeRow.Strain.LowersStartOfChoices);
    }

    [Fact]
    public void OverChoices_IdTyped_RaisesChanged()
    {
        _class.AddResourceCommand.Execute(null);

        var raised = NoticesOf(SurgeRow.Dominance, () => _class.SelectedResource!.Id = "calm");

        Assert.Contains(nameof(DominanceBoxes.OverChoices), raised);
    }

    [Fact]
    public void LowersStartOfChoices_IdTyped_RaisesChanged()
    {
        _class.AddResourceCommand.Execute(null);

        var raised = NoticesOf(SurgeRow.Strain, () => _class.SelectedResource!.Id = "calm");

        Assert.Contains(nameof(StrainBoxes.LowersStartOfChoices), raised);
    }

    [Fact]
    public void OverChoices_ResourceRemoved_LeavesItOut()
    {
        _class.RemoveResourceCommand.Execute(null);

        Assert.Empty(_class.Resources[0].Dominance.OverChoices);
    }

    // A typed field other than an id leaves the combos' lists as they are.
    [Fact]
    public void OverChoices_NameTyped_IsNotRaised()
    {
        var raised = NoticesOf(SurgeRow.Dominance, () => _class.Resources[1].Name = "Rage");

        Assert.DoesNotContain(nameof(DominanceBoxes.OverChoices), raised);
    }

    // A removed resource's typed id no longer reaches the class.
    [Fact]
    public void OverChoices_RemovedResourceIdTyped_IsNotRaised()
    {
        _class.AddResourceCommand.Execute(null);
        var removed = _class.SelectedResource!;
        _class.RemoveResourceCommand.Execute(null);

        var raised = NoticesOf(SurgeRow.Dominance, () => removed.Id = "calm");

        Assert.DoesNotContain(nameof(DominanceBoxes.OverChoices), raised);
    }

    // The combo would clear its text if the id it shows left the list: the typed id stays, listed, until retyped.
    [Fact]
    public void OverChoices_NamedResourceRenamed_KeepsTheTypedIdListed()
    {
        _class.AddResourceCommand.Execute(null);
        var added = _class.SelectedResource!;
        added.Id = "calm";
        SurgeRow.Dominance.Over = "calm";

        added.Id = "calmer";

        Assert.Equal("calm", SurgeRow.Dominance.Over);
        Assert.Equal(["calmer", "fury", "calm"], SurgeRow.Dominance.OverChoices);
    }

    [Fact]
    public void LowersStartOfChoices_DuplicatedClass_OffersTheOtherIds()
    {
        var copy = ClassViewModel.DuplicateOf(_class);

        Assert.Equal(["surge"], copy.Resources[1].Strain.LowersStartOfChoices);
    }

    private static List<string?> NoticesOf(INotifyPropertyChanged boxes, Action type)
    {
        var raised = new List<string?>();
        boxes.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        type();
        return raised;
    }
}
