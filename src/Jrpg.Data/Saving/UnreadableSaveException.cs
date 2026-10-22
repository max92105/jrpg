namespace Jrpg.Data.Saving;

/// <summary>
/// A save file exists but can't be loaded: it's damaged, has no format version (older than versioned saves, ADR 0007), or
/// is from a newer version.
/// </summary>
public sealed class UnreadableSaveException : Exception
{
    public UnreadableSaveException(string slotName, string reason)
        : base($"Save '{slotName}' can't be read: {reason}")
    {
        SlotName = slotName;
        Reason = reason;
    }

    public UnreadableSaveException(string slotName, string reason, Exception innerException)
        : base($"Save '{slotName}' can't be read: {reason}", innerException)
    {
        SlotName = slotName;
        Reason = reason;
    }

    public string SlotName { get; }

    /// <summary>Why, starting with a short reason the Game can show; a damaged file adds the technical detail after it.</summary>
    public string Reason { get; }
}
