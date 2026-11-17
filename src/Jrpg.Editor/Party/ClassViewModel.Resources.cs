using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Resources;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Party;

/// <summary>
/// The class's battle resources as the Resources tab types them (decision 950), in the order the game draws them: a
/// blank added after the chosen one, any removed, moved. A resource's typed field and every change of the list raise
/// <see cref="Changed"/> once (decision 1014).
/// </summary>
internal sealed partial class ClassViewModel
{
    /// <summary>The class's resources in order, each as typed.</summary>
    public ObservableCollection<ClassResourceViewModel> Resources { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(
        nameof(RemoveResourceCommand),
        nameof(MoveResourceUpCommand),
        nameof(MoveResourceDownCommand))]
    public partial ClassResourceViewModel? SelectedResource { get; set; }

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

    /// <summary>Puts a blank resource after the chosen one (last when none), and chooses it.</summary>
    [RelayCommand]
    private void AddResource()
    {
        var resource = ClassResourceViewModel.New();
        ListOrder.InsertAfter(Resources, resource, SelectedResource);
        SelectedResource = resource;
    }

    // Any resource, a saved one too (decision 950): while a class skill uses it, Data's line names the skill.
    [RelayCommand(CanExecute = nameof(HasSelectedResource))]
    private void RemoveResource() => SelectedResource = ListOrder.Remove(Resources, SelectedResource!);

    [RelayCommand(CanExecute = nameof(CanMoveResourceUp))]
    private void MoveResourceUp() => ListOrder.MoveBy(Resources, SelectedResource!, -1);

    [RelayCommand(CanExecute = nameof(CanMoveResourceDown))]
    private void MoveResourceDown() => ListOrder.MoveBy(Resources, SelectedResource!, 1);

    private bool HasSelectedResource() => SelectedResource is not null;

    private bool CanMoveResourceUp() => SelectedResource is { } chosen && Resources.IndexOf(chosen) > 0;

    private bool CanMoveResourceDown() =>
        SelectedResource is { } chosen && Resources.IndexOf(chosen) < Resources.Count - 1;

    private void WatchResources()
    {
        foreach (var resource in Resources)
        {
            resource.Changed += OnResourceTyped;
        }

        Resources.CollectionChanged += OnResourcesListChanged;
    }

    // A move changes where the chosen resource stands, so the moves' greying is read again with the list.
    private void OnResourcesListChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ListItemWatch.Follow<ClassResourceViewModel>(
            e,
            resource => resource.Changed += OnResourceTyped,
            resource => resource.Changed -= OnResourceTyped);
        MoveResourceUpCommand.NotifyCanExecuteChanged();
        MoveResourceDownCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(Problem));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void OnResourceTyped(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Problem));
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
