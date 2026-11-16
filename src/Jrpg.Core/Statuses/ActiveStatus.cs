namespace Jrpg.Core.Statuses;

/// <summary>A status on a combatant now.</summary>
/// <param name="Status">The record as it landed: its amounts raised by its granter (decision 799).</param>
/// <param name="TurnsLeft">How many more of the holder's turns it lasts.</param>
/// <param name="IsFresh">Granted during the turn now playing, so the end of that turn doesn't count it down.</param>
public sealed record ActiveStatus(StatusDefinition Status, int TurnsLeft, bool IsFresh);
