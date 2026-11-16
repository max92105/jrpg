using Jrpg.Core.Stats;

namespace Jrpg.Core.Statuses;

/// <summary>
/// A status grant as its user's Intelligence shapes it (decisions 296, 799): every part's amount and the chance rise by
/// the user's status percent (a tick, a change in damage taken, a resistance change; every grant's chance, decision
/// 801) and the turns grow by their extra status turns, except a skipped turn's, which never change (decision 317:
/// <c>TurnFlow</c> skips a turn per turn left, so extra turns would stun-lock). Durations are whole turns, so
/// Intelligence works in breakpoints a player can aim for.
/// </summary>
public static class GrantScaling
{
    /// <param name="grant">The grant as content wrote it.</param>
    /// <param name="statusPermille">What the user's grants have their amounts and chance multiplied by.</param>
    /// <param name="extraTurns">The turns the user's grants last longer.</param>
    public static StatusGrant Scaled(StatusGrant grant, int statusPermille, int extraTurns) => grant with
    {
        Status = grant.Status.Strengthened(statusPermille),
        Chance = StatFormula.Scaled(grant.Chance, statusPermille),
        Turns = grant.Status.Has(StatusPartKind.SkipTurn) ? grant.Turns : grant.Turns + extraTurns,
    };

    /// <summary>
    /// A grant's chance to land, in permille (decisions 801, 324): its scaled chance, less the target's crowd control
    /// resistance for a status that skips a turn only, kept within 0..100 %, so a sure grant and a hopeless one draw
    /// nothing (ADR 0026). A self-granted taunt is never cut: its holder would otherwise resist it.
    /// </summary>
    /// <param name="scaled">The grant as its user lands it (<see cref="Scaled"/>).</param>
    /// <param name="crowdControlResistancePermille">The target's crowd control resistance.</param>
    public static int ChancePermille(StatusGrant scaled, int crowdControlResistancePermille) => Math.Clamp(
        StatFormula.PermilleOf(scaled.Chance) - (scaled.Status.Has(StatusPartKind.SkipTurn) ? crowdControlResistancePermille : 0),
        0,
        StatFormula.PermilleOf(100));
}
