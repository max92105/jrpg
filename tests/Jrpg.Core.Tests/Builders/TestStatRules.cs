using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Builders;

/// <summary>Stat rules for tests.</summary>
internal static class TestStatRules
{
    /// <summary>Every rate 0: primaries change nothing, so a member's numbers are their class's (ADR 0026).</summary>
    public static StatRules Neutral { get; } = new(HpPercentPer100Constitution: 0);
}
