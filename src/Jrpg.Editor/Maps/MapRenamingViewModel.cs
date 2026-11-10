using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Maps;

/// <summary>
/// The Rename map dialog (decision 530): the new id for the shown map, prefilled from its display name as a new map's
/// would be, and what stops the rename.
/// </summary>
internal sealed partial class MapRenamingViewModel : ObservableObject
{
    private readonly IReadOnlyList<string> _takenIds;

    /// <param name="map">The map renamed.</param>
    /// <param name="takenIds">The ids of every map, saved or open, the map's own among them or not.</param>
    public MapRenamingViewModel(MapDefinition map, IReadOnlyCollection<string> takenIds)
    {
        OldId = map.Id;
        DisplayName = map.Settings.DisplayName;
        _takenIds = [.. takenIds.Where(id => !string.Equals(id, map.Id, StringComparison.OrdinalIgnoreCase))];
        NewId = MapIds.FromName(map.Settings.DisplayName, _takenIds);
    }

    public string OldId { get; }

    public string DisplayName { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem), nameof(CanApply))]
    public partial string NewId { get; set; }

    /// <summary>
    /// What stops the rename: an empty or ill-formed id, the map's own, or another map's (compared without case, since
    /// ids are file names); null when nothing.
    /// </summary>
    public string? Problem
    {
        get
        {
            var typed = NewId.Trim();
            if (!MapIds.IsWellFormed(typed))
            {
                return "An id is lower-case letters, digits and underscores, such as old_mill.";
            }

            if (string.Equals(typed, OldId, StringComparison.OrdinalIgnoreCase))
            {
                return "The id is already this map's.";
            }

            var taken = _takenIds.FirstOrDefault(id => string.Equals(id, typed, StringComparison.OrdinalIgnoreCase));
            return taken is null ? null : $"Map '{taken}' already has this id.";
        }
    }

    public bool CanApply => Problem is null;
}
