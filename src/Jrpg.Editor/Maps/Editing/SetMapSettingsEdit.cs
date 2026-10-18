using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>Changes a map's settings (display name, random battles) as one undo step.</summary>
internal sealed record SetMapSettingsEdit(MapSettings Before, MapSettings After) : IMapEdit
{
    public void Apply(MapDraft draft) => draft.Settings = After;

    public void Revert(MapDraft draft) => draft.Settings = Before;
}
