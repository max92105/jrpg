using Jrpg.Core.Events;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>Replaces a placed object's events as one undo step, as the event dialog's OK does.</summary>
/// <param name="Uid">The placed object.</param>
/// <param name="Before">Its events before.</param>
/// <param name="After">Its events after.</param>
internal sealed record SetObjectEventsEdit(string Uid, IReadOnlyList<MapEvent> Before, IReadOnlyList<MapEvent> After) : IMapEdit
{
    public void Apply(MapDraft draft) => draft.SetObjectEvents(Uid, After);

    public void Revert(MapDraft draft) => draft.SetObjectEvents(Uid, Before);
}
