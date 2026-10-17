using Jrpg.Core.Effects;

namespace Jrpg.Core.Skills;

/// <summary>One skill. MP is the only skill resource until classes get their own (plan 2026-10-17, guiding rule).</summary>
public sealed record SkillDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string IconFile { get; init; }

    public required int MpCost { get; init; }

    public required Effect Effect { get; init; }
}
