using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Tests.Builders;

/// <summary>
/// The statuses the tests grant and compare, in one place: the tests build grants, name statuses and read their numbers
/// only through here, so the status model can change under them with only these bodies following it. Each grant names a
/// one-part record of its own family (decision 800), built here with the grant's amount.
/// </summary>
internal static class TestStatuses
{
    public const string PoisonId = "poison";

    public const string RegenId = "regen";

    public const string GuardId = "guard";

    public const string ExposedId = "exposed";

    public const string StunId = "stun";

    public const string TauntId = "taunt";

    public const string WeakenedId = "weakened";

    public const string EmpoweredId = "empowered";

    public const string SlowedId = "slowed";

    public const string FocusedId = "focused";

    /// <summary>The holder loses <paramref name="amount"/> HP at the end of each of its turns.</summary>
    public static StatusGrant Poison(int amount, int turns) =>
        new(Record(PoisonId, new StatusPart(StatusPartKind.DamageEachTurn, amount)), turns);

    /// <summary>The holder regains <paramref name="amount"/> HP at the end of each of its turns.</summary>
    public static StatusGrant Regen(int amount, int turns) =>
        new(Record(RegenId, new StatusPart(StatusPartKind.HealEachTurn, amount)), turns);

    /// <summary>Every resistance of the holder rises by <paramref name="points"/>.</summary>
    public static StatusGrant Guard(int points, int turns) =>
        new(Record(GuardId, new StatusPart(StatusPartKind.ResistanceChange, points)), turns);

    /// <summary>Every hit on the holder lands harder by <paramref name="percent"/>.</summary>
    public static StatusGrant Exposed(int percent, int turns) =>
        new(Record(ExposedId, new StatusPart(StatusPartKind.DamageTakenChange, percent)), turns);

    /// <summary>The holder loses its next turn; <paramref name="chance"/> is the percent it lands with.</summary>
    public static StatusGrant Stun(int chance, int turns) =>
        new(Record(StunId, new StatusPart(StatusPartKind.SkipTurn)), turns) { Chance = chance };

    /// <summary>Every enemy attack goes to the holder.</summary>
    public static StatusGrant Taunt(int turns) => new(Record(TauntId, new StatusPart(StatusPartKind.DrawAttacks)), turns);

    /// <summary>Every damage the holder deals changes by <paramref name="percent"/>: Weakened's is negative.</summary>
    public static StatusGrant Weakened(int percent, int turns) =>
        new(Record(WeakenedId, new StatusPart(StatusPartKind.DamageDealtChange, percent)), turns);

    /// <summary>
    /// Every damage the holder deals rises by <paramref name="percent"/>, in a family apart from Weakened's.
    /// </summary>
    public static StatusGrant Empowered(int percent, int turns) =>
        new(Record(EmpoweredId, new StatusPart(StatusPartKind.DamageDealtChange, percent)), turns);

    /// <summary>
    /// The holder's <paramref name="stat"/> changes by <paramref name="points"/>: Slowed's are negative.
    /// </summary>
    public static StatusGrant Slowed(StatKind stat, int points, int turns) =>
        new(Record(SlowedId, new StatusPart(StatusPartKind.StatChange, points, Stat: stat)), turns);

    /// <summary>
    /// The holder's <paramref name="stat"/> changes by <paramref name="points"/>, in a family apart from Slowed's.
    /// </summary>
    public static StatusGrant Focused(StatKind stat, int points, int turns) =>
        new(Record(FocusedId, new StatusPart(StatusPartKind.StatChange, points, Stat: stat)), turns);

    public static string IdOf(StatusDefinition status) => status.Id;

    public static string IdOf(ActiveStatus status) => status.Status.Id;

    public static int AmountOf(ActiveStatus status) => status.Status.Parts.Single().Amount;

    public static int AmountOf(StatusGrant grant) => grant.Status.Parts.Single().Amount;

    public static int ChanceOf(StatusGrant grant) => grant.Chance;

    /// <summary>The effect, granting <paramref name="grant"/> to each target still standing after it.</summary>
    public static Effect Granting(Effect effect, StatusGrant grant) => effect with { Statuses = [grant] };

    // Harm and name are the game's concern, which these tests never read.
    private static StatusDefinition Record(string id, StatusPart part) =>
        new() { Id = id, Name = id, Description = "", Parts = [part] };
}
