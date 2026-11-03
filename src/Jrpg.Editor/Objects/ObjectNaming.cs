using CommunityToolkit.Mvvm.ComponentModel;

namespace Jrpg.Editor.Objects;

/// <summary>The id and category typed for a new object, with their problems; an edited object's category too.</summary>
internal sealed partial class ObjectNaming : ObservableObject
{
    private readonly IReadOnlyCollection<string> _takenIds;

    /// <param name="takenIds">The object ids already in the catalog.</param>
    /// <param name="categories">The categories objects use today, offered in the list; a new one can be typed.</param>
    public ObjectNaming(IReadOnlyCollection<string> takenIds, IReadOnlyList<string> categories)
    {
        _takenIds = takenIds;
        Categories = [.. categories.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
    }

    /// <summary>Distinct and sorted.</summary>
    public IReadOnlyList<string> Categories { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IdProblem))]
    public partial string Id { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CategoryProblem))]
    public partial string Category { get; set; } = "";

    /// <summary>Why the id can't be used (<see cref="ObjectIds"/>), or null.</summary>
    public string? IdProblem => ObjectIds.ProblemOf(Id, _takenIds);

    public string? CategoryProblem => string.IsNullOrWhiteSpace(Category) ? "The category must not be empty." : null;
}
