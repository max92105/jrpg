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

    /// <summary>Set for a resource whose points can make a spell backfire on its caster; null when safe.</summary>
    public BackfireRule? Backfire { get; init; }

    /// <summary>The color its pips or bar are drawn in, as <c>#RRGGBB</c>; content gives every resource one.</summary>
    public string Color { get; init; } = "";

    /// <summary>
    /// Set for a resource that keeps its amount on the member from battle to battle until a rest (ADR 0024); every
    /// other resource starts each battle at <see cref="StartsAt"/>.
    /// </summary>
    public bool IsLasting { get; init; }

    /// <summary>Set for a lasting resource whose missing points lower another's start; null when none.</summary>
    public StrainRule? Strain { get; init; }

    /// <summary>Set for a force with a rival (the druid's Growth and Decay); null for a lone resource.</summary>
    public DominanceRule? Dominance { get; init; }

    /// <summary>Set for a resource the holder's Will raises (decision 319); null when Will leaves it alone.</summary>
    public WillScaling? WillScaling { get; init; }
}
