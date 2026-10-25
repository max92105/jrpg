using Jrpg.Core.Effects;
using Jrpg.Core.Resources;

namespace Jrpg.Core.Skills;

/// <summary>One skill. It may need, spend or gain its user's class resources (ADR 0019); none costs MP.</summary>
public sealed record SkillDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    /// <summary>What the skill does, shown while choosing it; it may be empty, as an item's may.</summary>
    public required string Description { get; init; }

    public required string IconFile { get; init; }

    /// <summary>Applied in order when the skill is used; empty when absent.</summary>
    public IReadOnlyList<ResourceChange> ResourceChanges { get; init; } = [];

    /// <summary>Minimums the user must hold, checked before the skill's own changes; empty when absent.</summary>
    public IReadOnlyList<ResourceRequirement> Requirements { get; init; } = [];

    /// <summary>Keeps the actor's turn (decision 202): no upkeep, no advance. It must spend a resource.</summary>
    public bool IsQuick { get; init; }

    public required Effect Effect { get; init; }
}
