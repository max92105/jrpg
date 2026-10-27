namespace Jrpg.Core.Resources;

/// <summary>A resource's max and starting amount for one battle, which can differ from its definition's.</summary>
public sealed record ResourceStart(ResourceDefinition Definition, int Max, int Amount)
{
    /// <summary>The definition's own max and starting amount.</summary>
    public static ResourceStart Fixed(ResourceDefinition definition) => new(definition, definition.Max, definition.StartsAt);

    /// <summary>The points the holder's Will takes off every gain to the resource (decision 319); 0 for most.</summary>
    public int GainLowering { get; init; }
}
