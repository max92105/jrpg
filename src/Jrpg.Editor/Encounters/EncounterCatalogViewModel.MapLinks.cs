using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Jrpg.Editor.Encounters;

/// <summary>
/// The chosen encounter's maps: those whose random battles pick it, Link adding one through its dialog and Remove
/// taking the chosen one out, each at once as an undo step on that map, never waiting for the page's Apply. Only an
/// encounter the draft holds is linked, since a map can't name one the page hasn't applied yet.
/// </summary>
internal sealed partial class EncounterCatalogViewModel
{
    /// <summary>The maps whose random battles pick the chosen encounter, by name then id; none for a new one.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<MapLinkChoice> LinkedMaps { get; private set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UnlinkMapCommand))]
    public partial MapLinkChoice? SelectedLinkedMap { get; set; }

    /// <summary>False while no encounter is chosen or the chosen one waits for the page's Apply.</summary>
    public bool CanLinkMaps => SelectedEncounter is { IsNew: false };

    /// <summary>
    /// The Link dialog's state for the chosen encounter: the maps not rolling it yet, as they are now.
    /// </summary>
    /// <exception cref="InvalidOperationException">The chosen encounter can't be linked.</exception>
    public MapLinkViewModel StartLinkingMap()
    {
        if (!CanLinkMaps)
        {
            throw new InvalidOperationException("Only an applied encounter is linked to a map.");
        }

        var encounterId = SelectedEncounter!.Id;
        return new MapLinkViewModel(
            encounterId,
            [.. _maps.Maps.Where(map => !MapEncounterLink.IsLinked(map.Settings, encounterId))],
            _choices.Backgrounds);
    }

    /// <summary>Links the dialog's map as one undo step on it, and lists it.</summary>
    public void LinkMap(MapLinkViewModel link)
    {
        _maps.SetSettings(link.ToSettings());
        RefreshLinkedMaps();
    }

    /// <summary>
    /// Reads the chosen encounter's maps again, after a link, a removal, or a map's own step, undo or save; the chosen
    /// map stays chosen while it is still linked.
    /// </summary>
    public void RefreshLinkedMaps()
    {
        var chosenId = SelectedLinkedMap?.MapId;
        LinkedMaps = SelectedEncounter is { IsNew: false } encounter
            ? MapLinkChoice.AllOf(_maps.Maps.Where(map => MapEncounterLink.IsLinked(map.Settings, encounter.Id)))
            : [];
        SelectedLinkedMap = LinkedMaps.FirstOrDefault(map => map.MapId == chosenId);
    }

    partial void OnSelectedEncounterChanged(EncounterViewModel? value)
    {
        OnPropertyChanged(nameof(CanLinkMaps));
        RefreshLinkedMaps();
    }

    [RelayCommand(CanExecute = nameof(HasSelectedLinkedMap))]
    private void UnlinkMap()
    {
        var map = _maps.Maps.Single(map => map.Id == SelectedLinkedMap!.MapId);
        _maps.SetSettings(MapEncounterLink.Unlinked(map.Settings, SelectedEncounter!.Id));
        RefreshLinkedMaps();
    }

    private bool HasSelectedLinkedMap() => SelectedLinkedMap is not null;
}
