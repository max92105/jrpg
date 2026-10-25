namespace Jrpg.Core.Resources;

/// <summary>
/// One battle resource a class has (ADR 0019): its amount lives within 0..<see cref="Max"/> and starts every battle at
/// <see cref="StartsAt"/>.
/// </summary>
public sealed record ResourceDefinition(string Id, string Name, int Max, int StartsAt)
{
    /// <summary>What battle happenings add to the amount; none when absent.</summary>
    public ResourceGains Gains { get; init; } = new();
}
