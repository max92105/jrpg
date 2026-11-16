using Jrpg.Core.Effects;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Tests.Builders;

/// <summary>
/// The statuses the tests grant and compare, in one place: the tests build grants, name statuses and read their numbers
/// only through here, so the status model can change under them with only these bodies following it.
/// </summary>
internal static class TestStatuses
{
    public static StatusKind PoisonId => StatusKind.Poison;

    public static StatusKind RegenId => StatusKind.Regen;

    public static StatusKind GuardId => StatusKind.Guarded;

    public static StatusKind ExposedId => StatusKind.Exposed;

    public static StatusKind StunId => StatusKind.Stun;

    public static StatusKind TauntId => StatusKind.Taunt;

    /// <summary>The holder loses <paramref name="amount"/> HP at the end of each of its turns.</summary>
    public static StatusGrant Poison(int amount, int turns) => new(StatusKind.Poison, amount, turns);

    /// <summary>The holder regains <paramref name="amount"/> HP at the end of each of its turns.</summary>
    public static StatusGrant Regen(int amount, int turns) => new(StatusKind.Regen, amount, turns);

    /// <summary>Every resistance of the holder rises by <paramref name="points"/>.</summary>
    public static StatusGrant Guard(int points, int turns) => new(StatusKind.Guarded, points, turns);

    /// <summary>Every hit on the holder lands harder by <paramref name="percent"/>.</summary>
    public static StatusGrant Exposed(int percent, int turns) => new(StatusKind.Exposed, percent, turns);

    /// <summary>The holder loses its next turn; <paramref name="chance"/> is the percent it lands with.</summary>
    public static StatusGrant Stun(int chance, int turns) => new(StatusKind.Stun, 0, turns) { Chance = chance };

    /// <summary>Every enemy attack goes to the holder.</summary>
    public static StatusGrant Taunt(int turns) => new(StatusKind.Taunt, 0, turns);

    public static StatusKind IdOf(StatusKind status) => status;

    public static StatusKind IdOf(ActiveStatus status) => status.Kind;

    public static int AmountOf(ActiveStatus status) => status.Power;

    public static int AmountOf(StatusGrant grant) => grant.Power;

    public static int ChanceOf(StatusGrant grant) => grant.Chance;

    /// <summary>The effect, granting <paramref name="grant"/> to each target still standing after it.</summary>
    public static Effect Granting(Effect effect, StatusGrant grant) => effect with { Statuses = [grant] };
}
