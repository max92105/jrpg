namespace Jrpg.Core.Resources;

/// <summary>
/// One battle resource a class has (ADR 0019): its amount lives within 0..<see cref="Max"/> and starts every battle at
/// <see cref="StartsAt"/>, unless <see cref="MaxFromSpeed"/> sets both from the holder's speed.
/// </summary>
public sealed record ResourceDefinition(string Id, string Name, int Max = 0, int StartsAt = 0)
{
    /// <summary>What battle happenings add to the amount; none when absent.</summary>
    public ResourceGains Gains { get; init; } = new();

    /// <summary>
    /// Set when the max and starting amount are the holder's speed charges; then the numbers above go unused.
    /// </summary>
    public SpeedScaling? MaxFromSpeed { get; init; }
}
