using Jrpg.Data.Audio;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Maps.Settings;

namespace Jrpg.Editor.Shell;

/// <summary>
/// Map > Map settings: the dialog's state for the shown map, and its apply: the new size first, asking before it drops
/// anything, then the other fields, each one undo step.
/// </summary>
internal sealed partial class MainViewModel
{
    /// <summary>The settings dialog's state for the open map; null when no map is open.</summary>
    public MapSettingsViewModel? StartEditingSettings() =>
        CurrentMap is { } open ? new(open.Map, Catalog!.Encounters.Keys, AudioFolders.TracksIn(_audioRoot)) : null;

    /// <summary>
    /// Applies the settings dialog to the open map: the new size first, asking before it drops anything, then the name,
    /// the track, the area level, the banner and the random battles. Each is one undo step.
    /// </summary>
    /// <returns>False when nothing was applied: a field has a problem or the user kept the old size.</returns>
    public bool TryApplySettings(MapSettingsViewModel edited)
    {
        if (CurrentMap is not { } open || edited.Problem is not null || edited.NewSize is not { } size)
        {
            return false;
        }

        if (edited.IsResizing && !TryResize(open, size.Width, size.Height))
        {
            return false;
        }

        if (edited.AreSettingsChanged)
        {
            open.Do(new SetMapSettingsEdit(open.Map.Settings, edited.ToSettings()));
        }

        return true;
    }

    private bool TryResize(MapViewModel open, int width, int height)
    {
        var resize = new ResizeMapEdit(open.Map.Layout, width, height, Catalog!.Objects);
        var dropsSomething = resize.DroppedTileCount + resize.DroppedObjectCount + resize.DroppedCellEventCount > 0;
        var question = $"Resizing to {width} x {height} drops {resize.DroppedTileCount} tiles, "
            + $"{resize.DroppedObjectCount} objects and {resize.DroppedCellEventCount} cell events outside the new size. "
            + "Resize anyway?";
        if (dropsSomething && !_confirm(question))
        {
            return false;
        }

        open.Do(resize);
        return true;
    }
}
