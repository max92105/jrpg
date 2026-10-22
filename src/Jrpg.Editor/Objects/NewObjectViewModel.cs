using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Objects;

/// <summary>The New object dialog: the id and category typed for an object made from a rectangle of sheet cells.</summary>
internal sealed partial class NewObjectViewModel : ObservableObject
{
    private readonly IReadOnlyCollection<string> _takenIds;

    /// <param name="takenIds">The object ids already in the catalog.</param>
    /// <param name="categories">The categories objects use today, offered in the list; a new one can be typed.</param>
    /// <param name="cells">The cells the object is made from.</param>
    public NewObjectViewModel(IReadOnlyCollection<string> takenIds, IReadOnlyList<string> categories, SheetCellRectangle cells)
    {
        _takenIds = takenIds;
        Categories = [.. categories.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
        Cells = cells;
    }

    public SheetCellRectangle Cells { get; }

    /// <summary>Distinct and sorted.</summary>
    public IReadOnlyList<string> Categories { get; }

    /// <summary>"3 x 4 cells".</summary>
    public string SizeText => $"{Cells.Columns} x {Cells.Rows} cells";

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
