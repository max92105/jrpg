using System.Collections.Frozen;

namespace Jrpg.Core.Battle;

/// <summary>
/// Every trigger kind's shape in one table, so Data's number check and the target fit read it instead of switching over
/// the kinds (design.md, section 2.5).
/// </summary>
public static class TacticConditionKinds
{
    private static readonly FrozenDictionary<TacticConditionKind, TacticConditionShape> _shapes =
        new Dictionary<TacticConditionKind, TacticConditionShape>
        {
            [TacticConditionKind.OwnHpBelow] = new(TacticSubject.Itself, TacticConditionNumber.Percent),
            [TacticConditionKind.AllyHpBelow] = new(TacticSubject.Allies, TacticConditionNumber.Percent),
            [TacticConditionKind.AllyFallen] = new(TacticSubject.FallenAllies, TacticConditionNumber.None),
            [TacticConditionKind.WasHit] = new(TacticSubject.Foes, TacticConditionNumber.None, IsReaction: true),
            [TacticConditionKind.WasCriticallyHit] = new(TacticSubject.Foes, TacticConditionNumber.None, IsReaction: true),
            [TacticConditionKind.WasHitOnWeakness] = new(TacticSubject.Foes, TacticConditionNumber.None, IsReaction: true),
        }.ToFrozenDictionary();

    /// <exception cref="ArgumentOutOfRangeException">A kind the table doesn't hold.</exception>
    public static TacticConditionShape ShapeOf(TacticConditionKind kind) =>
        _shapes.TryGetValue(kind, out var shape)
            ? shape
            : throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown tactic condition.");
}
