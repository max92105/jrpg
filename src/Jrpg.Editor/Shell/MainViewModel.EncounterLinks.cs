using Jrpg.Core.Maps;
using Jrpg.Editor.Encounters;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The Encounters page's links to maps: every listed map as it is now, and a map's settings changed from the page as an
/// undo step on that map, which is opened in the session without being shown when it wasn't open.
/// </summary>
internal sealed partial class MainViewModel
{
    private EncounterMapLinks CreateEncounterMapLinks()
    {
        var links = new EncounterMapLinks(ReadListedMaps, SetSettingsOfMap);
        _openMaps.DirtyChanged += (_, _) => links.RaiseMapsChanged();
        return links;
    }

    // The saved maps, each open one as it is now in its place, then the new maps not saved yet.
    private IReadOnlyList<MapDefinition> ReadListedMaps() =>
        [.. Catalog!.Maps.Values.Select(saved => _openMaps.Find(saved.Id)?.Map ?? saved)
            .Concat(_openMaps.All.Where(open => !Catalog.Maps.ContainsKey(open.Map.Id)).Select(open => open.Map))];

    // The shown map's step reaches the undo order by itself; another map's is recorded here, and undone once that map
    // is shown, as any step of a map that isn't.
    private void SetSettingsOfMap(MapSettings settings)
    {
        var open = _openMaps.Find(settings.Id);
        if (open is null)
        {
            open = new MapViewModel(Catalog!.Maps[settings.Id]);
            _openMaps.Add(open);
        }

        open.Do(new SetMapSettingsEdit(open.Map.Settings, settings));
        if (open != CurrentMap)
        {
            History!.RecordStepOnMap(settings.Id);
        }
    }
}
