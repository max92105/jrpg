using Jrpg.Core.Stats;

namespace Jrpg.Core.Statuses;

/// <summary>
/// A status grant as its user's Intelligence shapes it (decision 296): the power and the chance rise by the user's
/// status percent (a Poison or Regen tick, Exposed's share, Guarded's points; every grant's chance, decision 801) and
/// the turns grow by their extra status turns, except Stun's, which never change (decision 317: <c>TurnFlow</c> skips a
/// turn per Stun turn left, so extra turns would stun-lock). Durations are whole turns, so Intelligence works in
/// breakpoints a player can aim for.
/// </summary>
public static class GrantScaling
{
    /// <param name="grant">The grant as content wrote it.</param>
    /// <param name="statusPermille">What the user's grants have their power multiplied by.</param>
    /// <param name="extraTurns">The turns the user's grants last longer.</param>
    public static StatusGrant Scaled(StatusGrant grant, int statusPermille, int extraTurns) => grant with
    {
        Power = StatFormula.Scaled(grant.Power, statusPermille),
        Chance = StatFormula.Scaled(grant.Chance, statusPermille),
        Turns = grant.Kind == StatusKind.Stun ? grant.Turns : grant.Turns + extraTurns,
    };

    /// <summary>
    /// A grant's chance to land, in permille (decisions 801, 324): its scaled chance, less the target's crowd control
    /// resistance for a Stun only, kept within 0..100 %, so a sure grant and a hopeless one draw nothing (ADR 0026). A
    /// self-granted taunt is never cut: its holder would otherwise resist it.
    /// </summary>
    /// <param name="scaled">The grant as its user lands it (<see cref="Scaled"/>).</param>
    /// <param name="crowdControlResistancePermille">The target's crowd control resistance.</param>
    public static int ChancePermille(StatusGrant scaled, int crowdControlResistancePermille) => Math.Clamp(
        StatFormula.PermilleOf(scaled.Chance) - (scaled.Kind == StatusKind.Stun ? crowdControlResistancePermille : 0),
        0,
        StatFormula.PermilleOf(100));
}
