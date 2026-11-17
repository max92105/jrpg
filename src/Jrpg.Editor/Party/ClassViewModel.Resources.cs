using System.Collections.ObjectModel;
using Jrpg.Core.Resources;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Party;

/// <summary>
/// The class's battle resources as the Resources tab types them (decision 950), in the order the game draws them. A
/// resource's typed field raises <see cref="Changed"/> once.
/// </summary>
internal sealed partial class ClassViewModel
{
    /// <summary>The class's resources in order, each as typed.</summary>
    public ObservableCollection<ClassResourceViewModel> Resources { get; }

    // The first resource's own problem, named by its id ("Resource mana: ..."), by position while a new one has none.
    private string? ResourcesProblem =>
        RecordListProblem.FirstOf(
            Resources,
            resource => resource.Id,
            resource => resource.ProblemAmong(ClassmateIdsOf(resource)),
            "Resource");

    private string[] ClassmateIdsOf(ClassResourceViewModel resource) =>
        [.. Resources.Where(other => other != resource).Select(other => other.Id)];

    // The record's own list while the resources as typed equal it in order, so an untouched class equals its record
    // (the record compares its lists by reference, decision 1014).
    private IReadOnlyList<ResourceDefinition> ResourcesAsTyped()
    {
        IReadOnlyList<ResourceDefinition> typed = [.. Resources.Select(resource => resource.ToResource())];
        return typed.SequenceEqual(_source.Resources) ? _source.Resources : typed;
    }

    private void WatchResources()
    {
        foreach (var resource in Resources)
        {
            resource.Changed += OnResourceTyped;
        }
    }

    private void OnResourceTyped(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Problem));
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
