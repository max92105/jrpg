using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Events;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Party;

/// <summary>
/// Every class as the Classes page edits it (decision 945): the draft's classes in id order, the ones added this
/// session after them; Add starts from the blank, Duplicate from the chosen class as typed (decision 585); only an
/// added class can be removed, since members name a saved one (decision 581). <see cref="Problem"/> is the chosen
/// class's own first, then another's, then Data's message for the whole list (decision 946). Its host chooses a class
/// by its id (<see cref="IEditedCatalog"/>).
/// </summary>
internal sealed partial class ClassCatalogViewModel : ObservableObject, IEditedCatalog
{
    private readonly ContentCatalog _content;

    /// <param name="content">
    /// The draft as it is: the classes edited, the skills they name, and the experience table and stat rules each
    /// class's Levels tab reads.
    /// </param>
    public ClassCatalogViewModel(ContentCatalog content)
    {
        _content = content;
        Classes =
        [
            .. content.Classes.Values
                .OrderBy(classDefinition => classDefinition.Id, StringComparer.Ordinal)
                .Select(classDefinition => ClassViewModel.From(classDefinition, content)),
        ];
        foreach (var listed in Classes)
        {
            listed.Changed += OnClassChanged;
        }

        SelectedClass = Classes.FirstOrDefault();
    }

    public ObservableCollection<ClassViewModel> Classes { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    [NotifyCanExecuteChangedFor(nameof(DuplicateClassCommand), nameof(RemoveClassCommand))]
    public partial ClassViewModel? SelectedClass { get; set; }

    /// <summary>
    /// Why the game would refuse the classes as typed: the chosen class's own first, as the page shows it, then
    /// another's named by id (by position while it has none), then Data's; null when the game would take them.
    /// </summary>
    public string? Problem =>
        SelectedClass?.Problem
        ?? RecordListProblem.FirstOf(Classes, listed => listed.Id, listed => listed.Problem, "Class")
        ?? CatalogProblems.OfClasses([.. Classes.Select(listed => listed.ToClass())], _content);

    /// <summary>The chosen class's id; null with none chosen or while a new one has no id yet.</summary>
    public string? ChosenKey => SelectedClass is { Id.Length: > 0 } chosen ? chosen.Id : null;

    public void Choose(string key) => SelectedClass = Classes.FirstOrDefault(listed => listed.Id == key) ?? SelectedClass;

    /// <exception cref="InvalidOperationException">
    /// There is a <see cref="Problem"/>; the page's host applies only without one.
    /// </exception>
    public IReadOnlyList<ClassDefinition> ToClasses() =>
        Problem is { } problem ? throw new InvalidOperationException(problem) : [.. Classes.Select(listed => listed.ToClass())];

    /// <summary>Adds a new class last and chooses it, so its id is the next thing typed.</summary>
    [RelayCommand]
    private void AddClass() => Insert(ClassViewModel.New(_content), Classes.Count);

    /// <summary>Adds a copy of the chosen class after it, its id left to type (decision 585).</summary>
    [RelayCommand(CanExecute = nameof(HasSelectedClass))]
    private void DuplicateClass() => Insert(ClassViewModel.DuplicateOf(SelectedClass!), Classes.IndexOf(SelectedClass!) + 1);

    [RelayCommand(CanExecute = nameof(CanRemoveClass))]
    private void RemoveClass()
    {
        var removed = SelectedClass!;
        removed.Changed -= OnClassChanged;
        SelectedClass = ListOrder.Remove(Classes, removed);
    }

    private bool HasSelectedClass() => SelectedClass is not null;

    private bool CanRemoveClass() => SelectedClass is { IsNew: true };

    private void Insert(ClassViewModel added, int index)
    {
        added.Changed += OnClassChanged;
        Classes.Insert(index, added);
        SelectedClass = added;
    }

    // Only the typed id, name, description, boxes and skill levels reach here, never a class's label or problem
    // notices, so Data's check runs once per change.
    private void OnClassChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(Problem));
}
