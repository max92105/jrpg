namespace Jrpg.Core.Content;

/// <summary>One character a new game may start led by (decision 158).</summary>
public sealed record LeaderChoice
{
    public required string MemberId { get; init; }
}
