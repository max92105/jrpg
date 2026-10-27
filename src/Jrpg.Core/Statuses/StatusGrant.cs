namespace Jrpg.Core.Statuses;

/// <summary>A status an effect inflicts, with its numbers from content (ADR 0021).</summary>
/// <param name="Kind">Which status.</param>
/// <param name="Power">What the kind's rule reads, such as the resistance added or Stun's chance in percent.</param>
/// <param name="Turns">How many of the holder's turns it lasts, at least 1.</param>
public sealed record StatusGrant(StatusKind Kind, int Power, int Turns);
