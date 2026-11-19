using Jrpg.Core.Maps;

namespace Jrpg.Editor.Encounters;

/// <summary>
/// The session's maps as the Encounters page links encounters to them: every listed map as it is now, unsaved changes
/// included, and a map's settings changed as one undo step on that map, opened in the session when it wasn't.
/// </summary>
/// <param name="readMaps">Every listed map as it is now.</param>
/// <param name="setSettings">Gives the map the settings name their new values, as one undo step.</param>
internal sealed class EncounterMapLinks(Func<IReadOnlyList<MapDefinition>> readMaps, Action<MapSettings> setSettings)
{
    /// <summary>
    /// A map took a step, undid or redid one, or was saved: which maps roll an encounter may have changed.
    /// </summary>
    public event EventHandler? MapsChanged;

    public IReadOnlyList<MapDefinition> Maps => readMaps();

    /// <param name="settings">The map's settings from now on, its id the map changed.</param>
    public void SetSettings(MapSettings settings) => setSettings(settings);

    /// <summary>Tells the page the maps changed; the shell calls it on every map step.</summary>
    public void RaiseMapsChanged() => MapsChanged?.Invoke(this, EventArgs.Empty);
}
