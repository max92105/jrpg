using Jrpg.Core.Effects;
using Jrpg.Core.Resources;

namespace Jrpg.Core.Skills;

/// <summary>One skill. It may cost MP, and it may spend or gain its user's class resources (ADR 0019).</summary>
public sealed record SkillDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    /// <summary>What the skill does, shown while choosing it; it may be empty, as an item's may.</summary>
    public required string Description { get; init; }

    public required string IconFile { get; init; }

    public required int MpCost { get; init; }

    /// <summary>Applied in order when the skill is used; empty when absent.</summary>
    public IReadOnlyList<ResourceChange> ResourceChanges { get; init; } = [];

    public required Effect Effect { get; init; }
}
