namespace Jrpg.Core.Events;

/// <summary>What happens when the player interacts with or touches an object or a cell.</summary>
public sealed record MapEvent
{
    /// <summary>Unique within its map; saves remember "once" events by it.</summary>
    public required string Id { get; init; }

    public required EventActivation Activation { get; init; }

    /// <summary>The hint shown when the player can interact, such as "Open".</summary>
    public string? Prompt { get; init; }

    /// <summary>Runs at most once per playthrough, such as a chest's contents.</summary>
    public bool IsOnce { get; init; }

    /// <summary>When set, the event only runs while its object is this one, such as a closed door but not an open one.</summary>
    public string? OnlyWhenObjectId { get; init; }

    /// <summary>When set, the event only runs while this member leads, such as each leader's intro (decision 185).</summary>
    public string? OnlyWhenLeaderId { get; init; }

    public required IReadOnlyList<EventStep> Steps { get; init; }

    public bool CanRunOn(string objectId) => OnlyWhenObjectId is null || OnlyWhenObjectId == objectId;

    public bool IsForLeader(string leaderId) => OnlyWhenLeaderId is null || OnlyWhenLeaderId == leaderId;
}
