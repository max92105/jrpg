namespace Jrpg.Editor.Events;

/// <summary>
/// One placed NPC of the event's map, as the move step offers it: shown as <c>object (uid)</c>; the step's box matches
/// an entry by its uid.
/// </summary>
internal sealed record NpcChoice(string Uid, string ObjectId)
{
    public string Label => $"{ObjectId} ({Uid})";
}
