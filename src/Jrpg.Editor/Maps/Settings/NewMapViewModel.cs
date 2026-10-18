using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Maps.Settings;

/// <summary>
/// The new map dialog: a name and a size (the legacy dialog's "New Map", 30 x 17, to start). The id the map will get
/// is shown as the name is typed, since it can't change afterwards (question 70).
/// </summary>
internal sealed partial class NewMapViewModel : ObservableObject
{
    private readonly IReadOnlyCollection<string> _takenIds;

    /// <param name="takenIds">The ids of the maps that exist.</param>
    public NewMapViewModel(IReadOnlyCollection<string> takenIds)
    {
        _takenIds = takenIds;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MapId), nameof(NameProblem))]
    public partial string Name { get; set; } = "New Map";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Size), nameof(SizeProblem))]
    public partial string WidthText { get; set; } = "30";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Size), nameof(SizeProblem))]
    public partial string HeightText { get; set; } = "17";

    /// <summary>The id the map gets: the name in snake_case, made unique.</summary>
    public string MapId => MapIds.FromName(Name, _takenIds);

    /// <summary>The size typed in; null when it isn't two whole numbers from 1 to <see cref="TypedCounts.MaxMapSide"/>.</summary>
    public (int Width, int Height)? Size => TypedCounts.ParseMapSize(WidthText, HeightText);

    public string? SizeProblem => Size is null ? TypedCounts.MapSizeProblem : null;

    /// <summary>The game shows the name on entering the map, so it can't be blank.</summary>
    public string? NameProblem => string.IsNullOrWhiteSpace(Name) ? "The map needs a name." : null;

    /// <summary>The new, empty map: no tiles, objects or random battles yet. Valid once both problems are null.</summary>
    public MapDefinition ToMap()
    {
        var (width, height) = Size ?? throw new InvalidOperationException(SizeProblem);
        return new MapDefinition(
            new MapLayout { Id = MapId, Width = width, Height = height, Layers = [], Objects = [] },
            new MapSettings { Id = MapId, DisplayName = Name.Trim() });
    }
}
