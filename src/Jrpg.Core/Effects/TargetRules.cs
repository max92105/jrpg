using Jrpg.Core.Stats;

namespace Jrpg.Core.Effects;

/// <summary>Which targets an effect may hit.</summary>
internal static class TargetRules
{
    /// <remarks>Fallen-ally targetings only accept fallen candidates; every other targeting only accepts living ones.</remarks>
    public static bool CanTarget(Targeting targeting, TargetRelation relation, Vitals candidate) => targeting switch
    {
        Targeting.SingleEnemy or Targeting.AllEnemies => relation == TargetRelation.Enemy && candidate.IsAlive,
        Targeting.SingleAlly or Targeting.AllAllies => relation != TargetRelation.Enemy && candidate.IsAlive,
        Targeting.Self => relation == TargetRelation.Self && candidate.IsAlive,
        Targeting.SingleFallenAlly or Targeting.AllFallenAllies => relation != TargetRelation.Enemy && !candidate.IsAlive,
        _ => throw new ArgumentOutOfRangeException(nameof(targeting), targeting, "Unknown targeting."),
    };

    public static bool HitsEveryValidTarget(Targeting targeting) =>
        targeting is Targeting.AllEnemies or Targeting.AllAllies or Targeting.AllFallenAllies;

    /// <summary>
    /// Whether <paramref name="chosen"/> is a legal set of targets: one valid target for a single-target effect,
    /// every valid target for an "all" effect, never empty, never the same target twice.
    /// </summary>
    public static bool IsLegalChoice<TTarget>(Targeting targeting, IReadOnlySet<TTarget> validTargets, IReadOnlyList<TTarget> chosen)
    {
        var expectedCount = HitsEveryValidTarget(targeting) ? validTargets.Count : 1;
        return chosen.Count > 0
            && chosen.Count == expectedCount
            && chosen.Distinct().Count() == chosen.Count
            && chosen.All(validTargets.Contains);
    }
}
