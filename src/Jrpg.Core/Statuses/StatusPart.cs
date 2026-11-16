using Jrpg.Core.Stats;

namespace Jrpg.Core.Statuses;

/// <summary>One rule a status applies while held, with its number (ADR 0055).</summary>
/// <param name="Kind">What the part does.</param>
/// <param name="Amount">
/// Its number, meaning what the kind says: HP for a tick, points for a resistance or a stat change, percent for a
/// change in damage taken; 0 and unread for a skipped turn or drawn attacks.
/// </param>
/// <param name="DamageType">
/// The type a damage tick meets resistance as; null ignores resistances (decision 803).
/// </param>
/// <param name="Stat">
/// The stat a stat change changes, one of <see cref="StatusModifiers.ChangeableStats"/>; null on every other kind.
/// </param>
public sealed record StatusPart(StatusPartKind Kind, int Amount = 0, DamageType? DamageType = null, StatKind? Stat = null);
