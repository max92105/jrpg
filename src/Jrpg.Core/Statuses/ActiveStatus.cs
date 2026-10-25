namespace Jrpg.Core.Statuses;

/// <summary>A status on a combatant now.</summary>
/// <param name="Kind">Which status.</param>
/// <param name="Power">The granting effect's power.</param>
/// <param name="TurnsLeft">How many more of the holder's turns it lasts.</param>
/// <param name="IsFresh">Granted during the turn now playing, so the end of that turn doesn't count it down.</param>
public sealed record ActiveStatus(StatusKind Kind, int Power, int TurnsLeft, bool IsFresh);
