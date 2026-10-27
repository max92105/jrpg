using Jrpg.Core.Stats;

namespace Jrpg.Core.Statuses;

/// <summary>
/// A status grant as its user's Intelligence shapes it (decision 296): the power rises by the user's status percent (a
/// Poison or Regen tick, Exposed's share, Guarded's points, Stun's chance) and the turns grow by their extra status
/// turns, except Stun's, which never change (decision 317: <c>TurnFlow</c> skips a turn per Stun turn left, so extra
/// turns would stun-lock). Durations are whole turns, so Intelligence works in breakpoints a player can aim for.
/// </summary>
public static class GrantScaling
{
    /// <param name="grant">The grant as content wrote it.</param>
    /// <param name="statusPermille">What the user's grants have their power multiplied by.</param>
    /// <param name="extraTurns">The turns the user's grants last longer.</param>
    public static StatusGrant Scaled(StatusGrant grant, int statusPermille, int extraTurns) => grant with
    {
        Power = StatFormula.Scaled(grant.Power, statusPermille),
        Turns = grant.Kind == StatusKind.Stun ? grant.Turns : grant.Turns + extraTurns,
    };

    /// <summary>
    /// Stun's chance to land, in permille (decision 324): its scaled power, a percent, less the target's crowd control
    /// resistance, kept within 0..100 %, so a sure stun and a hopeless one draw nothing (ADR 0026).
    /// </summary>
    public static int StunChancePermille(int scaledPower, int crowdControlResistancePermille) => Math.Clamp(
        StatFormula.PermilleOf(scaledPower) - crowdControlResistancePermille,
        0,
        StatFormula.PermilleOf(100));
}
