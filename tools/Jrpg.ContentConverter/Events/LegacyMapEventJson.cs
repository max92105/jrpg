
namespace Jrpg.ContentConverter.Events;

/// <summary>One event of a legacy placed object, with every field the file has.</summary>
internal sealed record LegacyMapEventJson
{
    public required LegacyEventActivation Activation { get; init; }

    public string? Prompt { get; init; }

    public required bool Once { get; init; }

    /// <summary>A legacy int object id.</summary>
    public int? OnlyWhenObjectId { get; init; }

    public required IReadOnlyList<LegacyEventActionJson> Actions { get; init; }
}
